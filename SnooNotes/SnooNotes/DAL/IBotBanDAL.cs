using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.DAL
{
    public interface IBotBanDAL
    {
        Task BanChannel(BannedEntity entity, string channelID, string mediaAuthor, VideoProvider vidProvider);
        Task BanUser(IEnumerable<BannedEntity> entities);
        Task<IEnumerable<BannedEntity>> GetBannedUsers(string subredditName);
    }
}