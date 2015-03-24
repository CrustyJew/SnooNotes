using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Claims;

namespace SnooNotesAPI.Controllers
{
    public class UserInfoController : ApiController
    {
        public List<string> GetModeratedSubreddits()
        {
            RedditSharp.Reddit rd = new RedditSharp.Reddit((User as ClaimsPrincipal).FindFirst("urn:reddit:accesstoken").Value);
            var subs = rd.User.ModeratorSubreddits;

            return subs.Select(s => s.Name).ToList<string>();

        }
    }
}
