using AutomatedTests.Api;
using AutomatedTests.LibraryB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework.Internal;
using System.Reflection;

namespace AutomatedTests.Tests;

internal static class TestHelpers
{
    public static void CompareToExistingFileAndOverwrite(
        string latest,
        string filePath,
        string failMessage) =>
            CompareToExistingFileAndOverwriteAsync(
                latest,
                filePath,
                failMessage).GetAwaiter().GetResult();

    public static async Task CompareToExistingFileAndOverwriteAsync(
        string latest,
        string filePath,
        string failMessage)
    {
        var previous = File.Exists(filePath)
            ? await File.ReadAllTextAsync(filePath)
            : null;

        if (previous is null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        }

        await File.WriteAllTextAsync(filePath, latest);

        if (latest != previous)
        {
            Assert.Fail(failMessage);
        }
    }

    public static string GetTestOutputDirectoryPath() =>
        Path.Combine(
            GetPathToRepositoryRoot(),
            "test-output",
            Assembly.GetExecutingAssembly()!.GetName().Name!);

    public static string GetTestOutputDirectoryPathForCurrentExecutingTest(string fileName)
    {
        var entryAssemblyName = Assembly.GetExecutingAssembly()!.GetName().Name!; // "AutomatedTests.Tests"
        var pathForTest = TestExecutionContext.CurrentContext.CurrentTest.FullName // "AutomatedTests.Tests.Regression.EfCoreLinqToSqlRegressionTests.Test"
            .Substring(entryAssemblyName.Length + 1) // Remove "AutomatedTests.Tests." prefix
            .Replace('.', '/'); // Convert to file path

        return Path.Combine(
            GetTestOutputDirectoryPath(),
            pathForTest,
            fileName);
    }

    public static string GetPathToRepositoryRoot()
    {
        var relativePathToRoot = "../../../../..";
        var executingAssembly = new FileInfo(Assembly.GetExecutingAssembly().Location);
        var repositoryRootDirectory = new DirectoryInfo(Path.Combine(executingAssembly.DirectoryName!, relativePathToRoot));

        return repositoryRootDirectory.FullName;
    }

    public static Type[] GetAllControllerTypes() =>
        Assembly
            .GetAssembly(typeof(Program))!
            .ExportedTypes
            .Where(x => x.IsAssignableTo(typeof(ControllerBase)))
            .ToArray();

    public static MyDbContext CreateCleanDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
        var trackRawSqlCommandInterceptor = new TrackRawSqlCommandInterceptor();

        optionsBuilder.AddInterceptors(trackRawSqlCommandInterceptor);
        optionsBuilder.UseSqlite("Data Source=data.db");

        var context = new MyDbContext(optionsBuilder.Options);

        context.Database.EnsureDeleted();
        context.Database.Migrate();

        trackRawSqlCommandInterceptor.IsEnabled = TrackRawSqlQueriesAttribute.IsEnabledForCurrentTestContext;

        return context;
    }
}
