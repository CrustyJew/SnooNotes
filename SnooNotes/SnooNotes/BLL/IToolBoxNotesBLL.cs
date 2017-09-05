using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;
using Hangfire;

namespace SnooNotes.BLL {
    public interface IToolBoxNotesBLL {

        [AutomaticRetry(Attempts = 0)]
        Task ImportToolboxNotes(List<Note> notes, string subreddit);
    }
}