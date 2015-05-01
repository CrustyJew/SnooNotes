using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Claims;

namespace SnooNotesAPI.Controllers
{
    [Authorize]
    public class NoteController : ApiController
    {
        Models.NoteMain nm = new Models.NoteMain();

        // POST: api/Note/GetNotes
        [HttpPost]
        public IEnumerable<Models.Note> GetNotes([FromBody]Models.UserNoteRequest req )
        {
            if (User.IsInRole(req.SubName))
            {
                return nm.GetNotesForUsers(req.SubName, req.Users);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }
        }
        [HttpPost]
        public Dictionary<string, IEnumerable<Models.BasicNote>> GetNotesForUsers(IEnumerable<string> usernames)
        {
            ClaimsPrincipal ident = User as ClaimsPrincipal;
            var result = nm.GetNotesForUsers(ident.FindAll((ident.Identity as ClaimsIdentity).RoleClaimType).Select(c => c.Value), usernames);
            Dictionary<string, IEnumerable<Models.BasicNote>> toReturn = new Dictionary<string, IEnumerable<Models.BasicNote>>();
            foreach (string user in usernames)
            {
                var notes = result.Where(u => u.AppliesToUsername == user).Select(n => new Models.BasicNote{Message = n.Message, NoteID = n.NoteID, NoteTypeID = n.NoteTypeID, Submitter = n.Submitter, SubName = n.SubName});
                toReturn.Add(user,notes);
            }
            return toReturn;
        }

        public IEnumerable<string> GetUsernamesWithNotes()
        {
            ClaimsPrincipal ident = User as ClaimsPrincipal;
            return nm.GetUsersWithNotes(ident.FindAll((ident.Identity as ClaimsIdentity).RoleClaimType).Select(c => c.Value));

        }
        public IEnumerable<Models.Note> GetNotesForSub(string sub)
        {
            if (User.IsInRole(sub))
            {
                var x = nm.GetNotesForSub(sub).ToList<Models.Note>();
                for (int i = 0; i < 10000; i++ ){
                    x.Add(x.First());
                }
                return x;
                //return nm.GetNotesForSub(sub);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }
        }

        // POST: api/Note
        public void Post([FromBody]Models.Note value)
        {
            if (User.IsInRole(value.SubName))
            {
                value.Submitter = User.Identity.Name;
                nm.AddNoteForUser(value);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }
        }

        // DELETE: api/Note/5
        public void Delete(Models.Note value)
        {
            if (User.IsInRole(value.SubName))
            {
                nm.DeleteNoteForUser(value);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }
        }
    }
}
