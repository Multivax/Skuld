﻿using Discord;
using Discord.Commands;
using Skuld.Discord.Extensions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Skuld.Discord.Utilities
{
    public static class DiscordUtilities
    {
        public static Regex UserMentionRegex = new Regex("<@.?[0-9]*?>");
        public static Regex RoleMentionRegex = new Regex("<&[0-9]*?>");
        public static Regex ChannelMentionRegex = new Regex("<#[0-9]*?>");

        public static GuildPermissions ModeratorPermissions = new GuildPermissions(268443650);

        public static string Level1ServerBoost = "server/level1.svg";
        public static string Level2ServerBoost = "server/level2.svg";
        public static string Level3ServerBoost = "server/level3.svg";

        public static string Level1UserBoost = "profile/level1.svg";
        public static string Level2UserBoost = "profile/level2.svg";
        public static string Level3UserBoost = "profile/level3.svg";

        public static string Empty = "<:empty:663083920992239638>";

        public static EmbedBuilder GetCommandHelp(CommandService commandService, ICommandContext context, string commandname)
        {
            if (commandname.ToLower() != "pasta")
            {
                var serch = commandService.Search(context, commandname).Commands;

                var summ = GetSummary(serch);

                if (summ == null)
                {
                    return null;
                }

                var embed = EmbedExtensions.FromMessage("Help", $"Here are some commands like **{commandname}**", Color.Teal, context);

                embed.AddField(string.Join(", ", serch[0].Command.Aliases), summ);

                return embed;
            }
            else
            {
                var pasta = "Here's how to do stuff with **pasta**:\n\n" +
                    "```cs\n" +
                    "   give   : Give a user your pasta\n" +
                    "   list   : List all pasta\n" +
                    "   edit   : Change the content of your pasta\n" +
                    "  change  : Same as above\n" +
                    "   new    : Creates a new pasta\n" +
                    "    +     : Same as above\n" +
                    "   who    : Gets information about a pasta\n" +
                    "    ?     : Same as above\n" +
                    "  upvote  : Upvotes a pasta\n" +
                    " downvote : Downvotes a pasta\n" +
                    "  delete  : deletes a pasta```";

                return EmbedExtensions.FromMessage("Pasta Recipe", pasta, Color.Teal, context);
            }
        }

        public static string GetSummary(IReadOnlyList<CommandMatch> Variants)
        {
            if (Variants != null)
            {
                if (Variants.Count > 0)
                {
                    var primary = Variants[0];

                    string summ = "Summary: " + primary.Command.Summary;
                    int totalparams = 0;

                    foreach (var com in Variants)
                    {
                        totalparams += com.Command.Parameters.Count;
                    }

                    if (totalparams > 0)
                    {
                        summ += "\nParameters:\n";

                        int instance = 0;

                        foreach (var cmd in Variants)
                        {
                            instance++;
                            if (Variants.Count > 1)
                            {
                                summ += $"**Command Version: {instance}**\n";
                            }
                            if (cmd.Command.Parameters.Count == 0)
                            {
                                summ += "No Parameters\n";
                            }
                            foreach (var param in cmd.Command.Parameters)
                            {
                                if (param.IsOptional)
                                {
                                    summ += $"**[Optional]** {param.Name} - {param.Type.Name}\n";
                                }
                                else
                                {
                                    summ += $"**[Required]** {param.Name} - {param.Type.Name}\n";
                                }
                            }
                        }

                        return summ;
                    }
                    return summ + "\nParameters: None";
                }
            }

            return null;
        }
    }
}