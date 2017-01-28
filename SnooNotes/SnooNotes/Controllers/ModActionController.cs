using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace SnooNotes.Controllers {
    [Authorize]
    [Route("api/ModAction")]
    public class ModActionController : Controller
    {
        private Signalr.ISnooNoteUpdates snooNoteUpdates;
        public ModActionController( Signalr.ISnooNoteUpdates snooNoteUpdates ) {
            this.snooNoteUpdates = snooNoteUpdates;
        }

        [HttpPost("{subreddit}")]
        public void ModAction([FromRoute]string subreddit, [FromForm]Models.ModAction act) {
            act.Subreddit = subreddit;
            if ( User.IsInRole( subreddit.ToLower() ) ) {
                act.Mod = User.Identity.Name;
                snooNoteUpdates.SendModAction( act );
            }
            else {
                throw new UnauthorizedAccessException( "You are not a moderator of that subreddit!" );
            }
        }
    }
}
