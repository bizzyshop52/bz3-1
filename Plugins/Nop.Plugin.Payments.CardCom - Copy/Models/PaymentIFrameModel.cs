using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.CardCom.Models
{
    public class PaymentIFrameModel : BaseNopModel
    {
        public string IFrameUrl { get; set; }
        public string OrderCompletedUrl { get; set; }

        public string ErrorMessage { get; set; }
        public bool IsIframe { get; set; }

        public int OrderId { get; set; }
    }
}