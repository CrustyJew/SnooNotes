using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SnooNotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SnooNotes.Controllers.Site {
    [Authorize(AuthenticationSchemes = "Snookie")]
    [Route("site/[controller]")]
    public class NoteController : Controller {
        private BLL.INotesBLL notesBLL;
        private IConfigurationRoot Configuration;
        private Signalr.ISnooNoteUpdates snUpdates;
        private UserManager<ApplicationUser> _userManager;
        public NoteController( IConfigurationRoot config, BLL.INotesBLL notesBLL, Signalr.ISnooNoteUpdates snooNoteUpdates, UserManager<ApplicationUser> userManager ) {
            this.notesBLL = notesBLL;
            Configuration = config;
            snUpdates = snooNoteUpdates;
            _userManager = userManager;
        }

        [HttpGet("{subName}/Export")]
        public async Task<Models.Export> ExportNotes([FromRoute]string subName) {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var claims = await _userManager.GetClaimsAsync(user);
            if (claims.Count(c => c.Type == "uri:snoonotes:admin" && c.Value.ToLower() == subName.ToLower()) == 0 ){
                throw new UnauthorizedAccessException("You are not a moderator of that subreddit!");
            }
            return await notesBLL.ExportNotes(subName);
        }


    }
}
