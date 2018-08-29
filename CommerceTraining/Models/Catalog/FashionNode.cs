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
    [CatalogContentType(GUID = "dcc1e8e9-afb7-4d58-9b2f-0e754212b117", MetaClassName = "Fashion_Node")]
    public class FashionNode : NodeContent
    {
        [CultureSpecific]
        [IncludeInDefaultSearch]
        [Searchable]
        [Tokenize]
        [Display(Name = "Main body", Description = "Use this to store the main body of information for the Fashion node.",
            GroupName = SystemTabNames.Content)]
        public virtual XhtmlString MainBody { get; set; }
    }
}