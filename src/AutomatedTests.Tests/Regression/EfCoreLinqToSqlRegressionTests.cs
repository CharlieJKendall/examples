using Microsoft.EntityFrameworkCore;
using NUnit.Framework.Internal;

namespace AutomatedTests.Tests.Regression;

internal class EfCoreLinqToSqlRegressionTests
{
    [Test]
    [TrackRawSqlQueries]
    public async Task No_entities_returned_for_id_equal_to_100()
    {
        // Arrange
        using var context = TestHelpers.CreateCleanDbContext();
        
        // Act
        var result = await context.MyEntities
            .Select(x => x.Id)
            .Where(x => x == 100)
            .FirstOrDefaultAsync();

        // Assert
        Assert.That(result, Is.EqualTo(default(int)));
    }
}
