using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using CommerceTraining.Models.Pages;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce;
using Mediachase.Commerce.Website.Helpers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Engine;
using System;
using EPiServer.Security;
using Mediachase.Commerce.Customers;
using EPiServer.ServiceLocation;
using CommerceTraining.Models.ViewModels;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Marketing;
using Mediachase.Data.Provider;

// for the extension-method
using Mediachase.Commerce.Security;
using EPiServer.Commerce.Order.Calculator;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Inventory;

namespace CommerceTraining.Controllers
{
    public class CheckOutController : PageController<CheckOutPage>
    {

        private const string DefaultCart = "Default";

        private readonly IContentLoader _contentLoader; // To get the StartPage --> Settings-links
        private readonly ICurrentMarket _currentMarket; // not in fund... yet
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IPromotionEngine _promotionEngine;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly ILineItemCalculator _lineItemCalculator;
        private readonly IInventoryProcessor _inventoryProcessor;
        private readonly ILineItemValidator _lineItemValidator;
        private readonly IPlacedPriceProcessor _placedPriceProcessor;

        public CheckOutController(IContentLoader contentLoader
    , ICurrentMarket currentMarket
    , IOrderRepository orderRepository
    , IPlacedPriceProcessor placedPriceProcessor
    , IInventoryProcessor inventoryProcessor
    , ILineItemValidator lineItemValidator
    , IOrderGroupCalculator orderGroupCalculator
    , ILineItemCalculator lineItemCalculator
    , IOrderGroupFactory orderGroupFactory
    , IPaymentProcessor paymentProcessor
    , IPromotionEngine promotionEngine)
        {
            _contentLoader = contentLoader;
            _currentMarket = currentMarket;
            _orderRepository = orderRepository;
            _orderGroupCalculator = orderGroupCalculator;
            _orderGroupFactory = orderGroupFactory;
            _paymentProcessor = paymentProcessor;
            _promotionEngine = promotionEngine;
            _lineItemCalculator = lineItemCalculator;
            _inventoryProcessor = inventoryProcessor;
            _lineItemValidator = lineItemValidator;
            _placedPriceProcessor = placedPriceProcessor;
        }

        // ToDo: in the first exercise (E1) Ship & Pay
        public ActionResult Index(CheckOutPage currentPage)
        {
            // Try to load the cart  

            var model = new CheckOutViewModel(currentPage)
            {
                // ToDo: Exercise (E1) - get shipments & payments
                PaymentMethods = GetPaymentMethods(),
                ShippingMethods = GetShipmentMethods(),
                ShippingRates = GetShippingRates()
            };

            return View(model);
        }


        // Exercise (E1) creation of GetPaymentMethods(), GetShipmentMethods() and GetShippingRates() goes below
        // ToDo: Get IEnumerables of Shipping and Payment methods along with ShippingRates
        public IEnumerable<PaymentMethodDto.PaymentMethodRow> GetPaymentMethods()
        {
            var payMethods = PaymentManager.GetPaymentMethodsByMarket(_currentMarket.GetCurrentMarket().MarketId.Value);
            return payMethods.PaymentMethod.ToList();
        }

        public IEnumerable<ShippingMethodDto.ShippingMethodRow> GetShipmentMethods()
        {
            var shipMethods = ShippingManager.GetShippingMethodsByMarket(_currentMarket.GetCurrentMarket().MarketId.Value, false);
            return shipMethods.ShippingMethod.ToList();
        }

        public IEnumerable<ShippingRate> GetShippingRates()
        {
            var shippingRates = new List<ShippingRate>();
            var shipMethods = GetShipmentMethods();
            foreach(var method in shipMethods)
            {
                shippingRates.Add(new ShippingRate(method.ShippingMethodId, method.DisplayName, 
                    new Money(method.BasePrice, method.Currency)));
            }
            return shippingRates;
        }


