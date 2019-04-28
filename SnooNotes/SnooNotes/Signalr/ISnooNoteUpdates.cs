using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.Signalr {
    public interface ISnooNoteUpdates {
        Task DeleteNoteAsync( Note anote, bool outOfNotes );
        Task RefreshNoteTypesAsync( IEnumerable<string> SubNames );
        Task SendNewNoteAsync( Note anote );
        Task SendModActionAsync( Models.ModAction action );
    }
}