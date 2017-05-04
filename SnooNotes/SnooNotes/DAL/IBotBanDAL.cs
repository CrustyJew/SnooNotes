using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.DAL
{
    public interface IBotBanDAL
    {
        Task<bool> BanChannel(BannedEntity entity, string channelID, string mediaAuthor, VideoProvider vidProvider);
        Task<bool> BanUser(IEnumerable<BannedEntity> entities);
        Task<IEnumerable<BannedEntity>> GetBannedUsers(string subredditName);
    }
}