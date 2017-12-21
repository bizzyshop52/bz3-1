using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.CardCom.Controllers;
using Nop.Plugin.Payments.CardCom.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;

namespace Nop.Plugin.Payments.CardCom
{
    /// <summary>
    /// CardCom payment processor
    /// </summary>
    public class CardComPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields
        private readonly IWorkContext _workContext;
        private readonly CardComPaymentSettings _cardComPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ITaxService _taxService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly HttpContextBase _httpContext;
        private readonly ICardComService _cardComService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        #endregion

        #region Ctor

        public CardComPaymentProcessor(IWorkContext workContext, CardComPaymentSettings cardComPaymentSettings,
            ISettingService settingService, ICurrencyService currencyService,
            CurrencySettings currencySettings, IWebHelper webHelper,
            ICheckoutAttributeParser checkoutAttributeParser, ITaxService taxService,
            IOrderTotalCalculationService orderTotalCalculationService, HttpContextBase httpContext,
            ICardComService cardComService, IGenericAttributeService genericAttributeService,
             ILocalizationService localizationService, ILogger logger)
        {
            this._workContext = workContext;
            this._cardComPaymentSettings = cardComPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._taxService = taxService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._httpContext = httpContext;
            this._cardComService = cardComService;
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._logger = logger;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var parseResponse = _cardComService.GetLowProfileCode(postProcessPaymentRequest);
            if (parseResponse["ResponseCode"] == "0") // request was ok !
            {
                // get LPC 
                string LPC = parseResponse["LowProfileCode"];
                // Create Billing URL 
                string URL = string.Format("https://secure.cardcom.co.il/external/LowProfileClearing/{0}.aspx?lowprofilecode={1}", _cardComPaymentSettings.TerminalNumber, LPC);
                if (_cardComPaymentSettings.UseIframe)
                {
                    _httpContext.Response.Redirect(_webHelper.GetStoreLocation(false) + "Plugins/PaymentCardCom/PaymentIFrame?url=" + URL);
                }
                else
                {
                    _httpContext.Response.Redirect(URL);
                }

            }
            else // Error In development 
            {
                var errorMessage = String.Format("CardCom Error: Order = {0}, responseCode = {1}, description = {2}", postProcessPaymentRequest.Order.Id, parseResponse["ResponseCode"], parseResponse["Description"]);
                _logger.Error(errorMessage);
                _httpContext.Response.Redirect(_webHelper.GetStoreLocation(false) + "Plugins/PaymentCardCom/Failed?errormessage=" + errorMessage);
            }
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _cardComPaymentSettings.AdditionalFee, _cardComPaymentSettings.AdditionalFeePercentage);
            return result;
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentCardCom";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.CardCom.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentCardCom";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.CardCom.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentCardComController);
        }

        public override void Install()
        {
            //settings
            var settings = new CardComPaymentSettings()
            {
                TerminalNumber = "1000",
                UserName = "yael29",
                CreateToken = false,
                CreateInvoice = false,
                MinPayments = 1,
                MaxPayments = 12,
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.RedirectionTip", "You will be redirected to CardCom site to complete the order.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.Operation", "Terminal number");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.Operation.Hint", "Terminal number");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.TerminalNumber", "Terminal number");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.TerminalNumber.Hint", "Terminal number");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.UserName", "User name");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.UserName.Hint", "Specify your Username.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.cardCom.Fields.AddReturnButtonToSecurePage", "Add return button to secure page");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.cardCom.Fields.AddReturnButtonToSecurePage.Hint", "Add return button to secure page");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.CreateToken", "Create Token");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.CreateToken.Hint", "Create Token");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.CreateInvoice", "Create Invoice");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.CreateInvoice.Hint", "Create Invoice");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.MaxPayments", "Max payments");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.MaxPayments.Hint", "Max payments");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.MinPayments", "Min payments");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.MinPayments.Hint", "Min payments");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.UseIframe", "Use IFRAME");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Fields.UseIframe.Hint", "Host redirect in IFRAME");

            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.ShippingFee", "Shipping fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.PaymentMethodFee", "Payment method fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.SalesTax", "Sales tax");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Discount", "Discount");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.CardCom.Failed", "An error occurred while processing your transaction<br /> Please try again later");


            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<CardComPaymentSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.Operation");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.Operation.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.TerminalNumber");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.TerminalNumber.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.UserName");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.UserName.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.AddReturnButtonToSecurePage");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.AddReturnButtonToSecurePage.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.AdditionalFeePercentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.CreateToken");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.CreateToken.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.CreateInvoice");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.CreateInvoice.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.MaxPayments");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.MaxPayments.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.MinPayments");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.MinPayments.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.UseIframe");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Fields.UseIframe.Hint");

            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.ShippingFee");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.PaymentMethodFee");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.SalesTax");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Discount");
            this.DeletePluginLocaleResource("Plugins.Payments.CardCom.Failed");


            base.Uninstall();
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get
            {
                return true;
            }
        }

        public string PaymentMethodDescription
        {
            get
            {
                return "";
            }
        }

        #endregion
    }
}
