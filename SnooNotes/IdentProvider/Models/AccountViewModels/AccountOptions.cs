using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentProvider.Models.AccountViewModels
{
    public class AccountOptions
    {
        public static bool AllowLocalLogin = false;
        public static bool AllowRememberLogin = true;
        public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays( 30 );

        public static bool ShowLogoutPrompt = false;
        public static bool AutomaticRedirectAfterSignOut = true;

        public static bool WindowsAuthenticationEnabled = false;
        // specify the Windows authentication schemes you want to use for authentication
        public static readonly string[] WindowsAuthenticationSchemes = new string[] { "Negotiate", "NTLM" };
        public static readonly string WindowsAuthenticationDisplayName = "Windows";

        public static string InvalidCredentialsErrorMessage = "Invalid username or password";
    }
}
