using System;
using System.Linq;
using EPiServer;
using EPiServer.Core;
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
        }

        private void ContentEv_PublishedContent(object sender, ContentEventArgs e)
        {
            var logger = LogManager.GetLogger();
            logger.Error("Content Published Event!");
            if (e.Content.GetOriginalType().Name == "ShirtVariation")
            {
                logger.Error($"{e.Content.GetOriginalType().Name}");
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