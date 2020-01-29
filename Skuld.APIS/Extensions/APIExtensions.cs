﻿using Booru.Net;
using Discord;
using HtmlAgilityPack;
using Kitsu.Anime;
using Kitsu.Manga;
using Skuld.APIS.Social.Reddit.Models;
using Steam.Models.SteamStore;
using SteamStoreQuery;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Skuld.APIS.Extensions
{
    public static class APIExtensions
    {
        private static readonly Random rnd = new Random();

        private static readonly string[] VideoExtensions = {
            ".webm",
            ".mkv",
            ".flv",
            ".vob",
            ".ogv",
            ".ogg",
            ".avi",
            ".mov",
            ".qt",
            ".wmv",
            ".mp4",
            ".m4v",
            ".mpg",
            ".mpeg"
        };

        private static readonly string[] ImageExtensions =
        {
            ".jpg",
            ".bmp",
            ".gif",
            ".png",
            ".apng"
        };

        public static List<string> BlacklistedTags { get; } = new List<string>
        {
            "loli",
            "shota",
            "cub",
            "gore",
            "guro",
            "vore",
            "death"
        };

        public static T RandomValue<T>(this IEnumerable<T> entries) where T : class
        {
            var list = entries.ToList();

            var index = rnd.Next(0, list.Count);

            return list[index];
        }

        public static StoreScreenshotModel Random(this IReadOnlyList<StoreScreenshotModel> elements)
            => elements[rnd.Next(0, elements.Count)];

        public static bool IsImageExtension(this string input)
        {
            foreach (var ext in ImageExtensions)
            {
                if (input.Contains(ext))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsVideoFile(this string input)
        {
            foreach (var x in VideoExtensions)
            {
                if (input.Contains(x) || input.EndsWith(x))
                    return true;
            }
            return false;
        }

        //https://gist.github.com/starquake/8d72f1e55c0176d8240ed336f92116e3
        public static string StripHtml(this string value)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(value);

            if (htmlDoc == null)
                return value;

            return htmlDoc.DocumentNode.InnerText;
        }

        #region Pagination

        public static IList<string> PaginateList(this IReadOnlyList<AnimeDataModel> list, int maxrows = 10)
        {
            var pages = new List<string>();
            string pagetext = "";

            for (int x = 0; x < list.Count; x++)
            {
                var obj = list[x];

                pagetext += $"{x + 1}. {obj.Attributes.CanonicalTitle}\n";

                if ((x + 1) % maxrows == 0 || (x + 1) == list.Count)
                {
                    pages.Add(pagetext);
                    pagetext = "";
                }
            }

            return pages;
        }

        public static IList<string> PaginateList(this IReadOnlyList<MangaDataModel> list, int maxrows = 10)
        {
            var pages = new List<string>();
            string pagetext = "";

            for (int x = 0; x < list.Count; x++)
            {
                var obj = list[x];

                pagetext += $"{x + 1}. {obj.Attributes.CanonicalTitle}\n";

                if ((x + 1) % maxrows == 0 || (x + 1) == list.Count)
                {
                    pages.Add(pagetext);
                    pagetext = "";
                }
            }

            return pages;
        }

        public static IList<string> PaginateList(this IReadOnlyList<Listing> list, int maxrows = 10)
        {
            var pages = new List<string>();
            string pagetext = "";

            for (int x = 0; x < list.Count; x++)
            {
                var obj = list[x];

                pagetext += $"{x + 1}. {obj.Name}\n";

                if ((x + 1) % maxrows == 0 || (x + 1) == list.Count)
                {
                    pages.Add(pagetext);
                    pagetext = "";
                }
            }

            return pages;
        }

        public static IList<string> PaginatePosts(this Post[] posts, ITextChannel channel, int maxrows = 10)
        {
            var Pages = new List<string>();

            string pagetext = "";

            for (int x = 0; x < posts.Length; x++)
            {
                var post = posts[x];

                string txt = $"[{post.Data.Title}](https://reddit.com{post.Data.Permalink})\n";

                if (post.Data.Over18 && channel.IsNsfw)
                {
                    pagetext += "**NSFW** " + txt;
                }
                else
                {
                    pagetext += txt;
                }
                pagetext += "\n";

                if ((x + 1) % maxrows == 0 || (x + 1) == posts.Length)
                {
                    Pages.Add(pagetext);
                    pagetext = "";
                }
            }
            return Pages;
        }

        #endregion

        #region Booru

        public static string GetMessage(this BooruImage image, string postUrl)
        {
            string message = $"`Score: {image.Score}` <{postUrl}>\n{image.ImageUrl}";

            if (image.ImageUrl.IsVideoFile())
            {
                message += " (Video)";
            }

            return message;
        }

        public static IList<string> AddBlacklistedTags(this IList<string> tags)
        {
            var newtags = new List<string>();
            newtags.AddRange(tags);
            BlacklistedTags.ForEach(x => newtags.Add("-" + x));
            return newtags;
        }

        public static bool ContainsBlacklistedTags(this string[] tags)
        {
            bool returnvalue = false;
            foreach (var tag in tags)
            {
                if (BlacklistedTags.Contains(tag.ToLowerInvariant()))
                {
                    returnvalue = true;
                }
            }
            return returnvalue;
        }

        #endregion
    }
}