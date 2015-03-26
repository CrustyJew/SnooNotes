using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Claims;
namespace SnooNotesAPI.Controllers
{
    public class NoteTypeController : ApiController
    {
        Models.NoteTypeMain ntm = new Models.NoteTypeMain();
        // GET: api/NoteType
        public IEnumerable<Models.NoteType> Get()
        {
            List<string> roles = new List<string>();
            ClaimsIdentity id = (User.Identity as ClaimsIdentity);
            roles = id.Claims.Where(c => c.Type == id.RoleClaimType).Select(c => c.Value).ToList();
            return ntm.GetNoteTypesForSubs(roles);
        }

        // GET: api/NoteType/5
        public Models.NoteType Get(int id)
        {
            Models.NoteType ntype = ntm.GetNoteType(id);
            if (ntype != null && System.Threading.Thread.CurrentPrincipal.IsInRole(ntype.SubName))
            {
                return ntype;
            }
            return null;
        }

        // POST: api/NoteType
        public void Post([FromBody]Models.NoteType value)
        {
            if (User.IsInRole(value.SubName))
            {
                
                ntm.AddNoteType(value);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }
        }

        // PUT: api/NoteType/5
        public void Put([FromBody]Models.NoteType value)
        {
            if (User.IsInRole(value.SubName))
            {
                ntm.UpdateNoteType(value);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }
        }

        // DELETE: api/NoteType/5
        public void Delete([FromBody]Models.NoteType value)
        {
            if (User.IsInRole(value.SubName))
            {
                ntm.DeleteNoteType(value);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }
        }
    }
}
