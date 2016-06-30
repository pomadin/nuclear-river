﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using LinqToDB;
using LinqToDB.Data;

using NuClear.Replication.Core;
using NuClear.Replication.Core.Actors;
using NuClear.Replication.Core.DataObjects;
using NuClear.StateInitialization.Core.Commands;

namespace NuClear.StateInitialization.Core.Actors
{
    public sealed class ReplaceDataObjectsInBulkActor<TDataObject> : IActor
        where TDataObject : class
    {
        private readonly IQueryable<TDataObject> _dataObjectsSource;
        private readonly DataConnection _targetDataConnection;

        public ReplaceDataObjectsInBulkActor(IStorageBasedDataObjectAccessor<TDataObject> dataObjectAccessor, DataConnection targetDataConnection)
        {
            _dataObjectsSource = dataObjectAccessor.GetSource();
            _targetDataConnection = targetDataConnection;
        }

        public IReadOnlyCollection<IEvent> ExecuteCommands(IReadOnlyCollection<ICommand> commands)
        {
            var command = commands.OfType<ReplaceDataObjectsInBulkCommand>().SingleOrDefault();
            if (command == null)
            {
                return Array.Empty<IEvent>();
            }

            try
            {
                var options = new BulkCopyOptions { BulkCopyTimeout = command.BulkCopyTimeout };
                _targetDataConnection.GetTable<TDataObject>().Delete();
                _targetDataConnection.BulkCopy(options, _dataObjectsSource);

                return Array.Empty<IEvent>();
            }
            catch (Exception ex)
            {
                throw new DataException($"Error occured while bulk replacing data for dataobject of type {typeof(TDataObject).Name}{Environment.NewLine}{_targetDataConnection.LastQuery}", ex);
            }
        }
    }
}