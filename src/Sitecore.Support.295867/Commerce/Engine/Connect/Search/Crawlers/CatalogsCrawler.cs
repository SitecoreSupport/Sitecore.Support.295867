using Sitecore.Diagnostics;
using System.Collections.Generic;
using Sitecore.Commerce.Plugin.ManagedLists;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Engine.Connect;
using Sitecore.Commerce.Engine.Connect.Search;
using Sitecore.Commerce.Engine.Connect.Search.Models;
using Sitecore.Data;
using Sitecore.Commerce.Engine.Connect.Search.Crawlers;

namespace Sitecore.Support.Commerce.Engine.Connect.Search.Crawlers
{
    public class CatalogsCrawler : CatalogCrawlerBase<Catalog>
    {
        private static readonly List<ID> InternalTemplateIds = new List<ID>
        {
            CommerceConstants.KnownTemplateIds.CommerceCatalogTemplate,
        };

        protected override IEnumerable<ID> IndexableTemplateIds => InternalTemplateIds;

        protected override ManagedList GetCatalogEntitiesToIndex(string environment, string listName, int itemsToSkip, int itemsToTake)
        {
            var itemsList = IndexUtility.GetCatalogsToIndex(environment, listName, itemsToSkip, itemsToTake);
            return itemsList;
        }

        protected override List<CommerceCatalogIndexableItem> GetItemsToIndex(ManagedList searchResults, Dictionary<string, bool> mappedCatalogsCache)
        {
            Assert.ArgumentNotNull(searchResults, nameof(searchResults));
            Assert.ArgumentNotNull(mappedCatalogsCache, nameof(mappedCatalogsCache));

            var indexableList = new List<CommerceCatalogIndexableItem>();
            foreach (Catalog catalog in searchResults.Items)
            {
                var isMapped = false;
                if (!mappedCatalogsCache.TryGetValue(catalog.SitecoreId, out isMapped))
                {
                    var catalogPathIdList = this.Repository.GetPathIdsForSitecoreId(catalog.SitecoreId);
                    isMapped = (catalogPathIdList != null && catalogPathIdList.Count > 0);
                    mappedCatalogsCache[catalog.SitecoreId] = isMapped;
                }

                if (isMapped)
                {
                    #region modified part. Added check for Null to prevent adding a Null element to the indexableList
                    CommerceCatalogIndexableItem item = this.TryGetItem(catalog, catalog.SitecoreId);
                    if (item != null)
                    {
                        indexableList.Add(item);
                    }
                    #endregion
                }
            }

            return indexableList;
        }
    }
}