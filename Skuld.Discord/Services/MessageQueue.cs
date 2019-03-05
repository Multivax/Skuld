﻿using Discord;
using Skuld.Core;
using Skuld.Discord.Handlers;
using Skuld.Discord.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Discord.Services
{
    public class MessageQueue
    {
        private static ConcurrentQueue<SkuldMessage> messageQueue = new ConcurrentQueue<SkuldMessage>();

        private static readonly int messageDelay = 50;

        private static async Task ExecuteAsync()
        {
            while(true)
            {
                if(!messageQueue.IsEmpty)
                {
                    await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("MQ-Exec", $"Queue Length: {messageQueue.Count} Est. Time till completion: {(messageQueue.Count * messageDelay) / 1000}sec", LogSeverity.Info)).ConfigureAwait(false);
                    if(messageQueue.TryDequeue(out SkuldMessage message))
                    {
                        try
                        {
                            switch(message.Meta.Type)
                            {
                                case Models.MessageType.Standard:
                                    await MessageSender.ReplyAsync(message.Channel, message.Content.Message, message.Content.Embed);
                                    break;
                                case Models.MessageType.Mention:
                                    await MessageSender.ReplyWithMentionAsync(message.Channel, message.Content.User, message.Content.Message, message.Content.Embed);
                                    break;
                                case Models.MessageType.Success:
                                    if (!string.IsNullOrEmpty(message.Content.Message))
                                    {
                                        await MessageSender.ReplySuccessAsync(message.Channel, message.Content.Message);
                                    }
                                    else
                                    {
                                        await MessageSender.ReplySuccessAsync(message.Channel);
                                    }
                                    break;
                                case Models.MessageType.Failed:
                                    if (!string.IsNullOrEmpty(message.Content.Message))
                                    {
                                        await MessageSender.ReplyFailedAsync(message.Channel, message.Content.Message);
                                    }
                                    else
                                    {
                                        await MessageSender.ReplyFailedAsync(message.Channel);
                                    }
                                    break;
                                case Models.MessageType.DMS:
                                    await MessageSender.ReplyDMsAsync(await message.Content.User.GetOrCreateDMChannelAsync(), message.Channel, message.Content.Message, message.Content.Embed);
                                    break;
                                case Models.MessageType.DMFail:
                                    await MessageSender.ReplyDMFailableAsync(await message.Content.User.GetOrCreateDMChannelAsync(), message.Content.Message, message.Content.Embed);
                                    break;
                                case Models.MessageType.Timed:
                                    await MessageSender.ReplyWithTimedMessage(message.Channel, message.Content.Message, message.Content.Embed, message.Meta.Timeout);
                                    break;
                                case Models.MessageType.File:
                                    await MessageSender.ReplyWithFileAsync(message.Channel, message.Content.Message, message.Content.File);
                                    break;
                                case Models.MessageType.MentionFile:
                                    await MessageSender.ReplyWithMentionAndFileAsync(message.Channel, message.Content.User, message.Content.Message, message.Content.File);
                                    break;
                            }

                            if (message.Meta.Type == (Models.MessageType.File | Models.MessageType.MentionFile))
                            {
                                File.Delete(message.Content.File);
                            }

                            await Task.Delay(messageDelay * messageQueue.Count);
                        }
                        catch(Exception ex)
                        {
                            await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("MsgQueue", ex.Message, LogSeverity.Critical, ex));
                            await MessageSender.ReplyFailedAsync(message.Channel, ex.Message);
                        }
                    }
                    else
                    {
                        await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("MsgQueue", "Error removing message from queue", LogSeverity.Error));
                    }
                }
                else
                {
                    await Task.Delay(25);
                }
            }
        }

        public static void AddMessage(SkuldMessage message)
        {
            messageQueue.Enqueue(message);
            if(message.Meta.Type != (Models.MessageType.DMS|Models.MessageType.DMFail))
            {
                GenericLogger.AddToLogsAsync(new Core.Models.LogMessage
                {
                    Source = "MQ-Queue",
                    Message = $"Queued a command in: {message.Channel}/{((IGuildChannel)message.Channel).Guild} for {message.Content.User}",
                    Severity = LogSeverity.Info,
                    TimeStamp = DateTime.Now
                }).ConfigureAwait(false);
            }
            else
            {
                GenericLogger.AddToLogsAsync(new Core.Models.LogMessage
                {
                    Source = "MQ-Queue",
                    Message = $"Queued a command in: {message.Content.User}/DMs",
                    Severity = LogSeverity.Info,
                    TimeStamp = DateTime.Now
                }).ConfigureAwait(false);
            }
        }

        public static void Run()
            => Task.Run(async () => await ExecuteAsync());
    }
}
