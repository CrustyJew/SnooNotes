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
        

        // POST: api/Note/GetNotes
        [HttpPost]
        public IEnumerable<Models.Note> GetNotes([FromBody]Models.UserNoteRequest req )
        {
            if (User.IsInRole(req.SubName))
            {
                return Models.Note.GetNotesForUsers(req.SubName, req.Users);
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
            var result = Models.Note.GetNotesForUsers(ident.FindAll((ident.Identity as ClaimsIdentity).RoleClaimType).Select(c => c.Value), usernames);
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
            return Models.Note.GetUsersWithNotes(ident.FindAll((ident.Identity as ClaimsIdentity).RoleClaimType).Select(c => c.Value));

        }
        [HttpGet]
        public Dictionary<string, IEnumerable<Models.BasicNote>> InitializeNotes()
        {
            var usernames = GetUsernamesWithNotes();
             ClaimsPrincipal ident = User as ClaimsPrincipal;
            var x = Models.Note.GetNotesForSubs(ident.FindAll((ident.Identity as ClaimsIdentity).RoleClaimType).Select(c => c.Value)).ToList();

                for (int i = 0; i < 10000; i++ ){
                    x.Add(x.First());
                }
                Dictionary<string, IEnumerable<Models.BasicNote>> toReturn = new Dictionary<string, IEnumerable<Models.BasicNote>>();
                foreach (string user in usernames)
                {
                    var notes = x.Where(u => u.AppliesToUsername == user).Select(n => new Models.BasicNote { Message = n.Message, NoteID = n.NoteID, NoteTypeID = n.NoteTypeID, Submitter = n.Submitter, SubName = n.SubName });
                    toReturn.Add(user, notes);
                }
                return toReturn;
                //return Models.Note.GetNotesForSub(sub);
           
        }

        // POST: api/Note
        public void Post([FromBody]Models.Note value)
        {
            if (User.IsInRole(value.SubName))
            {
                value.Submitter = User.Identity.Name;
                Models.Note.AddNoteForUser(value);
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
                Models.Note.DeleteNoteForUser(value);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }
        }
    }
}
