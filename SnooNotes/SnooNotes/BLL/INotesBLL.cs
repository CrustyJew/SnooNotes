using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.BLL {
    public interface INotesBLL {
        Task<Note> AddNoteForUser( Note value );
        Task<Note> AddNoteToCabal( Note value, string cabalSub );
        Task<bool> DeleteNoteForUser( Note note, string name );
        Task<Note> GetNoteByID( int id );
        Task<Dictionary<string, IEnumerable<BasicNote>>> GetNotesForSubs( IEnumerable<string> subnames );
        Task<Dictionary<string, IEnumerable<BasicNote>>> GetNotesForSubs( IEnumerable<string> subnames, IEnumerable<string> users );
        Task<IEnumerable<string>> GetUsersWithNotes( IEnumerable<string> subnames );
        Task<bool> UserHasNotes( IEnumerable<string> subnames, string username );
    }
}