using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnooNotes.BLL
{
    public class ModActionBLL : IModActionBLL {
        private DAL.IModActionDAL modActDAL;

        public ModActionBLL(DAL.IModActionDAL modActionDAL ) {
            modActDAL = modActionDAL;
        }

        public Task<IEnumerable<Models.SentinelModLogEntry>> GetModLogEntriesForThing(string thingid, string subreddit ) {
            return modActDAL.GetModLogEntriesForThing(thingid, subreddit);
        }
    }
}
