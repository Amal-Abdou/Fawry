using System;
using FluentValidation;
using Nop.Plugin.Payments.Fawry.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.Fawry.Validators
{
    public partial class PaymentInfoValidator : BaseNopValidator<PaymentInfoModel>
    {
        public PaymentInfoValidator(ILocalizationService localizationService)
        {

            RuleFor(x => x.CreditCardType).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Payment.CreditCardType.Required"));

        }
    }
}