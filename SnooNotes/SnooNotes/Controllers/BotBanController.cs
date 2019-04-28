using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SnooNotes.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "token")]
    public class BotBanController : Controller
    {
        private BLL.IBotBanBLL bbBLL;

        public BotBanController(BLL.IBotBanBLL botBanBLL)
        {
            bbBLL = botBanBLL;
        }
        [HttpPost("{sub}/Channel")]
        public Task<bool> BanChannel(string sub, [FromBody]Models.BannedEntity entity) {
            ClaimsPrincipal ident = User as ClaimsPrincipal;
            if (!ident.HasClaim("uri:snoonotes:admin", sub.ToLower())) throw new UnauthorizedAccessException("Not an admin of that sub");
            entity.SubName = sub;
            entity.BannedBy = ident.Identity.Name;
            entity.BanDate = DateTime.UtcNow;
            return bbBLL.BanChannel(entity);
        }

        [HttpPost("{sub}/User")]
        public Task<bool> BanUser(string sub, [FromBody] Models.BannedEntity entity)
        {
            ClaimsPrincipal ident = User as ClaimsPrincipal;
            if (!ident.HasClaim("uri:snoonotes:admin", sub.ToLower())) throw new UnauthorizedAccessException("Not an admin of that sub");
            entity.SubName = sub;
            entity.BannedBy = ident.Identity.Name;
            entity.BanDate = DateTime.UtcNow;
            return bbBLL.BanUser(entity);
        }

        [HttpDelete("{sub}/User/{id}")]
        public Task<bool> UnbanUser(string sub, int id)
        {
            ClaimsPrincipal ident = User as ClaimsPrincipal;
            if (!ident.HasClaim("uri:snoonotes:admin", sub.ToLower())) throw new UnauthorizedAccessException("Not an admin of that sub");
            return (bbBLL.RemoveUserBan(sub, id, User.Identity.Name));
        }

        [HttpPut("{sub}/User/{id}")]
        public Task<bool> UpdateAdditionalInfo(string sub, int id, [FromBody]string additionalInfo)
        {
            if (!User.HasClaim("uri:snoonotes:admin", sub.ToLower())) throw new UnauthorizedAccessException("Not an admin of that sub");
            return (bbBLL.UpdateAdditionalInfo(sub, id, additionalInfo, User.Identity.Name));
        }

        /// <summary>
        /// Searches for bot banned users
        /// </summary>
        /// <param name="subreddits">Comma separated string containing a list of subreddits to search. Defaults to all subs the user is an admin of.</param>
        /// <param name="limit">Limit per page for results to return. Defaults to 25</param>
        /// <param name="page">Page of results to request. Defaults to 1</param>
        /// <param name="searchTerm">Search term to use to limit results. Searches UserName, BannedBy, ThingUrl, AdditionalInfo, SubName, and BanReason</param>
        /// <param name="orderBy">Property to order by. Valid options are "username","bannedby","date","subreddit", and "reason". Defaults to "date"</param>
        /// <param name="ascending">Sort the <paramref name="orderBy"/> by ascending if true or descending if false. Defaults to false</param>
        /// <returns></returns>
        [HttpGet("Search/User")]
        public Task<Models.TableResults<Models.BannedEntity>> SearchBannedUsers([FromQuery]string subreddits = null,[FromQuery]int limit = 25,[FromQuery]int page = 1, [FromQuery]string searchTerm = null, [FromQuery]string orderBy = "date", [FromQuery]bool ascending=false)
        {
            IEnumerable<string> subsToSearch = null;
            if (string.IsNullOrWhiteSpace(subreddits))
            {
                subsToSearch = User.Claims.Where(c => c.Type == "uri:snoonotes:admin").Select(c => c.Value);
            }
            else
            {
                subsToSearch = subreddits.Split(',');
                foreach (string sub in subsToSearch) {
                    if (!string.IsNullOrWhiteSpace(sub)) {
                        if (!User.HasClaim("uri:snoonotes:admin", sub.ToLower())) throw new UnauthorizedAccessException("Not an admin of that sub");
                    }
                }
            }

            return bbBLL.SearchBannedUsers(subsToSearch,limit,page,searchTerm,orderBy,ascending);
        }
    }
}