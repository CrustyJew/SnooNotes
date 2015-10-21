using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
namespace SnooNotesAPI.Controllers
{
    [Authorize]
    [WikiRead]
    public class ToolBoxNotesController : ApiController
    {
        // GET: api/ToolBoxNotes
        public IEnumerable<string> Get()
        {
            return new string[2] { "a", "b" };
        }

        // GET: api/ToolBoxNotes/5
        
        public IEnumerable<RedditSharp.TBUserNote> Get(string id)
        {
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            if (!(User as ClaimsPrincipal).HasClaim("urn:snoonotes:subreddits:" + id.ToLower() + ":admin", "true"))
            {
                throw new Exception("Not an admin of this subreddit"); //TODO Fix exception type
            }
            var user = userManager.FindByName(User.Identity.Name);
            if (user.TokenExpires < DateTime.UtcNow)
            {
                Utilities.AuthUtils.GetNewToken(user);
            }
            Utilities.SNWebAgent agent = new Utilities.SNWebAgent(user.AccessToken);
            var notes = RedditSharp.ToolBoxUserNotes.GetUserNotes(agent, id);
            return notes;
        }

        // POST: api/ToolBoxNotes
        [ValidateModel]
        public int Post([FromBody]RequestObjects.TBImportMapping value)
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

            return Models.Note.AddNewToolBoxNotes(convertedNotes);
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
