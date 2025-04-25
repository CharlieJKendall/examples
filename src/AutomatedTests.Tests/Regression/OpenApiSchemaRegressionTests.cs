using NSwag.CodeGeneration.TypeScript;
using NSwag.Generation.WebApi;

namespace AutomatedTests.Tests.Regression;

internal class OpenApiSchemaRegressionTests
{
    [Test]
    public async Task Verify_openapi_schema_is_unchanged()
    {
        var controllersTypes = TestHelpers.GetAllControllerTypes();
        var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
        var openApiDocument = await generator.GenerateForControllersAsync(controllersTypes);
        string filePath = TestHelpers.GetTestOutputDirectoryPathForCurrentExecutingTest("OpenApi.json");
        var tsClientGenerator = new TypeScriptClientGenerator(openApiDocument, new TypeScriptClientGeneratorSettings());
        var clients = tsClientGenerator.GenerateFile();


        Assert.Multiple(async () =>
        {
            await TestHelpers.CompareToExistingFileAndOverwriteAsync(
                latest: openApiDocument.ToJson(),
                filePath: filePath,
                failMessage: "OpenApi schema has changed, automatically regenerating");

            await TestHelpers.CompareToExistingFileAndOverwriteAsync(
                latest: clients,
                filePath: TestHelpers.GetTestOutputDirectoryPathForCurrentExecutingTest("ApiClients.ts"),
                failMessage: "ApiClients.ts file has changed, automatically regenerating");
        });
    }
}
