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
    [CatalogContentType(GUID = "93d43112-3666-491b-bde7-2adac87523c7", MetaClassName = "My_Variation")]
    public class MyVariation : VariationContent
    {
    }
}