using IdentProvider.Models;
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

namespace SnooNotes.Controllers {
    [Authorize]
    //[WikiRead] ///TODO

    [Route( "api/[controller]" )]
    public class ToolBoxNotesController : Controller {

        private UserManager<ApplicationUser> userManager;
        private DAL.INotesDAL notesDAL;
        private Utilities.IAuthUtils authUtils;
        public ToolBoxNotesController(UserManager<ApplicationUser> userManager, DAL.INotesDAL notesDAL, Utilities.IAuthUtils authUtils ) {
            this.userManager = userManager;
            this.notesDAL = notesDAL;
            this.authUtils = authUtils;
        }
        [HttpGet]
        // GET: api/ToolBoxNotes
        public IEnumerable<string> Get()
        {
            return new string[2] { "a", "b" };
        }

        // GET: api/ToolBoxNotes/5
        [HttpGet("{id}")]
        public async Task<IEnumerable<RedditSharp.TBUserNote>> Get(string id)
        {
            if ( !User.HasClaim( "urn:snoonotes:admin", id.ToLower() ) ) {
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
            }
            var user = await userManager.FindByNameAsync( User.Identity.Name);
            if (user.TokenExpires < DateTime.UtcNow)
            {
                await authUtils.GetNewTokenAsync(user);
            }
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent();
            agent.AccessToken = user.AccessToken;
            var notes = await RedditSharp.ToolBoxUserNotes.GetUserNotesAsync(agent, id);
            return notes;
        }
        [HttpPost]
        // POST: api/ToolBoxNotes
        public async Task<int> Post([FromBody]Models.RequestObjects.TBImportMapping value)
        {
            if ( !User.HasClaim( "urn:snoonotes:admin", value.subName.ToLower() ) ) {
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
            }
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            if (user.TokenExpires < DateTime.UtcNow)
            {
                await authUtils.GetNewTokenAsync(user);

                await userManager.UpdateAsync(user);
            }


            RedditSharp.WebAgent agent = new RedditSharp.WebAgent(user.AccessToken);

            var notes = await RedditSharp.ToolBoxUserNotes.GetUserNotesAsync(agent, value.subName);
            List<Models.Note> convertedNotes = Utilities.TBNoteUtils.ConvertTBNotesToSnooNotes(value.subName, value.GetNoteTypeMapping(), notes.ToList());

            return await notesDAL.AddNewToolBoxNotesAsync(convertedNotes);
        }
    }
}
