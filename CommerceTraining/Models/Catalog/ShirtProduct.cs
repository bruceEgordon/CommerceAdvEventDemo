using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;

namespace CommerceTraining.Models.Catalog
{
    [CatalogContentType(GUID = "4b6a3fe5-2eb6-442e-be9f-16673082663d", MetaClassName = "Shirt_Product")]
    public class ShirtProduct : ProductContent
    {
        [CultureSpecific]
        [IncludeInDefaultSearch]
        [Searchable]
        [Tokenize]
        [Display(Name = "Main body", Description = "Use this to store the main body of information for the Shirt Product.",
            GroupName = SystemTabNames.Content)]
        public virtual XhtmlString MainBody { get; set; }

        public virtual string ClothesType { get; set; }

        public virtual string Brand { get; set; }
    }
}