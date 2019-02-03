using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JoinRpg.Dal.Impl;
using Microsoft.Extensions.Configuration;

namespace JoinRpg.WebPortal
{
    public class ConfigStorage : IJoinDbContextConfig
    {
        private IConfiguration Configuration { get; }

        public ConfigStorage(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <inheritdoc />
        public string ConnectionString => Configuration.GetConnectionString("DefaultConnection");
    }
}
