using System.Threading.Tasks;
using IdentityServer4.Services;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using IdentityServer4.Endpoints.Results;
using Microsoft.Extensions.Configuration;

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
        private string _extensionOrigin;
        void Init(HttpContext context)
        {
            _sessionId = _sessionId ?? context.RequestServices.GetRequiredService<ISessionIdService>();
            _extensionOrigin = _extensionOrigin ?? context.RequestServices.GetRequiredService<IConfigurationRoot>()["ExtensionOrigin"];
        }

    
    public Task ExecuteAsync(HttpContext context)
        {
            Init(context);
            string htmlResp = HTML.Replace("{cookieName}", _sessionId.GetCookieName()).Replace("{extensionOrigin}",_extensionOrigin);

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
<span id='extension-origin' style='display:none;'>{extensionOrigin}</span>
<script src='/js/CheckSession.js' type='text/javascript'></script>
</body>
</html>
";
    }
}