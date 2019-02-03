using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JoinRpg.Dal.Impl;
using JoinRpg.DataModel;
using Microsoft.AspNetCore.Identity;
using DbUser = JoinRpg.DataModel.User;

namespace JoinRpg.WebPortal.Identity
{
    internal class JoinUserStore : IUserStore<JoinIdentityUser>, IUserEmailStore<JoinIdentityUser>,
        IUserPasswordStore<JoinIdentityUser>,
        IUserRoleStore<JoinIdentityUser>
    {
        private readonly MyDbContext _ctx;
        private DbSet<DbUser> UserSet { get; }

        public JoinUserStore(MyDbContext ctx)
        {
            _ctx = ctx;
            UserSet = _ctx.Set<DbUser>();
        }

        /// <inheritdoc />
        public void Dispose() => _ctx?.Dispose();

        /// <inheritdoc />
        Task<string> IUserStore<JoinIdentityUser>.GetUserIdAsync(JoinIdentityUser user,
            CancellationToken cancellationToken) => Task.FromResult(user.Id.ToString());

        /// <inheritdoc />
        Task<string> IUserStore<JoinIdentityUser>.GetUserNameAsync(JoinIdentityUser user,
            CancellationToken cancellationToken) => Task.FromResult(user.UserName);

        /// <inheritdoc />
        Task IUserStore<JoinIdentityUser>.SetUserNameAsync(JoinIdentityUser user,
            string userName,
            CancellationToken cancellationToken)
        {
            user.UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            return Task.CompletedTask;
        }

        //TODO for performance reason we need to store normalized version of username in DB.
        //Revisit later
        /// <inheritdoc />
        Task<string> IUserStore<JoinIdentityUser>.GetNormalizedUserNameAsync(
            JoinIdentityUser user,
            CancellationToken cancellationToken) =>
            Task.FromResult(user.UserName.ToUpperInvariant());

        Task IUserStore<JoinIdentityUser>.SetNormalizedUserNameAsync(JoinIdentityUser user,
            string normalizedName,
            CancellationToken cancellationToken) => Task.CompletedTask;

        async Task<IdentityResult> IUserStore<JoinIdentityUser>.CreateAsync(JoinIdentityUser user,
            CancellationToken ct)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var hasAnyUser = await UserSet.AnyAsync(ct);

            var dbUser = new DbUser()
            {
                UserName = user.UserName,
                Email = user.UserName,
                Auth = new UserAuthDetails()
                {
                    RegisterDate = DateTime.UtcNow,
                },
                PasswordHash = user.PasswordHash,
            };

            if (!hasAnyUser)
            {
                dbUser.Auth.EmailConfirmed = true;
                dbUser.Auth.IsAdmin = true;
            }

            _ctx.UserSet.Add(dbUser);
            await _ctx.SaveChangesAsync(ct);
            user.Id = dbUser.UserId;

            return IdentityResult.Success;
        }

        /// <inheritdoc />
        public async Task<IdentityResult> UpdateAsync(JoinIdentityUser user, CancellationToken ct)
        {
            var dbUser = await LoadUser(user);
            dbUser.UserName = user.UserName;
            dbUser.Email = user.UserName;
            dbUser.PasswordHash = user.PasswordHash;
            await _ctx.SaveChangesAsync(ct);
            return IdentityResult.Success;
        }

        /// <inheritdoc />
        public Task<IdentityResult> DeleteAsync(JoinIdentityUser user, CancellationToken ct) =>
            throw new NotImplementedException();

        /// <inheritdoc />
        public async Task<JoinIdentityUser> FindByIdAsync(string userId, CancellationToken ct)
        {
            var dbUser = await LoadUserById(userId, ct);
            return dbUser?.ToIdentityUser();
        }

        /// <inheritdoc />
        public async Task<JoinIdentityUser> FindByNameAsync(string userName, CancellationToken ct)
        {
            var dbUser = await LoadUserByName(userName, ct);
            return dbUser?.ToIdentityUser();
        }


        Task IUserPasswordStore<JoinIdentityUser>.SetPasswordHashAsync(JoinIdentityUser user,
            string passwordHash,
            CancellationToken ct)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        Task<string> IUserPasswordStore<JoinIdentityUser>.GetPasswordHashAsync(
            JoinIdentityUser user,
            CancellationToken ct)
        {
            return Task.FromResult(user.PasswordHash);
        }

        Task<bool> IUserPasswordStore<JoinIdentityUser>.HasPasswordAsync(JoinIdentityUser user,
            CancellationToken ct)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(JoinIdentityUser user) =>
            throw new NotImplementedException();

        public Task SetLockoutEndDateAsync(JoinIdentityUser user, DateTimeOffset lockoutEnd) =>
            throw new NotImplementedException();

        public Task<int> IncrementAccessFailedCountAsync(JoinIdentityUser user) =>
            throw new NotImplementedException();

        public Task ResetAccessFailedCountAsync(JoinIdentityUser user) =>
            Task.FromResult<object>(null);

        public Task<int> GetAccessFailedCountAsync(JoinIdentityUser user) => Task.FromResult(0);

