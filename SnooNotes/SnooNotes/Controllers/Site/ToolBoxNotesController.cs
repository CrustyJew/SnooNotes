﻿using SnooNotes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hangfire;

namespace SnooNotes.Controllers.Site {
    [Authorize(AuthenticationSchemes = "Snookie,token")]
    //[wikiedit] ///TODO

    [Route( "site/[controller]" )]
    public class ToolBoxNotesController : Controller {
        private UserManager<ApplicationUser> userManager;
        private DAL.INoteTypesDAL noteTypesDAL;
        private Utilities.IAuthUtils authUtils;
        private RedditSharp.RefreshTokenWebAgentPool agentPool;
        private BLL.IToolBoxNotesBLL tbNotesBLL;
        public ToolBoxNotesController(UserManager<ApplicationUser> userManager, DAL.INoteTypesDAL noteTypesDAL, Utilities.IAuthUtils authUtils, RedditSharp.RefreshTokenWebAgentPool agentPool , BLL.IToolBoxNotesBLL tbNotesBLL) {
            this.userManager = userManager;
            this.authUtils = authUtils;
            this.agentPool = agentPool;
            this.noteTypesDAL = noteTypesDAL;
            this.tbNotesBLL = tbNotesBLL;
        }

        // GET: api/ToolBoxNotes/5
        [HttpGet("{subName}")]
        public async Task<string[]> Get(string subName)
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            var claims = await userManager.GetClaimsAsync(user);
            if (claims.Count(c => c.Type == "uri:snoonotes:admin" && c.Value.ToLower() == subName.ToLower()) == 0) {
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
            }
            var agent = await agentPool.GetOrCreateWebAgentAsync(User.Identity.Name, async (uname, uagent, rlimit) =>
            {
                return new RedditSharp.RefreshTokenPoolEntry(uname, user.RefreshToken, rlimit, uagent);
            });

            var warningTypes = await RedditSharp.ToolBoxUserNotes.GetWarningKeys(agent, subName);
            return warningTypes.Select(s=>s ?? "null").ToArray();
        }
        [HttpPost("{subName}")]
        // POST: api/ToolBoxNotes
        public async Task<int> Post([FromRoute]string subName,[FromBody]Dictionary<string,int> mapping)
        {
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            var claims = await userManager.GetClaimsAsync(user);
            if (claims.Count(c => c.Type == "uri:snoonotes:admin" && c.Value.ToLower() == subName.ToLower()) == 0) {
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
            }


            var agent = await agentPool.GetOrCreateWebAgentAsync(User.Identity.Name, async (uname, uagent, rlimit) =>
            {
                return new RedditSharp.RefreshTokenPoolEntry(uname, user.RefreshToken, rlimit, uagent);
            });

            var notes = await RedditSharp.ToolBoxUserNotes.GetUserNotesAsync(agent, subName);
            List<Models.Note> convertedNotes = Utilities.TBNoteUtils.ConvertTBNotesToSnooNotes(subName, mapping, notes.ToList());
            var noteTypes = (await noteTypesDAL.GetNoteTypesForSubs(new string[] { subName })).Select(n => n.NoteTypeID) ;
            if(mapping.Select(m=>m.Value).Distinct().Count(m=> !noteTypes.Contains(m)) > 0) {
                throw new ArgumentException("NoteType does not belong to subreddit");
            }
            BackgroundJob.Enqueue(() => tbNotesBLL.ImportToolboxNotes(convertedNotes, subName.ToLower()));
            //return await notesDAL.AddNewToolBoxNotesAsync(convertedNotes);
            return -1;
        }
        [HttpGet("{subName}/status")]
        public async Task<object> GetStatus([FromRoute]string subName) {
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            var claims = await userManager.GetClaimsAsync(user);
            if (claims.Count(c => c.Type == "uri:snoonotes:admin" && c.Value.ToLower() == subName.ToLower()) == 0) {
                throw new UnauthorizedAccessException("You are not an admin of this subreddit!");
            }
            string status = null;

            Helpers.ImportStatusHelper.ImportStatuses.TryGetValue(subName.ToLower(), out status);
            if(status == null) {
                return new { done = true};
            }
            if(status.StartsWith("Done!")) {
                Helpers.ImportStatusHelper.ImportStatuses.Remove(subName.ToLower());
                return new{ done = true, msg = status};
            }
            else {
                return new { done = false, msg = status };
            }
        }
    }
}
