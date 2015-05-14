﻿using Moq;

using NuClear.AdvancedSearch.Replication.CustomerIntelligence.Data.Context;
using NuClear.AdvancedSearch.Replication.Tests.Data;

using NUnit.Framework;

// ReSharper disable PossibleUnintendedReferenceComparison
namespace NuClear.AdvancedSearch.Replication.Tests.Transformation
{
    using Erm = CustomerIntelligence.Model.Erm;
    using Facts = CustomerIntelligence.Model.Facts;
    using CI = CustomerIntelligence.Model;

    [TestFixture]
    internal partial class FactsTransformationTests
    {
        [Test]
        public void ShouldInitializeCategoryIfCategoryCreated()
        {
            var source = Mock.Of<IFactsContext>(ctx => ctx.Categories == Inquire(new Facts::Category { Id = 1 }));

            Transformation.Create(source, FactsDb)
                          .Transform(Fact.Create<Facts::Category>(1))
                          .Verify(Inquire(Aggregate.Initialize<CI::Category>(1)));
        }

        [Test]
        public void ShouldRecalculateCategoryIfCategoryUpdated()
        {
            var source = Mock.Of<IFactsContext>(ctx => ctx.Categories == Inquire(new Facts::Category { Id = 1 }));

            FactsDb.Has(new Facts::Category { Id = 1 });

            Transformation.Create(source, FactsDb)
                          .Transform(Fact.Update<Facts::Category>(1))
                          .Verify(Inquire(Aggregate.Recalculate<CI::Category>(1)));
        }

        [Test]
        public void ShouldDestroyCategoryIfCategoryDeleted()
        {
            var source = Mock.Of<IFactsContext>();

            FactsDb.Has(new Facts::Category { Id = 1 });

            Transformation.Create(source, FactsDb)
                          .Transform(Fact.Delete<Facts::Category>(1))
                          .Verify(Inquire(Aggregate.Destroy<CI::Category>(1)));
        }
    }
}