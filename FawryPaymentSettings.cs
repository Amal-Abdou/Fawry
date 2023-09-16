using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Fawry
{
    public class FawryPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string MerchantCode { get; set; }

        public int PaymentExpiry { get; set; }

        public string SecurityKey { get; set; }

    }
}
