using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnooNotes.Controllers {
    [Authorize]
    [Route("api/ModAction")]
    public class ModActionController : Controller
    {
        private Signalr.ISnooNoteUpdates snooNoteUpdates;
        private BLL.IModActionBLL modActBLL;
        public ModActionController( Signalr.ISnooNoteUpdates snooNoteUpdates, BLL.IModActionBLL modActionBLL ) {
            this.snooNoteUpdates = snooNoteUpdates;
            modActBLL = modActionBLL;
        }

        [HttpPost("{subreddit}")]
        public void ModAction([FromRoute]string subreddit, [FromBody]Models.ModAction act) {
            act.Subreddit = subreddit;
            if ( User.IsInRole( subreddit.ToLower() ) ) {
                act.Mod = User.Identity.Name;
                snooNoteUpdates.SendModAction( act );
            }
            else {
                throw new UnauthorizedAccessException( "You are not a moderator of that subreddit!" );
            }
        }

        [HttpGet("{subreddit}/thing/{thingid}")]
        public Task<IEnumerable<Models.SentinelModLogEntry>> GetModActionHistory([FromRoute]string subreddit, [FromRoute]string thingid ) {
            if (!User.IsInRole(subreddit.ToLower())) {
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }

            return modActBLL.GetModLogEntriesForThing(thingid, subreddit);
        }
    }
}
