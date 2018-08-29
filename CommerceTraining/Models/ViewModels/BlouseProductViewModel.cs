using CommerceTraining.Models.Catalog;
using CommerceTraining.Models.Pages;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommerceTraining.Models.ViewModels
{
    public class BlouseProductViewModel : CatalogViewModel<BlouseProduct, StartPage>
    {
        public BlouseProductViewModel(BlouseProduct currentContent, StartPage currentPage) : base(currentContent, currentPage)
        {
        }

        public IEnumerable<EntryContentBase> productVariations { get; set; }

        public ContentReference campaignLink { get; set; }
    }
}