        public Task<bool> GetLockoutEnabledAsync(JoinIdentityUser user) => Task.FromResult(false);

        public Task SetLockoutEnabledAsync(JoinIdentityUser user, bool enabled) =>
            throw new NotImplementedException();

        public Task SetTwoFactorEnabledAsync(JoinIdentityUser user, bool enabled) =>
            throw new NotImplementedException();

        public Task<bool> GetTwoFactorEnabledAsync(JoinIdentityUser user) => Task.FromResult(false);

        Task IUserEmailStore<JoinIdentityUser>.SetEmailAsync(JoinIdentityUser user,
            string email,
            CancellationToken ct)
        {
            user.UserName = email;
            return Task.CompletedTask;
        }

        Task<string> IUserEmailStore<JoinIdentityUser>.GetEmailAsync(JoinIdentityUser user,
            CancellationToken ct) => Task.FromResult(user.UserName);

        Task<bool> IUserEmailStore<JoinIdentityUser>.GetEmailConfirmedAsync(JoinIdentityUser user,
            CancellationToken ct)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        Task IUserEmailStore<JoinIdentityUser>.SetEmailConfirmedAsync(JoinIdentityUser user,
            bool confirmed,
            CancellationToken ct)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        Task<string> IUserEmailStore<JoinIdentityUser>.GetNormalizedEmailAsync(
            JoinIdentityUser user,
            CancellationToken ct) => Task.FromResult(user.UserName.ToUpperInvariant());

        Task IUserEmailStore<JoinIdentityUser>.SetNormalizedEmailAsync(JoinIdentityUser user,
            string email,
            CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        async Task<JoinIdentityUser> IUserEmailStore<JoinIdentityUser>.FindByEmailAsync(
            string email,
            CancellationToken cancellationToken)
        {
            var user = await LoadUserByName(email, cancellationToken);
            return user.ToIdentityUser();
        }

        public async Task AddLoginAsync(JoinIdentityUser user, UserLoginInfo login)
        {
            var dbUser = await LoadUser(user);
            dbUser.ExternalLogins.Add(login.ToUserExternalLogin());
            await _ctx.SaveChangesAsync();
        }

        public async Task RemoveLoginAsync(JoinIdentityUser user, UserLoginInfo login)
        {
            var dbUser = await LoadUser(user);
            var el =
                dbUser.ExternalLogins.First(
                    externalLogin => externalLogin.Key == login.ProviderKey &&
                                     externalLogin.Provider == login.LoginProvider);
            _ctx.Set<UserExternalLogin>().Remove(el);
            await _ctx.SaveChangesAsync();
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(JoinIdentityUser user)
        {
            var dbUser = await LoadUser(user);
            return dbUser.ExternalLogins.Select(uel => uel.ToUserLoginInfo()).ToList();
        }

        public async Task<JoinIdentityUser> FindAsync(UserLoginInfo login)
        {
            var uel =
                await _ctx.Set<UserExternalLogin>()
                    .SingleOrDefaultAsync(u =>
                        u.Key == login.ProviderKey && u.Provider == login.LoginProvider);

            return uel?.User.ToIdentityUser();
        }

        #region Implementation of IUserRoleStore<User,in int>

        Task IUserRoleStore<JoinIdentityUser>.AddToRoleAsync(JoinIdentityUser user,
            string roleName,
            CancellationToken ct) =>
            throw new NotSupportedException();

        Task IUserRoleStore<JoinIdentityUser>.RemoveFromRoleAsync(JoinIdentityUser user,
            string roleName,
            CancellationToken ct) =>
            throw new NotSupportedException();

        public async Task<IList<string>> GetRolesAsync(JoinIdentityUser user, CancellationToken ct)
        {
            List<string> list;
            if (user.IsAdmin)
            {
                list = new List<string>() {Security.AdminRoleName};
            }
            else
            {
                list = new List<string>();
            }

            return list;
        }

        async Task<bool> IUserRoleStore<JoinIdentityUser>.IsInRoleAsync(JoinIdentityUser user,
            string roleName,
            CancellationToken ct)
        {
            var roles = await GetRolesAsync(user, ct);
            return roles.Contains(roleName);
        }

        /// <inheritdoc />
        Task<IList<JoinIdentityUser>> IUserRoleStore<JoinIdentityUser>.GetUsersInRoleAsync(
            string roleName,
            CancellationToken cancellationToken) => throw new NotImplementedException();

        #endregion

        private async Task<User> LoadUserByName(string userName, CancellationToken ct) =>
            await _ctx.UserSet.SingleOrDefaultAsync(user => user.Email == userName, ct);

        private async Task<User> LoadUserById(string id, CancellationToken ct) =>
            await _ctx.UserSet.SingleOrDefaultAsync(user => user.UserId.ToString() == id, ct);

        private async Task<DbUser> LoadUser(JoinIdentityUser joinIdentityUser) =>
            await _ctx.UserSet.Include(u => u.ExternalLogins)
                .SingleOrDefaultAsync(user => user.UserId == joinIdentityUser.Id);
    }
}
