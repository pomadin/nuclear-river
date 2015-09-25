﻿using System;
using System.Collections.Generic;
using System.Linq;

using NuClear.Metamodeling.Elements;
using NuClear.Metamodeling.Elements.Concrete.Hierarchy;
using NuClear.Metamodeling.Elements.Identities.Builder;
using NuClear.Metamodeling.Provider.Sources;
using NuClear.Replication.Metadata;
using NuClear.Replication.Metadata.Aggregates;

using CI = NuClear.CustomerIntelligence.Domain.Model.CI;
using Specs = NuClear.CustomerIntelligence.Domain.Specifications.Specs;

namespace NuClear.CustomerIntelligence.Domain
{
    public class StatisticsRecalculationMetadataSource : MetadataSourceBase<StatisticsRecalculationMetadataIdentity>
    {
        private readonly IReadOnlyDictionary<Uri, IMetadataElement> _metadata;

        public StatisticsRecalculationMetadataSource()
        {
            HierarchyMetadata statisticsRecalculationMetadataRoot =
                HierarchyMetadata
                    .Config
                    .Id.Is(Metamodeling.Elements.Identities.Builder.Metadata.Id.For<StatisticsRecalculationMetadataIdentity>())
                    .Childs(StatisticsRecalculationMetadata<CI::FirmCategoryStatistics>
                                .Config
                                .HasSource(Specs.Map.Facts.ToStatistics.FirmCategoryStatistics)
                                .HasTarget(Specs.Map.CI.ToStatistics.FirmCategoryStatistics)
                                .HasFilter(
                                    (projectId, categoryIds) =>
                                    categoryIds.Contains(null)
                                        ? Specs.Find.CI.FirmCategoryStatistics.ByProject(projectId)
                                        : Specs.Find.CI.FirmCategoryStatistics.ByProjectAndCategories(projectId, categoryIds))
                                .HasFieldComparer(new CI::FirmCategoryStatistics.FullEqualityComparer()));

            _metadata = new Dictionary<Uri, IMetadataElement> { { statisticsRecalculationMetadataRoot.Identity.Id, statisticsRecalculationMetadataRoot } };
        }

        public override IReadOnlyDictionary<Uri, IMetadataElement> Metadata
        {
            get { return _metadata; }
        }
    }
}