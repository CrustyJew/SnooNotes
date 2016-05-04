﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Threading.Tasks;

namespace SnooNotesAPI.Controllers
{
    [RoutePrefix("api/Dirtbag")]
    public class DirtbagController : ApiController
    {
        [HttpPost][Route("{subname}/TestConnection")]
        public async Task<bool> TestConnection(Models.DirtbagSettings settings, string subname ) {
            if ( !ClaimsPrincipal.Current.HasClaim( $"urn:snoonotes:subreddits:{subname.ToLower()}:admin", "true" ) )
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );

            if ( ! settings.DirtbagUrl.EndsWith( "/" ) ) settings.DirtbagUrl = settings.DirtbagUrl + "/";
            BLL.DirtbagBLL dirtbag = new BLL.DirtbagBLL();
            try {
                return await dirtbag.TestConnection( settings, subname );
            }
            catch (HttpRequestException ex) {
                throw new HttpResponseException( Request.CreateErrorResponse( HttpStatusCode.BadRequest, ex.GetBaseException().Message ) );
            }
            
        }

        [HttpPut][Route("{subname}")]
        public async Task<Models.DirtbagSettings> Update(Models.DirtbagSettings settings, string subname ) {
            if ( !ClaimsPrincipal.Current.HasClaim( $"urn:snoonotes:subreddits:{subname.ToLower()}:admin", "true" ) )
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );

            if ( !settings.DirtbagUrl.EndsWith( "/" ) ) settings.DirtbagUrl = settings.DirtbagUrl + "/";
            BLL.DirtbagBLL dirtbag = new BLL.DirtbagBLL();
            await dirtbag.SaveSettings( settings, subname );
            return settings;
        }

        [HttpGet][Route("{subname}/BanList")]
        public Task<IEnumerable<Models.BannedEntity>> GetBanList(string subname ) {
            if ( !ClaimsPrincipal.Current.HasClaim( $"urn:snoonotes:subreddits:{subname.ToLower()}:admin", "true" ) )
                throw new UnauthorizedAccessException( "You are not an admin of this subreddit!" );

            BLL.DirtbagBLL dirtbag = new BLL.DirtbagBLL();
            return dirtbag.GetBanList( subname );
        }
    }
}