        //Exercise (E2) Do CheckOut
        public ActionResult CheckOut(CheckOutViewModel model)
        {
            // ToDo: Load the cart
            var cart = _orderRepository.LoadCart<ICart>(GetContactId(), DefaultCart);
            if(cart == null)
            {
                throw new InvalidOperationException("There is no cart exception.");
            }

            // ToDo: Add an OrderAddress
            IOrderAddress theAddress = AddAddressToOrder(cart);

            // ToDo: Define/update Shipping
            AdjustFirstShipmentInOrder(cart, theAddress, model.SelectedShipId);

            // ToDo: Add a Payment to the Order 
            AddPaymentToOrder(cart, model.SelectedPayId);

            // ToDo: Add a transaction scope and convert the cart to PO
            IPurchaseOrder purchaseOrder;
            OrderReference orderReference;
            using (var scope = new TransactionScope())
            {
                var validationIssues = new Dictionary<ILineItem, ValidationIssue>();

                // Added - sets a lock on inventory... could come earlier (outside tran) depending on TypeOf-"store"
                _inventoryProcessor.AdjustInventoryOrRemoveLineItem(cart.GetFirstShipment()
                    , OrderStatus.InProgress, (item, issue) => validationIssues.Add(item, issue));

                if (validationIssues.Count >= 1)
                {
                    throw new Exception("Not possible right now"); // ...change approach and fork
                }

                // ECF 10 - void back
                cart.ProcessPayments();
                // ECF 11
                //IEnumerable<PaymentProcessingResult> PaymentProcessingResult = cart.ProcessPayments();
                //var xyz = PaymentProcessingResult.First().IsSuccessful;

                // just looking around - (nice extension methods)
                var cartTotal = cart.GetTotal();
                var handling = cart.GetHandlingTotal(); // OG-Calculator... aggregate on "all forms"
                var form = cart.GetFirstForm(); // 
                var formHandling = form.HandlingTotal; // "handling" sits here on OF

                /* orderGroupCalculator does:
                  - Catches up with the "IOrderFormCalculator"
                  - GetSubTotal - Form-Calculator does
                  - GetHandlingTotal - Form-Calc does
                  - GetShippingCost - Form-Calc. does with ShippingCalc. - "processes" the shipment
                  - GetTaxTotal - FormCalc. does with Tax-Calc.
                 */

                var totalProcessedAmount = cart.GetFirstForm().Payments.Where
                    (x => x.Status.Equals(PaymentStatus.Processed.ToString())).Sum(x => x.Amount);

                // could do more than this, but we just processed payment(s)
                if (totalProcessedAmount != cart.GetTotal(_orderGroupCalculator).Amount)
                {
                    // ...we're not happy, put back the reserved request
                    _inventoryProcessor.AdjustInventoryOrRemoveLineItem(cart.GetFirstShipment()
                        , OrderStatus.Cancelled, (item, issue) => validationIssues.Add(item, issue));

                    #region OldSchool Inventory check
                    //List<InventoryRequestItem> requestItems = new List<InventoryRequestItem>(); // holds the "items"
                    //InventoryRequestItem requestItem = new InventoryRequestItem();

                    //// calls for some logic
                    //requestItem.RequestType = InventoryRequestType.Cancel; // as a demo
                    //requestItem.OperationKey = reqKey;

                    //requestItems.Add(requestItem);

                    //InventoryRequest inventoryRequest = new InventoryRequest(DateTime.UtcNow, requestItems, null);
                    //InventoryResponse inventoryResponse = _invService.Service.Request(inventoryRequest);
                    //InventoryRecord rec4 = _invService.Service.Get(LI.Code, wh.Code);
                    #endregion OldSchool

                    throw new InvalidOperationException("Wrong amount"); // maybe change approach
                }

                // we're happy
                // ...could do this here - look at dhe different statuses
                cart.GetFirstShipment().OrderShipmentStatus = OrderShipmentStatus.InventoryAssigned;

                // decrement inventory and let it go
                _inventoryProcessor.AdjustInventoryOrRemoveLineItem(cart.GetFirstShipment()
                    , OrderStatus.Completed, (item, issue) => validationIssues.Add(item, issue));

                #region OldSchool Inventory check
                //List<InventoryRequestItem> requestItems1 = new List<InventoryRequestItem>(); // holds the "items"
                //InventoryRequestItem requestItem1 = new InventoryRequestItem();

                //// calls for some logic
                //requestItem1.RequestType = InventoryRequestType.Complete; // as a demo
                //requestItem1.OperationKey = reqKey;

                //requestItems1.Add(requestItem1);

                //InventoryRequest inventoryRequest1 = new InventoryRequest(DateTime.UtcNow, requestItems1, null);
                //InventoryResponse inventoryResponse1 = _invService.Service.Request(inventoryRequest1);
                #endregion OldSchool

                // we're even happier
                orderReference = _orderRepository.SaveAsPurchaseOrder(cart);
                _orderRepository.Delete(cart.OrderLink);

                scope.Complete();
            } // End TransactionScope

            // ToDo: Housekeeping (Statuses for Shipping and PO, OrderNotes and save the order)
            purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
            //_orderRepository.Load<IPurchaseOrder>()

            // check the below
            var theType = purchaseOrder.OrderLink.OrderType;
            var toString = purchaseOrder.OrderLink.ToString(); // Gets ID and Type ... combined

            #region ThisAndThat
            // should/could do some with OrderStatusManager, slightly old-school
            // OrderStatusManager.

            OrderStatus poStatus;
            poStatus = purchaseOrder.OrderStatus;
            //purchaseOrder.OrderStatus = OrderStatus.InProgress;

            //var info = OrderStatusManager.GetPurchaseOrderStatus(PO); // old-school
            //PO.Status = OrderStatus.InProgress.ToString();

            var shipment = purchaseOrder.GetFirstShipment();
            var status = shipment.OrderShipmentStatus;

            //shipment. ... no that much to do
            //shipment.OrderShipmentStatus = OrderShipmentStatus.InventoryAssigned;

            // Don't use OrderNoteManager ... it doesn't see the "new" stuff
            var notes = purchaseOrder.Notes; // IOrderNote is 0

            Mediachase.Commerce.Orders.OrderNote otherNote = new OrderNote //IOrderNote 
            {
                // Created = DateTime.Now, // do we need to set this ?? Nope .ctor does
                CustomerId = new Guid(), // can set this - regarded
                Detail = "Order ToString(): " + toString + " - Shipment tracking number: " + shipment.ShipmentTrackingNumber,
                LineItemId = purchaseOrder.GetAllLineItems().First().LineItemId,
                // OrderGroupId = 12, R/O - error
                // OrderNoteId = 12, // can define it, but it's disregarded - no error
                Title = "Some title",
                Type = OrderNoteTypes.Custom.ToString()
            };

            // not intended to be used, as it's "Internal"
            //IOrderNote theNote = new EPiServer.Commerce.Order.Internal.SerializableNote();

            purchaseOrder.Notes.Add(otherNote); // void back
            purchaseOrder.ExpirationDate = DateTime.Now.AddMonths(1);

            // ...need to come after adding notes
            _orderRepository.Save(purchaseOrder);

            #endregion

            // Final steps, navigate to the order confirmation page
            StartPage home = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            ContentReference orderPageReference = home.Settings.orderPage;

            // the below is a dummy, change to "PO".OrderNumber when done
            //string passingValue = String.Empty;
            string passingValue = purchaseOrder.OrderNumber;

            return RedirectToAction("Index", new { node = orderPageReference, passedAlong = passingValue });
        }


