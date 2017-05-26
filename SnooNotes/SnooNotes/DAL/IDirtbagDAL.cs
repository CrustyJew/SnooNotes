using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.DAL {
    public interface IDirtbagDAL {
        Task AddToBanList( DirtbagSettings conn, List<BannedEntity> list );
        Task<IEnumerable<BannedEntity>> GetBanList( DirtbagSettings conn, string subreddit );
        Task<bool> RemoveFromBanList( DirtbagSettings conn, int id, string modName, string subreddit );
        Task<bool> TestConnection( DirtbagSettings botSettings, string subreddit );
        Task<bool> TestConnection( string dirtbagUrl, string dirtbagUsername, string dirtbagPassword, string subreddit );
        Task UpdateBanReason( DirtbagSettings conn, string subName, int id, string reason, string modname );
    }
}