using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CommerceTraining.Models.Catalog;
using CommerceTraining.Models.Pages;
using CommerceTraining.Models.ViewModels;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Globalization;
using EPiServer.Security;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Security;

namespace CommerceTraining.Controllers
{
    public class ShirtVariationController : CatalogControllerBase<ShirtVariation>
    {
        private IOrderRepository _orderRepository;
        private IOrderGroupFactory _orderGroupFactory;
        private ILineItemValidator _lineItemValidator;

        public ShirtVariationController(IContentLoader contentLoader, UrlResolver urlResolver, AssetUrlResolver assetUrlResolver, ThumbnailUrlResolver thumbnailUrlResolver, IOrderRepository orderRepository, IOrderGroupFactory orderGroupFactory, ILineItemValidator lineItemValidator) : base(contentLoader, urlResolver, assetUrlResolver, thumbnailUrlResolver)
        {
            _orderRepository = orderRepository;
            _orderGroupFactory = orderGroupFactory;
            _lineItemValidator = lineItemValidator;
        }

        public ActionResult Index(ShirtVariation currentContent)
        {
            var viewModel = new ShirtVariationViewModel();
            viewModel.name = currentContent.Name;
            viewModel.MainBody = currentContent.MainBody;
            viewModel.priceString = currentContent.GetDefaultPrice().UnitPrice.ToString("C");
            viewModel.image = GetDefaultAsset(currentContent);
            viewModel.CanBeMonogrammed = currentContent.CanBeMonogrammed;
            return View(viewModel);
        }

        public ActionResult AddToCart(ShirtVariation currentContent, decimal Quantity, string Monogram)
        {
            // ToDo: (lab D1) add a LineItem to the Cart
            var cart = _orderRepository.LoadOrCreateCart<ICart>(PrincipalInfo.CurrentPrincipal.GetContactId(), "Default");
            var cartItem = cart.GetAllLineItems().SingleOrDefault(item => item.Code == currentContent.Code);

            if(cartItem == null)
            {
                cartItem = _orderGroupFactory.CreateLineItem(currentContent.Code, cart);
                cartItem.Quantity = Quantity;
                cart.AddLineItem(cartItem);
            }
            else
            {
                cartItem.Quantity += Quantity;
            }

            var validLineItem = _lineItemValidator.Validate(cartItem, cart.MarketId, (item, issue) => { });

            if (validLineItem)
            {
                cartItem.Properties["Monogram"] = Monogram;
                _orderRepository.Save(cart);
            }

            // if we want to redirect
            ContentReference cartRef = _contentLoader.Get<StartPage>(ContentReference.StartPage).Settings.cartPage;
            CartPage cartPage = _contentLoader.Get<CartPage>(cartRef);
            var name = cartPage.Name;
            var lang = ContentLanguage.PreferredCulture;
            string passingValue = cart.Name;

            // go to the cart page, if needed
            return RedirectToAction("Index", lang + "/" + name, new { passedAlong = passingValue });
        }


        public void AddToWishList(ShirtVariation currentContent)
        {

        }
    }
}