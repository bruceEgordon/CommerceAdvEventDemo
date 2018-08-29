using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;

namespace CommerceTraining.Models.Pages
{
    [ContentType(DisplayName = "StandardPage", GUID = "8e5c9371-0c12-4983-a26d-cad64f82c561", Description = "")]
    public class StandardPage : PageData
    {
        [CultureSpecific]
        [Display(
            Name = "Heading",
            Description = "The page heading.",
            GroupName = SystemTabNames.Content,
            Order = 0)]
        public virtual string Heading { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual XhtmlString MainBody { get; set; }

    }
}