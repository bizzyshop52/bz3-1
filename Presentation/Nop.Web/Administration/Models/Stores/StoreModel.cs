using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Stores
{
    [Validator(typeof(StoreValidator))]
    public partial class StoreModel : BaseNopEntityModel, ILocalizedModel<StoreLocalizedModel>
    {
        public StoreModel()
        {
            Locales = new List<StoreLocalizedModel>();
            AvailableLanguages = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.Url")]
        [AllowHtml]
        public string Url { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.SslEnabled")]
        public virtual bool SslEnabled { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.SecureUrl")]
        [AllowHtml]
        public virtual string SecureUrl { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.Hosts")]
        [AllowHtml]
        public string Hosts { get; set; }

        //default language
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.DefaultLanguage")]
        [AllowHtml]
        public int DefaultLanguageId { get; set; }
        public IList<SelectListItem> AvailableLanguages { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyName")]
        [AllowHtml]
        public string CompanyName { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyAddress")]
        [AllowHtml]
        public string CompanyAddress { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyPhoneNumber")]
        [AllowHtml]
        public string CompanyPhoneNumber { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyVat")]
        [AllowHtml]
        public string CompanyVat { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.CompanyMail")]
        [AllowHtml]
        public string CompanyMail { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.newLeadMsg")]
        [AllowHtml]
        public string newLeadMsg { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.newLeadSubject")]
        [AllowHtml]
        public string newLeadSubject { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.SmsUserName")]
        [AllowHtml]
        public string SmsUserName { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.SmsPassword ")]
        [AllowHtml]
        public string SmsPassword { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.SmsLeadMsg")]
        [AllowHtml]
        public string SmsLeadMsg { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.SmsLeadMsg")]
        [AllowHtml]
        public string CompanyMailLeads { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.SmsSender")]
        [AllowHtml]
        public string SmsSender { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.zapEnabled")]
        public bool zapEnabled { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.SendSmsOnOrderToOwner")]
        public bool SendSmsOnOrderToOwner { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.OwnerPhoneNumber")]
        public string OwnerPhoneNumber { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.SmsPaidOrderMsg")]
        public string SmsPaidOrderMsg { get; set; }

        public IList<StoreLocalizedModel> Locales { get; set; }
    }

    public partial class StoreLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }
}