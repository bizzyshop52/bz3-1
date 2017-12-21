using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.CardCom.Models;
using Nop.Plugin.Payments.CardCom.Services;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Nop.Plugin.Payments.CardCom.Controllers
{
    public class PaymentCardComController : BasePaymentController
    {
        #region Fields
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly PaymentSettings _paymentSettings;
        private readonly CardComPaymentSettings _cardComPaymentSettings;
        private readonly ICardComService _cardComService;
        #endregion

        #region Ctor
        public PaymentCardComController(IWorkContext workContext,
            IStoreService storeService, 
            ISettingService settingService, 
            IPaymentService paymentService, 
            IOrderService orderService, 
            IOrderProcessingService orderProcessingService, 
            IStoreContext storeContext,
            ILogger logger, 
            IWebHelper webHelper,
            PaymentSettings paymentSettings,
            CardComPaymentSettings cardComPaymentSettings,
            ICardComService cardComService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._storeContext = storeContext;
            this._logger = logger;
            this._webHelper = webHelper;
            this._paymentSettings = paymentSettings;
            this._cardComPaymentSettings = cardComPaymentSettings;
            this._cardComService = cardComService;
        }
        #endregion

        #region Views
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var cardComPaymentSettings = _settingService.LoadSetting<CardComPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.Operation = cardComPaymentSettings.Operation;
            model.TerminalNumber = cardComPaymentSettings.TerminalNumber;
            model.UserName = cardComPaymentSettings.UserName;
            model.AddReturnButtonToSecurePage = cardComPaymentSettings.AddReturnButtonToSecurePage;
            model.AdditionalFee = cardComPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = cardComPaymentSettings.AdditionalFeePercentage;
            model.CreateToken = cardComPaymentSettings.CreateToken;
            model.CreateInvoice = cardComPaymentSettings.CreateInvoice;
            model.MaxPayments = cardComPaymentSettings.MaxPayments;
            model.MinPayments = cardComPaymentSettings.MinPayments;
            model.UseIframe = cardComPaymentSettings.UseIframe;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.Operation_OverrideForStore = _settingService.SettingExists(cardComPaymentSettings, x => x.Operation, storeScope);
                model.TerminalNumber_OverrideForStore = _settingService.SettingExists(cardComPaymentSettings, x => x.TerminalNumber, storeScope);
                model.UserName_OverrideForStore = _settingService.SettingExists(cardComPaymentSettings, x => x.UserName, storeScope);
                model.AddReturnButtonToSecurePage_OverrideForStore = _settingService.SettingExists(cardComPaymentSettings, x => x.AddReturnButtonToSecurePage, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(cardComPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(cardComPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.CreateToken_OverrideForStore = _settingService.SettingExists(cardComPaymentSettings, x => x.CreateToken, storeScope);
                model.CreateInvoice_OverrideForStore = _settingService.SettingExists(cardComPaymentSettings, x => x.CreateInvoice, storeScope);
                model.MaxPayments_OverrideForStore = _settingService.SettingExists(cardComPaymentSettings, x => x.MaxPayments, storeScope);
                model.MinPayments_OverrideForStore = _settingService.SettingExists(cardComPaymentSettings, x => x.MinPayments, storeScope);
                model.UseIframe_OverrideForStore = _settingService.SettingExists(cardComPaymentSettings, x => x.UseIframe, storeScope);
            }

            return View("~/Plugins/Payments.CardCom/Views/PaymentCardCom/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var cardComPaymentSettings = _settingService.LoadSetting<CardComPaymentSettings>(storeScope);

            //save settings
            cardComPaymentSettings.Operation = model.Operation;
            cardComPaymentSettings.TerminalNumber = model.TerminalNumber;
            cardComPaymentSettings.UserName = model.UserName;
            cardComPaymentSettings.AddReturnButtonToSecurePage = model.AddReturnButtonToSecurePage;
            cardComPaymentSettings.AdditionalFee = model.AdditionalFee;
            cardComPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            cardComPaymentSettings.CreateToken = model.CreateToken;
            cardComPaymentSettings.CreateInvoice = model.CreateInvoice;
            cardComPaymentSettings.MaxPayments = model.MaxPayments;
            cardComPaymentSettings.MinPayments = model.MinPayments;
            cardComPaymentSettings.UseIframe = model.UseIframe;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.Operation_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(cardComPaymentSettings, x => x.Operation, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(cardComPaymentSettings, x => x.Operation, storeScope);

            if (model.TerminalNumber_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(cardComPaymentSettings, x => x.TerminalNumber, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(cardComPaymentSettings, x => x.TerminalNumber, storeScope);

            if (model.UserName_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(cardComPaymentSettings, x => x.UserName, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(cardComPaymentSettings, x => x.UserName, storeScope);

            if (model.AddReturnButtonToSecurePage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(cardComPaymentSettings, x => x.AddReturnButtonToSecurePage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(cardComPaymentSettings, x => x.AddReturnButtonToSecurePage, storeScope);
            
            if (model.AdditionalFee_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(cardComPaymentSettings, x => x.AdditionalFee, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(cardComPaymentSettings, x => x.AdditionalFee, storeScope);

            if (model.AdditionalFeePercentage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(cardComPaymentSettings, x => x.AdditionalFeePercentage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(cardComPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            if (model.CreateToken_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(cardComPaymentSettings, x => x.CreateToken, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(cardComPaymentSettings, x => x.CreateToken, storeScope);

            if (model.CreateInvoice_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(cardComPaymentSettings, x => x.CreateInvoice, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(cardComPaymentSettings, x => x.CreateInvoice, storeScope);

            if (model.MaxPayments_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(cardComPaymentSettings, x => x.MaxPayments, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(cardComPaymentSettings, x => x.MaxPayments, storeScope);

            if (model.MinPayments_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(cardComPaymentSettings, x => x.MinPayments, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(cardComPaymentSettings, x => x.MinPayments, storeScope);

            if (model.UseIframe_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(cardComPaymentSettings, x => x.UseIframe, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(cardComPaymentSettings, x => x.UseIframe, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.CardCom/Views/PaymentCardCom/PaymentInfo.cshtml");
        }

        public ActionResult PaymentIFrame(string url)
        {
            var model = new PaymentIFrameModel()
            {
                IFrameUrl = url,
            };
            return View("~/Plugins/Payments.CardCom/Views/PaymentCardCom/PaymentIFrame.cshtml", model);
        }
        public ActionResult Success(int orderId)
        {
            var model = new PaymentIFrameModel()
            {
                OrderCompletedUrl = _webHelper.GetStoreLocation(false) + "checkout/completed/" + orderId,
            };
            return View("~/Plugins/Payments.CardCom/Views/PaymentCardCom/Success.cshtml", model);
        }
        public ActionResult Failed( int orderId=0 ,string errorMessage="", bool isIframe = false)
        {
            if (errorMessage == "\"Deal Response = \"");
            {
                var model2 = new PaymentIFrameModel()
                {
                    OrderCompletedUrl = _webHelper.GetStoreLocation(false) + "checkout/completed/" + orderId,
                };
                return View("~/Plugins/Payments.CardCom/Views/PaymentCardCom/Success.cshtml", model2);
            }
             var model = new PaymentIFrameModel()
            {
                ErrorMessage = errorMessage,
                IsIframe = isIframe,
                OrderId = orderId,
            };
            return View("~/Plugins/Payments.CardCom/Views/PaymentCardCom/Failed.cshtml", model);
        }
        #endregion

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        public ActionResult CancelOrder(FormCollection form)
        {
            var order = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                    customerId: _workContext.CurrentCustomer.Id, pageSize: 1).FirstOrDefault();
            if (order != null)
            {
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [ValidateInput(false)]
        public ActionResult SuccessHandler(string responseCode = "", string description = "", string lowProfileCode="")
        {
            // create vars for post :
            Dictionary<string, string> vars = new Dictionary<string, string>();
            vars["Operation"] = _cardComPaymentSettings.Operation;
            vars["terminalnumber"] = _cardComPaymentSettings.TerminalNumber;
            vars["username"] = _cardComPaymentSettings.UserName;
            vars["lowprofilecode"] = lowProfileCode;

            string originalResponse = _cardComService.PostDic(vars, "https://secure.cardcom.co.il/Interface/BillGoldGetLowProfileIndicator.aspx");
            var parseResponse = new NameValueCollection(System.Web.HttpUtility.ParseQueryString(originalResponse));
            int orderId = 0;
            if (parseResponse["ResponseCode"] == "0") // request was ok.
            {
                string orderNumber = parseResponse["ReturnValue"];
                Guid orderNumberGuid = Guid.Empty;
                try
                {
                    orderNumberGuid = new Guid(orderNumber);
                }
                catch { }
                Order order = _orderService.GetOrderByGuid(orderNumberGuid);
                orderId = order.Id;
                if (order != null)
                {
                    var items = parseResponse.AllKeys.SelectMany(parseResponse.GetValues, (k, v) => new { key = k, value = v });
                    var sb = new StringBuilder();
                    sb.AppendLine("CardCom:");
                    foreach (var item in items)
                    {
                        sb.AppendLine(String.Format("{0} {1}", item.key, item.value));
                    }
                    if (parseResponse["DealResponse"] == "0")  // Billing Only was OK
                    {
                        sb.AppendLine("Payment: Success");
                        //mark order as paid
                        if (_orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            order.AuthorizationTransactionId = parseResponse["InternalDealNumber"];
                            _orderService.UpdateOrder(order);

                            _orderProcessingService.MarkOrderAsPaid(order);
                        }
                    
                    }
                    else // credit card faild. 
                    {
                        sb.AppendLine("Payment: Failed");
                    }
                    //order note
                    order.OrderNotes.Add(new OrderNote()
                    {
                        Note = sb.ToString(),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
            }
            else // Error In development 
            {
                _logger.Error(String.Format("CardCom. Error responseCode = {0}", parseResponse["ResponseCode"]));
            }
            

            //redirect
            //Fail
            if (parseResponse["DealResponse"] != "0")
            {
                    return RedirectToRoute("Plugin.Payments.CardCom.Failed", new {orderId = orderId, errorMessage = String.Format("Deal Response = {0} ", parseResponse["DealResponse"]), isIframe = _cardComPaymentSettings.UseIframe });
            }
            //Success
            if (_cardComPaymentSettings.UseIframe)
                return RedirectToRoute("Plugin.Payments.CardCom.Success", new { orderId = orderId });
            else
                return RedirectToRoute("CheckoutCompleted", new { orderId = orderId });
        }
    }
}