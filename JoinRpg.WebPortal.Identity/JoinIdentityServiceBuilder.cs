using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace JoinRpg.WebPortal.Identity
{
    public static class JoinIdentityServiceBuilder
    {
        /// <summary>
        /// Adds join impl to service collection
        /// </summary>
        public static void AddJoinIdentity(this IServiceCollection services)
        {
            services.AddDefaultIdentity<JoinIdentityUser>()
                .AddUserStore<JoinUserStore>();
        }
    }
}
