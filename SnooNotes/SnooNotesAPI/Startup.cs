using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(SnooNotesAPI.Startup))]

namespace SnooNotesAPI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
            ConfigureAuth(app);
            app.MapSignalR(new Microsoft.AspNet.SignalR.HubConfiguration { EnableJSONP = true, EnableDetailedErrors = true });
        }
    }
}
