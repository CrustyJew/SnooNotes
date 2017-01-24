using IdentProvider.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SnooNotesAPI.Controllers {
    [Authorize]
    //[WikiRead] ///TODO
    public class ToolBoxNotesController : Controller {

        private UserManager<ApplicationUser> userManager;
        public ToolBoxNotesController(UserManager<ApplicationUser> userManager ) {
            this.userManager = userManager;
        }
        // GET: api/ToolBoxNotes
        public IEnumerable<string> Get()
        {
            return new string[2] { "a", "b" };
        }

        // GET: api/ToolBoxNotes/5
        
        public async Task<IEnumerable<RedditSharp.TBUserNote>> Get(string id)
        {
            if (!ClaimsPrincipal.Current.HasClaim("urn:snoonotes:subreddits:" + id.ToLower() + ":admin", "true"))
            {
                throw new Exception("Not an admin of this subreddit"); //TODO Fix exception type
            }
            var user = await userManager.FindByNameAsync( ClaimsPrincipal.Current.Identity.Name);
            if (user.TokenExpires < DateTime.UtcNow)
            {
                await Utilities.AuthUtils.GetNewTokenAsync(user);
            }
            RedditSharp.WebAgent agent = new RedditSharp.WebAgent();
            agent.AccessToken = user.AccessToken;
            var notes = await RedditSharp.ToolBoxUserNotes.GetUserNotesAsync(agent, id);
            return notes;
        }

        // POST: api/ToolBoxNotes
        [ValidateModel]
        public Task<int> Post([FromBody]RequestObjects.TBImportMapping value)
        {
            if (!(User as ClaimsPrincipal).HasClaim("urn:snoonotes:subreddits:" + value.subName.ToLower() + ":admin", "true"))
            {
                throw new Exception("Not an admin of this subreddit"); //TODO Fix exception type
            }
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = userManager.FindByName(User.Identity.Name);
            if (user.TokenExpires < DateTime.UtcNow)
            {
                Utilities.AuthUtils.GetNewToken(user);

                userManager.Update(user);
            }


            Utilities.SNWebAgent agent = new Utilities.SNWebAgent(user.AccessToken);

            var notes = RedditSharp.ToolBoxUserNotes.GetUserNotes(agent, value.subName);
            List<Models.Note> convertedNotes = Utilities.TBNoteUtils.ConvertTBNotesToSnooNotes(value.subName, value.GetNoteTypeMapping(), notes.ToList());

            return new DAL.NotesDAL().AddNewToolBoxNotes(convertedNotes);
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
