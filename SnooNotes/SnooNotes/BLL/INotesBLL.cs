using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.BLL {
    public interface INotesBLL {
        Task<Note> AddNoteForUser( Note value );
        Task<Note> AddNoteToCabal( Note value, string cabalSub );
        Task<bool> DeleteNoteForUser( Note note, string name );
        Task<Note> GetNoteByID( int id );
        Task<Dictionary<string, IEnumerable<BasicNote>>> GetNotes( IEnumerable<string> subnames, IEnumerable<string> users, bool ascending = true );
        Task<IEnumerable<string>> GetUsersWithNotes( IEnumerable<string> subnames );
        Task<bool> UserHasNotes( IEnumerable<string> subnames, string username );
        Task<Export> ExportNotes(string subname);
    }
}