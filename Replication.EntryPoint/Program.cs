﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;

using Microsoft.Practices.Unity;

using NuClear.Jobs.Schedulers;
using NuClear.Settings.API;
using NuClear.Tracing.API;
using NuClear.Tracing.Environment;
using NuClear.Tracing.Log4Net;
using NuClear.Tracing.Log4Net.Config;

using Replication.EntryPoint.DI;
using Replication.EntryPoint.Settings;

namespace Replication.EntryPoint
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var isDebuggerMode = IsDebuggerMode(args);
            if (isDebuggerMode && !Debugger.IsAttached)
            {
                Debugger.Launch();
            }

            var settingsContainer = new ReplicationServiceSettings();
            var environmentSettings = settingsContainer.AsSettings<IEnvironmentSettings>();

            var tracerContextEntryProviders =
                    new ITracerContextEntryProvider[] 
                    {
                        new TracerContextConstEntryProvider(TracerContextKeys.Required.Environment, environmentSettings.EnvironmentName),
                        new TracerContextConstEntryProvider(TracerContextKeys.Required.EntryPoint, environmentSettings.EntryPointName),
                        new TracerContextConstEntryProvider(TracerContextKeys.Required.EntryPointHost, NetworkInfo.ComputerFQDN),
                        new TracerContextConstEntryProvider(TracerContextKeys.Required.EntryPointInstanceId, Guid.NewGuid().ToString()),
                        new TracerContextSelfHostedEntryProvider(TracerContextKeys.Required.UserAccount)
                    };

            var tracerContextManager = new TracerContextManager(tracerContextEntryProviders);
            var tracer = Log4NetTracerBuilder.Use
                                             .DefaultXmlConfig
                                             .Console
                                             .EventLog
                                             .Build;

            IUnityContainer container = null;
            try
            {
                container = Bootstrapper.ConfigureUnity(settingsContainer, tracer, tracerContextManager);
                var schedulerManager = container.Resolve<ISchedulerManager>();
                if (IsConsoleMode(args))
                {
                    schedulerManager.Start();

                    Console.WriteLine("ERM сервис запущен.");
                    Console.WriteLine("Нажмите ENTER для останова...");

                    Console.ReadLine();

                    Console.WriteLine("ERM сервис останавливается...");

                    schedulerManager.Stop();

                    Console.WriteLine("ERM сервис остановлен. Нажмите ENTER для выхода...");
                    Console.ReadLine();
                }
                else
                {
                    using (var replicationService = new ReplicationService(schedulerManager))
                    {
                        ServiceBase.Run(replicationService);
                    }
                }
            }
            finally
            {
                if (container != null)
                {
                    container.Dispose();
                }
            }
        }

        private static bool IsDebuggerMode(IEnumerable<string> args)
        {
            return args.Any(x => x.LastIndexOf("debug", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool IsConsoleMode(IEnumerable<string> args)
        {
            return args.Any(x => x.LastIndexOf("console", StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
