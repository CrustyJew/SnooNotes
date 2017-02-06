using System.Security.Claims;
using System.Threading.Tasks;
using IdentProvider.Models;
using SnooNotes.Models;

namespace SnooNotes.Utilities {
    public interface IAuthUtils {
        Task CheckTokenExpiration( ApplicationUser ident );
        Task CheckTokenExpirationAsync( ClaimsPrincipal user );
        Task GetNewTokenAsync( ApplicationUser ident );
        Task RevokeRefreshTokenAsync( string token );
        Task UpdateModeratedSubredditsAsync( ApplicationUser ident, ClaimsPrincipal user );
        Task<bool> UpdateModsForSubAsync( Subreddit sub, ClaimsPrincipal user );
    }
}