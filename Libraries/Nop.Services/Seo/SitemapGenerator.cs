using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using Nop.Core;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Security;
using Nop.Services.Catalog;
using Nop.Services.Topics;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Core.Domain.Media;

namespace Nop.Services.Seo
{
    /// <summary>
    /// Represents a sitemap generator
    /// </summary>
    public partial class SitemapGenerator : ISitemapGenerator
    {
        #region Constants

        private const string DateFormat = @"yyyy-MM-dd";

        /// <summary>
        /// At now each provided sitemap file must have no more than 50000 URLs
        /// </summary>
        private const int maxSitemapUrlNumber = 50000;

        #endregion

        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ITopicService _topicService;
        private readonly IWebHelper _webHelper;
        private readonly CommonSettings _commonSettings;
        private readonly BlogSettings _blogSettings;
        private readonly NewsSettings _newsSettings;
        private readonly ForumSettings _forumSettings;
        private readonly SecuritySettings _securitySettings;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;

        #endregion

        #region Ctor

       public SitemapGenerator(IStoreContext storeContext,
            ICategoryService categoryService,
            IProductService productService,
            IManufacturerService manufacturerService,
            ITopicService topicService,
            IWebHelper webHelper,
            CommonSettings commonSettings,
            BlogSettings blogSettings,
            NewsSettings newsSettings,
            ForumSettings forumSettings,
            SecuritySettings securitySettings,
            IPictureService pictureService,
            MediaSettings mediaSettings)
        {
            this._storeContext = storeContext;
            this._categoryService = categoryService;
            this._productService = productService;
            this._manufacturerService = manufacturerService;
            this._topicService = topicService;
            this._webHelper = webHelper;
            this._commonSettings = commonSettings;
            this._blogSettings = blogSettings;
            this._newsSettings = newsSettings;
            this._forumSettings = forumSettings;
            this._securitySettings = securitySettings;
            this._pictureService = pictureService;
            this._mediaSettings = mediaSettings;
        }

        #endregion

        #region Nested class

        /// <summary>
        /// Represents sitemap URL entry
        /// </summary>
        protected class SitemapUrl
        {
            public SitemapUrl(string location, UpdateFrequency frequency, DateTime updatedOn)
            {
                Location = location;
                UpdateFrequency = frequency;
                UpdatedOn = updatedOn;
            }

            /// <summary>
            /// Gets or sets URL of the page
            /// </summary>
            public string Location { get; set; }

            /// <summary>
            /// Gets or sets a value indicating how frequently the page is likely to change
            /// </summary>
            public UpdateFrequency UpdateFrequency { get; set; }

            /// <summary>
            /// Gets or sets the date of last modification of the file
            /// </summary>
            public DateTime UpdatedOn { get; set; }
        }

        #endregion

        #region Utilities
        public virtual List<CategoryZapModel> PrepareCategoryZapModels(int rootCategoryId,
        bool loadSubCategories = false, IList<Category> allCategories = null)
        {
            var result = new List<CategoryZapModel>();

            //little hack for performance optimization.
            //we know that this method is used to load top and left menu for categories.
            //it'll load all categories anyway.
            //so there's no need to invoke "GetAllCategoriesByParentCategoryId" multiple times (extra SQL commands) to load childs
            //so we load all categories at once
            //if you don't like this implementation if you can uncomment the line below (old behavior) and comment several next lines (before foreach)
            //var categories = _categoryService.GetAllCategoriesByParentCategoryId(rootCategoryId);
            if (allCategories == null)
            {
                //load categories if null passed
                //we implemeneted it this way for performance optimization - recursive iterations (below)
                //this way all categories are loaded only once
                allCategories = _categoryService.GetAllCategoriesByParentCategoryId(rootCategoryId);
            }
            var categories = allCategories.Where(c =>  c.Zap == true).ToList();
            foreach (var category in categories)
            {
                var categoryModel = new CategoryZapModel
                {
                    Id = category.Id,
                    Name = category.GetLocalized(x => x.Name),
                    SeName = category.GetSeName(),
                    Zap = category.Zap
                };

                //number of products in each category
              


                //var subCategories = PrepareCategorySimpleModels(category.Id, loadSubCategories, allCategories);
                //categoryModel.SubCategories.AddRange(subCategories);

                result.Add(categoryModel);
            }


            return result;
        }

