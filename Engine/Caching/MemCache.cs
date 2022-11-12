using TLDROverlay.Masterduel;
using System.Runtime.Caching;
using static TLDROverlay.Screen.ImageProcessing;

namespace TLDROverlay.Caching
{
    internal class MemCache
    {
        private static readonly CacheItemPolicy shortTermCachePolicy = new();
        private static readonly CacheItemPolicy longTermCachePolicy = new();
        private readonly int MaxPixelDiff;
        private readonly int Bins;
        private MemoryCache NonCardsCache;
        private MemoryCache CardsCache;
        private int MaxCacheSize = PropertiesLoader.Instance.Properties.MAX_CACHE_SIZE;
        public CardInfo? LastLookup;

        public MemCache(int maxPixelDiff, int splashSize)
        {
            Bins = (int)Math.Floor(Math.Pow(splashSize, 2) / maxPixelDiff);
            MaxPixelDiff = maxPixelDiff;

            InitializeCache();
        }

        public void AddToCache(ImageHash hash, CardInfo? card)
        {
            var bin = hash.HashSum / MaxPixelDiff;
            // Check it if breks if a card with nothing on it is cached

            if (card == null)
            {
                var list = (LinkedList<ImageHash>) NonCardsCache.GetCacheItem(bin.ToString()).Value;
                list.AddLast(hash);
                if (list.Count > MaxCacheSize)
                    list.RemoveFirst();
            }
            else
            {
                var list = (LinkedList<Dictionary<ImageHash, CardInfo>>) CardsCache.GetCacheItem(bin.ToString()).Value;
                list.AddLast(new Dictionary<ImageHash, CardInfo>() { { hash, card } });
                if (list.Count > MaxCacheSize)
                    list.RemoveFirst();
            }
        }

        public bool CheckInCache(ImageHash hash)
        {
            var bin = hash.HashSum / MaxPixelDiff;
            // TODO: Fix cache getting null references
            var list = (LinkedList<ImageHash>)NonCardsCache.AddOrGetExisting(bin.ToString(), new LinkedList<ImageHash>(), shortTermCachePolicy);

            ImageHash? res = null;
            foreach (var item in list)
            {
                if (item.Equals(hash))
                {
                    res = item;
                    break;
                }
            }

            if (res != null)
            {
                LastLookup = null;
                return true;
            }
            
            var list2 = (LinkedList<Dictionary<ImageHash, CardInfo>>) CardsCache.GetCacheItem(bin.ToString()).Value;

            foreach (var dict in list2)
            {
                foreach (var item in dict)
                {
                    if (item.Key.Equals(hash))
                    {
                        LastLookup = item.Value;
                        return true;
                    }
                }
            }
            return false;
        }
        
        public void ClearCache()
        {
            NonCardsCache.Dispose();
            CardsCache.Dispose();
            InitializeCache();
        }

        // Private methods
        private void InitializeCache()
        {
            // TODO: Have sliding expiration policies in cache
            //shortTermCachePolicy.SlidingExpiration = TimeSpan.FromMinutes(1);
            //shortTermCachePolicy.UpdateCallback = UpdateNonCardsCache;
            //longTermCachePolicy.SlidingExpiration = TimeSpan.FromMinutes(20);
            //longTermCachePolicy.UpdateCallback = UpdateCardsCache;
            NonCardsCache = new("NotCardsCacheBins");
            CardsCache = new("CardCacheBins");

            for (int i = 0; i <= Bins; i++)
            {
                CardsCache.Add(new CacheItem(i.ToString(), new LinkedList<Dictionary<ImageHash, CardInfo>>()), longTermCachePolicy);
            }

            for (int i = 0; i <= Bins; i++)
            {
                NonCardsCache.Add(new CacheItem(i.ToString(), new LinkedList<ImageHash>()), shortTermCachePolicy);
            }
        }
        
        private void RenovateNonCardsCache(CacheEntryRemovedArguments args)
        {
            NonCardsCache.Add(new CacheItem(args.CacheItem.Key, new LinkedList<ImageHash>())
                , shortTermCachePolicy);
        }
        private void RenovateCardsCache(CacheEntryRemovedArguments args)
        {
            CardsCache.Add(new CacheItem(args.CacheItem.Key, new LinkedList<Dictionary<ImageHash, CardInfo>>())
                , longTermCachePolicy);
        }
        private void UpdateNonCardsCache(CacheEntryUpdateArguments args)
        {
            var list = (LinkedList<ImageHash>) NonCardsCache.GetCacheItem(args.Key).Value;
            if (list.Count > MaxCacheSize)
                list.RemoveFirst();
        }
        private void UpdateCardsCache(CacheEntryUpdateArguments args)
        {
            var list = (LinkedList<Dictionary<ImageHash, CardInfo>>) CardsCache.GetCacheItem(args.Key).Value;
            if (list.Count > MaxCacheSize)
                list.RemoveFirst();
        }
    }
}
