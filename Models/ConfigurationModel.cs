using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.Fawry.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Fawry.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Fawry.Fields.MerchantCode")]
        public string MerchantCode { get; set; }
        public bool MerchantCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Fawry.Fields.PaymentExpiry")]
        public int PaymentExpiry { get; set; }
        public bool PaymentExpiry_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Fawry.Fields.SecurityKey")]
        public string SecurityKey { get; set; }
        public bool SecurityKey_OverrideForStore { get; set; }

    }
}