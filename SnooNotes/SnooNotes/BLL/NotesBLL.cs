using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SnooNotes.Models;
using Microsoft.Extensions.Configuration;

namespace SnooNotes.BLL {
    public class NotesBLL :INotesBLL {
        private DAL.INotesDAL notesDAL;
        private DAL.INoteTypesDAL noteTypesDAL;
        public NotesBLL(DAL.INotesDAL notesDAL, DAL.INoteTypesDAL noteTypesDAL) {
            this.notesDAL = notesDAL;
            this.noteTypesDAL = noteTypesDAL;
        }

        public Task<IEnumerable<string>> GetUsersWithNotes(IEnumerable<string> subnames) {
            return notesDAL.GetUsersWithNotes(subnames);
        }

        public async Task<Dictionary<string, IEnumerable<Models.BasicNote>>> GetNotes(IEnumerable<string> subnames, IEnumerable<string> users, bool ascending = true) {

            var notes = (await notesDAL.GetNotes(subnames, users, ascending)).ToList();
            Dictionary<string, IEnumerable<Models.BasicNote>> toReturn = new Dictionary<string, IEnumerable<Models.BasicNote>>();
            foreach(string user in notes.Select(n => n.AppliesToUsername.ToLower()).Distinct()) {
                var unotes = notes.Where(u => u.AppliesToUsername.ToLower() == user).Select(n => new Models.BasicNote { Message = n.Message, NoteID = n.NoteID, NoteTypeID = n.NoteTypeID, Submitter = n.Submitter, SubName = n.SubName, Url = n.Url, Timestamp = n.Timestamp, ParentSubreddit = n.ParentSubreddit });
                toReturn.Add(user, unotes);
            }
            return toReturn;
        }

        public Task<bool> UserHasNotes(IEnumerable<string> subnames, string username) {
            return notesDAL.UserHasNotes(subnames, username);
        }

        public Task<Note> AddNoteForUser(Note value) {
            return notesDAL.AddNoteForUser(value);
        }

        public Task<Note> AddNoteToCabal(Note value, string cabalSub) {
            return notesDAL.AddNoteToCabal(value, cabalSub);
        }

        public Task<Note> GetNoteByID(int id) {
            return notesDAL.GetNoteByID(id);
        }

        public Task<bool> DeleteNoteForUser(Note note, string name) {
            return notesDAL.DeleteNoteForUser(note, name);
        }

        public async Task<Export> ExportNotes(string subname) {
            var toReturn = new Export();
            toReturn.NoteTypes = await noteTypesDAL.GetNoteTypesForSubs(new string[] { subname });
            toReturn.Notes = await notesDAL.ExportNotes(new string[] { subname });
            return toReturn;
        }
    }
}