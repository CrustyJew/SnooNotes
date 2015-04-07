using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.Owin;
namespace SnooNotesAPI.Controllers
{
    [Authorize]
    public class AccountController : ApiController
    {

        public List<string> GetModeratedSubreddits()
        {
            return (User.Identity as ClaimsIdentity).Claims.Where(c => c.Type == (User.Identity as ClaimsIdentity).RoleClaimType).Select(c => c.Value).ToList<string>();
        }

    }
}