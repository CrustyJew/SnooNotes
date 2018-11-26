using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SnooNotes.Controllers {
    [Authorize(AuthenticationSchemes = "token")]
    [Route("api/[controller]")]
    public class NoteController : Controller {
        private BLL.INotesBLL notesBLL;
        private IConfigurationRoot Configuration;
        private Signalr.ISnooNoteUpdates snUpdates;
        public NoteController( IConfigurationRoot config, BLL.INotesBLL notesBLL, Signalr.ISnooNoteUpdates snooNoteUpdates) {
            this.notesBLL = notesBLL;
            Configuration = config;
            snUpdates = snooNoteUpdates;
        }
        
        [HttpGet( "[action]" )]
        public Task<IEnumerable<string>> GetUsernamesWithNotes() {
            ClaimsIdentity ident = User.Identity as ClaimsIdentity;
            var subs = ident.FindAll(ident.RoleClaimType).Select(c => c.Value).ToList();
            if (ident.HasClaim(c => c.Type == "uri:snoonotes:cabal" && c.Value == "true"))
            {
                subs.Add(Configuration["CabalSubreddit"]);
            }
            return notesBLL.GetUsersWithNotes( subs );
        }
        
        [HttpPost( "[action]" )]
        public Task<Dictionary<string, IEnumerable<Models.BasicNote>>> GetNotes( [FromBody]IEnumerable<string> users, [FromQuery]bool ascending = true ) {
            ClaimsIdentity ident = User.Identity as ClaimsIdentity;
            var subs = ident.FindAll(ident.RoleClaimType).Select(c => c.Value).ToList();
            if (ident.HasClaim(c => c.Type == "uri:snoonotes:cabal" && c.Value == "true"))
            {
                subs.Add(Configuration["CabalSubreddit"]);
            }
            return notesBLL.GetNotes( subs, users, ascending );
        }
        
        [HttpGet( "{username}/HasNotes" )]
        public Task<bool> UserHasNotes( string username ) {
            ClaimsIdentity ident = User.Identity as ClaimsIdentity;
            var subs = ident.FindAll(ident.RoleClaimType).Select(c => c.Value).ToList();
            if(ident.HasClaim(c=>c.Type == "uri:snoonotes:cabal" && c.Value == "true"))
            {
                subs.Add(Configuration["CabalSubreddit"]);
            }
            return notesBLL.UserHasNotes( subs , username );
        }
        [HttpPost]
        // POST: api/Note
        public async Task Post( [FromBody]Models.Note value ) {
            if ( User.IsInRole( value.SubName.ToLower() ) ) {
                value.Submitter = User.Identity.Name;
                value.Timestamp = DateTime.UtcNow;
                Models.Note insertedNote = await notesBLL.AddNoteForUser( value );

                await snUpdates.SendNewNoteAsync( insertedNote );
            }
            else {
                throw new UnauthorizedAccessException( "You are not a moderator of that subreddit!" );
            }
        }
        
        [HttpPost( "Cabal" )]
        public async Task AddNoteToCabal( int id, int typeid ) {
            string cabalSub = Configuration["CabalSubreddit"];
            Models.Note note = await notesBLL.GetNoteByID( id );
            if( !User.IsInRole( note.SubName.ToLower() ) ) {
                throw new UnauthorizedAccessException( "That note ID doesn't belong to you. Go on! GIT!" );
            }
            if ( User.IsInRole( cabalSub ) || User.HasClaim(c=>c.Type == "uri:snoonotes:cabal" && c.Value == "true") ) {
                note.Timestamp = DateTime.UtcNow;
                note.Submitter = User.Identity.Name;
                note.NoteTypeID = typeid;
                var insertedNote = await notesBLL.AddNoteToCabal( note, cabalSub );
                await snUpdates.SendNewNoteAsync( insertedNote );
            }
            else {
                throw new UnauthorizedAccessException( "You aren't a part of the cabal! Shoo!" );
            }
        }

        [HttpGet("{subName}/Export")]
        public Task<Models.Export> ExportNotes([FromRoute]string subName) {
            if(!User.IsInRole(subName.ToLower())) {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }
            return notesBLL.ExportNotes(subName);
        }

        [HttpDelete,HttpDelete("[action]")]
        // DELETE: api/Note/5
        public async Task Delete( int id ) {
            Models.Note note = await notesBLL.GetNoteByID( id );
            if ( User.IsInRole( note.SubName.ToLower() ) || ( !string.IsNullOrWhiteSpace( note.ParentSubreddit ) && User.IsInRole( note.ParentSubreddit.ToLower() ) ) ) {
                bool outOfNotes = await notesBLL.DeleteNoteForUser( note, User.Identity.Name );
                await snUpdates.DeleteNoteAsync( note, outOfNotes );
            }
            else {
                throw new UnauthorizedAccessException( "You are not a moderator of that subreddit!" );
            }
        }
    }
}
