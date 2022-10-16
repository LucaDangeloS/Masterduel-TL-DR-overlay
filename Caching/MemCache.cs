using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Masterduel_TLDR_overlay.Masterduel;
using System.Runtime.Caching;
using static Masterduel_TLDR_overlay.Screen.ImageProcessing;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Drawing.Text;

namespace Masterduel_TLDR_overlay.Caching
{
    internal class MemCache
    {
        private static readonly CacheItemPolicy shortTermCachePolicy = new();
        private static readonly CacheItemPolicy longTermCachePolicy = new();
        private readonly MemoryCache NonCardsCache = new("NotCardsCacheBins");
        private readonly MemoryCache CardsCache = new("CardCacheBins");
        public CardInfo? LastLookup;
        private readonly int MaxPixelDiff;
        private readonly int Bins;

        public MemCache(int maxPixelDiff, int splashSize)
        {
            shortTermCachePolicy.SlidingExpiration = TimeSpan.FromMinutes(5);
            shortTermCachePolicy.RemovedCallback = RenovateNonCardsCache;
            longTermCachePolicy.SlidingExpiration = TimeSpan.FromMinutes(20);
            longTermCachePolicy.RemovedCallback = RenovateCardsCache;
            Bins = (int) Math.Floor(Math.Pow(splashSize, 2) / maxPixelDiff);
            MaxPixelDiff = maxPixelDiff;
            
            for (int i = 0; i <= Bins; i++)
            {
                CardsCache.Add(new CacheItem((i).ToString(), new List<Dictionary<ImageHash, CardInfo>>()), longTermCachePolicy);
            }

            for (int i = 0; i <= Bins; i++)
            {
                NonCardsCache.Add(new CacheItem((i).ToString(), new List<ImageHash>()), shortTermCachePolicy);
            }

        }

        public void AddToCache(ImageHash hash, CardInfo? card)
        {
            var bin = hash.HashSum / MaxPixelDiff;

            if (card == null)
            {
                var list = (List<ImageHash>) NonCardsCache.GetCacheItem(bin.ToString()).Value;
                list.Add(hash);
            }
            else
            {
                var list = (List<Dictionary<ImageHash, CardInfo>>) CardsCache.GetCacheItem(bin.ToString()).Value;
                list.Add(new Dictionary<ImageHash, CardInfo>() { { hash, card } });
            }
        }

        public bool CheckInCache(ImageHash hash, float precision)
        {
            var bin = hash.HashSum / MaxPixelDiff;
            // TODO: Fix cache getting null references
            var list = (List<ImageHash>)NonCardsCache.AddOrGetExisting(bin.ToString(), new List<ImageHash>(), shortTermCachePolicy);

            ImageHash? res = list.Find((x) => x.CompareTo(hash) >= precision);
            
            if (res != null)
            {
                LastLookup = null;
                return true;
            }
            
            var list2 = (List<Dictionary<ImageHash, CardInfo>>) CardsCache.GetCacheItem(bin.ToString()).Value;

            foreach (var dict in list2)
            {
                foreach (var item in dict)
                {
                    if (item.Key.CompareTo(hash) >= precision)
                    {
                        LastLookup = item.Value;
                        return true;
                    }
                }
            }
            return false;
        }
        

        // Private methods
        private void RenovateNonCardsCache(CacheEntryRemovedArguments args)
        {
            NonCardsCache.Add(new CacheItem(args.CacheItem.Key, new List<ImageHash>())
                , shortTermCachePolicy);
        }
        private void RenovateCardsCache(CacheEntryRemovedArguments args)
        {
            CardsCache.Add(new CacheItem(args.CacheItem.Key, new List<Dictionary<ImageHash, CardInfo>>())
                , longTermCachePolicy);
        }
    }
}
