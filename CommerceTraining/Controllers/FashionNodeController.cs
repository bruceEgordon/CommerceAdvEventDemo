using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CommerceTraining.Models.Catalog;
using CommerceTraining.SupportingClasses;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;

namespace CommerceTraining.Controllers
{
    public class FashionNodeController : CatalogControllerBase<FashionNode>
    {
        public FashionNodeController(IContentLoader contentLoader, UrlResolver urlResolver, AssetUrlResolver assetUrlResolver, ThumbnailUrlResolver thumbnailUrlResolver) : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver)
        {
        }

        public ActionResult Index(FashionNode currentContent)
        {
            var catalogItems = new NodeEntryCombo();
            catalogItems.entries = GetEntries(currentContent.ContentLink);
            catalogItems.nodes = GetNodes(currentContent.ContentLink);

            return View(catalogItems);
        }
    }
}