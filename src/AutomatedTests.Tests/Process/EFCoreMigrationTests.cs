using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AutomatedTests.Tests.Process;

internal class EFCoreMigrationTests
{
    [Test]
    public void Ensure_no_pending_context_model_changes()
    {
        using var context = TestHelpers.CreateCleanDbContext();

        var modelDiffer = context.GetService<IMigrationsModelDiffer>();
        var migrationsAssembly = context.GetService<IMigrationsAssembly>();
        var modelInitializer = context.GetService<IModelRuntimeInitializer>();
        var designTimeModel = context.GetService<IDesignTimeModel>();

        var snapshotModel = migrationsAssembly.ModelSnapshot?.Model;
        if (snapshotModel is IMutableModel mutableModel)
        {
            snapshotModel = mutableModel.FinalizeModel();
        }

        if (snapshotModel is not null)
        {
            snapshotModel = modelInitializer.Initialize(snapshotModel);
        }

        var hasPendingModelChanges = modelDiffer.HasDifferences(
            source: snapshotModel?.GetRelationalModel(),
            target: designTimeModel.Model.GetRelationalModel());

        Assert.That(hasPendingModelChanges, Is.False);
    }
}
