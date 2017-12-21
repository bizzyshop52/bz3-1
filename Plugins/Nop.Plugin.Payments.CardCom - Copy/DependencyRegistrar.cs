using Autofac;
using Autofac.Integration.Mvc;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Payments.CardCom.Services;
using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.CardCom
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<CardComService>().As<ICardComService>().InstancePerHttpRequest();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
