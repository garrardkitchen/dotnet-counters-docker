using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace monitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(x=>x.AddConsole() )
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    [EventSource(Name = "OpenMessage")]
    internal class OpenMessageEventSource : EventSource
    {
        internal static readonly OpenMessageEventSource Instance = new OpenMessageEventSource();

        private long _rootRequest = 0;
        private long _delayRequest = 0;
        private IncrementingPollingCounter _inflightMessagesCounter;
        private EventCounter _messageDurationCounter;
        private IncrementingPollingCounter _processedCountCounter;
        private IncrementingEventCounter _rootHit;
        private IncrementingEventCounter _delayHit;


        private OpenMessageEventSource() { }

        public class Keywords
        {
            public const EventKeywords Page = (EventKeywords)1;
            public const EventKeywords DataBase = (EventKeywords)2;
            public const EventKeywords Diagnostic = (EventKeywords)4;
            public const EventKeywords Perf = (EventKeywords)8;
        }

        [Event(1, Message = "Root", Keywords = Keywords.Perf, Level = EventLevel.Informational)]
        public void Root()
        {
            WriteEvent(1);

            Interlocked.Increment(ref _rootRequest);
            Instance._rootHit.Increment(1);
        }

        [Event(2, Message = "Delay", Keywords = Keywords.Perf, Level = EventLevel.Informational)]
        public void Delay()
        {
            WriteEvent(2);

            Interlocked.Increment(ref _delayRequest);
            Instance._delayHit.Increment(1);
        }

        protected override void OnEventCommand(EventCommandEventArgs command)
        {
            if (command.Command == EventCommand.Enable)
            {
                _inflightMessagesCounter ??= new IncrementingPollingCounter("root-request", this, () => _rootRequest)
                {
                    DisplayName = "/",
                    DisplayUnits = "requests",
                    DisplayRateTimeScale = TimeSpan.FromSeconds(1)
                };
                _processedCountCounter ??= new IncrementingPollingCounter("delay-request", this, () => _delayRequest)
                {
                    DisplayName = "/delay",
                    DisplayUnits = "requests",
                    DisplayRateTimeScale = TimeSpan.FromSeconds(1)
                };
                _rootHit ??= new IncrementingEventCounter("root-hit", this)
                {
                    DisplayName = "root total",
                    DisplayUnits = "requests",
                    DisplayRateTimeScale = TimeSpan.FromDays(1)
                };
                _delayHit ??= new IncrementingEventCounter("delay-hit", this)
                {
                    DisplayName = "delay total",
                    DisplayUnits = "requests",
                    DisplayRateTimeScale = TimeSpan.FromDays(1)
                };
            }
        }
    }
}
