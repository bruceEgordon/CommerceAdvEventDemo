using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using CommerceTraining.Models.Catalog;
using CommerceTraining.Models.Pages;
using CommerceTraining.Models.ViewModels;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Framework.DataAnnotations;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Catalog;

namespace CommerceTraining.Controllers
{
    public class BlouseProductController : CatalogControllerBase<BlouseProduct>
    {
        public BlouseProductController(IContentLoader contentLoader, UrlResolver urlResolver, AssetUrlResolver assetUrlResolver, ThumbnailUrlResolver thumbnailUrlResolver) : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver)
        {
        }

        public ActionResult Index(BlouseProduct currentContent, StartPage currentPage)
        {
            IEnumerable<ContentReference> variationRefs = currentContent.GetVariants();  //easiest
            IEnumerable<EntryContentBase> variations = _contentLoader.GetItems(variationRefs, new LoaderOptions()).OfType<EntryContentBase>();
            ContentReference campLink = currentPage.campaignLink;

            var model = new BlouseProductViewModel(currentContent, currentPage)
            {
                productVariations = variations,
                campaignLink = campLink
            };

            return View(model);
        }

        public void CreateWithCode()
        {
            // ToDo: Use with Blouses in "Fund"... 
            string nodeName = "myNode";
            string productName = "myProduct";
            string skuName = "mySku";

            // Get ReferenceConverter and LinksRepository
            ReferenceConverter refConv = ServiceLocator.Current.GetInstance<ReferenceConverter>();

            //ILinksRepository linksRep = ServiceLocator.Current.GetInstance<ILinksRepository>(); Obsoleted
            IRelationRepository _relRep = ServiceLocator.Current.GetInstance<IRelationRepository>(); // the one to use

            // Create Node
            ContentReference linkToParentNode = refConv.GetContentLink("Women_1");
            // if an int... look at the second arg.
            //ContentReference cref0 = _referenceConverter.GetContentLink(42, CatalogContentType.CatalogNode, 0);

            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            var newNode = contentRepository.GetDefault<FashionNode>(linkToParentNode, new CultureInfo("en"));
            newNode.Code = nodeName;
            newNode.SeoUri = nodeName;
            newNode.Name = nodeName;
            newNode.DisplayName = nodeName;

            ContentReference newNodeRef = contentRepository.Save
                (newNode, SaveAction.Publish, EPiServer.Security.AccessLevel.NoAccess);

            // Create Product
            //LanguageSelector selEN = new LanguageSelector("en"); // obsoleted
            var newProduct = contentRepository.GetDefault<BlouseProduct>(newNodeRef, new CultureInfo("en"));

            //Set some required properties.
            newProduct.Code = productName;
            newProduct.SeoUri = productName;
            newProduct.Name = productName; // before: InternalName 
            //newProduct.CanBeMonogrammed = false;
            //newProduct.Brand = "Ford";
            //newProduct.Color = "Gold";
            newProduct.DisplayName = productName; // before: Name
            //newProduct. ClothesType = "CarClothes";
            newProduct.SeoInformation.Title = "SEO Title";
            newProduct.SeoInformation.Keywords = "Some keywords";
            newProduct.SeoInformation.Description = "A nice one";
            newProduct.MainBody = new XhtmlString("This new product is great");

            ContentReference newProductRef = contentRepository.Save
                (newProduct, SaveAction.Publish, EPiServer.Security.AccessLevel.NoAccess);

            // Create SKU
            var newSku = contentRepository.GetDefault<ShirtVariation>(newNodeRef, new CultureInfo("en"));

            newSku.Code = skuName;
            newSku.SeoUri = skuName;
            newSku.Name = skuName;
            newSku.DisplayName = skuName;

            ContentReference newSkuRef = contentRepository.Save
                (newSku, SaveAction.Publish, EPiServer.Security.AccessLevel.NoAccess);

            // what differs from CMS - ILinksRep. & IRelationRep
            ProductVariation prodVarRel = new ProductVariation();

            //prodVarRel.Target = newSkuRef;
            prodVarRel.Child = newSkuRef;
            //prodVarRel.Source = newProductRef;
            prodVarRel.Parent = newProductRef;
            prodVarRel.SortOrder = 100; // usable in 11

            _relRep.UpdateRelation(prodVarRel);
            //linksRep.UpdateRelation(prodVarRel); obsoleted

            // done, but... 
            /* ...still missing Market, Inventory, Pricing and Media + a few other things */
            // could have that as additional exercise

            // ToDo: ...some redirect... somewhere


        }
    }
}