﻿using Booru.Net;
using Discord;
using Discord.Commands;
using ImageMagick;
using IqdbApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Skuld.APIS;
using Skuld.APIS.Animals.Models;
using Skuld.APIS.Extensions;
using Skuld.APIS.NekoLife.Models;
using Skuld.APIS.WebComics.CAD.Models;
using Skuld.APIS.WebComics.Explosm.Models;
using Skuld.APIS.WebComics.XKCD.Models;
using Skuld.Bot.Extensions;
using Skuld.Bot.Globalization;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord.Attributes;
using Skuld.Discord.Extensions;
using Skuld.Discord.Preconditions;
using Skuld.Discord.Services;
using StatsdClient;
using SysEx.Net;
using SysEx.Net.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WenceyWang.FIGlet;

namespace Skuld.Bot.Commands
{
    [Group, RequireEnabledModule]
    public class Fun : ModuleBase<ShardedCommandContext>
    {
        public Random Random { get; set; }
        public AnimalClient Animals { get; set; }
        public Locale Locale { get; set; }
        public WebComicClients ComicClients { get; set; }
        public SysExClient SysExClient { get; set; }
        public YNWTFClient YNWTFcli { get; set; }
        public BooruClient BooruClient { get; set; }
        public NekosLifeClient NekoLife { get; set; }
        public IqdbClient IqdbClient { get; set; }
        private CommandService CommandService { get => BotService.CommandService; }

        private static readonly string[] eightball = {
            "SKULD_FUN_8BALL_YES1",
            "SKULD_FUN_8BALL_YES2",
            "SKULD_FUN_8BALL_YES3",
            "SKULD_FUN_8BALL_YES4",
            "SKULD_FUN_8BALL_YES5",
            "SKULD_FUN_8BALL_YES6",
            "SKULD_FUN_8BALL_YES7",
            "SKULD_FUN_8BALL_YES8",
            "SKULD_FUN_8BALL_YES9",
            "SKULD_FUN_8BALL_YES10",
            "SKULD_FUN_8BALL_MAYBE1",
            "SKULD_FUN_8BALL_MAYBE2",
            "SKULD_FUN_8BALL_MAYBE3",
            "SKULD_FUN_8BALL_MAYBE4",
            "SKULD_FUN_8BALL_MAYBE5",
            "SKULD_FUN_8BALL_NO1",
            "SKULD_FUN_8BALL_NO2",
            "SKULD_FUN_8BALL_NO3",
            "SKULD_FUN_8BALL_NO4",
            "SKULD_FUN_8BALL_NO5"
        };

