using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnooNotes.BLL {
    public class ToolBoxNotesBLL :IToolBoxNotesBLL {
        private DAL.INotesDAL notesDAL;

        public ToolBoxNotesBLL(DAL.INotesDAL notesDAL) {
            this.notesDAL = notesDAL;
        }
        [AutomaticRetry(Attempts = 0)]
        public async Task ImportToolboxNotes(List<Models.Note> notes, string subreddit) {
            var unames = notes.Select(n => n.AppliesToUsername).Distinct().ToArray();
            var webagent = new RedditSharp.WebAgent();
            webagent.UserAgent = "Snoonotes Importer";
            webagent.RateLimiter.Mode = RedditSharp.RateLimitMode.None;
            for(int i = 0; i < unames.Length; i += 20) {
                Helpers.ImportStatusHelper.ImportStatuses[subreddit] = $"Checking username {i + 1} of {unames.Length}";

                var curNames = unames.Skip(i).Take(20);
                List<Task<string>> tasks = new List<Task<string>>();
                foreach(var name in curNames) {
                    tasks.Add(GetRedditUsername(webagent, name));
                }
                while(tasks.Count > 0) {
                    var task = await Task.WhenAny(tasks).ConfigureAwait(false);
                    
                    tasks.Remove(task);

                    string name = await task.ConfigureAwait(false);
                    if(!string.IsNullOrWhiteSpace(name)) {
                        notes.ForEach(n => n.AppliesToUsername = n.AppliesToUsername == name.ToLower() ? name : n.AppliesToUsername);
                    }
                }
            }
            Helpers.ImportStatusHelper.ImportStatuses[subreddit] = "Saving notes to database...";
            var count = await notesDAL.AddNewToolBoxNotesAsync(notes).ConfigureAwait(false);
            Helpers.ImportStatusHelper.ImportStatuses[subreddit] = $"Done! Imported {count} new notes";
        }
        private async Task<string> GetRedditUsername(RedditSharp.IWebAgent webagent, string name) {
            try {
                var user = await RedditSharp.Things.RedditUser.GetUserAsync(webagent, name).ConfigureAwait(false);
                if(name == user.Name) return null;
                return user.Name;
            }
            catch {
                return null;
            }
        }
    }
}
