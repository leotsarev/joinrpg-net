using JoinRpg.DataModel;
using Microsoft.AspNetCore.Identity;

namespace JoinRpg.WebPortal.Identity
{
    internal static class Builders
    {
        public static JoinIdentityUser ToIdentityUser(this User dbUser)
            => new JoinIdentityUser()
            {
                UserName = dbUser.UserName,
                Id = dbUser.UserId,
                HasPassword = dbUser.PasswordHash != null,
                PasswordHash = dbUser.PasswordHash,
                EmailConfirmed = dbUser.Auth.EmailConfirmed,
            };

        public static UserExternalLogin ToUserExternalLogin(this UserLoginInfo login)
            => new UserExternalLogin()
            {
                Key = login.ProviderKey,
                Provider = login.LoginProvider,
            };

        //TODO have display name here
        public static UserLoginInfo ToUserLoginInfo(this UserExternalLogin uel)
            => new UserLoginInfo(uel.Provider, uel.Key, uel.Key);
    }
}
