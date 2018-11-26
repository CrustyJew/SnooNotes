using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.Utilities {
    public interface IAuthUtils {
        Task RevokeRefreshTokenAsync( string token, string username );
        Task UpdateModeratedSubredditsAsync( ApplicationUser ident );
        Task<bool> UpdateModsForSubAsync( Subreddit sub, IEnumerable<string> activeUsers );
    }
}