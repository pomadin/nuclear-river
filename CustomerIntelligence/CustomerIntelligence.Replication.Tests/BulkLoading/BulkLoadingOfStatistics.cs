using System;
using System.Data.Common;
using System.Linq;

using NuClear.CustomerIntelligence.Domain.Specifications;
using NuClear.CustomerIntelligence.Replication.Tests.Data;
using NuClear.CustomerIntelligence.Storage;
using NuClear.Storage.Readings;

using NUnit.Framework;

namespace NuClear.CustomerIntelligence.Replication.Tests.BulkLoading
{
    [TestFixture, Explicit("It's used to copy the data in bulk.")]
    internal class BulkLoadingOfStatistics : BulkLoadingFixtureBase
    {
        [Test]
        public void ReloadFirmCategoryStatistics()
        {
            Reload(query => Specs.Map.Facts.ToStatistics.FirmCategoryStatistics.Map(query));
        }

        private void Reload<T>(Func<IQuery, IQueryable<T>> loader)
            where T : class
        {
            using (var ermDb = CreateConnection("FactsSqlServer", Schema.Erm))
            using (var factDb = CreateConnection("CustomerIntelligenceSqlServer", Schema.Facts))
            {
                var query = new Query(new StubReadableDomainContextProvider((DbConnection)ermDb.Connection, ermDb));
                factDb.Reload(loader(query));
            }
        }
    }
}