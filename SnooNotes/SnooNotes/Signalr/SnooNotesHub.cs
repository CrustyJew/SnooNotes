using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Security.Claims;

namespace SnooNotes.Signalr
{
    [Authorize]
    [HubName("SnooNoteUpdates")]
    public class SnooNotesHub : Hub
    {
        private readonly SnooNoteUpdates _snUpdates;

        public SnooNotesHub(SnooNoteUpdates snUpdates)
        {
            _snUpdates = snUpdates;
        }

        public override Task OnConnected()
        {
            ClaimsPrincipal ident = Context.User as ClaimsPrincipal;

            foreach (string role in ident.FindAll((ident.Identity as ClaimsIdentity).RoleClaimType).Select(c => c.Value))
            {
                Groups.Add(Context.ConnectionId, role);
            }
            return base.OnConnected();
        }
        
        public string GetDateTime()
        {
            return DateTime.Now.ToString();
        }

    }
}