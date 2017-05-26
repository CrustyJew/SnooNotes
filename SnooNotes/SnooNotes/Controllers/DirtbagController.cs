using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace SnooNotes.Controllers {
    [Authorize]
    [Route( "api/Dirtbag" )]
    public class DirtbagController : Controller {

        private BLL.IDirtbagBLL dirtbag;
        public DirtbagController(BLL.IDirtbagBLL dirtbagBLL ) {
            dirtbag = dirtbagBLL;
        }
        [HttpPost( "{subname}/TestConnection" )]
        public async Task<bool> TestConnection( Models.DirtbagSettings settings, string subname ) {
            if ( !User.HasClaim( "uri:snoonotes:admin", subname.ToLower() ) ) {
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
            }
            if ( !settings.DirtbagUrl.EndsWith( "/" ) ) settings.DirtbagUrl = settings.DirtbagUrl + "/";

            return await dirtbag.TestConnection( settings, subname );


        }
        
        [HttpPut( "{subname}" )]
        public async Task<Models.DirtbagSettings> Update( Models.DirtbagSettings settings, string subname ) {
            if ( !User.HasClaim( "uri:snoonotes:admin", subname.ToLower() ) ) {
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
            }
            if ( !settings.DirtbagUrl.EndsWith( "/" ) ) settings.DirtbagUrl = settings.DirtbagUrl + "/";
            await dirtbag.SaveSettings( settings, subname );
            return settings;
        }
        
        [HttpGet( "{subname}/BanList" )]
        public Task<IEnumerable<Models.BannedEntity>> GetBanList( string subname ) {
            if ( !User.HasClaim( "uri:snoonotes:admin", subname.ToLower() ) ) {
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
            }
            return dirtbag.GetBanList( subname );
        }
        
        [HttpDelete( "{subname}/BanList/{id}" )]
        public Task<bool> RemoveBan( string subname, int id ) {
            if ( !User.HasClaim( "uri:snoonotes:admin", subname.ToLower() ) ) {
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
            }
            return dirtbag.RemoveBan( id, User.Identity.Name, subname );
        }
        
        [HttpPut( "{subname}/Banlist/{id}" )]
        public Task UpdateBan( string subname, int id, [FromBody] string reason ) {
            if ( !User.HasClaim( "uri:snoonotes:admin", subname.ToLower() ) ) {
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
            }
            return dirtbag.UpdateBanReason( subname, id, reason, User.Identity.Name );
        }
        
        //[HttpPost( "{subname}/BanList/Channels" )]
        //public Task BanChannel( Models.BannedEntity entity, string subname ) {
        //    if ( !User.HasClaim( "uri:snoonotes:admin", subname.ToLower() ) ) {
        //        throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
        //    }
        //    return dirtbag.BanChannel( subname, entity.EntityString, entity.BanReason, entity.ThingID, User.Identity.Name );
        //}
        
        //[HttpPost( "{subname}/BanList/Users" )]
        //public Task BanUser( Models.BannedEntity entity, string subname ) {
        //    if ( !User.HasClaim( "uri:snoonotes:admin", subname.ToLower() ) ) {
        //        throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );
        //    }
        //    return dirtbag.BanUser( subname, entity.EntityString, entity.BanReason, entity.ThingID, User.Identity.Name );
        //}
    }
}
