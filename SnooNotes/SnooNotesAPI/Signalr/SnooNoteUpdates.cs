using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR;

namespace SnooNotesAPI.Signalr
{
    public class SnooNoteUpdates
    {
        private readonly static Lazy<SnooNoteUpdates> _instance = new Lazy<SnooNoteUpdates>(
            () => new SnooNoteUpdates(GlobalHost.ConnectionManager.GetHubContext<SnooNotesHub>().Clients));
    
        public static SnooNoteUpdates Instance{
            get{
                return _instance.Value;
            }
        }

         private IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

         private SnooNoteUpdates(IHubConnectionContext<dynamic> clients)
         {
             Clients = clients;

         }
    
    }
    
}