        /// <summary>
        /// Get HTTP protocol
        /// </summary>
        /// <returns>Protocol name as string</returns>
        protected virtual string GetHttpProtocol()
        {
            return _securitySettings.ForceSslForAllPages ? "https" : "http";
        }

        /// <summary>
        /// Generate URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <returns>List of URL for the sitemap</returns>
        protected virtual IList<SitemapUrl> GenerateUrls(UrlHelper urlHelper)
        {
            var sitemapUrls = new List<SitemapUrl>();

            //home page
            var homePageUrl = urlHelper.RouteUrl("HomePage", null, GetHttpProtocol());
            sitemapUrls.Add(new SitemapUrl(homePageUrl, UpdateFrequency.Weekly, DateTime.UtcNow));

            //search products
            var productSearchUrl = urlHelper.RouteUrl("ProductSearch", null, GetHttpProtocol());
            sitemapUrls.Add(new SitemapUrl(productSearchUrl, UpdateFrequency.Weekly, DateTime.UtcNow));

            //contact us
            var contactUsUrl = urlHelper.RouteUrl("ContactUs", null, GetHttpProtocol());
            sitemapUrls.Add(new SitemapUrl(contactUsUrl, UpdateFrequency.Weekly, DateTime.UtcNow));

            //news
            if (_newsSettings.Enabled)
            {
                var url = urlHelper.RouteUrl("NewsArchive", null, GetHttpProtocol());
                sitemapUrls.Add(new SitemapUrl(url, UpdateFrequency.Weekly, DateTime.UtcNow));
            }

            //blog
            if (_blogSettings.Enabled)
            {
                var url = urlHelper.RouteUrl("Blog", null, GetHttpProtocol());
                sitemapUrls.Add(new SitemapUrl(url, UpdateFrequency.Weekly, DateTime.UtcNow));
            }

            //blog
            if (_forumSettings.ForumsEnabled)
            {
                var url = urlHelper.RouteUrl("Boards", null, GetHttpProtocol());
                sitemapUrls.Add(new SitemapUrl(url, UpdateFrequency.Weekly, DateTime.UtcNow));
            }

            //categories
            if (_commonSettings.SitemapIncludeCategories)
                sitemapUrls.AddRange(GetCategoryUrls(urlHelper, 0));

            //manufacturers
            if (_commonSettings.SitemapIncludeManufacturers)
                sitemapUrls.AddRange(GetManufacturerUrls(urlHelper));

            //products
            if (_commonSettings.SitemapIncludeProducts)
                sitemapUrls.AddRange(GetProductUrls(urlHelper));

            //topics
            sitemapUrls.AddRange(GetTopicUrls(urlHelper));

            //custom URLs
            sitemapUrls.AddRange(GetCustomUrls());

            return sitemapUrls;
        }

        /// <summary>
        /// Get category URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="parentCategoryId">Parent category identifier</param>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual IEnumerable<SitemapUrl> GetCategoryUrls(UrlHelper urlHelper, int parentCategoryId)
        {
            return _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId).SelectMany(category =>
            {
                var sitemapUrls = new List<SitemapUrl>();
                var url = urlHelper.RouteUrl("Category", new { SeName = category.GetSeName() }, GetHttpProtocol());
                sitemapUrls.Add(new SitemapUrl(url, UpdateFrequency.Weekly, category.UpdatedOnUtc));
                sitemapUrls.AddRange(GetCategoryUrls(urlHelper, category.Id));

                return sitemapUrls;
            });
        }

        /// <summary>
        /// Get manufacturer URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual IEnumerable<SitemapUrl> GetManufacturerUrls(UrlHelper urlHelper)
        {
            return _manufacturerService.GetAllManufacturers(storeId: _storeContext.CurrentStore.Id).Select(manufacturer =>
            {
                var url = urlHelper.RouteUrl("Manufacturer", new { SeName = manufacturer.GetSeName() }, GetHttpProtocol());
                return new SitemapUrl(url, UpdateFrequency.Weekly, manufacturer.UpdatedOnUtc);
            });
        }

