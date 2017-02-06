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
        Task<IEnumerable<Note>> GetNotesForSubs( IEnumerable<string> subnames );
        Task<IEnumerable<Note>> GetNotesForSubs( IEnumerable<string> subnames, IEnumerable<string> users );
        Task<IEnumerable<Note>> GetNotesForUsers( string subname, IEnumerable<string> usernames );
        Task<IEnumerable<Note>> GetNotesForUsers( IEnumerable<string> subnames, IEnumerable<string> usernames );
        Task<IEnumerable<string>> GetUsersWithNotes( IEnumerable<string> subnames );
        Task<bool> UserHasNotes( IEnumerable<string> subnames, string username );
    }
}