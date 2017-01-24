﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SnooNotesAPI.Controllers {
    [Route( "api/Dirtbag" )]
    public class DirtbagController : Controller {
        [HttpPost]
        [Route( "{subname}/TestConnection" )]
        public async Task<bool> TestConnection( Models.DirtbagSettings settings, string subname ) {
            if ( !ClaimsPrincipal.Current.HasClaim( $"urn:snoonotes:subreddits:{subname.ToLower()}:admin", "true" ) )
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );

            if ( !settings.DirtbagUrl.EndsWith( "/" ) ) settings.DirtbagUrl = settings.DirtbagUrl + "/";
            BLL.DirtbagBLL dirtbag = new BLL.DirtbagBLL();

            return await dirtbag.TestConnection( settings, subname );


        }

        [HttpPut]
        [Route( "{subname}" )]
        public async Task<Models.DirtbagSettings> Update( Models.DirtbagSettings settings, string subname ) {
            if ( !ClaimsPrincipal.Current.HasClaim( $"urn:snoonotes:subreddits:{subname.ToLower()}:admin", "true" ) )
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );

            if ( !settings.DirtbagUrl.EndsWith( "/" ) ) settings.DirtbagUrl = settings.DirtbagUrl + "/";
            BLL.DirtbagBLL dirtbag = new BLL.DirtbagBLL();
            await dirtbag.SaveSettings( settings, subname );
            return settings;
        }

        [HttpGet]
        [Route( "{subname}/BanList" )]
        public Task<IEnumerable<Models.BannedEntity>> GetBanList( string subname ) {
            if ( !ClaimsPrincipal.Current.HasClaim( $"urn:snoonotes:subreddits:{subname.ToLower()}:admin", "true" ) )
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );

            BLL.DirtbagBLL dirtbag = new BLL.DirtbagBLL();
            return dirtbag.GetBanList( subname );
        }

        [HttpDelete]
        [Route( "{subname}/BanList/{id}" )]
        public Task<bool> RemoveBan( string subname, int id ) {
            if ( !ClaimsPrincipal.Current.HasClaim( $"urn:snoonotes:subreddits:{subname.ToLower()}:admin", "true" ) )
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );

            BLL.DirtbagBLL dirtbag = new BLL.DirtbagBLL();
            return dirtbag.RemoveBan( id, ClaimsPrincipal.Current.Identity.Name, subname );
        }

        [HttpPut]
        [Route( "{subname}/Banlist/{id}" )]
        public Task UpdateBan( string subname, int id, [FromBody] string reason ) {
            if ( !ClaimsPrincipal.Current.HasClaim( $"urn:snoonotes:subreddits:{subname.ToLower()}:admin", "true" ) )
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );

            BLL.DirtbagBLL dirtbag = new BLL.DirtbagBLL();
            return dirtbag.UpdateBanReason( subname, id, reason, ClaimsPrincipal.Current.Identity.Name );
        }

        [HttpPost]
        [Route( "{subname}/BanList/Channels" )]
        public Task BanChannel( Models.BannedEntity entity, string subname ) {
            if ( !ClaimsPrincipal.Current.HasClaim( $"urn:snoonotes:subreddits:{subname.ToLower()}:admin", "true" ) )
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );

            BLL.DirtbagBLL dirtbag = new BLL.DirtbagBLL();
            return dirtbag.BanChannel( subname, entity.EntityString, entity.BanReason, entity.ThingID, ClaimsPrincipal.Current.Identity.Name );
        }

        [HttpPost]
        [Route( "{subname}/BanList/Users" )]
        public Task BanUser( Models.BannedEntity entity, string subname ) {
            if ( !ClaimsPrincipal.Current.HasClaim( $"urn:snoonotes:subreddits:{subname.ToLower()}:admin", "true" ) )
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );

            BLL.DirtbagBLL dirtbag = new BLL.DirtbagBLL();
            return dirtbag.BanUser( subname, entity.EntityString, entity.BanReason, entity.ThingID, ClaimsPrincipal.Current.Identity.Name );
        }
    }
}
