using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SnooNotesAPI.Controllers {
    [Authorize]
    public class NoteController : ApiController {
        private BLL.NotesBLL notesBLL;
        public NoteController() {
            notesBLL = new BLL.NotesBLL();
        }

        [HttpGet]
        public Task<IEnumerable<string>> GetUsernamesWithNotes() {
            ClaimsPrincipal ident = ClaimsPrincipal.Current;
            return notesBLL.GetUsersWithNotes( ident.FindAll( ( ident.Identity as ClaimsIdentity ).RoleClaimType ).Select( c => c.Value ) );

        }
        [HttpGet]
        public Task<Dictionary<string, IEnumerable<Models.BasicNote>>> InitializeNotes() {
            ClaimsPrincipal ident = User as ClaimsPrincipal;
            return notesBLL.GetNotesForSubs( ident.FindAll( ( ident.Identity as ClaimsIdentity ).RoleClaimType ).Select( c => c.Value ) );
        }

        [HttpPost]
        [Route( "api/Note/GetNotes" )]
        public Task<Dictionary<string, IEnumerable<Models.BasicNote>>> GetNotes( [FromBody]IEnumerable<string> users ) {
            ClaimsIdentity ident = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            return notesBLL.GetNotesForSubs( ident.FindAll( ident.RoleClaimType ).Select( c => c.Value ), users );
        }

        [HttpGet]
        [Route( "api/Note/{username}/HasNotes" )]
        public Task<bool> UserHasNotes( string username ) {
            ClaimsIdentity ident = ClaimsPrincipal.Current.Identity as ClaimsIdentity;
            return notesBLL.UserHasNotes( ident.FindAll( ident.RoleClaimType ).Select( c => c.Value ), username );
        }
        // POST: api/Note
        public async Task Post( [FromBody]Models.Note value ) {
            if ( User.IsInRole( value.SubName.ToLower() ) ) {
                value.Submitter = User.Identity.Name;
                value.Timestamp = DateTime.UtcNow;
                Models.Note insertedNote = await notesBLL.AddNoteForUser( value );

                Signalr.SnooNoteUpdates.Instance.SendNewNote( insertedNote );
            }
            else {
                throw new UnauthorizedAccessException( "You are not a moderator of that subreddit!" );
            }
        }

        [HttpPost]
        [Route( "api/Note/Cabal" )]
        public async Task AddNoteToCabal( Models.Note value ) {
            string cabalSub = System.Configuration.ConfigurationManager.AppSettings["CabalSubreddit"].ToLower();
            if ( User.IsInRole( cabalSub ) ) {
                value.Timestamp = DateTime.UtcNow;
                value.Submitter = ClaimsPrincipal.Current.Identity.Name;
                var insertedNote = await notesBLL.AddNoteToCabal( value, cabalSub );
                Signalr.SnooNoteUpdates.Instance.SendNewNote( insertedNote );
            }
            else {
                throw new UnauthorizedAccessException( "You aren't a part of the cabal! Shoo!" );
            }
        }

        // DELETE: api/Note/5
        public async Task Delete( int id ) {
            Models.Note note = await notesBLL.GetNoteByID( id );
            if ( User.IsInRole( note.SubName.ToLower() ) || ( !string.IsNullOrWhiteSpace( note.ParentSubreddit ) && User.IsInRole( note.ParentSubreddit.ToLower() ) ) ) {
                bool outOfNotes = await notesBLL.DeleteNoteForUser( note, User.Identity.Name );
                Signalr.SnooNoteUpdates.Instance.DeleteNote( note, outOfNotes );
            }
            else {
                throw new UnauthorizedAccessException( "You are not a moderator of that subreddit!" );
            }
        }
    }
}
