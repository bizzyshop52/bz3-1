using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Newsletter
{
    public partial class NewsletterBoxModel : BaseNopModel
    {
        public string NewsletterEmail { get; set; }
        public bool AllowToUnsubscribe { get; set; }
        public string phone { get; set; }

        public string name { get; set; }

        public string subject { get; set; }
    }
}