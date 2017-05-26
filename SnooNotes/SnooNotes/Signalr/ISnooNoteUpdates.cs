using System.Collections.Generic;
using SnooNotes.Models;

namespace SnooNotes.Signalr {
    public interface ISnooNoteUpdates {
        void DeleteNote( Note anote, bool outOfNotes );
        void RefreshNoteTypes( IEnumerable<string> SubNames );
        void SendNewNote( Note anote );
        void SendModAction( Models.ModAction action );
    }
}