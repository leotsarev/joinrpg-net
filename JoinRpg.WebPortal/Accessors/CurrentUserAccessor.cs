using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using JoinRpg.WebPortal.Managers.Interfaces;

namespace JoinRpg.WebPortal.Accessors
{
    public class CurrentUserAccessor : ICurrentUserAccessor
    {
        private IPrincipal CurrentUser { get; }

        public CurrentUserAccessor(IPrincipal currentUser)
        {
            CurrentUser = currentUser;
        }
        /// <inheritdoc />
        public int? UserIdOrDefault
        {
            get
            {
                var identity = CurrentUser.Identity as ClaimsIdentity;
                var userIdString = identity?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (userIdString == null)
                {
                    return null;
                }
                else
                {
                    return int.TryParse(userIdString, out var i) ? (int?)i : null;
                }
            }
        }

        /// <inheritdoc />
        public int UserId => throw new NotImplementedException();
    }
}
