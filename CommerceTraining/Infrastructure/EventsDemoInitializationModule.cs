using System;
using System.IO;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.Provider;
using EPiServer.Core;
using EPiServer.Events.Clients;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace CommerceTraining.Infrastructure
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule), typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    public class EventsDemoInitializationModule : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var contentEv = context.Locate.ContentEvents();
            contentEv.PublishedContent += ContentEv_PublishedContent;

            var catEv = CatalogEventHandler.Instance;//ServiceLocator.Current.GetInstance<CatalogEventHandler>();

            catEv.ContentUpdated += CatEv_ContentUpdated;

        }

        private void CatEv_ContentUpdated(object sender, ContentEventArgs e)
        {
            string mydocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using (StreamWriter ouputFile = new StreamWriter(Path.Combine(mydocPath, "EventsDemo.txt"), true))
            {
                ouputFile.WriteLine($"CatalogContentEvent fired for {e.Content.ToString()}");
            }
        }

        private void ContentEv_PublishedContent(object sender, ContentEventArgs e)
        {
            string mydocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            using(StreamWriter ouputFile = new StreamWriter(Path.Combine(mydocPath, "EventsDemo.txt"), true))
            {
                ouputFile.WriteLine("Published content event fired!");
            }
            if (e.Content.GetOriginalType().Name == "ShirtVariation")
            {
                
            }
        }

        private void ContentEv_SavedContent(object sender, EPiServer.ContentEventArgs e)
        {
            var logger = LogManager.GetLogger();
            logger.Error("Content Saved Event!");
            if (e.Content.GetOriginalType().Name == "ShirtVariation")
            {
                logger.Error($"{e.Content.GetOriginalType().Name}");
            }
        }

        public void Uninitialize(InitializationEngine context)
        {
            //Add uninitialization logic
        }
    }
}