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
    public class NoteTypeController : ApiController
    {
        
        // GET: api/NoteType
        public Dictionary<string, IEnumerable<Models.BasicNoteType>> Get()
        {
            List<string> roles = new List<string>();
            ClaimsIdentity id = (User.Identity as ClaimsIdentity);
            roles = id.Claims.Where(c => c.Type == id.RoleClaimType).Select(c => c.Value).ToList();
            var notetypes = Models.NoteType.GetNoteTypesForSubs(roles);
            Dictionary<string, IEnumerable<Models.BasicNoteType>> toReturn = new Dictionary<string, IEnumerable<Models.BasicNoteType>>();
            foreach(string sub in roles){
                var basicNoteTypesForSub = notetypes.Where(t => t.SubName == sub).Select(t => new Models.BasicNoteType(){ Bold = t.Bold, ColorCode = t.ColorCode, DisplayName = t.DisplayName, DisplayOrder= t.DisplayOrder, Italic = t.Italic, NoteTypeID = t.NoteTypeID}).OrderBy(bt => bt.DisplayOrder);
                toReturn.Add(sub,basicNoteTypesForSub);
            }
            return toReturn;
        }

        // GET: api/NoteType/5
        public Models.NoteType Get(int id)
        {
            Models.NoteType ntype = Models.NoteType.GetNoteType(id);
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
                
                Models.NoteType.AddNoteType(value);
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
                Models.NoteType.UpdateNoteType(value);
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
                Models.NoteType.DeleteNoteType(value);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }
        }
    }
}
