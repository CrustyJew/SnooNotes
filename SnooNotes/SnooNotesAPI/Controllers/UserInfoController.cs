using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Security.Claims;
using Microsoft.Owin;
namespace SnooNotesAPI.Controllers
{
    [Authorize]
    public class UserInfoController : ApiController
    {
        
    }
}
