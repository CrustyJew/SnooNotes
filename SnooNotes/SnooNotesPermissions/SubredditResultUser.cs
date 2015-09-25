using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooNotesPermissions
{
    class SubredditResultUser
    {
        public bool HasRead { get; set; }
        public string UserName { get; set; }
        public int ClaimID { get; set; }
        public int AdminClaimId { get; set; }
    }
}