        [Command("fuse"), Summary("Fuses 2 of the 1st generation pokemon")]
        public async Task Fuse(int int1, int int2)
        {
            if (int1 > 151 || int1 < 0) await EmbedExtensions.FromError($"{int1} over/under limit. (151)", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            else if (int2 > 151 || int2 < 0) await EmbedExtensions.FromError($"{int2} over/under limit. (151)", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            else
            {
                await EmbedExtensions.FromMessage(Context)
                    .WithImageUrl($"http://images.alexonsager.net/pokemon/fused/{int1}/{int1}.{int2}.png")
                    .QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        #region WeebAnimals

        [Command("neko"), Summary("neko grill"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Neko()
        {
            var neko = await NekoLife.GetAsync(NekoImageType.Neko).ConfigureAwait(false);
            DogStatsd.Increment("web.get");
            if (neko != null) await EmbedExtensions.FromMessage(Context).WithImageUrl(neko).QueueMessageAsync(Context).ConfigureAwait(false);
            else await EmbedExtensions.FromError("Hmmm <:Thunk:350673785923567616> I got an empty response.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("kitsune"), Summary("Kitsunemimi Grill"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Kitsune()
        {
            var kitsu = await SysExClient.GetKitsuneAsync().ConfigureAwait(false);
            DogStatsd.Increment("web.get");
            await EmbedExtensions.FromMessage(Context).WithImageUrl(kitsu).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        #endregion WeebAnimals

        #region Animals

        [Command("kitty"), Summary("kitty"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("cat", "cats", "kittycat", "kitty cat", "meow", "kitties", "kittys")]
        public async Task Kitty()
        {
            var kitty = await Animals.GetAnimalAsync(AnimalType.Kitty).ConfigureAwait(false);
            DogStatsd.Increment("web.get");

            if (kitty.IsVideoFile())
                await kitty.QueueMessageAsync(Context).ConfigureAwait(false);
            if (kitty == "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg")
                await EmbedExtensions.FromImage(kitty, Color.Red, Context).QueueMessageAsync(Context, content: "Both the api's are down, that makes the sad a big sad. <:blobcry:350681079415439361>").ConfigureAwait(false);
            else
                await EmbedExtensions.FromImage(kitty, EmbedExtensions.RandomEmbedColor(), Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("doggo"), Summary("doggo"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("dog", "dogs", "doggy")]
        public async Task Doggo()
        {
            var doggo = await Animals.GetAnimalAsync(AnimalType.Doggo).ConfigureAwait(false);
            DogStatsd.Increment("web.get");
            if (doggo.IsVideoFile())
                await doggo.QueueMessageAsync(Context).ConfigureAwait(false);
            if (doggo == "https://i.imgur.com/ZSMi3Zt.jpg")
                await EmbedExtensions.FromImage(doggo, Color.Red, Context).QueueMessageAsync(Context, content: "Both the api's are down, that makes the sad a big sad. <:blobcry:350681079415439361>").ConfigureAwait(false);
            else
                await EmbedExtensions.FromImage(doggo, EmbedExtensions.RandomEmbedColor(), Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("bird"), Summary("birb"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("birb")]
        public async Task Birb()
        {
            var birb = await Animals.GetAnimalAsync(AnimalType.Bird).ConfigureAwait(false);
            DogStatsd.Increment("web.get");
            if (birb.IsVideoFile())
                await birb.QueueMessageAsync(Context).ConfigureAwait(false);
            else
                await EmbedExtensions.FromImage(birb, EmbedExtensions.RandomEmbedColor(), Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("llama"), Summary("Llama"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Llama()
        {
            var llama = await SysExClient.GetLlamaAsync().ConfigureAwait(false);
            DogStatsd.Increment("web.get");
            await EmbedExtensions.FromImage(llama, EmbedExtensions.RandomEmbedColor(), Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("seal"), Summary("Seal"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Seal()
        {
            var seal = await SysExClient.GetSealAsync().ConfigureAwait(false);
            DogStatsd.Increment("web.get");
            await EmbedExtensions.FromImage(seal, EmbedExtensions.RandomEmbedColor(), Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        #endregion Animals

        #region RNG

        [Command("eightball"), Summary("Eightball")]
        [Alias("8ball")]
        public async Task Eightball([Remainder]string question = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var user = await Database.GetUserAsync(Context.User).ConfigureAwait(false);

            var answer = Locale.GetLocale(user.Language).GetString(eightball[Random.Next(0, eightball.Length)]);

            var message = "";

            if (!string.IsNullOrEmpty(question))
                message = $"Your Question:\n{question}\n\n";

            message += $"And the :8ball: says:\n{answer}";

            await
                new EmbedBuilder()
                    .AddAuthor(Context.Client)
                    .AddFooter(Context)
                    .WithDescription(message)
                    .WithRandomColor()
                    .QueueMessageAsync(Context, type: Discord.Models.MessageType.Mention)
                .ConfigureAwait(false);
        }

        [Command("roll"), Summary("Roll a die")]
        public async Task Roll(string roll)
        {
            if (int.TryParse(roll, out int upper))
            {
                await
                    EmbedExtensions.FromMessage(SkuldAppContext.GetCaller(),
                                                $"{Context.User.Mention} just rolled and got a {Random.Next(1, (upper + 1))}",
                                                Color.Teal,
                                                Context)
                    .QueueMessageAsync(Context)
                    .ConfigureAwait(false);
            }
            else
            {
            }
        }

        [Command("choose"), Summary("Choose from things, eg: \"reading books\" \"playing games\"")]
        public async Task Choose(params string[] choices)
        {
            if (choices.Any())
            {
                var choice = choices[Random.Next(0, choices.Length)];

                int critereon = 0;
                int maxCritereon = 3;
                while (string.IsNullOrEmpty(choice) || string.IsNullOrWhiteSpace(choice))
                {
                    if (critereon <= maxCritereon)
                        break;

                    choice = choices[Random.Next(0, choices.Length)];
                    critereon++;
                }

                if (string.IsNullOrEmpty(choice) || string.IsNullOrWhiteSpace(choice))
                {
                    await
                        EmbedExtensions.FromError("Couldn't choose a viable result. Please verify input and try again", Context)
                        .QueueMessageAsync(Context)
                        .ConfigureAwait(false);
                }
                else
                {
                    await
                        new EmbedBuilder()
                            .AddAuthor(Context.Client)
                            .AddFooter(Context)
                            .WithDescription($"I choose: **{choice}**")
                            .WithThumbnailUrl("https://cdn.discordapp.com/emojis/350673785923567616.png")
                        .QueueMessageAsync(Context)
                        .ConfigureAwait(false);
                }
            }
            else
            {
                await
                    EmbedExtensions.FromError("Please give me a choice or two ☹☹", Context)
                    .QueueMessageAsync(Context)
                    .ConfigureAwait(false);
            }
        }

        [Command("yn"), Summary("Yes, no, maybe. I don't know, can you repeat the question?")]
        public async Task YN([Remainder]string question = null)
        {
            var YNResp = await YNWTFcli.AskYNWTF().ConfigureAwait(false);

            var message = "";

            if (!string.IsNullOrEmpty(question))
                message = $"Your Question:\n{question}\n\n";

            message += $"I'd say {YNResp.Answer}";

            if (YNResp.Answer.ToLowerInvariant() != "yes" && YNResp.Answer.ToLowerInvariant() != "no")
                message += "¯\\_(ツ)_/¯";

            await
                new EmbedBuilder()
                    .AddAuthor(Context.Client)
                    .AddFooter(Context)
                    .WithDescription(message)
                    .WithImageUrl(YNResp.Image)
                    .WithRandomColor()
                .QueueMessageAsync(Context)
                .ConfigureAwait(false);
        }

        #endregion RNG

        #region Pasta

        [Command("pasta"), Summary("Pastas are nice"), RequireDatabase]
        public async Task Pasta(string cmd, string title, [Remainder]string content)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var user = await Database.GetUserAsync(Context.User).ConfigureAwait(false);

            switch (cmd.ToLowerInvariant())
            {
                case "new":
                case "+":
                case "create":
                    {
                        switch (title.ToLowerInvariant())
                        {
                            case "list":
                            case "help":
                                {
                                    await EmbedExtensions.FromError($"Cannot create a Pasta with the name: {title}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                                    DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                                }
                                break;

                            default:
                                {
                                    var pasta = Database.Pastas.FirstOrDefault(x => x.Name.ToLower() == title.ToLower());
                                    if (pasta != null)
                                    {
                                        await EmbedExtensions.FromError($"Pasta already exists with name: **{title}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                                        DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                                    }
                                    else
                                    {
                                        Database.Pastas.Add(new Pasta
                                        {
                                            OwnerId = Context.User.Id,
                                            Name = title,
                                            Content = content,
                                            Created = DateTime.UtcNow.ToEpoch()
                                        });

                                        await Database.SaveChangesAsync().ConfigureAwait(false);

                                        if (Database.Pastas.FirstOrDefault(x => x.Name.ToLower() == title.ToLower()) != null)
                                        {
                                            await EmbedExtensions.FromSuccess($"Added: **{title}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    break;

                case "edit":
                case "change":
                case "modify":
                    {
                        var pasta = Database.Pastas.FirstOrDefault(x => x.Name.ToLower() == title.ToLower());

                        if (pasta != null)
                        {
                            if (pasta.IsOwner(user))
                            {
                                content = content.Replace("\'", "\\\'");
                                content = content.Replace("\"", "\\\"");

                                pasta.Content = content;

                                await Database.SaveChangesAsync().ConfigureAwait(false);

                                if (Database.Pastas.FirstOrDefault(x => x.Name.ToLower() == title.ToLower()) != pasta)
                                {
                                    await EmbedExtensions.FromSuccess($"Changed the content of **{title}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                                }
                                else
                                {
                                    await EmbedExtensions.FromError($"Error changing content of **{title}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                DogStatsd.Increment("commands.errors", 1, 1, new string[] { "unm-precon" });
                                await EmbedExtensions.FromError("I'm sorry, but you don't own the Pasta", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                            await $"Whoops, `{title}` doesn't exist".QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        [Command("pasta"), Summary("Pastas are nice"), RequireDatabase]
        public async Task Pasta(string cmd, string title)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var pasta = Database.Pastas.FirstOrDefault(x => x.Name.ToLower() == title.ToLower());
            var user = await Database.GetUserAsync(Context.User).ConfigureAwait(false);

            if (pasta != null)
            {
                switch (cmd.ToLowerInvariant())
                {
                    case "who":
                    case "?":
                        {
                            var embed = new EmbedBuilder
                            {
                                Color = EmbedExtensions.RandomEmbedColor(),
                                Title = pasta.Name
                            };

                            var usr = Context.Client.GetUser(pasta.OwnerId);
                            if (usr != null)
                                embed.AddField("Author", usr.Username + "#" + usr.Discriminator, inline: true);
                            else
                                embed.AddField("Author", $"Unknown User ({pasta.OwnerId})");
                            embed.AddField("Created", pasta.Created.FromEpoch().ToString(new CultureInfo((await Database.GetUserAsync(Context.User).ConfigureAwait(false)).Language)), inline: true);
                            //embed.AddField("UpVotes", ":arrow_double_up: " + pasta.Upvotes, inline: true);
                            //embed.AddField("DownVotes", ":arrow_double_down: " + pasta.Downvotes, inline: true);

                            await embed.Build().QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        break;

                    case "upvote":
                        {
                            /*var result = await await Database.GetUserAsync(Context.User).CastPastaVoteAsync(pastaLocal, true);

                            if (result.Successful)
                            {
                                await $"Added your vote to `{pastaLocal.Name}`".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                            }
                            else
                            {
                                await result.Error.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                            }*/
                        }
                        break;

                    case "downvote":
                        {
                            /*var result = await await Database.GetUserAsync(Context.User).CastPastaVoteAsync(pastaLocal, false);

                            if (result.Successful)
                            {
                                await $"Added your vote to `{pastaLocal.Name}`".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                            }
                            else
                            {
                                await result.Error.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                            }*/
                        }
                        break;

                    case "delete":
                        {
                            if (pasta.IsOwner(user))
                            {
                                Database.Pastas.Remove(pasta);
                                await Database.SaveChangesAsync().ConfigureAwait(false);

                                if (Database.Pastas.FirstOrDefault(x => x == pasta) == null)
                                {
                                    await EmbedExtensions.FromSuccess($"Successfully deleted: **{title}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                                }
                            }
                        }
                        break;
                }
            }
            else
            {
                DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                await EmbedExtensions.FromError($"Pasta `{title}` doesn't exist. :/ Sorry.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("pasta"), Summary("Pastas are nice"), RequireDatabase]
        public async Task Pasta(string title)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var pastas = await Database.Pastas.AsQueryable().ToListAsync();

            switch (title.ToLowerInvariant())
            {
                case "list":
                    {
                        if (pastas.Any())
                        {
                            string top = "```\n";

                            string pastanames = "";

                            foreach (var pasta in pastas)
                            {
                                if (pasta == pastas.LastOrDefault())
                                {
                                    pastanames += pasta.Name;
                                }
                                else
                                {
                                    pastanames += pasta.Name + ", ";
                                }
                            }

                            string bottom = "\n```";

                            if ((top + pastanames + bottom).Length < 2000)
                            {
                                await $"I found:\n{pastanames}".QueueMessageAsync(Context).ConfigureAwait(false);
                            }
                            else
                            {
                                using var stream = new MemoryStream();
                                using var sw = new StreamWriter(stream);
                                sw.Write(pastanames);

                                await $"Here's a list".QueueMessageAsync(Context, stream, type: Discord.Models.MessageType.File).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await EmbedExtensions.FromError("No pastas exist", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                    }
                    break;

                case "help":
                    {
                        await CommandService.GetCommandHelp(Context, "pasta").QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                    break;

                default:
                    {
                        var pasta = pastas.FirstOrDefault(x => x.Name.ToLower() == title.ToLower());

                        if (pasta != null)
                        {
                            await pasta.Content.QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        else
                        {
                            DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                            await $"Whoops, `{title}` doesn't exist".QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                    }
                    break;
            }
        }

        #endregion Pasta

        #region Emoji

        [Command("emoji"), Summary("Turns text into bigmoji")]
        public async Task Emojify([Remainder]string message)
            => await message.ToRegionalIndicator().QueueMessageAsync(Context).ConfigureAwait(false);

        [Command("emojidance"), Summary("Dancing Emoji")]
        public async Task DanceEmoji([Remainder]string message)
            => await message.ToDancingEmoji().QueueMessageAsync(Context).ConfigureAwait(false);

        #endregion Emoji

        #region Webcomics

        [Command("xkcd"), Summary("Get's Random XKCD comic"), Ratelimit(5, 1, Measure.Minutes)]
        public async Task XKCD(int comicid = -1)
        {
            if (comicid == -1)
            {
                await SendXKCD((await ComicClients.GetRandomXKCDComic().ConfigureAwait(false)) as XKCDComic).ConfigureAwait(false);
                DogStatsd.Increment("web.get");
            }
            else
            {
                await SendXKCD((await ComicClients.GetXKCDComic(comicid).ConfigureAwait(false)) as XKCDComic).ConfigureAwait(false);
                DogStatsd.Increment("web.get");
            }
        }

        public async Task SendXKCD(XKCDComic comic)
        {
            if (comic != null)
            {
                string datefixed;
                string monthfixed;
                DateTime dateTime;
                if (comic.day.Length == 1 && comic.month.Length == 1)
                {
                    datefixed = "0" + comic.day;
                    monthfixed = "0" + comic.month;
                    dateTime = DateTime.ParseExact($"{datefixed} {monthfixed} {comic.year}", "dd MM yyyy", CultureInfo.InvariantCulture);
                }
                else if (comic.day.Length == 1)
                {
                    datefixed = "0" + comic.day;
                    dateTime = DateTime.ParseExact($"{datefixed} {comic.month} {comic.year}", "dd MM yyyy", CultureInfo.InvariantCulture);
                }
                else if (comic.month.Length == 1)
                {
                    monthfixed = "0" + comic.month;
                    dateTime = DateTime.ParseExact($"{comic.day} {monthfixed} {comic.year}", "dd MM yyyy", CultureInfo.InvariantCulture);
                }
                else
                {
                    dateTime = DateTime.ParseExact($"{comic.day} {comic.month} {comic.year}", "dd MM yyyy", CultureInfo.InvariantCulture);
                }

                await new EmbedBuilder()
                    .WithAuthor(
                        new EmbedAuthorBuilder()
                        .WithName("Randall Patrick Munroe - XKCD")
                        .WithUrl($"https://xkcd.com/{comic.num}/")
                        .WithIconUrl("https://pbs.twimg.com/profile_images/602808103281692673/8lIim6cB_400x400.png")
                    )
                    .WithColor(Color.Teal)
                    .WithFooter(
                        new EmbedFooterBuilder()
                        .WithText("Strip released on")
                    )
                    .WithTimestamp(dateTime)
                    .WithImageUrl(comic.img)
                    .WithDescription(comic.alt)
                    .WithUrl(comic.link)
                    .QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("cah"), Summary("Gets a Random Cynaide & Happiness Comic"), Alias("cyanide&happiness", "c&h"), Ratelimit(5, 1, Measure.Minutes)]
        public async Task CAH()
        {
            try
            {
                var comic = await ComicClients.GetCAHComicAsync().ConfigureAwait(false) as CAHComic;
                DogStatsd.Increment("web.get");

                await new EmbedBuilder()
                    .WithAuthor(
                    new EmbedAuthorBuilder()
                        .WithName($"Strip done {comic.Author}")
                        .WithUrl(comic.AuthorURL)
                        .WithIconUrl(comic.AuthorAvatar)
                    )
                    .WithImageUrl(comic.ImageURL)
                    .WithUrl(comic.URL)
                    .WithColor(Color.Teal)
                    .QueueMessageAsync(Context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error("CAH-Cmd", "Error parsing website", ex);
            }
        }

        [Command("cad"), Summary("Gets a random CAD comic"), Ratelimit(5, 1, Measure.Minutes)]
        public async Task CAD()
        {
            try
            {
                var comic = await ComicClients.GetCADComicAsync().ConfigureAwait(false) as CADComic;
                DogStatsd.Increment("web.get");
                await
                    EmbedExtensions.FromImage(comic.ImageURL, EmbedExtensions.RandomEmbedColor(), Context)
                        .WithAuthor(
                            new EmbedAuthorBuilder()
                            .WithName("Tim Buckley")
                        )
                        .WithTitle(comic.Title)
                        .WithUrl(comic.URL)
                    .QueueMessageAsync(Context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error("CAD-Cmd", "Error parsing website", ex);
                await EmbedExtensions.FromError($"Error parsing website, try again later", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        #endregion Webcomics

        #region Magik

        [Command("magik"), Summary("Magiks an image"), Alias("magick", "magic", "liquify"), Ratelimit(5, 1, Measure.Minutes)]
        public async Task Magik()
        {
            string url = "";

            var msgsRaw = Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, 25);
            var msgs = await msgsRaw.FlattenAsync().ConfigureAwait(false);

            foreach (var msg in msgs)
            {
                if (msg.Attachments.Any())
                {
                    url = msg.Attachments.FirstOrDefault().Url;
                    break;
                }
                if (msg.Embeds.Any())
                {
                    var embed = msg.Embeds.FirstOrDefault();
                    if (embed.Image.HasValue)
                    {
                        url = embed.Image.Value.Url;
                        break;
                    }
                    if (embed.Thumbnail.HasValue)
                    {
                        url = embed.Image.Value.Url;
                        break;
                    }
                    if (embed.Author.HasValue)
                    {
                        if (!string.IsNullOrEmpty(embed.Author.Value.IconUrl))
                        {
                            url = embed.Author.Value.IconUrl;
                            break;
                        }
                    }
                }
            }

            if (url != "")
            {
                await Magik(new Uri(url)).ConfigureAwait(false);
                return;
            }

            await EmbedExtensions.FromError("Couldn't find an image", Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("magik"), Summary("Magiks an image"), Alias("magick", "magic", "liquify"), Ratelimit(5, 1, Measure.Minutes)]
        public async Task Magik(Uri image)
        {
            using var magikImag = new MagickImage(await HttpWebClient.ReturnStreamAsync(image).ConfigureAwait(false));
            using var magik2 = magikImag.Clone();

            magik2.Resize(800, 600);

            magik2.LiquidRescale(new MagickGeometry
            {
                X = 1,
                Width = Convert.ToInt32(magik2.Width * 0.5),
                Height = Convert.ToInt32(magik2.Height * 0.5)
            });
            magik2.LiquidRescale(new MagickGeometry
            {
                X = 2,
                Width = Convert.ToInt32(magik2.Width * 1.5),
                Height = Convert.ToInt32(magik2.Height * 1.5)
            });

            magik2.Resize(magikImag.Width, magikImag.Height);

            MemoryStream stream = new MemoryStream();

            magik2.Write(stream);

            stream.Position = 0;

            await "".QueueMessageAsync(Context, stream, type: Discord.Models.MessageType.File).ConfigureAwait(false);
        }

        [Command("magik"), Summary("Magiks an image"), Alias("magick", "magic", "liquify"), Ratelimit(5, 1, Measure.Minutes)]
        public async Task Magik([Remainder]IGuildUser user)
            => await Magik(new Uri(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())).ConfigureAwait(false);

        #endregion Magik

        #region Jokes

        [Command("roast"), Summary("\"Roasts\" a user, these are all taken as jokes, and aren't actually meant to cause harm.")]
        public async Task RoastCmd(IUser user = null)
        {
            if (user == null)
                user = Context.User;
            var roast = await SysExClient.GetRoastAsync().ConfigureAwait(false);
            DogStatsd.Increment("web.get");

            await $"{user.Mention} {roast}".QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("dadjoke"), Summary("Gives you a bad dad joke to facepalm at.")]
        public async Task DadJoke()
        {
            var joke = await SysExClient.GetDadJokeAsync().ConfigureAwait(false);
            DogStatsd.Increment("web.get");

            await new EmbedBuilder
            {
                Title = joke.Setup,
                Description = joke.Punchline,
                Color = EmbedExtensions.RandomEmbedColor()
            }.QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("pickup", RunMode = RunMode.Async), Summary("Cringe at these bad user-submitted pick up lines. (Don't actually use these or else you'll get laughed at. :3)"), Alias("pickupline")]
        public async Task PickUp()
        {
            var pickup = await SysExClient.GetPickupLineAsync().ConfigureAwait(false);
            DogStatsd.Increment("web.get");

            await new EmbedBuilder
            {
                Title = pickup.Setup,
                Description = pickup.Punchline,
                Color = EmbedExtensions.RandomEmbedColor()
            }.QueueMessageAsync(Context).ConfigureAwait(false);
        }

        #endregion Jokes

        #region Figlet

        private const int FIGLETWIDTH = 16;

        [Command("figlet"), Summary("Make a big ascii text lol")]
        public async Task Figlet([Remainder]string text)
        {
            var splittext = text.ToCharArray();
            var textrows = new List<string>();
            if (splittext.Count() > FIGLETWIDTH)
            {
                int count = (int)Math.Round(splittext.Count() / (decimal)FIGLETWIDTH, MidpointRounding.AwayFromZero);

                int prevamount = 0;
                for (int x = 1; x <= count; x++)
                {
                    int amount = x * FIGLETWIDTH;
                    string txt = string.Concat(splittext.Skip(prevamount).Take(amount));
                    textrows.Add(txt);
                    prevamount = amount;
                }

                if (count * FIGLETWIDTH < splittext.Count())
                {
                    var temp = splittext.ToList();
                    temp.RemoveRange(0, count * FIGLETWIDTH);

                    textrows.Add(string.Join(' ', temp));
                }
            }
            else
            {
                textrows.Add(text);
            }

            string result = "```\n";

            foreach (var row in textrows)
            {
                var figlet = new AsciiArt(row);
                foreach (var line in figlet.Result)
                {
                    result += line + "\n";
                }
                result += "\n";
            }

            result = result[0..^2];
            result += "```";

            await result.QueueMessageAsync(Context).ConfigureAwait(false);
        }

        #endregion Figlet

        #region Images

        [Command("meme"), Summary("Does a funny haha meme"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Memay(string template = null, params string[] sources)
        {
            if (template == null && !sources.Any())
            {
                var endpoints = JsonConvert.DeserializeObject<MemeResponse>(await HttpWebClient.ReturnStringAsync(new Uri("https://api.skuldbot.uk/fun/meme/?endpoints")).ConfigureAwait(false)).Endpoints;

                var pages = endpoints.PaginateList(35);

                int index = 0;
                foreach (var page in pages)
                {
                    await EmbedExtensions.FromMessage($"__Current Templates ({index + 1}/{pages.Count})__", page, Context)
                        .QueueMessageAsync(Context).ConfigureAwait(false);
                    index++;
                }

                return;
            }

            List<string> imageLinks = new List<string>();

            foreach (var str in sources)
            {
                if (str.IsImageExtension())
                {
                    imageLinks.Add(str.TrimEmbedHiders());
                }

                if (DiscordUtilities.UserMentionRegex.IsMatch(str))
                {
                    var userid = str.Replace("<@!", "").Replace("<@", "").Replace(">", "");
                    ulong.TryParse(userid, out ulong useridl);

                    var user = Context.Guild.GetUser(useridl);

                    imageLinks.Add(user.GetAvatarUrl(ImageFormat.Png, 1024) ?? user.GetDefaultAvatarUrl());
                }
            }

            if (imageLinks.All(x => x.IsImageExtension()))
            {
                var endpoints = JsonConvert.DeserializeObject<MemeResponse>(await HttpWebClient.ReturnStringAsync(new Uri("https://api.skuldbot.uk/fun/meme/?endpoints")).ConfigureAwait(false)).Endpoints;

                if (endpoints.Any(x => x.Name.ToLower() == template.ToLower()))
                {
                    var endpoint = endpoints.First(x => x.Name.ToLower() == template.ToLower());
                    if (endpoint.RequiredSources == imageLinks.Count())
                    {
                        var resp = await SysExClient.GetMemeImageAsync(endpoint.Name, imageLinks.ToArray()).ConfigureAwait(false);

                        if (resp != null && resp is Stream)
                        {
                            var folderPath = Path.Combine(AppContext.BaseDirectory, "storage/meme/");

                            var filePath = Path.Combine(folderPath, $"{template}-{Context.User.Id}-{Context.Channel.Id}.png");

                            if (!Directory.Exists(folderPath))
                            {
                                Directory.CreateDirectory(folderPath);
                            }

                            await "".QueueMessageAsync(Context, resp as Stream, type: Discord.Models.MessageType.File).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await EmbedExtensions.FromError($"You don't have enough sources. You need {endpoint.RequiredSources} source images", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                }
                else
                {
                    await EmbedExtensions.FromError($"Template `{template}` does not exist", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
            else
            {
                await EmbedExtensions.FromError("Sources need to be an image link", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("iqdb"), Summary("Reverse image-search")]
        public async Task IQDB(string image)
        {
            var results = await IqdbClient.SearchUrl(image).ConfigureAwait(false);

            if (results.IsFound)
            {
                var sorted = results.Matches.OrderByDescending(x => x.Similarity);
                var mostlikely = sorted.FirstOrDefault();
                string url = !mostlikely.Url.Contains("https:") && !mostlikely.Url.Contains("http:") ? "https:" + mostlikely.Url : mostlikely.Url;

                await EmbedExtensions.FromMessage(SkuldAppContext.GetCaller(), $"Similarity: {mostlikely.Similarity}%", Context)
                    .WithImageUrl(url).QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await EmbedExtensions.FromError("No results found", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("safebooru"), Summary("Gets stuff from safebooru"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("safe")]
        public async Task Safebooru(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags()) await "Your tags contains a banned tag, please remove it.".QueueMessageAsync(Context).ConfigureAwait(false);
            else
            {
                var cleantags = tags.AddBlacklistedTags();
                var posts = await BooruClient.GetSafebooruImagesAsync(cleantags).ConfigureAwait(false);
                DogStatsd.Increment("web.get");
                var post = GetSafeImage(posts);
                if (post != null)
                {
                    await post.GetMessage(post.PostUrl).QueueMessageAsync(Context).ConfigureAwait(false);
                    return;
                }
                await EmbedExtensions.FromError("Couldn't find an image", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        public SafebooruImage GetSafeImage(IReadOnlyList<SafebooruImage> posts, int EdgeCase = 0)
        {
            var post = posts.RandomValue(BotService.Services.GetRequiredService<Random>());
            EdgeCase++;
            if (EdgeCase <= 5)
            {
                if (post.Rating != Rating.Safe)
                {
                    return GetSafeImage(posts, EdgeCase);
                }
                else
                {
                    return post;
                }
            }
            else
            {
                return null;
            }
        }

        #endregion Images
    }
}