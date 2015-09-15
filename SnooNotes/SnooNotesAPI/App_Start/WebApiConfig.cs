using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace SnooNotesAPI {
	public static class WebApiConfig {
		public static void Register( HttpConfiguration config ) {
			// Web API configuration and services

			// Web API routes
			config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "ControllerAPI",
                routeTemplate: "api/{controller}/{action}/",
                defaults: new { }
            );
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.Routes.MapHttpRoute(
				name: "DefaultRESTApi",
				routeTemplate: "restapi/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);
            //config.Filters.Add(new ValidateModelAttribute());
		}
       
    }
    public class ValidateModelAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ModelState.IsValid == false)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, actionContext.ModelState);
            }
        }
    }
    public class WikiReadAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            //
        }
    }
}
