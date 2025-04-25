using AutomatedTests.Api;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;

namespace AutomatedTests.Tests.Consistency
{
    public class DtoMemberTests
    {
        [Test]
        public void Ensure_all_enum_members_are_nullable_on_dtos()
        {
            var endpointMethods = Assembly
                .GetAssembly(typeof(Program))!
                .GetExportedTypes()
                .SelectMany(type => type.GetMethods())
                .Where(method => method.CustomAttributes.Any(attribute => attribute.AttributeType.IsAssignableTo(typeof(HttpMethodAttribute))))
                .ToArray();

            var inputParameters = endpointMethods
                .SelectMany(method => method.GetParameters())
                .Select(parameter => parameter.ParameterType)
                .ToArray();

            var returnParameters = endpointMethods
                .Select(method => method.ReturnParameter.ParameterType)
                .ToArray();

            var allDtoTypes = returnParameters
                .Concat(inputParameters)
                .SelectMany(GetAllMemberTypesRecursive)
                .Distinct()
                .Where(type => type.Namespace!.StartsWith("AutomatedTests"))
                .ToArray();

            var dtoTypesWithEnumsMissingDefaultValue = allDtoTypes
                .Where(type => GetAllMemberTypes(type).Any(memberType => memberType.IsEnum && Enum.IsDefined(memberType, 0) is false))
                .ToArray();

            CollectionAssert.IsEmpty(
                dtoTypesWithEnumsMissingDefaultValue,
                "DTO types should not have non-nullable boolean members");
        }

        private static Type[] GetAllMemberTypesRecursive(Type type)
        {
            var types = new HashSet<Type>();

            GetAllMemberTypesImpl(type);

            return [.. types];

            void GetAllMemberTypesImpl(Type type)
            {
                var associatedTypes = GetAssociatedTypes(type);

                foreach (var associatedType in associatedTypes)
                {
                    if (types.Add(associatedType) is false)
                    {
                        continue;
                    }

                    var memberTypes = GetAllMemberTypes(associatedType);

                    foreach (var memberType in memberTypes)
                    {
                        GetAllMemberTypesImpl(memberType);
                    }
                }
            }

            static Type[] GetAssociatedTypes(Type type) => type switch
            {
                _ when type.IsGenericType => [type, .. type.GetGenericArguments()],
                _ when type.IsArray => [type, type.GetElementType()!],
                _ => [type],
            };
        }

        private static Type[] GetAllMemberTypes(Type type)
        {

            var properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(x => x.PropertyType)
                .ToArray();

            var fields = type
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Select(x => x.FieldType)
                .ToArray();

            return [.. properties, .. fields];
        }
    }
}