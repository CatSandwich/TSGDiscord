﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TSGDiscord.Commands.Attributes;
using TSGDiscord.Commands.Attributes.Preconditions;

namespace TSGDiscord.Commands
{
    public static class Commands
    {
        [Command("help")]
        [Description("DM's user a list of all commands and their descriptions")]
        private static async Task Help(Bot bot, SocketMessage sm)
        {
            var eb = new EmbedBuilder { Title = "Commands" };

            foreach (var command in bot.Commands.Values.Distinct())
            {
                var description = new StringBuilder();
                if(command.Names.Length > 1) description.AppendLine($"Aliases: {string.Join(", ", command.Names[1..].Select(name => $"{Config.Prefix}{name}"))}");
                if(command.Description != null) description.AppendLine(command.Description);

                foreach (var roles in command.Preconditions.OfType<RequireRoleAttribute>().Select(att => att.Roles))
                {
                    description.AppendLine($"Requires one role of: {string.Join(", ", roles.Select(role => bot.GetRole(role)?.Name ?? "null"))}");
                }

                var value = description.ToString();
                if (value == "") value = "No information provided.";

                eb.AddField($"{Config.Prefix}{command.Names[0]}", value);
            }

            await sm.Author.SendMessageAsync(embed: eb.Build());
        }

        [Command("testscheduler")]
        [Description("DO NOT USE")]
        private static async Task TestScheduler(Bot bot, SocketMessage sm)
        {
            var time = GetRequiredDateTimeArgument(sm, "time");

            bot.Scheduler.Schedule("test", time, async () => { await sm.Channel.SendMessageAsync("Scheduled message."); });
        }

        [Command("testrepeating")]
        [Description("DO NOT USE")]
        private static async Task TestSchedulerRepeating(Bot bot, SocketMessage sm)
        {
            var time = GetRequiredDateTimeArgument(sm, "time");
            var repeat = GetRequiredIntArgument(sm, "repeat");

            bot.Scheduler.ScheduleRepeating("test", time, async () =>
            {
                await sm.Channel.SendMessageAsync("Scheduled message repeating.");
            }, new TimeSpan(0, 0, repeat));
        }

        [Command("raidsignup", "signup")]
        [Description("Posts a raid signup embed with reaction roles")]
        private static async Task RaidSignup(Bot bot, SocketMessage sm)
        {
            var slots = GetRequiredSignupPresetArgument(sm);
            var start = (DateTimeOffset) GetRequiredDateTimeArgument(sm, "start");
            var end = (DateTimeOffset) GetRequiredDateTimeArgument(sm, "end");

            var message = await sm.Channel.SendMessageAsync("Creating...");
            var signup = new RaidsSignup(sm.Channel.Id, message.Id, (ulong) start.ToUnixTimeSeconds(), (ulong) end.ToUnixTimeSeconds(), slots);
            bot.RaidSignups.Add(signup.MessageId, signup);
            bot.Serialize();
            await bot.EditRaidSignup(signup);
        }

        [Command("reset", "untilreset", "timeuntilreset", "toreset", "timetoreset")]
        [Description("Returns the time until reset in GW2")]
        private static async Task TimeToReset(Bot bot, SocketMessage sm)
        {
            var now = DateTime.UtcNow;
            var reset = new DateTime(now.Year, now.Month, now.Day + 1, 0, 0, 0);

            var timeRemaining = reset - now;

            await sm.Channel.SendMessageAsync($"The Time Remaining Until Daily Reset Is: {timeRemaining.Hours}:{timeRemaining.Minutes:D2}");
        }

        private static async Task CheckForPromotions(Bot bot, SocketMessage sm, ulong uid)
        {
            SocketGuildUser user = Bot.Instance.GetUser(uid);

                ulong[] roles = user.Roles.Select(x => x.Id).ToArray();

                if (sm.Author.IsNCM())
                {
                    var currentrank = Config.NcmTuples.First(x => roles.Contains(x.roleid)).roleid;
                    var currentRankIndex = Array.FindIndex(Config.NcmTuples, x => x.roleid == currentrank);

                    if (bot.Participation[uid] >= Config.NcmTuples[currentRankIndex + 1].ppoints)
                    {
                        await user.RemoveRoleAsync(Config.NcmTuples[currentRankIndex].roleid);
                        await user.AddRoleAsync(Config.NcmTuples[currentRankIndex + 1].roleid);
                        await sm.Channel.SendMessageAsync(
                            $"{uid.Mention()} You have been promoted to the rank of {Config.NcmTuples[currentRankIndex + 1].roleid.Role()}");
                    }
                    
                    return;
                }
                else if (sm.Author.IsOfficer())
                {
                    var currentrank = Config.OfficerTuples.Where(x => roles.Contains(x.roleid)).ToList()[0].roleid;
                    var currentRankIndex = Array.FindIndex(Config.OfficerTuples, x => x.roleid == currentrank);


                    if (bot.Participation[uid] >= Config.NcmTuples[currentRankIndex + 1].ppoints)
                    {
                        await user.RemoveRoleAsync(Config.OfficerTuples[currentRankIndex].roleid);
                        await user.AddRoleAsync(Config.OfficerTuples[currentRankIndex + 1].roleid);
                        await sm.Channel.SendMessageAsync(
                            $"{uid.Mention()} You have been promoted to the rank of {Config.OfficerTuples[currentRankIndex + 1].roleid.Role()}");
                    }

                    return;
                }
                else
                {
                    return;
                }
        }