        /// <summary>
        /// Get product URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual IEnumerable<SitemapUrl> GetProductUrls(UrlHelper urlHelper)
        {
            return _productService.SearchProducts(storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true, orderBy: ProductSortingEnum.CreatedOn).Select(product =>
            { 
                var url = urlHelper.RouteUrl("Product", new { SeName = product.GetSeName() }, GetHttpProtocol());
                return new SitemapUrl(url, UpdateFrequency.Weekly, product.UpdatedOnUtc);
            });
        }

        /// <summary>
        /// Get topic URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual IEnumerable<SitemapUrl> GetTopicUrls(UrlHelper urlHelper)
        {
            return _topicService.GetAllTopics(_storeContext.CurrentStore.Id).Where(t => t.IncludeInSitemap).Select(topic =>
            {
                var url = urlHelper.RouteUrl("Topic", new { SeName = topic.GetSeName() }, GetHttpProtocol());
                return new SitemapUrl(url, UpdateFrequency.Weekly, DateTime.UtcNow);
            });
        }

        /// <summary>
        /// Get custom URLs for the sitemap
        /// </summary>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual IEnumerable<SitemapUrl> GetCustomUrls()
        {
            var storeLocation = _webHelper.GetStoreLocation();

            return _commonSettings.SitemapCustomUrls.Select(customUrl => 
                new SitemapUrl(string.Concat(storeLocation, customUrl), UpdateFrequency.Weekly, DateTime.UtcNow));
        }

