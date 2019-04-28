using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace SnooNotes.DAL {
    public class ModActionDAL : IModActionDAL {
        private IDbConnection sentinelConn;

        public ModActionDAL(IDbConnection sentinelConn ) {
            this.sentinelConn = sentinelConn;
        }

        public Task<IEnumerable<Models.SentinelModLogEntry>> GetModLogEntriesForThing(string thingid, string subreddit ) {
            string query = @"
select ml.thing_id ""ThingID"", ml.mod ""Mod"", ml.action ""ModAction"", ml.actionreason ""ActionReason"", ml.description ""Description"", ml.modactionid ""ModActionID"", ml.action_utc ""Timestamp"" from modlog ml
inner join subreddit s on s.id = ml.subreddit_id
where s.subreddit_name = @subreddit and ml.thing_id = @thingid
";
            return sentinelConn.QueryAsync<Models.SentinelModLogEntry>(query, new { thingid, subreddit });
        }
        //select ml.thing_id "ThingID", ml.mod "Mod", ml.action "ModAction", ml.actionreason "ActionReason", ml.description "Description", ml.modactionid "ModActionID", ml.action_utc "Timestamp" from modlog ml
        //inner join subreddit s on s.id = ml.subreddit_id
    }
}
