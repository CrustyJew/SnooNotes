using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;
using System.Security.Claims;

namespace SnooNotes.BLL {
    public interface ISubredditBLL {
        Task AddSubreddit( Subreddit newSub, string modname, string ip );
        Task<List<Subreddit>> GetActiveSubs(ClaimsPrincipal user);
        Task<IEnumerable<Subreddit>> GetSubreddits( IEnumerable<string> subs, ClaimsPrincipal user );
        Task<object> UpdateSubreddit( Subreddit sub, ClaimsPrincipal user );
    }
}