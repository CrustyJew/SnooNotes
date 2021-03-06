﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace SnooNotes.Signalr
{
    [Authorize(AuthenticationSchemes = "token")]
    public class SnooNotesHub : Hub
    {
        //private readonly SnooNoteUpdates _snUpdates;
        protected static ConcurrentDictionary<string, string> connIDToUsername = new ConcurrentDictionary<string, string>();
        protected static ConcurrentDictionary<string, List<string>> groupToConnIDs = new ConcurrentDictionary<string, List<string>>();
        private IConfigurationRoot Configuration;
        public SnooNotesHub(IConfigurationRoot config)//SnooNoteUpdates snUpdates)
        {
            //_snUpdates = snUpdates;
            Configuration = config;
        }

        public async override Task OnConnectedAsync()
        {
            ClaimsPrincipal ident = Context.User as ClaimsPrincipal;
            connIDToUsername.AddOrUpdate(Context.ConnectionId, ident.Identity.Name, (key, cur) => { return ident.Identity.Name; });

            foreach (string role in ident.FindAll((ident.Identity as ClaimsIdentity).RoleClaimType).Select(c => c.Value))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, role.ToLower());
                groupToConnIDs.AddOrUpdate(role, new List<string> { Context.ConnectionId }, (key, cur) => { cur.Add(Context.ConnectionId); return cur; });
            }
            string cabalSub = Configuration["CabalSubreddit"];
            if (!string.IsNullOrWhiteSpace(cabalSub) && ident.HasClaim(c => c.Type == "uri:snoonotes:cabal" && c.Value == "true"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, cabalSub.ToLower());
                groupToConnIDs.AddOrUpdate(cabalSub.ToLower(), new List<string> { Context.ConnectionId }, (key, cur) => { cur.Add(Context.ConnectionId); return cur; });
            }
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception ex)
        {
            string username = "";
            connIDToUsername.TryGetValue(Context.ConnectionId, out username);
            if (!string.IsNullOrWhiteSpace(username))
            {
                connIDToUsername.TryRemove(Context.ConnectionId, out var u);
                foreach(var grp in groupToConnIDs.Where(g => g.Value.Contains(Context.ConnectionId)))
                {
                    groupToConnIDs.AddOrUpdate(grp.Key, grp.Value.Where(g => g != Context.ConnectionId).ToList(), (key, cur) => { cur.Remove(Context.ConnectionId); return cur; });
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, grp.Key);
                }
            }
            
            await base.OnDisconnectedAsync(ex);
        }


        public string GetDateTime()
        {
            return DateTime.Now.ToString();
        }

    }
}