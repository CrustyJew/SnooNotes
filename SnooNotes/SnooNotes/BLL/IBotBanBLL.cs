using System.Threading.Tasks;
using RedditSharp.Things;
using SnooNotes.Models;

namespace SnooNotes.BLL
{
    public interface IBotBanBLL
    {
        Task BanChannel(BannedEntity channel);
        Task BanUser(BannedEntity user);
        Task<bool> SaveAutoModConfig(string editReason, RedditSharp.Wiki wiki);
    }
}