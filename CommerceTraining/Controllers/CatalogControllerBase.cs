using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CommerceTraining.SupportingClasses;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;

namespace CommerceTraining.Controllers
{
    public class CatalogControllerBase<T> : ContentController<T> where T : CatalogContentBase
    {
        public readonly IContentLoader _contentLoader;
        public readonly UrlResolver _urlResolver;
        public readonly AssetUrlResolver _assetUrlResolver;
        public readonly ThumbnailUrlResolver _thumbnailUrlResolver;

        public CatalogControllerBase(IContentLoader contentLoader,
            UrlResolver urlResolver,
            AssetUrlResolver assetUrlResolver,
            ThumbnailUrlResolver thumbnailUrlResolver)
        {
            this._contentLoader = contentLoader;
            this._urlResolver = urlResolver;
            this._assetUrlResolver = assetUrlResolver;
            this._thumbnailUrlResolver = thumbnailUrlResolver;
        }

        public string GetDefaultAsset(IAssetContainer assetContainer)
        {
            return _assetUrlResolver.GetAssetUrl(assetContainer);
        }

        public string GetNamedAsset(IAssetContainer assetContainer, string propName)
        {
            return _thumbnailUrlResolver.GetThumbnailUrl(assetContainer, propName);
        }

        public string GetUrl(ContentReference contentReference)
        {
            return _urlResolver.GetUrl(contentReference);
        }

        public List<NameAndUrls> GetNodes(ContentReference contentReference)
        {
            var items = FilterForVisitor.Filter(_contentLoader.GetChildren<NodeContent>(contentReference));
            var returnItems = new List<NameAndUrls>();
            foreach(NodeContent item in items)
            {
                var catItem = new NameAndUrls
                {
                    name = item.Name,
                    url = GetUrl(item.ContentLink),
                    imageThumbUrl = GetNamedAsset(item, "Thumbnail"),
                    imageUrl = GetDefaultAsset(item)
                };
                returnItems.Add(catItem);
            }
            return returnItems;
        }

        public List<NameAndUrls> GetEntries(ContentReference contentReference)
        {
            var items = FilterForVisitor.Filter(_contentLoader.GetChildren<EntryContentBase>(contentReference));
            var returnItems = new List<NameAndUrls>();
            foreach (EntryContentBase item in items)
            {
                var catItem = new NameAndUrls
                {
                    name = item.Name,
                    url = GetUrl(item.ContentLink),
                    imageThumbUrl = GetNamedAsset(item, "Thumbnail"),
                    imageUrl = GetDefaultAsset(item)
                };
                returnItems.Add(catItem);
            }
            return returnItems;
        }
    }
}