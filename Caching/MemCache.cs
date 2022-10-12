using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Masterduel_TLDR_overlay.Masterduel;
using System.Runtime.Caching;
using static Masterduel_TLDR_overlay.Screen.ImageProcessing;
using System.Security.Cryptography.X509Certificates;

namespace Masterduel_TLDR_overlay.Caching
{
    internal class MemCache
    {
        private static readonly CacheItemPolicy shortTermCachePolicy = new();
        private static readonly CacheItemPolicy longTermCachePolicy = new();
        private readonly MemoryCache NonCardsCache = new("NotACard");
        private BinaryTree NonCardsBTree = new();
        private BinaryTree CardsBTree = new();
        private readonly MemoryCache CardsCache = new("Cards");
        public CardInfo? LastLookup;
        private int MaxPixelDiff;

        public MemCache(int maxPixelDiff)
        {
            shortTermCachePolicy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(20);
            longTermCachePolicy.AbsoluteExpiration = DateTimeOffset.Now.AddHours(3);
            MaxPixelDiff = maxPixelDiff;
            //NonCardsCache.Add("NotACard", NonCardsBTree, shortTermCachePolicy);
            //CardsCache.Add("Cards", CardsBTree, longTermCachePolicy);

            //var testElement = (BinaryTree)NonCardsCache.GetCacheItem("NotACard").Value;

            //testElement.Add
            //    (
            //        new Node(new ImageHash(new List<bool> { true, true, true, true, false, false, false, false, true }, (3, 3)), new CardInfo("Not a card", "This is not a card"))
            //    );
            //testElement.Add
            //    (
            //        new Node(new ImageHash(new List<bool> { true, true, false, false, false, false, false, false, true }, (3, 3)), new CardInfo("Not a card", "This is not a card"))
            //    );
            //testElement.Add
            //    (
            //        new Node(new ImageHash(new List<bool> { true, true, true, true, true, false, false, true, true }, (3, 3)), new CardInfo("Not a card", "This is not a card"))
            //    );
            //testElement.Add
            //    (
            //        new Node(new ImageHash(new List<bool> { true, true, true, true, true, true, true, true, true }, (3, 3)), new CardInfo("Not a card", "This is not a card"))
            //    );
            //testElement.Add
            //    (
            //        new Node(new ImageHash(new List<bool> { true, true, true, true, false, false, false, true, true }, (3, 3)), new CardInfo("Not a card", "This is not a card"))
            //    );
            //var testElement2 = (BinaryTree)NonCardsCache.GetCacheItem("NotACard").Value;
            //var testElement3 = testElement2.Search(10, 2);
        }

        public void AddToCache(ImageHash hash, CardInfo? card)
        {
            if (card == null)
            {
                NonCardsBTree.Add(new Node(hash, card), MaxPixelDiff);
            }
            else
            {
                CardsBTree.Add(new Node(hash, card), MaxPixelDiff);
            }
        }

        public bool CheckInCache(ImageHash hash, float precision)
        {
            var searchRes = CardsBTree.Search(hash.HashSum, MaxPixelDiff);
            if (searchRes != null && searchRes.Hash.Any((x) => x.CompareTo(hash) >= precision))
            {
                LastLookup = searchRes.Card;
                return true;
            }
            searchRes = NonCardsBTree.Search(hash.HashSum, MaxPixelDiff);
            if (searchRes != null && searchRes.Hash.Any((x) => x.CompareTo(hash) >= precision))
            {
                LastLookup = searchRes.Card;
                return true;
            }
            return false;
        }

        // -------------------------- Binary Tree -------------------------- //
        protected class Node
        {
            public Node? Left, Right;
            public int HashSum;
            public List<ImageHash> Hash = new();
            public CardInfo? Card;

            public Node(ImageHash hash, CardInfo? card)
            {
                Hash.Add(hash);
                HashSum = hash.HashSum;
                Card = card;
            }

            public void AddImage(ImageHash im)
            {
                Hash.Add(im);
                HashSum = Hash.Sum(x => x.HashSum) / Hash.Count;
            }
        }

        protected class BinaryTree
        {
            Node? root;

            public BinaryTree(ImageHash hash, CardInfo? card)
            {
                root = new Node(hash, card);
            }

            public BinaryTree()
            {
                root = null;
            }

            public void Add(Node node, int maxDiff)
            {
                if (root == null)
                {
                    root = node;
                    return;
                }

                Node current = root;
                
                while (true)
                {
                    if (node.HashSum < current.HashSum - maxDiff)
                    {
                        if (current.Left == null)
                        {
                            current.Left = node;
                            break;
                        }
                        current = current.Left;
                    }
                    else if (node.HashSum > current.HashSum + maxDiff)
                    {
                        if (current.Right == null)
                        {
                            current.Right = node;
                            break;
                        }
                        current = current.Right;
                    }
                    else
                    {
                        // Duplicates
                        current.AddImage(node.Hash.First());
                        break;
                    }
                }
            }

            /// <summary>
            /// Returns the node with the closest hashSum to the given hashSum.
            /// </summary>
            /// <param name="hashSum">Sum of the Hash list of an image.</param>
            /// <param name="maxDiff">The maximum abs difference from the hashSum.</param>
            /// <returns>Returns <see cref="Node"/> element. If no results were found returns null.</returns>
            public Node? Search(int hashSum, int maxDiff)
            {
                List<Node> resultsList = new();

                RecursiveSearch(ref resultsList, root, hashSum, maxDiff);

                resultsList.Sort((a, b) => Math.Abs(a.HashSum - hashSum).CompareTo(Math.Abs(b.HashSum - hashSum)));

                return resultsList.FirstOrDefault();
            }
            
            // Private methods
            private void RecursiveSearch(ref List<Node> resultsList, 
                Node? currentNode, int hashSum, int maxDiff)
            {
                if (currentNode == null)
                    return;

                int currDiff = currentNode.HashSum - hashSum;

                if (currDiff < -maxDiff) // Value is too small, go through the Right
                {
                    if (currentNode.Right == null) return;
                    RecursiveSearch(ref resultsList, currentNode.Right, hashSum, maxDiff);
                }
                else if (currDiff > maxDiff) // Value is too big, go through the Left
                {
                    if (currentNode.Left == null) return;
                    RecursiveSearch(ref resultsList, currentNode.Left, hashSum, maxDiff);
                } 
                else
                {
                    resultsList.Add(currentNode);

                    int absDiff = Math.Abs(currDiff);

                    // Left branch
                    RecursiveSearch(ref resultsList, currentNode.Left, hashSum, maxDiff);

                    // Right branch
                    RecursiveSearch(ref resultsList, currentNode.Right, hashSum, maxDiff);
                }
            }
        }

        // ----------------------------------------------------------------- //
    }
}
