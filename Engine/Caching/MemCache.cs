using TLDROverlay.Masterduel;
using System.Runtime.Caching;
using static TLDROverlay.Screen.ImageProcessing;
using System.Timers;
using System.Diagnostics;

namespace TLDROverlay.Caching
{
    internal class MemCache
    {
        private static readonly CacheItemPolicy shortTermCachePolicy = new();
        private static readonly CacheItemPolicy longTermCachePolicy = new();
        private int MaxCacheSize = PropertiesLoader.Instance.Properties.MAX_CACHE_SIZE;
        private readonly int MaxPixelDiff;
        private readonly int Bins;
        private MemoryCache NonCardsCache;
        private MemoryCache CardsCache;
        public CardInfo? LastLookup;

        private int CacheTimerPollInterval = 10000;
        private TimeSpan MaxCacheLifetime = TimeSpan.FromMinutes(1);
        private List<DateTime> LastAccessedCardsCacheTimes;
        private List<DateTime> LastAccessedNonCardsCacheTimes;
        private List<bool> ActiveNonCardsCacheItems;
        private List<bool> ActiveCardsCacheItems;
        private static System.Threading.Timer CacheTimer;

        public MemCache(int maxPixelDiff, int splashSize)
        {
            Bins = (int)Math.Floor(Math.Pow(splashSize, 2) / maxPixelDiff);
            int n_bins = Bins + 1;
            MaxPixelDiff = maxPixelDiff;
            
            LastAccessedCardsCacheTimes = Enumerable.Repeat(DateTime.Now, n_bins).ToList(); ;
            LastAccessedNonCardsCacheTimes = Enumerable.Repeat(DateTime.Now, n_bins).ToList();
            ActiveNonCardsCacheItems = Enumerable.Repeat(false, n_bins).ToList();
            ActiveCardsCacheItems = Enumerable.Repeat(false, n_bins).ToList();
            CacheTimer = new System.Threading.Timer(CheckCacheExpiry, null, CacheTimerPollInterval, Timeout.Infinite);
            //cacheTimer.Elapsed += new ElapsedEventHandler(CheckCacheExpiry);
            
            InitializeCache();
        }

        public void AddToCache(ImageHash hash, CardInfo? card)
        {
            var bin = hash.HashSum / MaxPixelDiff;
            // Check it if breks if a card with nothing on it is cached

            if (card == null)
            {
                var list = GetNonCardsCache(bin);
                list.AddLast(hash);
                if (list.Count > MaxCacheSize)
                {
                    list.RemoveFirst();
                }
                ActiveNonCardsCacheItems[bin] = true;
                LastAccessedNonCardsCacheTimes[bin] = DateTime.Now;
            }
            else
            {
                var list = GetCardsCache(bin);
                list.AddLast(new Dictionary<ImageHash, CardInfo>() { { hash, card } });
                if (list.Count > MaxCacheSize)
                {
                    list.RemoveFirst();
                }
                ActiveCardsCacheItems[bin] = true;
                LastAccessedCardsCacheTimes[bin] = DateTime.Now;
            }
        }

        public bool CheckInCache(ImageHash hash)
        {
            var bin = hash.HashSum / MaxPixelDiff;
            // TODO: Fix cache getting null references
            var NonCardsList = GetNonCardsCache(bin);
            ImageHash? res = null;

            foreach (var item in NonCardsList)
            {
                if (item.Equals(hash))
                {
                    res = item;
                    LastAccessedNonCardsCacheTimes[bin] = DateTime.Now;
                    break;
                }
            }

            if (res != null)
            {
                LastLookup = null;
                return true;
            }

            var CardsList = GetCardsCache(bin);

            foreach (var dict in CardsList)
            {
                foreach (var item in dict)
                {
                    if (item.Key.Equals(hash))
                    {
                        LastLookup = item.Value;
                        LastAccessedCardsCacheTimes[bin] = DateTime.Now;
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
        
        private LinkedList<ImageHash> GetNonCardsCache(int bin)
        {
            return (LinkedList<ImageHash>)NonCardsCache.AddOrGetExisting(bin.ToString(), new LinkedList<ImageHash>(), shortTermCachePolicy);
        }

        private LinkedList<Dictionary<ImageHash, CardInfo>> GetCardsCache(int bin)
        {
            return (LinkedList<Dictionary<ImageHash, CardInfo>>)CardsCache.GetCacheItem(bin.ToString()).Value;
        }

        private void CheckCacheExpiry(object? source)
        {
            // If any cache access time is older than MaxCacheLifetime, reset entry
            Debug.WriteLine("Checking cache expiry");
            for (int i = 0; i < Bins; i++)
            {
                if (ActiveNonCardsCacheItems[i] && (DateTime.Now - LastAccessedNonCardsCacheTimes[i]) > MaxCacheLifetime)
                {
                    Debug.WriteLine($"Entry {i} of NonCardsCache expired");
                    ActiveNonCardsCacheItems[i] = false;
                    GetNonCardsCache(i).Clear();
                }
            };

            for (int i = 0; i < Bins; i++)
            {
                if (ActiveCardsCacheItems[i] && (DateTime.Now - LastAccessedCardsCacheTimes[i]) > MaxCacheLifetime)
                {
                    Debug.WriteLine($"Entry {i} of CardsCache expired");
                    ActiveCardsCacheItems[i] = false;
                    GetCardsCache(i).Clear();
                }
            };
            CacheTimer.Change(CacheTimerPollInterval, Timeout.Infinite);
        }

        //private void ClearCacheEntry(int bin)
        //{

        //}

        //private void RenovateNonCardsCache(CacheEntryRemovedArguments args)
        //{
        //    NonCardsCache.Add(new CacheItem(args.CacheItem.Key, new LinkedList<ImageHash>())
        //        , shortTermCachePolicy);
        //}
        //private void RenovateCardsCache(CacheEntryRemovedArguments args)
        //{
        //    CardsCache.Add(new CacheItem(args.CacheItem.Key, new LinkedList<Dictionary<ImageHash, CardInfo>>())
        //        , longTermCachePolicy);
        //}
        //private void UpdateNonCardsCache(CacheEntryUpdateArguments args)
        //{
        //    var list = (LinkedList<ImageHash>) NonCardsCache.GetCacheItem(args.Key).Value;
        //    if (list.Count > MaxCacheSize)
        //        list.RemoveFirst();
        //}
        //private void UpdateCardsCache(CacheEntryUpdateArguments args)
        //{
        //    var list = (LinkedList<Dictionary<ImageHash, CardInfo>>) CardsCache.GetCacheItem(args.Key).Value;
        //    if (list.Count > MaxCacheSize)
        //        list.RemoveFirst();
        //}
    }
}