        /// <summary>
        /// Write sitemap index file into the stream
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="stream">Stream</param>
        /// <param name="sitemapNumber">The number of sitemaps</param>
        protected virtual void WriteSitemapIndex(UrlHelper urlHelper, Stream stream, int sitemapNumber)
        {
            using (var writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("sitemapindex");
                writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
                writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

                //write URLs of all available sitemaps
                for (var id = 1; id <= sitemapNumber; id++)
                {
                    var url = urlHelper.RouteUrl("sitemap-indexed.xml", new { Id = id }, GetHttpProtocol());
                    var location = XmlHelper.XmlEncode(url);

                    writer.WriteStartElement("sitemap");
                    writer.WriteElementString("loc", location);
                    writer.WriteElementString("lastmod", DateTime.UtcNow.ToString(DateFormat));
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Write sitemap file into the stream
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="stream">Stream</param>
        /// <param name="sitemapUrls">List of sitemap URLs</param>
        protected virtual void WriteSitemap(UrlHelper urlHelper, Stream stream, IList<SitemapUrl> sitemapUrls)
        {
            using (var writer = new XmlTextWriter(stream, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("urlset");
                writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
                writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

                //write URLs from list to the sitemap
                foreach (var url in sitemapUrls)
                {
                    writer.WriteStartElement("url");
                    var location = XmlHelper.XmlEncode(url.Location);

                    writer.WriteElementString("loc", location);
                    writer.WriteElementString("changefreq", url.UpdateFrequency.ToString().ToLowerInvariant());
                    writer.WriteElementString("lastmod", url.UpdatedOn.ToString(DateFormat));
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// This will build an xml sitemap for better index with search engines.
        /// See http://en.wikipedia.org/wiki/Sitemaps for more information.
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="id">Sitemap identifier</param>
        /// <returns>Sitemap.xml as string</returns>
        public virtual string Generate(UrlHelper urlHelper, int? id)
        {
            using (var stream = new MemoryStream())
            {
                Generate(urlHelper, stream, id);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// This will build an xml sitemap for better index with search engines.
        /// See http://en.wikipedia.org/wiki/Sitemaps for more information.
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="id">Sitemap identifier</param>
        /// <param name="stream">Stream of sitemap.</param>
        public virtual void Generate(UrlHelper urlHelper, Stream stream, int? id)
        {
            
            //generate all URLs for the sitemap
            var sitemapUrls = GenerateUrls(urlHelper);

            //split URLs into separate lists based on the max size 
            var sitemaps = sitemapUrls.Select((url, index) => new { Index = index, Value = url })
                .GroupBy(group => group.Index / maxSitemapUrlNumber).Select(group => group.Select(url => url.Value).ToList()).ToList();

            if (!sitemaps.Any())
                return;

            if (id.HasValue)
            {
                //requested sitemap does not exist
                if (id.Value == 0 || id.Value > sitemaps.Count)
                    return;

                //otherwise write a certain numbered sitemap file into the stream
                WriteSitemap(urlHelper, stream, sitemaps.ElementAt(id.Value - 1));
                
            }
            else
            {
                //URLs more than the maximum allowable, so generate a sitemap index file
                if (sitemapUrls.Count >= maxSitemapUrlNumber)
                {
                    //write a sitemap index file into the stream
                    WriteSitemapIndex(urlHelper, stream, sitemaps.Count);
                }
                else
                {
                    //otherwise generate a standard sitemap
                    WriteSitemap(urlHelper, stream, sitemaps.First());
                }
            }
        }


        public virtual void ZapModel()
        {
          

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string zapHtml = baseDirectory +"/"+"zap.html";
            var rootCategories = PrepareCategoryZapModels(0, false);
            using (Stream stream = File.Open(zapHtml, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read)) 
            using (TextWriter writer = new StreamWriter(stream))
            using (var HTMLWriter = new System.Web.UI.HtmlTextWriter( writer))
            {

               var t11 = HTMLWriter.Encoding.ToString();

                string encoding = HTMLWriter.Encoding.ToString();
                
                HTMLWriter.BeginRender();
                HTMLWriter.RenderBeginTag("head");
                HTMLWriter.AddAttribute("http-equiv", "Content-Type");
                HTMLWriter.AddAttribute("content", "text/html; charset=utf-8");
                HTMLWriter.RenderBeginTag("meta");
                HTMLWriter.RenderEndTag();
                HTMLWriter.RenderEndTag();


                // HTMLWriter.;
                HTMLWriter.RenderBeginTag("head");
                HTMLWriter.AddAttribute("dir", "rtl");
                HTMLWriter.RenderBeginTag("div");
               
                HTMLWriter.RenderBeginTag("ul");
             foreach (var rootCategory in rootCategories)
                {
                    HTMLWriter.RenderBeginTag("li");
                    GenerateCategoryProductsXML(rootCategory.Id);
                    HTMLWriter.AddAttribute("href", _storeContext.CurrentStore.Url + @"zapXML\" + rootCategory.Id.ToString()+".xml");
                    HTMLWriter.RenderBeginTag("a");
                    //HTMLWriter.WriteEncodedText(rootCategory.Name);
                    HTMLWriter.Write(rootCategory.Name);
                    HTMLWriter.RenderEndTag();
                    HTMLWriter.RenderEndTag();
                    HTMLWriter.RenderBeginTag("ul");
                    var Categories2 = PrepareCategoryZapModels(rootCategory.Id, false);
                    foreach (var Category2 in Categories2)
                    {
                        HTMLWriter.RenderBeginTag("li");
                        GenerateCategoryProductsXML(Category2.Id);
                        HTMLWriter.AddAttribute("href", _storeContext.CurrentStore.Url + @"zapXML\" + Category2.Id.ToString() + ".xml");
                        HTMLWriter.RenderBeginTag("a");
                        HTMLWriter.Write(Category2.Name);
                        HTMLWriter.RenderEndTag();
                        HTMLWriter.RenderEndTag();
                        HTMLWriter.RenderBeginTag("ul");
                        var Categories3 = PrepareCategoryZapModels(Category2.Id, false);
                        foreach (var Category3 in Categories3)
                        {
                            HTMLWriter.RenderBeginTag("li");
                            GenerateCategoryProductsXML(Category3.Id);
                            HTMLWriter.AddAttribute("href", _storeContext.CurrentStore.Url + @"zapXML\" + Category3.Id.ToString() + ".xml");
                            HTMLWriter.RenderBeginTag("a");
                            HTMLWriter.Write(Category3.Name);
                            HTMLWriter.RenderEndTag();
                            HTMLWriter.RenderEndTag();
                            HTMLWriter.RenderBeginTag("ul");
                            var Categories4 = PrepareCategoryZapModels(Category3.Id, false);
                            foreach (var Category4 in Categories4)
                            {
                                HTMLWriter.RenderBeginTag("li");
                                GenerateCategoryProductsXML(Category4.Id);
                                HTMLWriter.AddAttribute("href", _storeContext.CurrentStore.Url + @"zapXML\" + Category4.Id.ToString() + ".xml");
                                HTMLWriter.RenderBeginTag("a");
                                HTMLWriter.Write(Category4.Name);
                                HTMLWriter.RenderEndTag();
                                HTMLWriter.RenderEndTag();
                                HTMLWriter.RenderBeginTag("ul");
                                var Categories5 = PrepareCategoryZapModels(Category4.Id, false);
                                foreach (var Category5 in Categories5)
                                {
                                    HTMLWriter.RenderBeginTag("li");
                                    GenerateCategoryProductsXML(Category5.Id);
                                    HTMLWriter.AddAttribute("href", _storeContext.CurrentStore.Url + @"zapXML\" + Category5.Id.ToString() + ".xml");
                                    HTMLWriter.RenderBeginTag("a");
                                    HTMLWriter.Write(Category5.Name);
                                    HTMLWriter.RenderEndTag();
                                    HTMLWriter.RenderEndTag();
                                    HTMLWriter.RenderBeginTag("ul");
                                }
                                HTMLWriter.RenderEndTag();
                            }
                            HTMLWriter.RenderEndTag();
                        }
                        HTMLWriter.RenderEndTag();
                    }
                    HTMLWriter.RenderEndTag();




                }
                HTMLWriter.RenderEndTag();
                HTMLWriter.RenderEndTag();
                HTMLWriter.EndRender();



            }

            // need to return cached result 

        }
        public virtual string GenerateCategoryProductsXML(int categoryID)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string catXML = baseDirectory  + @"zapXML\" + categoryID.ToString()+".xml";
            using (Stream stream = File.Open(catXML, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            using (TextWriter writer = new StreamWriter(stream))
            using (var XmlWriter = new XmlTextWriter(writer))
            {
                
                XmlWriter.WriteStartElement("STORE");
                XmlWriter.WriteStartElement("PRODUCTS");
                var prodCats = _categoryService.GetProductCategoriesByCategoryId(categoryID);
                ProductPicture prodCatImg;
                string imageUrl;
                foreach (var prodCat in prodCats)
                {
                    if (prodCat.Product.Zap)
                    {
                        try
                        {
                            prodCatImg = prodCat.Product.ProductPictures.Where(P => P.DisplayOrder == 0).ToList()[0];
                            imageUrl = _pictureService.GetPictureUrl(prodCatImg.Picture, _mediaSettings.ProductThumbPictureSize);
                        }
                        catch (Exception e)
                        {
                            imageUrl = "";
                        }
                        
                      
                        XmlWriter.WriteStartElement("PRODUCT");
                        XmlWriter.WriteElementString("PRODUCT_URL", _storeContext.CurrentStore.Url + "Products/" + prodCat.Product.Id.ToString());
                        XmlWriter.WriteElementString("PRODUCT_NAME", prodCat.Product.Name);
                        XmlWriter.WriteElementString("MODEL", prodCat.Product.ZapMODEL);
                        XmlWriter.WriteElementString("DETAILS", prodCat.Product.ZapDETAILS);
                        XmlWriter.WriteElementString("CATALOG_NUMBER", prodCat.Product.Gtin);
                        XmlWriter.WriteElementString("CURRENCY", "ILS");
                        XmlWriter.WriteElementString("PRICE", prodCat.Product.Price.ToString());
                        XmlWriter.WriteElementString("SHIPMENT_COST", prodCat.Product.ZapSHIPMENT_COST);
                        XmlWriter.WriteElementString("DELIVERY_TIME", prodCat.Product.ZapDELIVERY_TIME);
                        XmlWriter.WriteElementString("MANUFACTURER", prodCat.Product.ZapMANUFACTURER);
                        XmlWriter.WriteElementString("WARRANTY", prodCat.Product.ZapWARRANTY);
                        XmlWriter.WriteElementString("IMAGE", imageUrl);
                        XmlWriter.WriteElementString("IMAGE", "");
                        XmlWriter.WriteEndElement();




                    }
                    //XmlWriter.WriteEndElement();
                    //XmlWriter.WriteEndElement();



                }
                //XmlWriter.WriteEndDocument();
            }

                return catXML;
        }

        #endregion
    }

    public class CategoryZapModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string SeName { get; set; }

        public int? NumberOfProducts { get; set; }

        public bool Zap { get; set; }
    }
}
