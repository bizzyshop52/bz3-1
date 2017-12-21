using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.CardCom
{
    public class CardComPaymentSettings : ISettings
    {

        public string Operation { get; set; }
        public string TerminalNumber { get; set; }
        public string UserName { get; set; }

        public bool AddReturnButtonToSecurePage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
        /// <summary>
        /// Additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        public bool CreateToken { get; set; }

        public bool CreateInvoice { get; set; }

        public int MaxPayments { get; set; }
        public int MinPayments { get; set; }

        public bool UseIframe { get; set; }
    }
}
