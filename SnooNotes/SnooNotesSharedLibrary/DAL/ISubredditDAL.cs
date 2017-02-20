using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.DAL {
    public interface ISubredditDAL {
        Task<string> AddSubreddit( Subreddit sub );
        Task<List<Subreddit>> GetActiveSubs();
        Task<DirtbagSettings> GetBotSettings( string subName );
        Task<IEnumerable<Subreddit>> GetSubreddits( IEnumerable<string> subnames );
        Task<bool> UpdateBotSettings( DirtbagSettings settings, string subName );
        Task<bool> UpdateSubredditSettings( Subreddit sub );
    }
}