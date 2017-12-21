using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Nop.Plugin.Payments.CardCom.Services
{
    /// <summary>
    /// cardCom service
    /// </summary>
    public partial class CardComService : ICardComService
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
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor
        public CardComService(IWorkContext workContext, CardComPaymentSettings cardComPaymentSettings,
            ISettingService settingService, ICurrencyService currencyService,
            CurrencySettings currencySettings, IWebHelper webHelper,
            ICheckoutAttributeParser checkoutAttributeParser, ITaxService taxService,
            IOrderTotalCalculationService orderTotalCalculationService, HttpContextBase httpContext,
            IGenericAttributeService genericAttributeService,
             ILocalizationService localizationService,
            IEventPublisher eventPublisher)
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
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        private void AddInvoiceVars(Dictionary<string, string> vars, PostProcessPaymentRequest postProcessPaymentRequest)
        {
            // article for invoice vars:  http://kb.cardcom.co.il/article/AA-00244/0
            var customer = _workContext.CurrentCustomer;
            var firstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName);
            var lastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName);

            // customer info :
            vars["InvoiceHead.CustName"] = firstName + " " + lastName; // customer name
            vars["InvoiceHead.SendByEmail"] = "true"; // will the invoice be send by email to the customer
            vars["InvoiceHead.Language"] = GetLanguageNameInvoice(_workContext.WorkingLanguage.UniqueSeoCode);
            vars["InvoiceHead.CoinID"] = GetCurrencyIdByCurrencyName(_currencyService.GetCurrencyById(_workContext.WorkingCurrency.Id).CurrencyCode).ToString();
            vars["InvoiceHead.Email"] = postProcessPaymentRequest.Order.BillingAddress.Email; // value that will be return and save in CardCom system
            vars["InvoiceHead.CustAddresLine1"] = postProcessPaymentRequest.Order.BillingAddress.Address1;
            vars["InvoiceHead.CustAddresLine2"] = postProcessPaymentRequest.Order.BillingAddress.Address2;
            vars["InvoiceHead.CustCity"] = postProcessPaymentRequest.Order.BillingAddress.City;
            vars["InvoiceHead.CustLinePH"] = postProcessPaymentRequest.Order.BillingAddress.PhoneNumber;

            //get the items in the cart
            decimal cartTotal = decimal.Zero;
            var cartItems = postProcessPaymentRequest.Order.OrderItems;
            int itemsCount = 1;

            foreach (var item in cartItems) //50 chars limit
            {
                var unitPriceExclTax = item.UnitPriceExclTax;
                var priceExclTax = item.PriceExclTax;
                //round
                var unitPriceExclTaxRounded = Math.Round(unitPriceExclTax, 2);
                if (item.Product.WeightProduct)
                {
                    if (item.Product.ByUnit.HasValue && item.Product.ByUnit.Value == true)
                    {
                        vars["InvoiceLines" + itemsCount.ToString() + ".Quantity"] = HttpUtility.UrlEncode((item.Quantity).ToString() + " " + "יחידות");
                        vars["InvoiceLines" + itemsCount.ToString() + ".Price"] = Math.Round(item.Product.UnitPrice.Value).ToString("0.00", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        vars["InvoiceLines" + itemsCount.ToString() + ".Quantity"] = HttpUtility.UrlEncode((item.Quantity * item.Product.BaseWeight.Value).ToString() + " " + item.Product.WeightUnit);
                        vars["InvoiceLines" + itemsCount.ToString() + ".Price"] = unitPriceExclTaxRounded.ToString("0.00", CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    vars["InvoiceLines" + itemsCount.ToString() + ".Quantity"] = HttpUtility.UrlEncode(item.Quantity.ToString());
                    vars["InvoiceLines" + itemsCount.ToString() + ".Price"] = unitPriceExclTaxRounded.ToString("0.00", CultureInfo.InvariantCulture);
                }

                vars["InvoiceLines" + itemsCount.ToString() + ".Description"] = HttpUtility.UrlEncode(item.Product.Name);
                vars["InvoiceLines" + itemsCount.ToString() + ".ProductID"] = (String.IsNullOrWhiteSpace(item.Product.Sku)) ? item.Product.Sku : "";

                itemsCount++;
                cartTotal += priceExclTax;
            }
            //the checkout attributes that have a dollar value and send them to CardCom as items to be paid for
            var caValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(postProcessPaymentRequest.Order.CheckoutAttributesXml);
            foreach (var val in caValues)
            {
                var attPrice = _taxService.GetCheckoutAttributePrice(val, false, postProcessPaymentRequest.Order.Customer);
                //round
                var attPriceRounded = Math.Round(attPrice, 2);
                if (attPrice > decimal.Zero) //if it has a price
                {
                    var ca = val.CheckoutAttribute;
                    if (ca != null)
                    {
                        var attName = ca.Name; //set the name
                        vars["InvoiceLines" + itemsCount.ToString() + ".Description"] = HttpUtility.UrlEncode(attName);
                        vars["InvoiceLines" + itemsCount.ToString() + ".Price"] = attPriceRounded.ToString("0.00", CultureInfo.InvariantCulture); //amount
                        vars["InvoiceLines" + itemsCount.ToString() + ".Quantity"] = "1"; //quantity
                        itemsCount++;
                        cartTotal += attPrice;
                    }
                }
            }
            //order totals

            //shipping
            var orderShippingExclTax = postProcessPaymentRequest.Order.OrderShippingExclTax;
            var orderShippingExclTaxRounded = Math.Round(orderShippingExclTax, 2);
            if (orderShippingExclTax > decimal.Zero)
            {
                vars["InvoiceLines" + itemsCount.ToString() + ".Description"] = _localizationService.GetResource("Plugins.Payments.CardCom.ShippingFee");
                vars["InvoiceLines" + itemsCount.ToString() + ".Price"] = orderShippingExclTaxRounded.ToString("0.00", CultureInfo.InvariantCulture);
                vars["InvoiceLines" + itemsCount.ToString() + ".Quantity"] = "1"; //quantity
                itemsCount++;
                cartTotal += orderShippingExclTax;
            }

            //payment method additional fee
            var paymentMethodAdditionalFeeExclTax = postProcessPaymentRequest.Order.PaymentMethodAdditionalFeeExclTax;
            var paymentMethodAdditionalFeeExclTaxRounded = Math.Round(paymentMethodAdditionalFeeExclTax, 2);
            if (paymentMethodAdditionalFeeExclTax > decimal.Zero)
            {
                vars["InvoiceLines" + itemsCount.ToString() + ".Description"] = _localizationService.GetResource("Plugins.Payments.CardCom.PaymentMethodFee");
                vars["InvoiceLines" + itemsCount.ToString() + ".Price"] = paymentMethodAdditionalFeeExclTaxRounded.ToString("0.00", CultureInfo.InvariantCulture);
                vars["InvoiceLines" + itemsCount.ToString() + ".Quantity"] = "1"; //quantity
                itemsCount++;
                cartTotal += paymentMethodAdditionalFeeExclTax;
            }
            //tax
            var orderTax = postProcessPaymentRequest.Order.OrderTax;
            var orderTaxRounded = Math.Round(orderTax, 2);
            if (orderTax > decimal.Zero)
            {
                //add tax as item
                vars["InvoiceLines" + itemsCount.ToString() + ".Description"] = _localizationService.GetResource("Plugins.Payments.CardCom.SalesTax"); //name
                vars["InvoiceLines" + itemsCount.ToString() + ".Price"] = orderTaxRounded.ToString("0.00", CultureInfo.InvariantCulture); //amount
                vars["InvoiceLines" + itemsCount.ToString() + ".Quantity"] = "1"; //quantity

                cartTotal += orderTax;
                itemsCount++;
            }

            if (cartTotal > postProcessPaymentRequest.Order.OrderTotal)
            {
                /* Take the difference between what the order total is and what it should be and use that as the "discount".
                 * The difference equals the amount of the gift card and/or reward points used. 
                 */
                decimal discountTotal = cartTotal - postProcessPaymentRequest.Order.OrderTotal;
                discountTotal = Math.Round(discountTotal, 2) * (-1);
                //gift card or rewared point amount applied to cart in nopCommerce - shows in Paypal as "discount"
                vars["InvoiceLines" + itemsCount.ToString() + ".Description"] = _localizationService.GetResource("Plugins.Payments.CardCom.Discount"); //name
                vars["InvoiceLines" + itemsCount.ToString() + ".Price"] = discountTotal.ToString("0.00", CultureInfo.InvariantCulture);
                vars["InvoiceLines" + itemsCount.ToString() + ".Quantity"] = "1"; //quantity
            }

        }

        public NameValueCollection GetLowProfileCode(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var createToken = _cardComPaymentSettings.CreateToken;
            var createInvoice = _cardComPaymentSettings.CreateInvoice;
            var cartItems = postProcessPaymentRequest.Order.OrderItems;
            var laltItem = cartItems.Last();
            string productNames = "";
            foreach (var item in cartItems) //50 chars limit
            {
                productNames += item.Product.Name;
                if (item != laltItem)
                    productNames += ", ";
            }

            Dictionary<string, string> vars = new Dictionary<string, string>();
            string tokenOrCharge = "ChargeInfo";
            if (createToken)
                tokenOrCharge = "TokenToCreate";

            string successReturnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentCardCom/SuccessHandler";
            string failReturnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentCardCom/SuccessHandler";
            string cancelReturnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentCardCom/CancelOrder";
            string notifyReturnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentCardCom/NotifyHandler";

            // common account vars
            vars["Operation"] = _cardComPaymentSettings.Operation;
            vars["terminalnumber"] = _cardComPaymentSettings.TerminalNumber;
            vars["username"] = _cardComPaymentSettings.UserName;
            vars["APILevel"] = "10"; // req
            vars["codepage"] = "65001"; // unicode
            vars["ReturnValue"] = postProcessPaymentRequest.Order.Id.ToString();
            //vars["SuspendedDealID"] = "987";


            // billing info article : http://kb.cardcom.co.il/article/AA-00243/0
            //vars[tokenOrCharge + ".SumToBill"] = Math.Round(postProcessPaymentRequest.Order.OrderTotal, 2).ToString("0.00", CultureInfo.InvariantCulture);
            //vars[tokenOrCharge + ".CoinID"] = GetCurrencyIdByCurrencyName(_currencyService.GetCurrencyById(_workContext.WorkingCurrency.Id).CurrencyCode).ToString();
            //vars[tokenOrCharge + ".Language"] = GetLanguageNameRedirectPage(_workContext.WorkingLanguage.UniqueSeoCode); // page languge he- hebrew , en - english , ru , ar
            //vars[tokenOrCharge + ".ProductName"] = productNames; // Product Name 
            vars["SumToBill"] = Math.Round(postProcessPaymentRequest.Order.OrderTotal, 2).ToString("0.00", CultureInfo.InvariantCulture);
            vars["CoinID"] = GetCurrencyIdByCurrencyName(_currencyService.GetCurrencyById(_workContext.WorkingCurrency.Id).CurrencyCode).ToString();
            vars["Language"] = GetLanguageNameRedirectPage(_workContext.WorkingLanguage.UniqueSeoCode); // page languge he- hebrew , en - english , ru , ar
            vars["ProductName"] = productNames; // Product Name 



            // redirect url
            vars["SuccessRedirectUrl"] = successReturnUrl;//"https://secure.cardcom.co.il/DealWasSuccessful.aspx"; // Success Page
            vars["ErrorRedirectUrl"] = failReturnUrl;  //"https://secure.cardcom.co.il/DealWasUnSuccessful.aspx?customVar=1234"; // Error Page
            // vars[tokenOrCharge + ".IndicatorUrl"] = notifyReturnUrl;//"http://www.yoursite.com/NotifyURL"; // Indicator Url \ Notify URL - optional
            vars["CancelUrl"] = cancelReturnUrl;
            if (_cardComPaymentSettings.AddReturnButtonToSecurePage)
                vars["CancelType"] = "2";

            // Other optinal vars :
            vars["ReturnValue"] = postProcessPaymentRequest.Order.OrderGuid.ToString(); // value that will be return and save in CardCom system

            //payments
            if (createToken)
            {
                vars["TokenChargeInfo.MaxNumOfPayments"] = _cardComPaymentSettings.MaxPayments.ToString(); // max num of payments to show  to the user
                vars["TokenChargeInfo.MinNumOfPayments"] = _cardComPaymentSettings.MinPayments.ToString(); // max num of payments to show  to the user
            }
            else
            {
                //vars["ChargeInfo.MaxNumOfPayments"] = _cardComPaymentSettings.MaxPayments.ToString(); // max num of payments to show  to the user
                //vars["ChargeInfo.MinNumOfPayments"] = _cardComPaymentSettings.MinPayments.ToString(); // max num of payments to show  to the user
                vars["MaxNumOfPayments"] = _cardComPaymentSettings.MaxPayments.ToString(); // max num of payments to show  to the user
                vars["MinNumOfPayments"] = _cardComPaymentSettings.MinPayments.ToString(); // max num of payments to show  to the user
            }

            if (createToken)
            {
                vars["TokenToCreate.Salt"] = _workContext.CurrentCustomer.Id.ToString();
                var exDate = DateTime.Now.AddYears(10).ToString("dd/MM/yy");
                vars["TokenToCreate.DeleteDate"] = exDate;
                vars["TokenToCreate.IsCardApproval"] = "False"; //charge and create token
                vars["TokenToCreate.JValidateType"] = "5";// 2 or 5 
            }

            if (createInvoice) //(IsCreateInvoice)
            {
                // Add Invoice Vars :
                AddInvoiceVars(vars, postProcessPaymentRequest);
            }

            string cardcomUrl = "https://secure.cardcom.co.il/Interface/LowProfile.aspx"; //"https://secure.cardcom.co.il/interface/SuspendedDealActivate.aspx";//"https://secure.cardcom.co.il//Interface/PerformSimpleCharge.aspx";//
            if (createToken)
                cardcomUrl = "https://secure.cardcom.co.il/interface/SuspendedDealActivate.aspx"; //"https://secure.cardcom.co.il//Interface/CreateToken.aspx";

            // Post Parameters to secure.cardcom.co.il server
            string originalResponse = PostDic(vars, cardcomUrl);

            var parseResponse = new NameValueCollection(System.Web.HttpUtility.ParseQueryString(originalResponse));

            return parseResponse;
        }


        /// <summary>
        /// Help Function to send POST data To Servers
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="PostRequestToURL"></param>
        /// <returns></returns>
        public string PostDic(Dictionary<string, string> dic, string PostRequestToURL)
        {
            // Create Vars
            StringBuilder RequstString = new StringBuilder(1024);
            foreach (KeyValuePair<string, string> keyValuePair in dic)
            {
                RequstString.AppendFormat("{0}={1}&", keyValuePair.Key, System.Web.HttpUtility.UrlEncode(keyValuePair.Value, Encoding.UTF8));
            }
            RequstString.Remove(RequstString.Length - 1, 1); // Remove the &

            // Post Information
            WebRequest request = WebRequest.Create(PostRequestToURL);
            request.Method = "POST";
            byte[] byteArray = Encoding.UTF8.GetBytes(RequstString.ToString());
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            using (Stream writeStream = request.GetRequestStream())
            {
                writeStream.Write(byteArray, 0, byteArray.Length);
            }
            using (WebResponse response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }

        }

        private int GetCurrencyIdByCurrencyName(string currencyName)
        {
            switch (currencyName)
            {
                case "ILS":
                    return 1;
                case "USD":
                    return 2;
                case "GBP ":
                    return 826;
                case "EUR ":
                    return 978;
                default:
                    return 1;
            }
        }

        private string GetLanguageNameRedirectPage(string lan)
        {
            switch (lan)
            {
                case "he":
                    return "he";
                case "en":
                    return "en";
                case "ru":
                    return "ru";
                case "ar":
                    return "ar";
                default:
                    return "he";
            }
        }
        private string GetLanguageNameInvoice(string lan)
        {
            switch (lan)
            {
                case "he":
                    return "he";
                case "en":
                    return "en";
                default:
                    return "he";
            }
        }
    }
}
