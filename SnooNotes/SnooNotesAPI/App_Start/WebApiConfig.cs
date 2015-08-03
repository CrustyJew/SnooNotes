using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

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
		}
	}
}