        // Prewritten 
        private string ValidateCart(ICart cart)
        {
            var validationMessages = string.Empty;

            cart.ValidateOrRemoveLineItems((item, issue) =>
                validationMessages += CreateValidationMessages(item, issue), _lineItemValidator);

            cart.UpdatePlacedPriceOrRemoveLineItems(GetContact(), (item, issue) =>
                validationMessages += CreateValidationMessages(item, issue), _placedPriceProcessor);

            cart.UpdateInventoryOrRemoveLineItems((item, issue) =>
                validationMessages += CreateValidationMessages(item, issue), _inventoryProcessor);

            return validationMessages; 
        }

        private static string CreateValidationMessages(ILineItem item, ValidationIssue issue)
        {
            return string.Format("Line item with code {0} had the validation issue {1}.", item.Code, issue);
        }

        private void AdjustFirstShipmentInOrder(ICart cart, IOrderAddress orderAddress, Guid selectedShip)
        {
            var shippingMethod = ShippingManager.GetShippingMethod(selectedShip).ShippingMethod.First();
            IShipment theShip = cart.GetFirstShipment();
            theShip.ShippingMethodId = shippingMethod.ShippingMethodId;
            theShip.ShippingAddress = orderAddress;
            Money cost0 = theShip.GetShippingCost(
                _currentMarket.GetCurrentMarket()
                , _currentMarket.GetCurrentMarket().DefaultCurrency); // to make it easy

            // done by the "default calculator"
            Money cost1 = theShip.GetShippingItemsTotal(_currentMarket.GetCurrentMarket().DefaultCurrency);

            theShip.ShipmentTrackingNumber = "ABC123";
        }

