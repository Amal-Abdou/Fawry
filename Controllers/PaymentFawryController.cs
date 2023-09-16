using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Fawry.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.Fawry.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class PaymentFawryController : BasePaymentController
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        #endregion

        #region Ctor

        public PaymentFawryController(IGenericAttributeService genericAttributeService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ILogger logger,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IWorkContext workContext,
            ShoppingCartSettings shoppingCartSettings)
        {
            _genericAttributeService = genericAttributeService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
            _shoppingCartSettings = shoppingCartSettings;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var fawryPaymentSettings = await _settingService.LoadSettingAsync<FawryPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = fawryPaymentSettings.UseSandbox,
                MerchantCode = fawryPaymentSettings.MerchantCode,
                PaymentExpiry = fawryPaymentSettings.PaymentExpiry,
                SecurityKey = fawryPaymentSettings.SecurityKey,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope <= 0)
                return View("~/Plugins/Payments.Fawry/Views/Configure.cshtml", model);

            model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(fawryPaymentSettings, x => x.UseSandbox, storeScope);
            model.MerchantCode_OverrideForStore = await _settingService.SettingExistsAsync(fawryPaymentSettings, x => x.MerchantCode, storeScope);
            model.PaymentExpiry_OverrideForStore = await _settingService.SettingExistsAsync(fawryPaymentSettings, x => x.PaymentExpiry, storeScope);
            model.SecurityKey_OverrideForStore = await _settingService.SettingExistsAsync(fawryPaymentSettings, x => x.SecurityKey, storeScope);
            return View("~/Plugins/Payments.Fawry/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]        
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var fawryPaymentSettings = await _settingService.LoadSettingAsync<FawryPaymentSettings>(storeScope);

            fawryPaymentSettings.UseSandbox = model.UseSandbox;
            fawryPaymentSettings.MerchantCode = model.MerchantCode;
            fawryPaymentSettings.PaymentExpiry = model.PaymentExpiry;
            fawryPaymentSettings.SecurityKey = model.SecurityKey;

            await _settingService.SaveSettingOverridablePerStoreAsync(fawryPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(fawryPaymentSettings, x => x.MerchantCode, model.MerchantCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(fawryPaymentSettings, x => x.PaymentExpiry, model.PaymentExpiry_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(fawryPaymentSettings, x => x.SecurityKey, model.SecurityKey_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        //action displaying notification (warning) to a store owner about inaccurate PayPal rounding
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> RoundingWarning(bool passProductNamesAndTotals)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //prices and total aren't rounded, so display warning
            if (passProductNamesAndTotals && !_shoppingCartSettings.RoundPricesDuringCalculation)
                return Json(new { Result = await _localizationService.GetResourceAsync("Plugins.Payments.Fawry.RoundingWarning") });

            return Json(new { Result = string.Empty });
        }

        public async Task<IActionResult> PDTHandler(
                 string type,
                 string referenceNumber,
                 string merchantRefNumber,
                 string orderAmount,
                 string paymentAmount,
                 string fawryFees,
                 string orderStatus,
                 string paymentMethod,
                 string expirationTime,
                 string customerMobile,
                 string customerMail,
                 string customerProfileId,
                 string signature,
                 string taxes,
                 string statusCode,
                 string statusDescription,
                 string basketPayment
                 )
        {
            var order = await _orderService.GetOrderByIdAsync(int.Parse(merchantRefNumber));
            if (order != null && !string.IsNullOrEmpty(orderStatus) && orderStatus == "PAID")
            {
                await _orderProcessingService.MarkOrderAsPaidAsync(order);
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
            else if (order != null && !string.IsNullOrEmpty(orderStatus) && (orderStatus == "EXPIRED" || orderStatus == "CANCELLED"))
            {
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = "The order cancelled because payment failed, " + statusDescription,
                    DisplayToCustomer = true,
                    CreatedOnUtc = DateTime.UtcNow
                });
                order.OrderStatusId = (int)OrderStatus.Cancelled;
                await _orderService.UpdateOrderAsync(order);
                return RedirectToAction("Index", "Home", new { area = string.Empty });

            }
            else if (order != null && !string.IsNullOrEmpty(orderStatus) && orderStatus == "UNPAID")
            {
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
            return RedirectToAction("Index", "Home", new { area = string.Empty });
        }


        #endregion
    }
}