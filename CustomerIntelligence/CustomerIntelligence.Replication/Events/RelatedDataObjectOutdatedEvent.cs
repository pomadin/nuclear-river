﻿using System;

using NuClear.Replication.Core;

namespace NuClear.CustomerIntelligence.Replication.Events
{
    public class RelatedDataObjectOutdatedEvent<TDataObjectId> : IEvent
    {
        public RelatedDataObjectOutdatedEvent(Type relatedDataObjectType, TDataObjectId relatedDataObjectId)
            : this(relatedDataObjectType, relatedDataObjectId, DateTime.UtcNow)
        {
        }

        public RelatedDataObjectOutdatedEvent(Type relatedDataObjectType, TDataObjectId relatedDataObjectId, DateTime eventHappenedTime)
        {
            RelatedDataObjectType = relatedDataObjectType;
            RelatedDataObjectId = relatedDataObjectId;
            HappenedTime = eventHappenedTime;
        }

        public Type RelatedDataObjectType { get; }
        public TDataObjectId RelatedDataObjectId { get; }
        public DateTime HappenedTime { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((RelatedDataObjectOutdatedEvent<TDataObjectId>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((RelatedDataObjectType?.GetHashCode() ?? 0) * 397) ^ RelatedDataObjectId.GetHashCode();
            }
        }

        private bool Equals(RelatedDataObjectOutdatedEvent<TDataObjectId> other)
        {
            return RelatedDataObjectType == other.RelatedDataObjectType && Equals(RelatedDataObjectId, other.RelatedDataObjectId);
        }
    }
}