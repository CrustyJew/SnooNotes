using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.DAL {
    public interface INoteTypesDAL {
        Task<IEnumerable<NoteType>> AddMultipleNoteTypes( IEnumerable<NoteType> ntypes, string uname );
        Task<bool> DeleteMultipleNoteTypes( IEnumerable<NoteType> ntypes, string uname );
        Task<NoteType> GetNoteType( int id );
        Task<IEnumerable<NoteType>> GetNoteTypesForSubs( IEnumerable<string> subredditNames );
        Task UpdateMultipleNoteTypes( NoteType[] ntypes, string uname );
        Task<bool> ValidateNoteTypesInSubs( IEnumerable<NoteType> ntypes );
    }
}