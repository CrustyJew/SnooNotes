using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.BLL {
    public interface IModActionBLL {
        Task<IEnumerable<SentinelModLogEntry>> GetModLogEntriesForThing( string thingid, string subreddit );
    }
}