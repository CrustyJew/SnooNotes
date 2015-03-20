using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup( typeof( SnooNotesFrontend.Startup ) )]

namespace SnooNotesFrontend {
	public partial class Startup {
		public void Configuration( IAppBuilder app ) {
			ConfigureAuth( app );
		}
	}
}
