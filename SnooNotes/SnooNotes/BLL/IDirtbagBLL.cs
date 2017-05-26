using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.BLL {
    public interface IDirtbagBLL {
        Task BanChannel( string subName, string url, string reason, string thingID, string bannedBy );
        Task BanUser( string subName, string username, string reason, string thingID, string bannedBy );
        Task<IEnumerable<BannedEntity>> GetBanList( string subName );
        Task<bool> RemoveBan( int id, string modName, string subName );
        Task<bool> SaveSettings( DirtbagSettings settings, string subName );
        Task<bool> TestConnection( string subName );
        Task<bool> TestConnection( DirtbagSettings newSettings, string subName );
        Task UpdateBanReason( string subName, int id, string reason, string modname );
    }
}