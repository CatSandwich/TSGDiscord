using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace TSGDiscord
{
    public static class Commands
    {
        public static Command Help = new Command("", async (bot, sm) =>
        {
            string help = "All Commands Require The Precursor Symbol of ! To Register As Commands \n\n";

            foreach (var id in bot.Commands)
            {
                help += $"Command: \"!{id.Key}\" --- Description: {id.Value.Description} \n";
            }

            await sm.Author.SendMessageAsync(help);
        });

        public static Command TestScheduler = new Command("", async (bot, sm) =>
        {
            var time = GetRequiredDateTimeArgument(sm, "time");

            bot.Scheduler.Schedule("test", time, async () =>
            {
                await sm.Channel.SendMessageAsync("Scheduled message.");
            });
        });

        public static Command TestSchedulerRepeating = new Command("", async (bot, sm) =>
        {
            var time = GetRequiredDateTimeArgument(sm, "time");
            var repeat = GetRequiredIntArgument(sm, "repeat");

            bot.Scheduler.ScheduleRepeating("test", time, async () =>
            {
                await sm.Channel.SendMessageAsync("Scheduled message repeating.");
            }, new TimeSpan(0, 0, repeat));
        });

        public static Command RaidSignup = new Command("Prints a raid signup sheet with usable reactions for signup", async(bot, sm) =>
        {
            var slots = GetRequiredSignupPresetArgument(sm);
            var start = GetRequiredUlongArgument(sm, "start");
            var end = GetRequiredUlongArgument(sm, "end");

            var message = await sm.Channel.SendMessageAsync("Creating...");
            var signup = new RaidsSignup(sm.Channel.Id, message.Id, start, end, slots);
            bot.RaidSignups.Add(signup.MessageId, signup);
            bot.Serialize();
            await bot.EditRaidSignup(signup);
        });

        public static Command ReturnTimeToDailyReset = new Command("", async (bot, sm) =>
        {
            var now = DateTime.UtcNow;
            var reset = new DateTime(now.Year, now.Month, now.Day + 1, 0, 0, 0);

            var timeRemaining = reset - now;

            await sm.Channel.SendMessageAsync($"The Time Remaining Until Daily Reset Is: {timeRemaining.Hours}:{timeRemaining.Minutes:D2}");
        });


        #region Participation

        public static Command AddOnePaP = new Command("", async (bot, sm) =>
        {
            RequireOfficerRole(sm);

            foreach (var id in sm.Content.GetMentions())
            {
                if (!bot.Participation.ContainsKey(id)) bot.Participation[id] = 0;
                bot.Participation[id]++;
                await sm.Channel.SendMessageAsync($"{id.Mention()}'s Participation Score is: {bot.Participation[id]}");
            }

            bot.SerializeParticipation();
        });

        public static Command RemovePaps = new Command("", async (bot, sm) =>
        {
            RequireGMRole(sm);

            foreach (var id in sm.Content.GetMentions())
            {
                bot.Participation[id] = 0;
                await sm.Channel.SendMessageAsync($"{id.Mention()}'s Participation Score is: {bot.Participation[id]}");
                bot.SerializeParticipation();
            }
        });

        public static Command SetUserPaps = new Command("", async (bot, sm) =>
        {
            RequireOfficerRole(sm);

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
        });


        public static Command PrintAllParticipationScores = new Command("", async (bot, sm) =>
        {
            RequireOfficerRole(sm);

            foreach (var (key, value) in bot.Participation)
            {
                string allUsers = $"User: {key},  Participation Score: {value}";

                await sm.Author.SendMessageAsync(allUsers);

                Console.WriteLine(allUsers);
            }

            await sm.Author.SendMessageAsync("All User Paps Printed");

        });
        #endregion

        #region Preconditions
        private static void RequireRole(SocketMessage sm, params ulong[] roles)
        {
            if (!(sm.Author is SocketGuildUser socketUser)) throw new PreconditionFailedException("Failed to find roles of user.");
            if (socketUser.Roles.All(role => !roles.Contains(role.Id))) throw new PreconditionFailedException("Insufficient permissions.");
        }

        private static void RequireOfficerRole(SocketMessage sm) => RequireRole(sm, Config.OfficerRoles);

        private static void RequireGMRole(SocketMessage sm) => RequireRole(sm, Config.GuildMasterRoles);

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

        public class Command
        {
            public string Description;
            public Func<Bot, SocketMessage, Task> Handler;

            public Command(string description, Func<Bot, SocketMessage, Task> handler)
            {
                Description = description;
                Handler = handler;
            }
        }
    }
}
