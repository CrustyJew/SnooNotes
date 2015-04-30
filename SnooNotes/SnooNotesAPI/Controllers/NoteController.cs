using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SnooNotesAPI.Controllers
{
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
