using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Fawry.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //PDT
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.Fawry.PDTHandler", "Plugins/PaymentFawry/PDTHandler",
                 new { controller = "PaymentFawry", action = "PDTHandler" });

            //Cancel
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.Fawry.CancelOrder", "Plugins/PaymentFawry/CancelOrder",
                 new { controller = "PaymentFawry", action = "CancelOrder" });
        }

        public int Priority => -1;
    }
}