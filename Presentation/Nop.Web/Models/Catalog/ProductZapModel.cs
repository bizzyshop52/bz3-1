using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Catalog
{
    public class ProductZapModel : BaseNopEntityModel
    {
        public string PRODUCT_URL { get; set; }
        public string PRODUCT_NAME { get; set; }
        public string MODEL { get; set; }
        public string DETAILS { get; set; }
        public string CURRENCY { get; set; }
        public string PRICE { get; set; }
        public string SHIPMENT_COST { get; set; }
        public string DELIVERY_TIME { get; set; }
        public string MANUFACTURER { get; set; }
        public string WARRANTY { get; set; }
        public string IMAGE { get; set; }
        public string TAX { get; set; }



    }
}