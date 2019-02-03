using System.Linq;
using Autofac;
using JoinRpg.Dal.Impl;

namespace JoinRpg.WebPortal
{
    internal class AutofacModule : Autofac.Module
    {
        /// <inheritdoc />
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<MyDbContext>();

            builder.RegisterType<ConfigStorage>().AsImplementedInterfaces();

            builder.RegisterTypes(Managers.Registration.GetTypes().ToArray());
        }
    }
}
