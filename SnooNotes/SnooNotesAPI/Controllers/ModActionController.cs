using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SnooNotesAPI.Controllers
{
    [RoutePrefix("api/ModAction")]
    public class ModActionController : ApiController
    {
        [Route("{subreddit}"),HttpPost]
        public void ModAction(string subreddit, Models.ModAction action) {
            action.Subreddit = subreddit;
            if ( User.IsInRole( subreddit.ToLower() ) ) {
                action.Mod = User.Identity.Name;
                Signalr.SnooNoteUpdates.Instance.SendModAction( action );
            }
            else {
                throw new UnauthorizedAccessException( "You are not a moderator of that subreddit!" );
            }
        }
    }
}
