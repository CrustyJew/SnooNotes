using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.DAL
{
    public interface IBotBanDAL
    {
        Task<bool> BanChannel(BannedEntity entity, string channelID, string mediaAuthor, VideoProvider vidProvider);
        Task<bool> BanUser(IEnumerable<BannedEntity> entities);
        Task<IEnumerable<string>> GetBannedUserNames(string subredditName);
        Task<TableResults<BannedEntity>> SearchBannedUsers(IEnumerable<string> subredditNames, int limit, int page, string searchTerm, string orderBy, bool ascending);
        Task<bool> DeleteUserBan(string sub, int id);
        Task<Models.BannedEntity> GetBanByID(int id);
}