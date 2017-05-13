using System.Threading.Tasks;
using RedditSharp.Things;
using SnooNotes.Models;
using System.Collections.Generic;

namespace SnooNotes.BLL
{
    public interface IBotBanBLL
    {
        Task<bool> BanChannel(BannedEntity channel);
        Task<bool> BanUser(BannedEntity user);
        Task<bool> SaveAutoModConfig(string editReason, RedditSharp.Wiki wiki);
        Task<TableResults<BannedEntity>> SearchBannedUsers(IEnumerable<string> subreddits, int limit, int page, string searchTerm, string orderBy, bool ascending);
        Task<bool> RemoveUserBan(string sub, int id, string unbanby);
        Task<bool> UpdateAdditionalInfo(string sub, int id, string additionalInfo, string user);
    }
}