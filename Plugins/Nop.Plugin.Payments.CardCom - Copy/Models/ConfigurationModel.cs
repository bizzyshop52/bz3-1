using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.CardCom.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.CardCom.Fields.Operation")]
        public string Operation { get; set; }
        public bool Operation_OverrideForStore { get; set; }



        [NopResourceDisplayName("Plugins.Payments.CardCom.Fields.TerminalNumber")]
        public string TerminalNumber { get; set; }
        public bool TerminalNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.CardCom.Fields.UserName")]
        public string UserName { get; set; }
        public bool UserName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.CardCom.Fields.AddReturnButtonToSecurePage")]
        public bool AddReturnButtonToSecurePage { get; set; }
        public bool AddReturnButtonToSecurePage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.CardCom.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.CardCom.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.CardCom.Fields.CreateToken")]
        public bool CreateToken { get; set; }
        public bool CreateToken_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.CardCom.Fields.CreateInvoice")]
        public bool CreateInvoice { get; set; }
        public bool CreateInvoice_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.CardCom.Fields.MaxPayments")]
        public int MaxPayments { get; set; }
        public bool MaxPayments_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.CardCom.Fields.MinPayments")]
        public int MinPayments { get; set; }
        public bool MinPayments_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.CardCom.Fields.UseIframe")]
        public bool UseIframe { get; set; }
        public bool UseIframe_OverrideForStore { get; set; }
    }
}