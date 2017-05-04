using System.Threading.Tasks;
using RedditSharp.Things;
using SnooNotes.Models;

namespace SnooNotes.BLL
{
    public interface IBotBanBLL
    {
        Task<bool> BanChannel(BannedEntity channel);
        Task<bool> BanUser(BannedEntity user);
        Task<bool> SaveAutoModConfig(string editReason, RedditSharp.Wiki wiki);
    }
}