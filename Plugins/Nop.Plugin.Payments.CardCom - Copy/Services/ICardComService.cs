using Nop.Services.Payments;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Nop.Plugin.Payments.CardCom.Services
{
    public partial interface ICardComService
    {
        NameValueCollection GetLowProfileCode(PostProcessPaymentRequest postProcessPaymentRequest);
        /// <summary>
        /// Help Function to send POST data To Servers
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="PostRequestToURL"></param>
        /// <returns></returns>
        string PostDic(Dictionary<string, string> dic, string PostRequestToURL);

       
    }
}