        #region Participation
        [Command("pap", "participation"), RequireOfficer]
        [Description("Adds a participation point to the mentioned user, Format !pap @user - REQUIRES: OFFICER")]
        private static async Task AddParticipation(Bot bot, SocketMessage sm)
        {
            foreach (var id in sm.Content.GetMentions())
            {
                if (!bot.Participation.ContainsKey(id)) bot.Participation[id] = 0;
                bot.Participation[id]++;
                await sm.Channel.SendMessageAsync($"{id.Mention()}'s Participation Score is: {bot.Participation[id]}");

                await CheckForPromotions(bot, sm, id);
            }

            bot.SerializeParticipation();
        }

        [Command("setpap", "setparticipation"), RequireOfficer]
        [Description("Sets a mentioned users participation score, Format !setpap @user -score=X - REQUIRES: OFFICER")]
        private static async Task _setPap(Bot bot, SocketMessage sm)
        {
            var newPaP = GetRequiredIntArgument(sm, "score");

            if (newPaP < 0)
            {
                await sm.Channel.SendMessageAsync("Invalid Format");
                return;
            }

            foreach (var id in sm.Content.GetMentions())
            {
                bot.Participation[id] = newPaP;
                await sm.Channel.SendMessageAsync($"{id.Mention()}'s Participation Score is: {bot.Participation[id]}");
                bot.SerializeParticipation();
            }
        }

        [Command("printpap", "printparticipation"), RequireOfficer]
        [Description("DMs user a list of all userid's with their current participation scores - REQUIRES: OFFICER")]
        private static async Task _printPap(Bot bot, SocketMessage sm)
        {
            foreach (var (key, value) in bot.Participation)
            {
                string allUsers = $"User: {key},  Participation Score: {value}";

                await sm.Author.SendMessageAsync(allUsers);

                Console.WriteLine(allUsers);
            }

            await sm.Author.SendMessageAsync("All User Paps Printed");
        }

        [Command("promotions")]
        [Description("Posts all NCM & Officer roles and their required participation scores")]
        private static async Task _promotions(Bot bot, SocketMessage sm)
        {
            var promotionsNCM = new StringBuilder();
            var promotionsOfficer = new StringBuilder();

            promotionsNCM.Append("-------------------------------------\n");

            promotionsNCM.Append(" --- NCM Ranks ---\n\n");
            promotionsOfficer.Append(" --- Officer Ranks ---\n\n");

            foreach (var (roleName,_,ppoints) in Config.NcmTuples)
            {
                promotionsNCM.Append($"Rank Name: {roleName}, Participation Points: {ppoints} \n");
            }

            promotionsNCM.Append("-------------------------------------\n");

            foreach (var (roleName,_,ppoints) in Config.OfficerTuples)
            {
                promotionsOfficer.Append($"Rank Name: {roleName}, Participation Points: {ppoints} \n");
            }

            promotionsOfficer.Append("-------------------------------------\n");

            await sm.Channel.SendMessageAsync(promotionsNCM.ToString());
            await sm.Channel.SendMessageAsync(promotionsOfficer.ToString());
        }

        #endregion

        #region Preconditions
        private static string GetRequiredStringArgument(SocketMessage sm, string name)
        {
            var arg = sm.Content.GetArgument(name);

            if (arg is null) throw new PreconditionFailedException($"Missing required argument: `{name}`");
            return arg;
        }

        private static int GetRequiredIntArgument(SocketMessage sm, string name)
        {
            var str = GetRequiredStringArgument(sm, name);

            if (!int.TryParse(str, out var value)) throw new PreconditionFailedException($"Failed to parse argument `{name}` as int.");
            return value;
        }

        private static ulong GetRequiredUlongArgument(SocketMessage sm, string name)
        {
            var str = GetRequiredStringArgument(sm, name);

            if (!ulong.TryParse(str, out var value)) throw new PreconditionFailedException($"Failed to parse argument `{name}` as ulong.");
            return value;
        }

        private static DateTime GetRequiredTimeArgument(SocketMessage sm, string name)
        {
            var str = GetRequiredStringArgument(sm, name);

            if (!DateTime.TryParse(str, out var value)) throw new PreconditionFailedException($"Failed to parse argument `{name}` as DateTime.");
            return value;
        }

        private static RaidSlot[] GetRequiredSignupPresetArgument(SocketMessage sm)
        {
            var str = GetRequiredStringArgument(sm, "preset");

            if (!RaidsSignup.Presets.TryGetValue(str, out var value)) throw new PreconditionFailedException($"Unknown preset `{str}`. Valid presets are: {string.Join(", ", RaidsSignup.Presets.Keys)}");
            return value;
        }

        private static DateTime GetRequiredDateTimeArgument(SocketMessage sm, string name)
        {
            var str = GetRequiredStringArgument(sm, name);

            if (!DateTime.TryParse(str, null, DateTimeStyles.AssumeLocal, out var value)) throw new PreconditionFailedException($"Failed to parse argument `{name}` as DateTime.");
            return value;
        }

        public class PreconditionFailedException : Exception
        {
            public string Reason;

            public PreconditionFailedException(string reason)
            {
                Reason = reason;
            }
        }
#endregion
    }
}
