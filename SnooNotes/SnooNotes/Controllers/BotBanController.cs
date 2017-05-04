using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SnooNotes.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
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
    }
}