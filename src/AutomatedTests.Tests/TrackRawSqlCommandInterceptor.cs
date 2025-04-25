using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework.Internal;
using System.Collections.Concurrent;
using System.Data.Common;

namespace AutomatedTests.Tests;

public class TrackRawSqlCommandInterceptor : DbCommandInterceptor
{
    private static readonly ConcurrentDictionary<string, ConcurrentBag<string>> _trackedSqlQueries = new();
    
    public static string[]? GetSqlQueriesExecutedForTest(TestExecutionContext context) =>
        _trackedSqlQueries.GetValueOrDefault(context.CurrentTest.Id)?.ToArray();

    public bool IsEnabled { get; set; }

    public override DbCommand CommandInitialized(CommandEndEventData eventData, DbCommand result)
    {
        if (IsEnabled)
        {
            _trackedSqlQueries
                .GetOrAdd(TestExecutionContext.CurrentContext.CurrentTest.Id, _ => [])
                .Add(result.CommandText);
        }

        return base.CommandInitialized(eventData, result);
    }
}
