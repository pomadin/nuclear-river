﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

using NuClear.Metamodeling.Elements;
using NuClear.Metamodeling.Provider;
using NuClear.Replication.Core.API.Aggregates;
using NuClear.Replication.Metadata;
using NuClear.Replication.Metadata.Operations;
using NuClear.Telemetry.Probing;

namespace NuClear.Replication.Core.Aggregates
{
    public sealed class AggregatesConstructor : IAggregatesConstructor
    {
        private readonly IMetadataProvider _metadataProvider;
        private readonly IAggregateProcessorFactory _aggregateProcessorFactory;

        public AggregatesConstructor(IMetadataProvider metadataProvider, IAggregateProcessorFactory aggregateProcessorFactory)
        {
            _metadataProvider = metadataProvider;
            _aggregateProcessorFactory = aggregateProcessorFactory;
        }

        public void Construct(IEnumerable<AggregateOperation> operations)
        {
            using (Probe.Create("ETL2 Transforming"))
            {
                MetadataSet metadataSet;
                if (!_metadataProvider.TryGetMetadata<ReplicationMetadataIdentity>(out metadataSet))
                {
                    throw new NotSupportedException(string.Format("Metadata for identity '{0}' cannot be found.", typeof(ReplicationMetadataIdentity).Name));
                }

                var slices = operations.GroupBy(x => new { Operation = x.GetType(), x.AggregateType })
                                       .OrderByDescending(x => x.Key.Operation, new AggregateOperationPriorityComparer());

                foreach (var slice in slices)
                {
                    var operation = slice.Key.Operation;
                    var aggregateType = slice.Key.AggregateType;

                    IMetadataElement aggregateMetadata;
                    if (!metadataSet.Metadata.Values.TryGetElementById(new Uri(aggregateType.Name, UriKind.Relative), out aggregateMetadata))
                    {
                        throw new NotSupportedException(string.Format("The aggregate of type '{0}' is not supported.", aggregateType));
                    }

                    var aggregateIds = slice.Select(x => x.AggregateId).Distinct().ToArray();
                    using (var transaction = new TransactionScope(TransactionScopeOption.Required,
                                                                  new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.Zero }))
                    {
                        using (Probe.Create("ETL2 Transforming", aggregateType.Name))
                        {
							var processor = _aggregateProcessorFactory.Create(aggregateType, aggregateMetadata);

							if (operation == typeof(InitializeAggregate))
                            {
								processor.Initialize(aggregateIds);
								continue;
                            }

                            if (operation == typeof(RecalculateAggregate))
                            {
								processor.Recalculate(aggregateIds);
								continue;
                            }

                            if (operation == typeof(DestroyAggregate))
                            {
								processor.Destroy(aggregateIds);
								continue;
                            }
                        }

                        transaction.Complete();
                    }
                }
            }
        }
    }
}

