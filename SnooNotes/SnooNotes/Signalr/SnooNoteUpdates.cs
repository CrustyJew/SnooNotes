using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SnooNotes.Signalr
{
    public class SnooNoteUpdates : ISnooNoteUpdates
    {
        private IHubContext<Signalr.SnooNotesHub> hub;
        public SnooNoteUpdates( IHubContext<Signalr.SnooNotesHub> hub, IConfigurationRoot config ) {
            this.hub = hub;
        }


        public Task SendNewNoteAsync(Models.Note anote) {
            return hub.Clients.Group(anote.SubName.ToLower()).SendAsync("addNewNote",anote);
        }

        public Task DeleteNoteAsync(Models.Note anote, bool outOfNotes)
        {
            return hub.Clients.Group(anote.SubName.ToLower()).SendAsync("deleteNote",anote.AppliesToUsername,anote.NoteID, outOfNotes);
        }

        public async Task RefreshNoteTypesAsync(IEnumerable<string> SubNames)
        {
            foreach (string SubName in SubNames)
            {
                await hub.Clients.Group(SubName.ToLower()).SendAsync("refreshNoteTypes");
            }
        }
        public Task SendModActionAsync( Models.ModAction action ) {
            return hub.Clients.Group( action.Subreddit.ToLower() ).SendAsync("modAction",action.ThingID, action.Mod, action.Action );
        }

    }
    
}