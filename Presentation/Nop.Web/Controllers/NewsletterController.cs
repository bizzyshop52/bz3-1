using System;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Factories;
using Nop.Web.Framework;

namespace Nop.Web.Controllers
{
    public partial class NewsletterController : BasePublicController
    {
        private readonly INewsletterModelFactory _newsletterModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IStoreContext _storeContext;
        private readonly IEmailSender _emailSender;

        private readonly CustomerSettings _customerSettings;

        public NewsletterController(INewsletterModelFactory newsletterModelFactory,
            ILocalizationService localizationService,
            IWorkContext workContext,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IWorkflowMessageService workflowMessageService,
            IStoreContext storeContext,
            CustomerSettings customerSettings,
            IEmailSender emailSender)
        {
            this._newsletterModelFactory = newsletterModelFactory;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._workflowMessageService = workflowMessageService;
            this._storeContext = storeContext;
            this._customerSettings = customerSettings;
            this._emailSender = emailSender;
        }

        [ChildActionOnly]
        public virtual ActionResult NewsletterBox()
        {
            if (_customerSettings.HideNewsletterBlock)
                return Content("");

            var model = _newsletterModelFactory.PrepareNewsletterBoxModel();
            return PartialView(model);
        }

        //available even when a store is closed
        [StoreClosed(true)]
        [HttpPost]
        [ValidateInput(false)]
        public virtual ActionResult SubscribeNewsletter(string phone = "", string name = "", string subject = "", string email = "", bool subscribe = true)
        {
            string result;
            bool success = false;

            if (!CommonHelper.IsValidIsraelPhone(phone))
            {
                result = _localizationService.GetResource("Newsletter.phone.Wrong");
            }
            else
            {
                if (string.IsNullOrEmpty(email))
                {
                    //email = email.Trim();
                }
                phone = phone.Trim();



                NewsLetterSubscription subscription = new NewsLetterSubscription
                    {
                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                        Email = email,
                        phone= phone,
                        name = name,
                        subject = subject,
                        Active = true,
                        StoreId = _storeContext.CurrentStore.Id,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    _newsLetterSubscriptionService.InsertNewsLetterSubscription(subscription);

                  
                //_emailSender.SendEmail()
                    _workflowMessageService.SendNewsLetterSubscriptionMsgForAdmin(subscription, _workContext.WorkingLanguage.Id);
                _emailSender.SendSmS(phone, _storeContext.CurrentStore.SmsLeadMsg, _storeContext.CurrentStore.SmsUserName, _storeContext.CurrentStore.SmsPassword, "", _storeContext.CurrentStore.SmsSender);
                result = _localizationService.GetResource("Newsletter.SubscribeSmsSent");
              
                success = true;
            }

            return Json(new
            {
                Success = success,
                Result = result,
            });
        }

        //available even when a store is closed
        [StoreClosed(true)]
        public virtual ActionResult SubscriptionActivation(Guid token, bool active)
        {
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByGuid(token);
            if (subscription == null)
                return RedirectToRoute("HomePage");

            if (active)
            {
                subscription.Active = true;
                _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
            }
            else
                _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);

            var model = _newsletterModelFactory.PrepareSubscriptionActivationModel(active);
            return View(model);
        }
    }
}
