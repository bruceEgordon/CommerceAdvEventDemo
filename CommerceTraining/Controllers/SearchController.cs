using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Pages;
using CommerceTraining.Models.ViewModels;
using EPiServer.Commerce.Catalog.ContentTypes;
using Mediachase.Commerce.Website.Search;
using Mediachase.Search.Extensions;
using Mediachase.Search;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using System;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace CommerceTraining.Controllers
{
    public class SearchController : PageController<SearchPage>
    {
        public IEnumerable<IContent> localContent { get; set; }
        public readonly IContentLoader _contentLoader;
        public readonly ReferenceConverter _referenceConverter;
        public readonly UrlResolver _urlResolver;

        public SearchController(IContentLoader contentLoader, ReferenceConverter referenceConverter, UrlResolver urlResolver)
        {
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
            _urlResolver = urlResolver;
        }

        public ActionResult Index(SearchPage currentPage)
        {
            var model = new SearchPageViewModel
            {
                CurrentPage = currentPage,
            };

            return View(model);
        }

        public ActionResult Search(string keyWord)
        {
            // ToDo: SearchHelper and Criteria 


            // ToDo: Search 


            // ToDo: Facets


            // ToDo: As a last step - un-comment and fill up the ViewModel
            var searchResultViewModel = new SearchResultViewModel();
            /*
            searchResultViewModel.totalHits = new List<string> { "" }; // change
            searchResultViewModel.nodes = localContent.OfType<FashionNode>();
            searchResultViewModel.products = localContent.OfType<FashionProduct>();
            searchResultViewModel.variants = localContent.OfType<FashionVariation>();
            searchResultViewModel.allContent = localContent;
            searchResultViewModel.facets = facetList;
            */

            return View(searchResultViewModel);
        }
    }
}