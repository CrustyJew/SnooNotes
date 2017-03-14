using System.Threading.Tasks;
using IdentityServer4.Services;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using IdentityServer4.Endpoints.Results;

namespace IdentProvider.Controllers
{
    public class CustomCheckSessionEndpoint : IEndpoint
    {
        private readonly ILogger<CustomCheckSessionEndpoint> _logger;

        public CustomCheckSessionEndpoint(ILogger<CustomCheckSessionEndpoint> logger)
        {
            _logger = logger;
        }

        public Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            IEndpointResult result;

            if (context.Request.Method != "GET")
            {
                _logger.LogWarning("Invalid HTTP method for check session endpoint");
                result = new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
            }
            else
            {
                _logger.LogDebug("Rendering check session result");
                result = new CustomCheckSessionResult();
            }

            return Task.FromResult(result);
        }
    }

    public class CustomCheckSessionResult : IEndpointResult
    {
        private ISessionIdService _sessionId;

        void Init(HttpContext context)
        {
            _sessionId = _sessionId ?? context.RequestServices.GetRequiredService<ISessionIdService>();
        }

    
    public Task ExecuteAsync(HttpContext context)
        {
            Init(context);
            string htmlResp = HTML.Replace("{cookieName}", _sessionId.GetCookieName());
            return context.Response.WriteHtmlAsync(htmlResp);
           
        }


        private const string HTML = @"
<!DOCTYPE html>
<html>
<head>
    <meta http-equiv='X-UA-Compatible' content='IE=edge' />
    <title>Check Session IFrame</title>
</head>
<body>
<span id='cookie-name' style='display:none;'>{cookieName}</span>
<script src='/js/CheckSession.js' type='text/javascript' />
</body>
</html>
";
    }
}