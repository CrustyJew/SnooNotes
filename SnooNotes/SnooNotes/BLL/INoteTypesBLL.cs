using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.BLL {
    public interface INoteTypesBLL {
        Task<IEnumerable<NoteType>> AddMultipleNoteTypes( IEnumerable<NoteType> values, string name, ClaimsPrincipal user );
        Task<IEnumerable<int>> DeleteMultipleNoteTypes( NoteType[] values, string name, ClaimsPrincipal user );
        Task<NoteType> GetNoteType( int id );
        Task<Dictionary<string, IEnumerable<BasicNoteType>>> GetNoteTypesForSubs( IEnumerable<string> subs );
        Task<IEnumerable<NoteType>> UpdateMultipleNoteTypes( NoteType[] values, string name, ClaimsPrincipal user );
    }
}