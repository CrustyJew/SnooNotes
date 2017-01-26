using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SnooNotes.Models;
using Microsoft.Extensions.Configuration;

namespace SnooNotes.BLL {
    public class NotesBLL {
        private DAL.NotesDAL notesDAL;
        public NotesBLL(IConfigurationRoot config) {
            notesDAL = new DAL.NotesDAL(config);
        }

        public Task<IEnumerable<string>> GetUsersWithNotes( IEnumerable<string> subnames ) {
            return notesDAL.GetUsersWithNotes( subnames );
        }

        public async Task<Dictionary<string, IEnumerable<Models.BasicNote>>> GetNotesForSubs( IEnumerable<string> subnames ) {

            var notes = (await notesDAL.GetNotesForSubs( subnames )).ToList();
            Dictionary<string, IEnumerable<Models.BasicNote>> toReturn = new Dictionary<string, IEnumerable<Models.BasicNote>>();
            foreach ( string user in notes.Select(n=>n.AppliesToUsername).Distinct() ) {
                var unotes = notes.Where( u => u.AppliesToUsername == user ).Select( n => new Models.BasicNote { Message = n.Message, NoteID = n.NoteID, NoteTypeID = n.NoteTypeID, Submitter = n.Submitter, SubName = n.SubName, Url = n.Url, Timestamp = n.Timestamp } );
                toReturn.Add( user, unotes );
            }
            return toReturn;
        }

        public async Task<Dictionary<string, IEnumerable<Models.BasicNote>>> GetNotesForSubs( IEnumerable<string> subnames, IEnumerable<string> users ) {

            var notes = ( await notesDAL.GetNotesForSubs( subnames, users ) ).ToList();
            Dictionary<string, IEnumerable<Models.BasicNote>> toReturn = new Dictionary<string, IEnumerable<Models.BasicNote>>();
            foreach ( string user in notes.Select( n => n.AppliesToUsername ).Distinct() ) {
                var unotes = notes.Where( u => u.AppliesToUsername == user ).Select( n => new Models.BasicNote { Message = n.Message, NoteID = n.NoteID, NoteTypeID = n.NoteTypeID, Submitter = n.Submitter, SubName = n.SubName, Url = n.Url, Timestamp = n.Timestamp, ParentSubreddit = n.ParentSubreddit } );
                toReturn.Add( user, unotes );
            }
            return toReturn;
        }

        public Task<bool> UserHasNotes(IEnumerable<string> subnames, string username ) {
            return notesDAL.UserHasNotes( subnames, username );
        }

        public Task<Note> AddNoteForUser( Note value ) {
            return notesDAL.AddNoteForUser( value );
        }

        public Task<Note> AddNoteToCabal(Note value, string cabalSub ) {
            return notesDAL.AddNoteToCabal( value, cabalSub );
        }

        public Task<Note> GetNoteByID( int id ) {
            return notesDAL.GetNoteByID( id );
        }

        public Task<bool> DeleteNoteForUser( Note note, string name ) {
            return notesDAL.DeleteNoteForUser( note, name );
        }
    }
}