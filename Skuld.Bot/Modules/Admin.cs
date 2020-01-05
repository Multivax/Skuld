﻿using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Skuld.Bot.Services;
using Skuld.Core.Extensions;
using Skuld.Core.Generic.Models;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord.Extensions;
using Skuld.Discord.Handlers;
using Skuld.Discord.Models;
using Skuld.Discord.Preconditions;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, RequireRole(AccessLevel.ServerMod), RequireEnabledModule]
    public class Admin : InteractiveBase<ShardedCommandContext>
    {
        public SkuldConfig Configuration { get => HostSerivce.Configuration; }
        private CommandService CommandService { get => MessageHandler.CommandService; }

        [Command("say"), Summary("Say something to a channel")]
        public async Task Say(ITextChannel channel, [Remainder]string message)
            => await channel.SendMessageAsync(message).ConfigureAwait(false);

        #region GeneralManagement
        [Command("guild-feature"), Summary("Configures guild features"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureGuildFeatures(string module, int value)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (value > 1) await Messages.FromError("Value over max limit: `1`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            if (value < 0) await Messages.FromError("Value under min limit: `0`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            else
            {
                module = module.ToLowerInvariant();
                var settings = new Dictionary<string, string>()
                {
                    {"pinning", "pinning" },
                    {"levels", "experience" }
                };
                if (settings.ContainsKey(module) || settings.ContainsValue(module))
                {
                    var features = Database.Features.FirstOrDefault(x => x.Id == Context.Guild.Id);
                    var prev = features;

                    if (features != null)
                    {
                        var setting = settings.FirstOrDefault(x => x.Key == module || x.Value == module);

                        switch (setting.Value)
                        {
                            case "pinning":
                                features.Pinning = Convert.ToBoolean(value);
                                break;

                            case "experience":
                                features.Experience = Convert.ToBoolean(value);
                                break;
                        }

                        await Database.SaveChangesAsync().ConfigureAwait(false);

                        if (value == 0) await $"I disabled the `{module}` feature".QueueMessageAsync(Context).ConfigureAwait(false);
                        else await $"I enabled the `{module}` feature".QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                }
                else
                {
                    string modulelist = "";
                    foreach (var mod in settings) modulelist += mod.Key + " (" + mod.Value + ")" + ", ";

                    modulelist = modulelist.Remove(modulelist.Length - 2);

                    await Messages.FromError($"Cannot find module: `{module}` in a list of all available modules (raw name in brackets). \nList of available modules: \n{modulelist}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
        }

        [Command("guild-module"), Summary("Configures guild modules"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureGuildModules(string module, int value)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (value > 1) await Messages.FromError("Value over max limit: `1`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            if (value < 0) await Messages.FromError("Value under min limit: `0`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            else
            {
                module = module.ToLowerInvariant();

                var gldmods = Database.Modules.FirstOrDefault(x => x.Id == Context.Guild.Id);

                string[] modules = null;

                List<string> mods = new List<string>();

                foreach (var mod in CommandService.Modules.ToArray())
                {
                    mods.Add(mod.Name.ToLowerInvariant());
                }

                modules = mods.ToArray();

                if (modules.Contains(module))
                {
                    if (gldmods != null)
                    {
                        switch (module)
                        {
                            case "accounts":
                                gldmods.Accounts = Convert.ToBoolean(value);
                                break;

                            case "actions":
                                gldmods.Actions = Convert.ToBoolean(value);
                                break;

                            case "admin":
                                gldmods.Admin = Convert.ToBoolean(value);
                                break;

                            case "fun":
                                gldmods.Fun = Convert.ToBoolean(value);
                                break;

                            case "gambling":
                                gldmods.Gambling = Convert.ToBoolean(value);
                                break;

                            case "information":
                                gldmods.Information = Convert.ToBoolean(value);
                                break;

                            case "lewd":
                                gldmods.Lewd = Convert.ToBoolean(value);
                                break;

                            case "search":
                                gldmods.Search = Convert.ToBoolean(value);
                                break;

                            case "space":
                                gldmods.Space = Convert.ToBoolean(value);
                                break;

                            case "stats":
                                gldmods.Stats = Convert.ToBoolean(value);
                                break;

                            case "weeb":
                                gldmods.Weeb = Convert.ToBoolean(value);
                                break;
                        }

                        await Database.SaveChangesAsync().ConfigureAwait(false);
                        if (value == 0) await Messages.FromSuccess($"I disabled the `{module}` module", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        else await Messages.FromSuccess($"I enabled the `{module}` module", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                }
                else
                {
                    string modulelist = string.Join(", ", modules);
                    await Messages.FromError($"Cannot find module: `{module}` in a list of all available modules. \nList of available modules: \n{modulelist}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
        }

        [Command("configurechannel"), Summary("Some features require channels to be set"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureChannel(string module, IChannel channel)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            module = module.ToLowerInvariant();
            var modules = new Dictionary<string, string>()
            {
                {"userjoin","userjoinchan" },
                {"userjoined","userjoinchan" },
                {"userleave","userleavechan" },
                {"userleft","userleavechan" }
            };
            if (modules.ContainsKey(module) || modules.ContainsValue(module))
            {
                var guild = await Database.GetGuildAsync(Context.Guild);

                if (guild != null)
                {
                    modules.TryGetValue(module, out string key);
                    switch (key)
                    {
                        case "userjoinchan":
                            guild.JoinChannel = channel.Id;
                            break;

                        case "userleavechan":
                            guild.LeaveChannel = channel.Id;
                            break;
                    }
                    await Database.SaveChangesAsync().ConfigureAwait(false);

                    await $"I set `{channel.Name}` as the channel for the `{module}` module".QueueMessageAsync(Context).ConfigureAwait(false);
                }
                else await Database.InsertGuildAsync(Context.Guild, MessageHandler.cmdConfig.Prefix, MessageHandler.cmdConfig.MoneyName, MessageHandler.cmdConfig.MoneyIcon).ConfigureAwait(false);
            }
            else
            {
                string modulelist = string.Join(", ", modules);
                modulelist = modulelist.Remove(modulelist.Length - 2);

                await new EmbedBuilder
                {
                    Title = "Error with command",
                    Description = $"Cannot find module: `{module}` in a list of all available modules. \nList of available modules: \n{modulelist}",
                    Color = new Color(255, 0, 0)
                }
                .Build().QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("guild-money"), Summary("Set's the guilds money name or money icon"), RequireDatabase]
        public async Task GuildMoney(Emoji icon = null, [Remainder]string name = null)
        {
            using var database = new SkuldDbContextFactory().CreateDbContext();
            var guild = await database.GetGuildAsync(Context.Guild).ConfigureAwait(false);

            if (icon == null && name == null)
            {
                guild.MoneyIcon = MessageHandler.cmdConfig.MoneyIcon;
                guild.MoneyName = MessageHandler.cmdConfig.MoneyName;

                await database.SaveChangesAsync().ConfigureAwait(false);

                await Messages.FromSuccess($"Reset the icon to: `{guild.MoneyIcon}` & the name to: `{guild.MoneyName}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);

                return;
            }

            if(icon != null && name == null)
            {
                await Messages.FromError($"Parameter \"{nameof(name)}\" needs a value", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            guild.MoneyIcon = icon.ToString();
            guild.MoneyName = name;

            await database.SaveChangesAsync().ConfigureAwait(false);

            await Messages.FromSuccess($"Set the icon to: `{guild.MoneyIcon}` & the name to: `{guild.MoneyName}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }
        #endregion

        #region Mute/Prune
        [Command("mute"), Summary("Mutes a user")]
        [RequireBotAndUserPermission(GuildPermission.ManageRoles)]
        public async Task Mute(IUser usertomute)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var guild = Context.Guild;
            var roles = guild.Roles;
            var user = usertomute as IGuildUser;
            var channels = guild.TextChannels;

            var gld = await Database.GetGuildAsync(guild).ConfigureAwait(false);

            try
            {
                if (gld.MutedRole == 0)
                {
                    var role = await guild.CreateRoleAsync("Muted", GuildPermissions.None).ConfigureAwait(false);
                    foreach (var chan in channels)
                    {
                        await chan.AddPermissionOverwriteAsync(role, OverwritePermissions.DenyAll(chan)).ConfigureAwait(false);
                    }

                    gld.MutedRole = role.Id;

                    await Database.SaveChangesAsync().ConfigureAwait(false);

                    await user.AddRoleAsync(role).ConfigureAwait(false);
                    await Messages.FromInfo($"{Context.User.Mention} just muted **{usertomute.Username}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
                else
                {
                    var role = guild.GetRole(gld.MutedRole);
                    await user.AddRoleAsync(role).ConfigureAwait(false);
                    await Messages.FromInfo($"{Context.User.Mention} just muted **{usertomute.Username}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
            catch
            {
                await Messages.FromError($"Failed to give {usertomute.Username} the muted role, ensure I have `MANAGE_ROLES` an that my highest role is above the mute role", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("unmute"), Summary("Unmutes a user")]
        [RequireBotAndUserPermission(GuildPermission.ManageRoles)]
        public async Task Unmute(IUser usertounmute)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var guild = Context.Guild;
            var user = usertounmute as IGuildUser;

            try
            {
                var dbGuild = await Database.GetGuildAsync(guild);

                if (dbGuild.MutedRole == 0)
                {
                    await Messages.FromError("Role doesn't exist, so I cannot unmute", Context).QueueMessageAsync(Context).ConfigureAwait(false);

                    DogStatsd.Increment("commands.errors", 1, 1, new[] { "generic" });
                }
                else
                {
                    var role = guild.GetRole(dbGuild.MutedRole);
                    await user.RemoveRoleAsync(role).ConfigureAwait(false);
                    await Messages.FromInfo($"{Context.User.Mention} just unmuted **{usertounmute.Username}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
            catch
            {
                await Messages.FromError($"Failed to remove the muted role from {usertounmute.Username}, ensure I have `MANAGE_ROLES` an that my highest role is above the mute role", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("prune"), Summary("Cleans set amount of messages.")]
        [RequireBotAndUserPermission(GuildPermission.ManageMessages)]
        public async Task Prune(short amount, IUser user = null)
        {
            if (amount < 0)
            {
                await Messages.FromError($"{Context.User.Mention} Your amount `{amount}` is under 0.", Context).QueueMessageAsync(Context).ConfigureAwait(false);

                DogStatsd.Increment("commands.errors", 1, 1, new[] { "unm-precon" });
                return;
            }
            await Context.Message.DeleteAsync().ConfigureAwait(false);
            if (user == null)
            {
                var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync().ConfigureAwait(false);
                ITextChannel chan = (ITextChannel)Context.Channel;
                await chan.DeleteMessagesAsync(messages).ContinueWith(async x =>
                {
                    if (x.IsCompleted)
                    {
                        await Messages.FromSuccess(":ok_hand: Done!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
            }
            else
            {
                var messages = await Context.Channel.GetMessagesAsync(100).FlattenAsync().ConfigureAwait(false);
                var usermessages = messages.Where(x => x.Author.Id == user.Id);
                usermessages = usermessages.Take(amount);
                ITextChannel chan = (ITextChannel)Context.Channel;
                await chan.DeleteMessagesAsync(usermessages).ContinueWith(async x =>
                {
                    if (x.IsCompleted)
                    {
                        await Messages.FromSuccess(":ok_hand: Done!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
            }
        }
        #endregion

        #region Ban/Kick
        [Command("kick"), Summary("Kicks a user"), Alias("dab", "dabon")]
        [RequireBotAndUserPermission(GuildPermission.KickMembers)]
        public async Task Kick(IGuildUser user, [Remainder]string reason = null)
        {
            try
            {
                var msg = $"You have been kicked from **{Context.Guild.Name}** by: {Context.User.Username}#{Context.User.Discriminator}";
                var guild = Context.Guild as IGuild;
                if (reason == null)
                {
                    await user.KickAsync($"Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
                    if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) == null)
                    {
                        await Messages.FromInfo($"Successfully kicked: `{user.Username}`\tResponsible Moderator:  {Context.User.Username}#{Context.User.Discriminator}", Context).QueueMessageAsync(Context).ConfigureAwait(false);

                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                            await dmchan.SendMessageAsync(msg).ConfigureAwait(false);
                        }
                        catch { }
                    }
                }
                else
                {
                    msg += $" with reason:```\n{reason}```";
                    await user.KickAsync(reason + $" Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
                    if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) == null)
                    {
                        await Messages.FromInfo($"Successfully kicked: `{user}`\tResponsible Moderator:  {Context.User.Username}#{Context.User.Discriminator}\nReason: {reason}", Context).QueueMessageAsync(Context).ConfigureAwait(false);

                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                            await dmchan.SendMessageAsync(msg).ConfigureAwait(false);
                        }
                        catch { }
                    }
                }
            }
            catch
            {
                await Messages.FromSuccess($"Couldn't kick {user.Username}#{user.DiscriminatorValue}! Do they exist in the server or is their highest role higher than mine?", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("ban"), Summary("Bans a user"), Alias("naenae")]
        [RequireBotAndUserPermission(GuildPermission.BanMembers)]
        public async Task Ban(IGuildUser user, [Remainder]string reason = null)
        {
            try
            {
                var msg = $"You have been banned from **{Context.Guild}** by: {Context.User.Username}#{Context.User.Discriminator}";
                var guild = Context.Guild as IGuild;
                if (reason == null)
                {
                    await Context.Guild.AddBanAsync(user, 7, $"Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
                    if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) == null)
                    {
                        await Messages.FromSuccess($"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                            await dmchan.SendMessageAsync(msg).ConfigureAwait(false);
                        }
                        catch { }
                    }
                }
                else
                {
                    msg += $" with reason:```\n{reason}```";
                    await Context.Guild.AddBanAsync(user, 7, reason + $" Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
                    if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) == null)
                    {
                        await Messages.FromSuccess($"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}\nReason given: {reason}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                            await dmchan.SendMessageAsync(msg).ConfigureAwait(false);
                        }
                        catch { }
                    }
                }
            }
            catch
            {
                await Messages.FromError($"Couldn't ban {user.Username}#{user.DiscriminatorValue}! Do they exist in the server or is their highest role higher than mine?", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("ban"), Summary("Bans a user"), Alias("naenae")]
        [RequireBotAndUserPermission(GuildPermission.BanMembers)]
        public async Task Ban(IGuildUser user, int daystoprune = 7, [Remainder]string reason = null)
        {
            try
            {
                var msg = $"You have been banned from **{Context.Guild}** by: {Context.User.Username}#{Context.User.Discriminator}";
                var guild = Context.Guild as IGuild;
                if (reason == null)
                {
                    await Context.Guild.AddBanAsync(user, daystoprune, $"Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
                    if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) == null)
                    {
                        await Messages.FromSuccess($"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                            await dmchan.SendMessageAsync(msg).ConfigureAwait(false);
                        }
                        catch { }
                    }
                }
                else
                {
                    msg += $" with reason:```\n{reason}```";
                    await Context.Guild.AddBanAsync(user, daystoprune, reason + $" Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}").ConfigureAwait(false);
                    if (await guild.GetUserAsync(user.Id).ConfigureAwait(false) == null)
                    {
                        await Messages.FromSuccess($"Successfully banned: `{user.Username}#{user.Discriminator}` Responsible Moderator: {Context.User.Username}#{Context.User.Discriminator}\nReason given: {reason}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        try
                        {
                            var dmchan = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                            await dmchan.SendMessageAsync(msg).ConfigureAwait(false);
                        }
                        catch { }
                    }
                }
            }
            catch
            {
                await Messages.FromError($"Couldn't ban {user.Username}#{user.DiscriminatorValue}! Do they exist in the server or is their highest role higher than mine?", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("hackban"), Summary("Hackbans a set of userids Must be in this format hackban [id1] [id2] [id3]")]
        [RequireBotAndUserPermission(GuildPermission.BanMembers)]
        public async Task HackBan(params string[] ids)
        {
            if (ids.Any())
            {
                foreach (var id in ids)
                    await Context.Guild.AddBanAsync(Convert.ToUInt64(id)).ConfigureAwait(false);

                await Messages.FromSuccess($"Banned IDs: {string.Join(", ", ids)}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await Messages.FromError($"Couldn't parse list of ID's.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                DogStatsd.Increment("commands.errors", 1, 1, new[] { "parse-fail" });
            }
        }

        [Command("softban"), Summary("Softbans a user")]
        [RequireBotAndUserPermission(GuildPermission.BanMembers)]
        public async Task SoftBan(IUser user, [Remainder]string reason = null)
        {
            var newreason = $"Softban - Responsible Moderator: {Context.User.Username}#{Context.User.DiscriminatorValue}";
            if (reason == null)
            {
                await Context.Guild.AddBanAsync(user, 7, newreason).ConfigureAwait(false);
                await Messages.FromSuccess($"Successfully softbanned: `{user.Username}#{user.Discriminator}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                await Context.Guild.RemoveBanAsync(user).ConfigureAwait(false);
            }
            else
            {
                newreason += " - Reason: " + reason;
                await Context.Guild.AddBanAsync(user, 7, newreason).ConfigureAwait(false);
                await Messages.FromSuccess($"Successfully softbanned: `{user.Username}#{user.Discriminator}`\nReason given: {reason}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                await Context.Guild.RemoveBanAsync(user).ConfigureAwait(false);
            }
        }
        #endregion

        #region RoleManagement
        [Command("roleids"), Summary("Gets all role ids")]
        public async Task GetRoleIds()
        {
            string lines = "";
            var guild = Context.Guild;
            var roles = guild.Roles;

            foreach (var item in roles)
            {
                lines = lines + $"{Convert.ToString(item.Id)} - \"{item.Name}\"" + Environment.NewLine;
            }

            if (lines.Length > 2000)
            {
                var paddedlines = lines.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                var pages = paddedlines.PaginateCodeBlockList(25);

                foreach(var page in pages)
                {
                    await page.QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
            else
            {
                await $"```cs\n{lines}```".QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("setjrole"), Summary("Allows a role to be auto assigned on userjoin"), RequireDatabase]
        public async Task AutoRole(IRole role = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var guild = Context.Guild;

            var gld = await Database.GetGuildAsync(guild).ConfigureAwait(false);

            if (role == null)
            {
                if (gld.JoinRole != 0)
                {
                    gld.JoinRole = 0;
                    await Database.SaveChangesAsync().ConfigureAwait(false);

                    if ((await Database.GetGuildAsync(guild).ConfigureAwait(false)).JoinRole == 0)
                    {
                        await Messages.FromSuccess($"Successfully removed the member join role", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                    else
                    {
                        await Messages.FromError($"Error Removing Join Role, reason unknown.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                var roleidprev = gld.JoinRole;

                gld.JoinRole = role.Id;
                await Database.SaveChangesAsync().ConfigureAwait(false);

                if ((await Database.GetGuildAsync(guild).ConfigureAwait(false)).JoinRole != roleidprev)
                {
                    await Messages.FromSuccess($"Successfully set **{role.Name}** as the member join role", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
                else
                {
                    await Messages.FromError($"Error Changing Join Role, reason unknown.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
        }

        [Command("autorole"), Summary("Get's guilds current autorole"), RequireDatabase]
        public async Task AutoRole()
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var jrole = (await Database.GetGuildAsync(Context.Guild)).JoinRole;

            if (jrole == 0)
            {
                await $"Currently, **{Context.Guild.Name}** has no auto role.".QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await $"**{Context.Guild.Name}**'s current auto role is `{Context.Guild.GetRole(jrole).Name}`".QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("addassignablerole"), Summary("Adds a new self assignable role. Supported:cost=[cost] require-level=[level] require-role=[rolename/roleid/mention]")]
        [Alias("asar")]
        public async Task AddSARole(IRole role, [Remainder]GuildRoleConfig config)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var features = Database.Features.FirstOrDefault(x => x.Id == Context.Guild.Id);

            if (config.RequireLevel != 0 && !features.Experience)
            {
                await Messages.FromError("Configuration Error!", $"Enable Experience module first by using `{(await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false)).Prefix}guild-feature experience 1`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            Database.IAmRoles.Add(new IAmRole
            {
                GuildId = Context.Guild.Id,
                LevelRequired = config.RequireLevel,
                Price = config.Cost,
                RequiredRoleId = config.RequiredRole != null ? config.RequiredRole.Id : 0,
                RoleId = role.Id
            });

            try
            {
                await Database.SaveChangesAsync().ConfigureAwait(false);
                await Messages.FromSuccess(Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Messages.FromError(ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);

                Log.Error("ASAR-CMD", ex.Message, ex);
            }
        }

        [Command("addlevelrole"), Summary("Adds a new self assignable role. Supported:automatic=[true/false] require-level=[level] require-role=[rolename/roleid/mention]")]
        [Alias("alr")]
        public async Task AddLRole(IRole role, [Remainder]GuildRoleConfig config)
        {
            var levelReward = new LevelRewards
            {
                GuildId = Context.Guild.Id,
                LevelRequired = config.RequireLevel,
                RoleId = role.Id
            };

            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if(Database.LevelRewards.Contains(levelReward))
            {
                await Messages.FromError("Level reward already exists in this configuration", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            Database.LevelRewards.Add(levelReward);

            try
            {
                await Database.SaveChangesAsync().ConfigureAwait(false);

                await Messages.FromInfo($"Added new Level Reward with configuration `{config}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Messages.FromError(ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);

                Log.Error("ALR-CMD", ex.Message, ex);
            }
        }

        [Command("deletesr"), Summary("Removes a Self Assignable Role from the list")]
        public async Task DeleteSelfRole([Remainder]IRole role)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var r = Database.IAmRoles.FirstOrDefault(x => x.RoleId == role.Id);
            
            try
            {
                Database.IAmRoles.Remove(r);

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await Messages.FromInfo("Command Successful", $"Removed Self Assignable Role `{role}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Messages.FromError($"Command Error", ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);

                Log.Error("DA-CMD", ex.Message, ex);
            }
        }

        [Command("deletelr"), Summary("Removes a Level Grant Role from the list")]
        public async Task DeleteLevelRole([Remainder]IRole role)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var r = Database.LevelRewards.FirstOrDefault(x => x.RoleId == role.Id);

            try
            {
                Database.LevelRewards.Remove(r);

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await Messages.FromInfo("Command Successful", $"Removed Self Assignable Role `{role}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Messages.FromError($"Command Error", ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);

                Log.Error("DLR-CMD", ex.Message, ex);
            }
        }
        #endregion

        #region Prefix
        [Command("setprefix"), Summary("Sets the prefix, or resets on empty prefix"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetPrefix(string prefix = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);
            if (prefix != null)
            {
                var oldprefix = gld.Prefix;

                gld.Prefix = prefix;

                await Database.SaveChangesAsync();

                if ((await Database.GetGuildAsync(Context.Guild)).Prefix != oldprefix)
                    await Messages.FromSuccess($"Successfully set `{prefix}` as the Guild's prefix", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                else
                    await Messages.FromError($":thinking: It didn't change. Probably because it is the same as the current prefix.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                gld.Prefix = Configuration.Prefix;

                await Database.SaveChangesAsync();

                if ((await Database.GetGuildAsync(Context.Guild)).Prefix == Configuration.Prefix)
                    await Messages.FromSuccess($"Successfully reset the Guild's prefix", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                else
                    await Messages.FromError($":thinking: It didn't change.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("resetprefix"), Summary("Resets prefix"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ResetPrefix()
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            if (gld != null)
            {
                gld.Prefix = MessageHandler.cmdConfig.Prefix;

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await $"Reset the prefix back to `{MessageHandler.cmdConfig.Prefix}`".QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await Database.InsertGuildAsync(Context.Guild, MessageHandler.cmdConfig.Prefix, MessageHandler.cmdConfig.MoneyName, MessageHandler.cmdConfig.MoneyIcon).ConfigureAwait(false);
            }
        }
        #endregion

        #region Welcome
        //Set Channel
        [Command("setwelcome"), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWelcome([Remainder]string welcome)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            var oldmessage = gld.JoinMessage;

            gld.JoinChannel = Context.Channel.Id;
            gld.JoinMessage = welcome;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            if ((await Database.GetGuildAsync(Context.Guild)).JoinMessage != oldmessage)
                await Messages.FromSuccess($"Set Welcome message!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        //Current Channel
        [Command("setwelcome"), Summary("Sets the welcome message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWelcome(ITextChannel channel, [Remainder]string welcome)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            var oldmessage = gld.JoinMessage;
            var oldchannel = gld.JoinChannel;

            gld.JoinChannel = channel.Id;
            gld.JoinMessage = welcome;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var ngld = await Database.GetGuildAsync(Context.Guild);

            if (ngld.JoinChannel != oldchannel && ngld.JoinMessage != oldmessage)
                await Messages.FromSuccess("Set Welcome message!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        //Deletes
        [Command("unsetwelcome"), Summary("Clears the welcome message"), Alias("clearwelcome"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetWelcome()
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            gld.JoinChannel = 0;
            gld.JoinMessage = "";

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var ngld = await Database.GetGuildAsync(Context.Guild);

            if (ngld.JoinChannel == 0 && ngld.JoinMessage == "")
                await Messages.FromSuccess("Cleared Welcome message!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }
        #endregion

        #region Leave
        //Set Channel
        [Command("setleave"), RequireDatabase]
        [Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetLeave(ITextChannel channel, [Remainder]string leave)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            var oldleave = gld.LeaveChannel;
            var oldmessg = gld.LeaveMessage;

            gld.LeaveChannel = channel.Id;
            gld.LeaveMessage = leave;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var ngld = await Database.GetGuildAsync(Context.Guild);

            if (ngld.LeaveMessage != oldmessg && ngld.LeaveChannel != oldleave)
            {
                await Messages.FromSuccess("Set Leave message!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        //Current Channel
        [Command("setleave"), Alias("clearleave"), RequireDatabase]
        [Summary("Sets the leave message, -u shows username, -m mentions user, -s shows server name, -uc shows usercount (excluding bots)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetLeave([Remainder]string leave)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            var oldleave = gld.LeaveChannel;
            var oldmessg = gld.LeaveMessage;

            gld.LeaveChannel = Context.Channel.Id;
            gld.LeaveMessage = leave;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var ngld = await Database.GetGuildAsync(Context.Guild);

            if (ngld.LeaveMessage != oldmessg && ngld.LeaveChannel != oldleave)
            {
                await Messages.FromSuccess("Set Leave message!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        //Deletes
        [Command("unsetleave"), Summary("Clears the leave message"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetLeave()
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild);

            var oldleave = gld.LeaveChannel;
            var oldmessg = gld.LeaveMessage;

            gld.LeaveChannel = 0;
            gld.LeaveMessage = "";

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var ngld = await Database.GetGuildAsync(Context.Guild);

            if (ngld.LeaveMessage != oldmessg && ngld.LeaveChannel != oldleave)
            {
                await Messages.FromSuccess("Set Leave message!", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }
        #endregion

        #region Levels
        [Command("setlevelupmessage"), Summary("Sets the level up message, -u says the users name, -m mentions the user, -l shows the level they achieved"), RequireDatabase]
        [RequireRole(AccessLevel.ServerMod)]
        public async Task SetLevelUp([Remainder]string message)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);

            var oldmessg = gld.LeaveMessage;

            gld.LevelUpMessage = message;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var ngld = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);

            if (ngld.LevelUpMessage != oldmessg)
            {
                await Messages.FromSuccess(Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("levelchannel"), Summary("Sets the levelup channel")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ConfigureLevelChannel(IGuildChannel channel = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);
            var oldchan = gld.LevelUpChannel;

            if (channel == null)
            {
                gld.LevelUpChannel = channel.Id;

                await Database.SaveChangesAsync().ConfigureAwait(false);

                var ngld = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);

                if (ngld.LevelUpChannel != oldchan)
                {
                    await Messages.FromSuccess(Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
            else
            {
                gld.LevelUpChannel = 0;

                await Database.SaveChangesAsync().ConfigureAwait(false);

                var ngld = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);

                if (ngld.LevelUpChannel != oldchan)
                {
                    await Messages.FromSuccess(Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
        }

        [Command("levelnotification"), Summary("Sets the levelup notification")]
        [Alias("levelnotif")]
        public async Task ConfigureLevelNotif(LevelNotification level)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var gld = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);

            var old = gld.LevelNotification;

            gld.LevelNotification = level;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var ngld = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);

            if (ngld.LevelNotification != old)
            {
                await Messages.FromSuccess(Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }
        #endregion

        #region CustomCommands
        [Command("addcommand"), Summary("Adds a custom command"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddCustomCommand(string name, [Remainder]string content)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (name.IsWebsite())
            {
                await Messages.FromError("Commands can't be a url/website", Context).QueueMessageAsync(Context, Discord.Models.MessageType.Timed, timeout: 5).ConfigureAwait(false);
                return;
            }
            if (name.Split(' ').Length > 1)
            {
                await Messages.FromError("Commands can't contain a space", Context).QueueMessageAsync(Context, Discord.Models.MessageType.Timed, timeout: 5).ConfigureAwait(false);
                return;
            }
            else
            {
                var cmdsearch = CommandService.Search(Context, name);
                if (cmdsearch.Commands != null)
                {
                    await Messages.FromError("The bot already has this command", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
                else
                {
                    var cmd = Database.CustomCommands.FirstOrDefault(x => x.Name.ToLower() == name.ToLower() && x.GuildId == Context.Guild.Id);

                    if (cmd != null)
                    {
                        await Messages.FromInfo($"Custom command named `{name}` already exists, overwrite with new content? Y/N", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        var msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                        if (msg != null)
                        {
                            if (msg.Content.ToLower() == "y")
                            {
                                var c = content;

                                cmd.Content = content;

                                await Database.SaveChangesAsync().ConfigureAwait(false);

                                var cmd2 = Database.CustomCommands.FirstOrDefault(x => x.Name.ToLower() == name.ToLower() && x.GuildId == Context.Guild.Id);

                                if (cmd2.Content != c)
                                {
                                    await Messages.FromInfo("Updated the command.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                                }
                                else
                                {
                                    await Messages.FromError("Couldn't update the command", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                                }
                            }
                        }
                        else
                        {
                            await "Reply timed out, not updating.".QueueMessageAsync(Context, Discord.Models.MessageType.Timed, timeout: 5).ConfigureAwait(false);
                        }
                        return;
                    }
                    else
                    {
                        Database.CustomCommands.Add(new CustomCommand
                        {
                            GuildId = Context.Guild.Id,
                            Name = name,
                            Content = content
                        });
                        await Database.SaveChangesAsync().ConfigureAwait(false);

                        cmd = Database.CustomCommands.FirstOrDefault(x => x.Name.ToLower() == name.ToLower() && x.GuildId == Context.Guild.Id);

                        if (cmd != null)
                            await Messages.FromInfo("Added the command.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        else
                            await Messages.FromError("Couldn't insert the command", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                }
            }
        }

        [Command("deletecommand"), Summary("Deletes a custom command"), RequireDatabase]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DeleteCustomCommand(string name)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (name.Split(' ').Length > 1)
            {
                await "Commands can't contain a space".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }
            else
            {
                await "Are you sure? Y/N".QueueMessageAsync(Context, Discord.Models.MessageType.Timed, timeout: 5).ConfigureAwait(false);

                var msg = await NextMessageAsync(true, true, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                if (msg != null)
                {
                    if (msg.Content.ToLower() == "y")
                    {
                        Database.CustomCommands.Remove(Database.CustomCommands.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.Name.ToLower() == name.ToLower()));
                        await Database.SaveChangesAsync();

                        if (Database.CustomCommands.FirstOrDefault(x => x.GuildId == Context.Guild.Id && x.Name.ToLower() == name.ToLower()) == null)
                            await Messages.FromInfo("Deleted the command.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        else
                            await Messages.FromError("Failed removing the command, try again", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                }
            }
        }
        #endregion
    }
}