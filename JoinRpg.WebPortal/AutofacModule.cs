using System.Linq;
using Autofac;
using JoinRpg.Dal.Impl;
using JoinRpg.WebPortal.Accessors;

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

            builder.RegisterTypes(RepositoriesRegistraton.GetTypes().ToArray())
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterTypes(Services.Impl.Services.GetTypes().ToArray())
                .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<CurrentUserAccessor>().AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}
