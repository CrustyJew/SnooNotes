using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SnooNotes.Controllers
{
    [Authorize]
    [Route("restapi/[controller]")]
    public class NoteTypeController : Controller
    {
        private BLL.INoteTypesBLL noteTypeBLL;
        public NoteTypeController(BLL.INoteTypesBLL noteTypesBLL) {
            noteTypeBLL = noteTypesBLL;
        }
        [HttpGet]
        // GET: api/NoteType
        public Task<Dictionary<string, IEnumerable<Models.BasicNoteType>>> Get()
        {
            List<string> roles = new List<string>();
            ClaimsIdentity id = (User.Identity as ClaimsIdentity);
            roles = id.Claims.Where(c => c.Type == id.RoleClaimType).Select(c => c.Value).ToList();
            return noteTypeBLL.GetNoteTypesForSubs(roles);
            
        }
        [HttpGet("{id:int}")]
        // GET: api/NoteType/5
        public async Task<Models.NoteType> Get(int id)
        {
            Models.NoteType ntype = await noteTypeBLL.GetNoteType(id);
            if (ntype != null && User.IsInRole(ntype.SubName))
            {
                return ntype;
            }
            return null;
        }
        [HttpPost]
        // POST: api/NoteType
        public Task<IEnumerable<Models.NoteType>> Post([FromBody]IEnumerable<Models.NoteType> values)
        {
            foreach ( string subname in values.Select( v => v.SubName ) ) {
                if ( !User.HasClaim( "urn:snoonotes:admin", subname.ToLower() ) ) {
                    throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
                }
            }
            var ret = noteTypeBLL.AddMultipleNoteTypes(values,User.Identity.Name,User);
            return ret;
        }
        [HttpPut]
        // PUT: api/NoteType/5
        public Task<IEnumerable<Models.NoteType>> Put([FromBody]Models.NoteType[] values)
        {
            foreach ( string subname in values.Select( v => v.SubName ) ) {
                if ( !User.HasClaim( "urn:snoonotes:admin", subname.ToLower() ) ) {
                    throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
                }
            }
            return noteTypeBLL.UpdateMultipleNoteTypes(values,User.Identity.Name, User);
        }
        [HttpDelete]
        // DELETE: api/NoteType/5
        public Task<IEnumerable<int>> Delete([FromBody]Models.NoteType[] values)
        {
            foreach ( string subname in values.Select( v => v.SubName ) ) {
                if ( !User.HasClaim( "urn:snoonotes:admin", subname.ToLower() ) ) {
                    throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
                }
            }
            return noteTypeBLL.DeleteMultipleNoteTypes(values,User.Identity.Name, User);
        }


       
    }
}
