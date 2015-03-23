using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using System.Security.Claims;
namespace SnooNotesFrontend
{
    public class IdentityConfig
    {
    }

    public class UserManager : UserManager<Models.User>
    {
        public UserManager(IUserStore<Models.User> store)
            : base(store)
        {
            
        }
        public static UserManager Create(IdentityFactoryOptions<UserManager> options, IOwinContext context){
            var manager = new UserManager(new UserStore<Models.User>(context.Get<Models.ApplicationDbContext>()));
            manager.UserValidator = new UserValidator<Models.User>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            manager.UserLockoutEnabledByDefault = true;
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<Models.User>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
        
    }

    public class ApplicationSignInManager : SignInManager<Models.User, string>
    {
        public ApplicationSignInManager(UserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(Models.User user)
        {
            return user.GenerateUserIdentityAsync((UserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<UserManager>(), context.Authentication);
        }
    }
}