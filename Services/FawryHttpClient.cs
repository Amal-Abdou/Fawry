using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Nop.Core;

namespace Nop.Plugin.Payments.Fawry.Services
{

    public partial class FawryHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly FawryPaymentSettings _fawryPaymentSettings;

        #endregion

        #region Ctor

        public FawryHttpClient(HttpClient client,
            FawryPaymentSettings fawryPaymentSettings)
        {
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, $"nopCommerce-{NopVersion.CURRENT_VERSION}");
            
            _httpClient = client;
            _fawryPaymentSettings = fawryPaymentSettings;
        }

        #endregion

        #region Methods

        public async Task<string> GetPdtDetailsAsync(string tx)
        {
            var url = _fawryPaymentSettings.UseSandbox ?
                "https://migs-mtf.mastercard.com.au/vpcpay" :
                "https://migs-mtf.mastercard.com.au/vpcpay";
            var requestContent = new StringContent($"cmd=_notify-synch&at=&tx={tx}",
                Encoding.UTF8, MimeTypes.ApplicationXWwwFormUrlencoded);
            var response = await _httpClient.PostAsync(url, requestContent);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> VerifyIpnAsync(string formString)
        {
            var url = _fawryPaymentSettings.UseSandbox ?
                "https://migs-mtf.mastercard.com.au/vpcpay" :
                "https://migs-mtf.mastercard.com.au/vpcpay";
            var requestContent = new StringContent($"cmd=_notify-validate&{formString}",
                Encoding.UTF8, MimeTypes.ApplicationXWwwFormUrlencoded);
            var response = await _httpClient.PostAsync(url, requestContent);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        #endregion
    }
}