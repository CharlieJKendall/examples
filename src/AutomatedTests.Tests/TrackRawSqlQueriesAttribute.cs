﻿using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using System.Reflection;

namespace AutomatedTests.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TrackRawSqlQueriesAttribute : Attribute, IWrapSetUpTearDown
{
    public static bool IsEnabledForCurrentTestContext =>
        Type.GetType(TestContext.CurrentContext.Test.ClassName!)
        ?.GetMethod(TestContext.CurrentContext.Test.MethodName!)
        ?.GetCustomAttribute<TrackRawSqlQueriesAttribute>() is not null;

    TestCommand ICommandWrapper.Wrap(TestCommand command) => new TrackRawSqlQueriesCommand(command);

    private class TrackRawSqlQueriesCommand : AfterTestCommand
    {
        public TrackRawSqlQueriesCommand(TestCommand innerCommand)
            : base(innerCommand)
        {
            AfterTest = WriteTrackedSqlCommandsToFile;
        }

        private static void WriteTrackedSqlCommandsToFile(TestExecutionContext context)
        {
            var queries = TrackRawSqlCommandInterceptor.GetSqlQueriesExecutedForTest(context);
            if (queries is null)
            {
                return;
            }

            TestHelpers.CompareToExistingFileAndOverwrite(
                latest: string.Join("\r\n\r\n----------------------\r\n\r\n", queries),
                filePath: TestHelpers.GetTestOutputDirectoryPathForCurrentExecutingTest("queries.sql"),
                failMessage: "SQL generated by EF Core has changed for test, automatically regenerating");
        }
    }
}
