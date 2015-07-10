using System;
using System.Collections.Generic;
using System.Linq;

using NuClear.Messaging.API;
using NuClear.Messaging.API.Receivers;
using NuClear.OperationsProcessing.Transports.SQLStore.Final;
using NuClear.Replication.OperationsProcessing.Transports.SQLStore;
using NuClear.Telemetry;
using NuClear.Telemetry.Probing;

namespace NuClear.Replication.OperationsProcessing.Performance
{
    public sealed class SqlStoreReceiverTelemetryWrapper : IMessageReceiver
    {
        private readonly IMessageReceiver _receiver;
        private readonly ITelemetryPublisher _telemetryPublisher;

        public SqlStoreReceiverTelemetryWrapper(SqlStoreReceiver receiver, ITelemetryPublisher telemetryPublisher)
        {
            _receiver = receiver;
            _telemetryPublisher = telemetryPublisher;
        }

        public IReadOnlyList<IMessage> Peek()
        {
            using (var probe = new Probe("Peek Aggregate Operations"))
            {
                return _receiver.Peek();
            }
        }

        public void Complete(IEnumerable<IMessage> successfullyProcessedMessages, IEnumerable<IMessage> failedProcessedMessages)
        {
            var enqueuedTime = successfullyProcessedMessages
                .Concat(failedProcessedMessages)
                .Cast<PerformedOperationsFinalProcessingMessage>()
                .SelectMany(message => message.FinalProcessings)
                .Select(message => message.CreatedOn)
                .Min();

            _telemetryPublisher.Publish<FinalProcessingDelayIdentity>((long)(DateTime.UtcNow - enqueuedTime).TotalMilliseconds);

            using (var probe = new Probe("Complete Aggregate Operations"))
            {
                _receiver.Complete(successfullyProcessedMessages, failedProcessedMessages);
            }
        }
    }
}