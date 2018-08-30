using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Provider;
using EPiServer.Core;
using EPiServer.Events.Clients;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Events;

namespace CommerceTraining.Infrastructure
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule), typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    public class EventsDemoInitializationModule : IInitializableModule
    {
        IContentEvents contentEv;
        ICatalogEvents catalogEv;
        Event evListner;
        public void Initialize(InitializationEngine context)
        {
            contentEv = context.Locate.ContentEvents();
            contentEv.PublishedContent += ContentEv_PublishedContent;

            catalogEv = ServiceLocator.Current.GetInstance<ICatalogEvents>();
            catalogEv.EntryUpdated += CatalogEv_EntryUpdated;

            evListner = Event.Get(CatalogEventBroadcaster.CommerceProductUpdated);
            evListner.Raised += EvListner_Raised;
        }

        private void EvListner_Raised(object sender, EPiServer.Events.EventNotificationEventArgs e)
        {
            CatalogContentUpdateEventArgs eventArgs = DeSerialize((byte[])e.Param);
            
            if(eventArgs.EventType == CatalogEventBroadcaster.CatalogEntryUpdatedEventType)
            {
                int entryId = eventArgs.CatalogEntryIds.First();
                ReferenceConverter refConvert = ServiceLocator.Current.GetInstance<ReferenceConverter>();
                ContentReference catRef = refConvert.GetEntryContentLink(entryId);
                IContentLoader loader = ServiceLocator.Current.GetInstance<IContentLoader>();
                var catEntry = loader.Get<IContent>(catRef);

                var info = new List<string>
                {
                    "Remote Catalog Event Fired!",
                    $"The name of the item updated: {catEntry.Name}"
                };
                WriteToTextFile(info);
            }
        }

        private void CatalogEv_EntryUpdated(object sender, EntryEventArgs e)
        {
            var chg = e.Changes.First();
            ReferenceConverter refConvert = ServiceLocator.Current.GetInstance<ReferenceConverter>();
            ContentReference catRef = refConvert.GetEntryContentLink(chg.EntryId);
            IContentLoader loader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var catEntry = loader.Get<IContent>(catRef);

            var info = new List<string>
                {
                    "Entry Updated Catalog Event Fired!",
                    $"The name of the item updated: {catEntry.Name}"
                };
            WriteToTextFile(info);
        }

        private void ContentEv_PublishedContent(object sender, ContentEventArgs e)
        {
            var info = new List<string>
                {
                    "Published Content Event Fired!",
                    $"The name of the item published: {e.Content.Name}"
                };
            WriteToTextFile(info);
        }

        public void Uninitialize(InitializationEngine context)
        {
            catalogEv.EntryUpdated -= CatalogEv_EntryUpdated;
            contentEv.PublishedContent -= ContentEv_PublishedContent;
            evListner.Raised -= EvListner_Raised;
        }

        private void WriteToTextFile(List<string> lines)
        {
            string mydocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (lines != null && lines.Count > 0)
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(mydocPath, "EventsDemo.txt"), true))
                {
                    foreach (string line in lines)
                    {
                        outputFile.WriteLine(line);
                    }
                    outputFile.WriteLine();
                }
            }

        }

        private CatalogContentUpdateEventArgs DeSerialize(byte[] buffer)
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream(buffer);
            return formatter.Deserialize(stream) as CatalogContentUpdateEventArgs;
        }
    }
}