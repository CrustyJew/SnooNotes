using System.Collections.Generic;
using System.Threading.Tasks;
using SnooNotes.Models;

namespace SnooNotes.DAL {
    public interface IModActionDAL {
        Task<IEnumerable<SentinelModLogEntry>> GetModLogEntriesForThing( string thingid, string subreddit );
    }
}