        private void AddPaymentToOrder(ICart cart, Guid selectedPaymentGuid)
        {
            if (cart.GetFirstForm().Payments.Any())
            {
                // should maybe clean up in the cart here
            }

            var selectedPaymentMethod = PaymentManager.GetPaymentMethod(selectedPaymentGuid).PaymentMethod.First();

            var payment = _orderGroupFactory.CreatePayment(cart);

            // check if str "soliciting" still works
            payment.PaymentMethodId = selectedPaymentMethod.PaymentMethodId;
            //payment.PaymentType = PaymentType.Other;
            payment.PaymentMethodName = selectedPaymentMethod.Name;

            // check this thing with "classname"/PaymentType as string

            // usless - crash, doesn't support "Serializable"
            //var type = PaymentTransactionTypeManager.GetPaymentTransactionType((Payment)payment);

            // this one... but not really useful
            var className = selectedPaymentMethod.ClassName;
            // it's a string (as we see it in "Adv" when creating options & methods)

            // ...we also have - cart.GetTotal(_orderGroupCalculator)
            payment.Amount = _orderGroupCalculator.GetTotal(cart).Amount; // need a debug here

            cart.AddPayment(payment);
            // could add payment.BillingAddress = theAddress ... if we had it here
        }

        private IOrderAddress AddAddressToOrder(ICart cart)
        {
            IOrderAddress shippingAddress = null;

            if (CustomerContext.Current.CurrentContact == null)
            {
                IShipment shipment = cart.GetFirstShipment();
                if(shipment.ShippingAddress != null)
                {
                    //using the cart.GetFirstShipment() seems only useful if we are checking for an existing address, otherwise
                    //the solution code's recommended 'new school' approach doesn't use it.
                }
                IOrderAddress myOrderAddress = _orderGroupFactory.CreateOrderAddress(cart);
                myOrderAddress.CountryName = "United States";
                myOrderAddress.Id = "HomersShippingAddress";
                myOrderAddress.Email = "homer@acme.com";

                shippingAddress = myOrderAddress;
            }
            else
            {
                //User is logged in
                if(CustomerContext.Current.CurrentContact.PreferredShippingAddress == null)
                {
                    CustomerAddress newCustAddress = CustomerAddress.CreateInstance();
                    newCustAddress.AddressType = CustomerAddressTypeEnum.Shipping | CustomerAddressTypeEnum.Public; //mandatory
                    newCustAddress.ContactId = CustomerContext.Current.CurrentContact.PrimaryKeyId;
                    newCustAddress.CountryCode = "USA";
                    newCustAddress.CountryName = "United States";
                    newCustAddress.Name = "Wilbur's customer address"; // mandatory
                    newCustAddress.DaytimePhoneNumber = "9008675309";
                    newCustAddress.FirstName = CustomerContext.Current.CurrentContact.FirstName;
                    newCustAddress.LastName = CustomerContext.Current.CurrentContact.LastName;
                    newCustAddress.Email = "wilbur@acme.com";

                    // note: Line1 & City is what is shown in CM at a few places... not the Name
                    CustomerContext.Current.CurrentContact.AddContactAddress(newCustAddress);
                    CustomerContext.Current.CurrentContact.SaveChanges();

                    // ... needs to be in this order
                    CustomerContext.Current.CurrentContact.PreferredShippingAddress = newCustAddress;
                    CustomerContext.Current.CurrentContact.SaveChanges(); // need this ...again 

                    // then, for the cart
                    //.Cart.OrderAddresses.Add(new OrderAddress(newCustAddress)); - OLD
                    shippingAddress = new OrderAddress(newCustAddress); // - NEW
                }
                else
                {
                    shippingAddress = new OrderAddress(CustomerContext.Current.CurrentContact.PreferredShippingAddress);
                }
            }

            return shippingAddress;
        }

        private static CustomerContact GetContact()
        {
            return CustomerContext.Current.GetContactById(GetContactId());
        }

        private static Guid GetContactId()
        {
            return PrincipalInfo.CurrentPrincipal.GetContactId();
        }
    }
}