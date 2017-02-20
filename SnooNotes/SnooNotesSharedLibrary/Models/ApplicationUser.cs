using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SnooNotes.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        public DateTime TokenExpires { get; set; }
        public bool HasRead { get; set; }
        public bool HasWiki { get; set; }

        public DateTime LastUpdatedRoles { get; set; }
    }
}
