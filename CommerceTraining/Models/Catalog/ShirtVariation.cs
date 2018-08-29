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
    [CatalogContentType(GUID = "34717c0e-62ab-4968-ad34-d66788fc5566", MetaClassName = "Shirt_Variation")]
    public class ShirtVariation : VariationContent
    {
        [CultureSpecific]
        [IncludeInDefaultSearch]
        [Searchable]
        [Tokenize]
        [Display(Name = "Main body", Description = "Use this to store the main body of information for the Shirt Variant.",
            GroupName = SystemTabNames.Content)]
        public virtual XhtmlString MainBody { get; set; }

        [IncludeInDefaultSearch]
        public virtual string Size { get; set; }

        [IncludeInDefaultSearch]
        public virtual string Color { get; set; }

        public virtual bool CanBeMonogrammed { get; set; }

        [Searchable]
        [Tokenize]
        [IncludeValuesInSearchResults]
        public virtual string ThematicTag { get; set; }
    }
}