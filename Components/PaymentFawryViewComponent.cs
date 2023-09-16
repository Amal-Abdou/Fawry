using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Payments.Fawry.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Fawry.Components
{
    [ViewComponent(Name = "PaymentFawry")]
    public class PaymentFawryViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel()
            {
                CreditCardTypes = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Master Card", Value = "mastercard" },
                    new SelectListItem { Text = "Credit Card", Value = "creditcard" },
                }
            };

            return View("~/Plugins/Payments.Fawry/Views/PaymentInfo.cshtml", model);
        }
    }
}

