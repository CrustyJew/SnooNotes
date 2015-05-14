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
        

        [HttpGet]
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

                Dictionary<string, IEnumerable<Models.BasicNote>> toReturn = new Dictionary<string, IEnumerable<Models.BasicNote>>();
                foreach (string user in usernames)
                {
                    var notes = x.Where(u => u.AppliesToUsername == user).Select(n => new Models.BasicNote { Message = n.Message, NoteID = n.NoteID, NoteTypeID = n.NoteTypeID, Submitter = n.Submitter, SubName = n.SubName, Url=n.Url, Timestamp=n.Timestamp });
                    toReturn.Add(user, notes);
                }
                return toReturn;
                //return Models.Note.GetNotesForSub(sub);
           
        }

        // POST: api/Note
        public void Post([FromBody]Models.Note value)
        {
            if (User.IsInRole(value.SubName.ToLower()))
            {
                value.Submitter = User.Identity.Name;
                value.Timestamp = DateTime.UtcNow;
                Models.Note.AddNoteForUser(value);
                Signalr.SnooNoteUpdates.Instance.SendNewNote(value);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }
        }

        // DELETE: api/Note/5
        public void Delete(Models.Note value)
        {
            if (User.IsInRole(value.SubName.ToLower()))
            {
                Models.Note.DeleteNoteForUser(value);
                Signalr.SnooNoteUpdates.Instance.DeleteNote(value);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }
        }
    }
}
