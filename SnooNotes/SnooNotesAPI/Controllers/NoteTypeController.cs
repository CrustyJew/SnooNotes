using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SnooNotesAPI.Controllers
{
    [Authorize]
    public class NoteTypeController : ApiController
    {
        private BLL.NoteTypesBLL noteTypeBLL;
        public NoteTypeController() {
            noteTypeBLL = new BLL.NoteTypesBLL();
        }
        // GET: api/NoteType
        public Task<Dictionary<string, IEnumerable<Models.BasicNoteType>>> Get()
        {
            List<string> roles = new List<string>();
            ClaimsIdentity id = (User.Identity as ClaimsIdentity);
            roles = id.Claims.Where(c => c.Type == id.RoleClaimType).Select(c => c.Value).ToList();
            return noteTypeBLL.GetNoteTypesForSubs(roles);
            
        }

        // GET: api/NoteType/5
        public async Task<Models.NoteType> Get(int id)
        {
            Models.NoteType ntype = await noteTypeBLL.GetNoteType(id);
            if (ntype != null && ClaimsPrincipal.Current.IsInRole(ntype.SubName))
            {
                return ntype;
            }
            return null;
        }

        // POST: api/NoteType
        public Task<IEnumerable<Models.NoteType>> Post([FromBody]IEnumerable<Models.NoteType> values)
        {
            foreach ( string subname in values.Select( v => v.SubName ) ) {
                if ( !ClaimsPrincipal.Current.HasClaim( $"urn:snoonotes:subreddits:{subname.ToLower()}:admin", "true" ) )
                    throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
            }
            var ret = noteTypeBLL.AddMultipleNoteTypes(values,User.Identity.Name);
            return ret;
        }

        // PUT: api/NoteType/5
        public Task<IEnumerable<Models.NoteType>> Put([FromBody]Models.NoteType[] values)
        {
            foreach ( string subname in values.Select( v => v.SubName ) ) {
                if ( !ClaimsPrincipal.Current.HasClaim( $"urn:snoonotes:subreddits:{subname.ToLower()}:admin", "true" ) )
                    throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
            }
            return noteTypeBLL.UpdateMultipleNoteTypes(values,User.Identity.Name);
        }

        // DELETE: api/NoteType/5
        public Task<IEnumerable<int>> Delete([FromBody]Models.NoteType[] values)
        {
            foreach ( string subname in values.Select( v => v.SubName ) ) {
                if ( !ClaimsPrincipal.Current.HasClaim( $"urn:snoonotes:subreddits:{subname.ToLower()}:admin", "true" ) )
                    throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
            }
            return noteTypeBLL.DeleteMultipleNoteTypes(values,User.Identity.Name);
        }


       
    }
}
