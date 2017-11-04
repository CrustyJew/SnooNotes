using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.DataContracts;

namespace SnooNotes
{
    public class TelemetryFilter : ITelemetryProcessor {
        private ITelemetryProcessor Next { get; set; }

        // You can pass values from .config
        public string MyParamFromConfigFile { get; set; }

        // Link processors to each other in a chain.
        public TelemetryFilter( ITelemetryProcessor next ) {
            this.Next = next;
        }

        public void Process( ITelemetry item ) {
            var req = item as RequestTelemetry;
            if(req != null && req.Url.AbsoluteUri.Contains("signalr")) {
                return;
            }
            else {
                Next.Process(item);
            }
        }
    }
}
