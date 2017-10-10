using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.DAL {
    public interface INotesDAL {
        Task<int> AddNewToolBoxNotesAsync( List<Note> tbNotes );
        Task<Note> AddNoteForUser( Note anote );
        Task<Note> AddNoteToCabal( Note anote, string cabalSub );
        Task<bool> DeleteNoteForUser( Note anote, string uname );
        Task<Note> GetNoteByID( int id );
        Task<IEnumerable<Note>> GetNotes( string subname, IEnumerable<string> usernames, bool ascending = true);
        Task<IEnumerable<Note>> GetNotes( IEnumerable<string> subnames, IEnumerable<string> usernames, bool ascending = true );
        Task<IEnumerable<Note>> ExportNotes( IEnumerable<string> subnames );
        Task<IEnumerable<string>> GetUsersWithNotes( IEnumerable<string> subnames );
        Task<bool> UserHasNotes( IEnumerable<string> subnames, string username );
    }
}