using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.DiscountRules.TargetPrice
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.DiscountRules.TargetPrice.Configure",
                 "Plugins/DiscountRulesTargetPrice/Configure",
                 new { controller = "DiscountRulesTargetPrice", action = "Configure" },
                 new[] { "Nop.Plugin.DiscountRules.TargetPrice.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.TargetPrice.ProductAddPopup",
                 "Plugins/DiscountRulesTargetPrice/ProductAddPopup",
                 new { controller = "DiscountRulesTargetPrice", action = "ProductAddPopup" },
                 new[] { "Nop.Plugin.DiscountRules.TargetPrice.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.TargetPrice.ProductAddPopupList",
                 "Plugins/DiscountRulesTargetPrice/ProductAddPopupList",
                 new { controller = "DiscountRulesTargetPrice", action = "ProductAddPopupList" },
                 new[] { "Nop.Plugin.DiscountRules.TargetPrice.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.TargetPrice.LoadProductFriendlyNames",
                 "Plugins/DiscountRulesTargetPrice/LoadProductFriendlyNames",
                 new { controller = "DiscountRulesTargetPrice", action = "LoadProductFriendlyNames" },
                 new[] { "Nop.Plugin.DiscountRules.TargetPrice.Controllers" }
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
