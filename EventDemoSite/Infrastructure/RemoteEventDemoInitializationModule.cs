using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using EPiServer.Events.Clients;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using Mediachase.Commerce.Engine.Events;

namespace EventDemoSite.Infrastructure
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class RemoteEventDemoInitializationModule : IInitializableModule
    {
        Event evListener;
        public void Initialize(InitializationEngine context)
        {
            evListener = Event.Get(CatalogKeyEventBroadcaster.CatalogKeyEventGuid);
            evListener.Raised += EvListener_Raised;
        }

        private void EvListener_Raised(object sender, EPiServer.Events.EventNotificationEventArgs e)
        {
            var eventArgs = (CatalogKeyEventArgs)DeSerialize((byte[])e.Param);
            var priceUpdatedEventArgs = eventArgs as PriceUpdateEventArgs;
            var inventoryUpdatedEventArgs = eventArgs as InventoryUpdateEventArgs;
            var logger = LogManager.GetLogger();
            if(priceUpdatedEventArgs != null)
            {
                logger.Error($"the price was changed!");
            }
            if(inventoryUpdatedEventArgs != null)
            {
                logger.Error($"the inventory was changed!");
            }
        }

        protected virtual CatalogKeyEventArgs DeSerialize(byte[] buffer)
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream(buffer);
            return formatter.Deserialize(stream) as CatalogKeyEventArgs;
        }
        public void Uninitialize(InitializationEngine context)
        {
            evListener.Raised -= EvListener_Raised;
        }
    }
}