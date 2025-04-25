using System.Xml.Linq;

namespace AutomatedTests.Tests.AutoGeneration;

internal class DependencyTreeGenerationTests
{
    [Test]
    public async Task Generate_full_dependency_tree_yaml()
    {
        var pathToRepositoryRoot = TestHelpers.GetPathToRepositoryRoot();
        var projectFiles = GetReferencedProjectFiles(new FileInfo("../../../AutomatedTests.Tests.csproj"));
        var formattedProjectFiles = projectFiles
            .Select(x => x.FullName)
            .Select(x => x[pathToRepositoryRoot.Length..])
            .Select(x => $"- {x}");
        
        await TestHelpers.CompareToExistingFileAndOverwriteAsync(
            latest: string.Join("\r\n", formattedProjectFiles),
            filePath: TestHelpers.GetTestOutputDirectoryPathForCurrentExecutingTest("ProjectDependencies.txt"),
            failMessage: "Project dependencies have changed for AutomatedTests.Tests.csproj, automatically regenerating");
    }

    private static FileInfo[] GetReferencedProjectFiles(FileInfo projectFile)
    {
        var projectFiles = new HashSet<FileInfo>();

        Impl(projectFile);

        return [..projectFiles];

        void Impl(FileInfo project)
        {
            if (projectFiles.Add(project) is false)
            {
                return;
            }

            var projectReferences = XDocument
                .Load(project.FullName)
                .Descendants("ProjectReference")
                .Attributes("Include")
                .Select(attr => new FileInfo(Path.Combine(project.DirectoryName!, attr.Value)));

            foreach (var reference in projectReferences)
            {
                Impl(reference);
            }
        }
    }
}
