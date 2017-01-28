using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.BLL {
    public interface ISubredditBLL {
        Task AddSubreddit( Subreddit newSub, string modname, string ip );
        Task<List<Subreddit>> GetActiveSubs();
        Task<IEnumerable<Subreddit>> GetSubreddits( IEnumerable<string> subs );
        Task<object> UpdateSubreddit( Subreddit sub );
    }
}