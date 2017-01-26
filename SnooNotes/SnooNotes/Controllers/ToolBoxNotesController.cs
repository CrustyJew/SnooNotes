using IdentProvider.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    public class ToolBoxNotesController : Controller {

        private UserManager<ApplicationUser> userManager;
        private DAL.NotesDAL notesDAL;
        private Utilities.AuthUtils authUtils;
        public ToolBoxNotesController(UserManager<ApplicationUser> userManager, IConfigurationRoot config, ILoggerFactory loggerFactory, IMemoryCache memCache ) {
            this.userManager = userManager;
            notesDAL = new DAL.NotesDAL( config );
            authUtils = new Utilities.AuthUtils( config, userManager, loggerFactory, memCache );
        }
        // GET: api/ToolBoxNotes
        public IEnumerable<string> Get()
        {
            return new string[2] { "a", "b" };
        }

        // GET: api/ToolBoxNotes/5
        
        public async Task<IEnumerable<RedditSharp.TBUserNote>> Get(string id)
        {
            if (!User.HasClaim("urn:snoonotes:subreddits:" + id.ToLower() + ":admin", "true"))
            {
                throw new Exception("Not an admin of this subreddit"); //TODO Fix exception type
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

        // POST: api/ToolBoxNotes
        public async Task<int> Post([FromBody]Models.RequestObjects.TBImportMapping value)
        {
            if (!(User as ClaimsPrincipal).HasClaim("urn:snoonotes:subreddits:" + value.subName.ToLower() + ":admin", "true"))
            {
                throw new Exception("Not an admin of this subreddit"); //TODO Fix exception type
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

        // PUT: api/ToolBoxNotes/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ToolBoxNotes/5
        public void Delete(int id)
        {
        }
    }
}
