using Nop.Web.Framework.Mvc.Routes;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nop.Plugin.Payments.CardCom
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //SuccessHandler
            routes.MapRoute("Plugin.Payments.CardCom.SuccessHandler",
                 "Plugins/PaymentCardCom/SuccessHandler",
                 new { controller = "PaymentCardCom", action = "SuccessHandler" },
                 new[] { "Nop.Plugin.Payments.CardCom.Controllers" }
            );
            //FailHandler
            routes.MapRoute("Plugin.Payments.CardCom.FailHandler",
                 "Plugins/PaymentCardCom/FailHandler",
                 new { controller = "PaymentCardCom", action = "FailHandler" },
                 new[] { "Nop.Plugin.Payments.CardCom.Controllers" }
            );
            //Cancel
            routes.MapRoute("Plugin.Payments.CardCom.CancelOrder",
                 "Plugins/PaymentCardCom/CancelOrder",
                 new { controller = "PaymentCardCom", action = "CancelOrder" },
                 new[] { "Nop.Plugin.Payments.CardCom.Controllers" }
            );
            //PaymentIFrame
            routes.MapRoute("Plugin.Payments.CardCom.PaymentIFrame",
                 "Plugins/PaymentCardCom/PaymentIFrame",
                 new { controller = "PaymentCardCom", action = "PaymentIFrame" },
                 new[] { "Nop.Plugin.Payments.CardCom.Controllers" }
            );
            //PaymentIFrame success
            routes.MapRoute("Plugin.Payments.CardCom.Success",
                 "Plugins/PaymentCardCom/Success",
                 new { controller = "PaymentCardCom", action = "Success" },
                 new[] { "Nop.Plugin.Payments.CardCom.Controllers" }
            );
            //PaymentIFrame success
            routes.MapRoute("Plugin.Payments.CardCom.Failed",
                 "Plugins/PaymentCardCom/Failed",
                 new { controller = "PaymentCardCom", action = "Failed" },
                 new[] { "Nop.Plugin.Payments.CardCom.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
