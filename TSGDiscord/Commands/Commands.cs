using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TSGDiscord.Commands.Attributes;
using TSGDiscord.Commands.Attributes.Preconditions;

namespace TSGDiscord.Commands
{
    public static class Commands
    {
        [Command("help"), Description("DMs you this message.")]
        private static async Task Help(Bot bot, SocketMessage sm)
        {
            string help = "All Commands Require The Precursor Symbol of ! To Register As Commands \n\n";

            foreach (var id in bot.Commands)
            {
                help += $"Command: \"!{id.Key}\" --- Description: {id.Value.Description} \n";
            }

            await sm.Author.SendMessageAsync(help);
        }

        [Command("testscheduler")]
        private static async Task TestScheduler(Bot bot, SocketMessage sm)
        {
            var time = GetRequiredDateTimeArgument(sm, "time");

            bot.Scheduler.Schedule("test", time, async () => { await sm.Channel.SendMessageAsync("Scheduled message."); });
        }

        [Command("testrepeating")]
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
        private static async Task RaidSignup(Bot bot, SocketMessage sm)
        {
            var slots = GetRequiredSignupPresetArgument(sm);
            var start = GetRequiredUlongArgument(sm, "start");
            var end = GetRequiredUlongArgument(sm, "end");

            var message = await sm.Channel.SendMessageAsync("Creating...");
            var signup = new RaidsSignup(sm.Channel.Id, message.Id, start, end, slots);
            bot.RaidSignups.Add(signup.MessageId, signup);
            bot.Serialize();
            await bot.EditRaidSignup(signup);
        }

        [Command("reset", "untilreset", "timeuntilreset", "toreset", "timetoreset")]
        private static async Task TimeToReset(Bot bot, SocketMessage sm)
        {
            var now = DateTime.UtcNow;
            var reset = new DateTime(now.Year, now.Month, now.Day + 1, 0, 0, 0);

            var timeRemaining = reset - now;

            await sm.Channel.SendMessageAsync($"The Time Remaining Until Daily Reset Is: {timeRemaining.Hours}:{timeRemaining.Minutes:D2}");
        }


        #region Participation
        [Command("pap", "participation"), RequireOfficer]
        private static async Task AddParticipation(Bot bot, SocketMessage sm)
        {
            foreach (var id in sm.Content.GetMentions())
            {
                if (!bot.Participation.ContainsKey(id)) bot.Participation[id] = 0;
                bot.Participation[id]++;
                await sm.Channel.SendMessageAsync($"{id.Mention()}'s Participation Score is: {bot.Participation[id]}");
            }

            bot.SerializeParticipation();
        }

        [Command("removepap", "removeparticipation"), RequireGM]
        private static async Task RemovePap(Bot bot, SocketMessage sm)
        {
            bot.Participation = new Dictionary<ulong, int>();
            bot.SerializeParticipation();
        }

        [Command("setpap", "setparticipation"), RequireOfficer]
        private static async Task SetPap(Bot bot, SocketMessage sm)
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
        private static async Task PrintPap(Bot bot, SocketMessage sm)
        {
            foreach (var (key, value) in bot.Participation)
            {
                string allUsers = $"User: {key},  Participation Score: {value}";

                await sm.Author.SendMessageAsync(allUsers);

                Console.WriteLine(allUsers);
            }

            await sm.Author.SendMessageAsync("All User Paps Printed");
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

        private static RaidSlot[] GetRequiredSignupPresetArgument(SocketMessage sm)
        {
            var str = GetRequiredStringArgument(sm, "preset");

            if (!RaidsSignup.Presets.TryGetValue(str, out var value)) throw new PreconditionFailedException($"Unknown preset `{str}`. Valid presets are: {string.Join(", ", RaidsSignup.Presets.Keys)}");
            return value;
        }

        private static DateTime GetRequiredDateTimeArgument(SocketMessage sm, string name)
        {
            var str = GetRequiredStringArgument(sm, name);

            if (!DateTime.TryParse(str, out var value)) throw new PreconditionFailedException($"Failed to parse argument `{name}` as DateTime.");
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
