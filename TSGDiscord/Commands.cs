using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace TSGDiscord
{
    public static class Commands
    {
        public static async Task RaidSignup(Bot bot, SocketMessage sm)
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

        public static async Task ReturnTimeToDailyReset(Bot bot, SocketMessage sm)
        {
            var now = DateTime.UtcNow;
            var reset = new DateTime(now.Year, now.Month, now.Day + 1, 0, 0, 0);

            var timeRemaining = reset - now;

            await sm.Channel.SendMessageAsync($"The Time Remaining Until Daily Reset Is: {timeRemaining.Hours}:{timeRemaining.Minutes:D2}");
        }

        #region Participation
        public static async Task AddOnePaP(Bot bot, SocketMessage sm)
        {
            if (!sm.IsFromOfficer())
            {
                await sm.Channel.SendMessageAsync("Only officers may use this command.");
                return;
            }

            foreach (var id in sm.Content.GetMentions())
            {
                if (!bot.Participation.ContainsKey(id)) bot.Participation[id] = 0;
                bot.Participation[id]++;
                await sm.Channel.SendMessageAsync($"{id.Mention()}'s Participation Score is: {bot.Participation[id]}");
            }

            bot.SerializeParticipation();
        }

        public static async Task RemovePaps(Bot bot, SocketMessage sm)
        {
            RequireOfficerRole(sm);

            foreach (var id in sm.Content.GetMentions())
            {
                bot.Participation[id] = 0;
                await sm.Channel.SendMessageAsync($"{id.Mention()}'s Participation Score is: {bot.Participation[id]}");
                bot.SerializeParticipation();
            }
        }

        public static async Task SetUserPaps(Bot bot, SocketMessage sm)
        {
            RequireOfficerRole(sm);

            var newPaP = Utils.ReturnIntBetweenBrackets(sm.Content);

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

        public static async Task PrintAllParticipationScores(Bot bot, SocketMessage sm)
        {
            RequireOfficerRole(sm);

            var allUsers = "";
            foreach (var (key, value) in bot.Participation)
            {
                allUsers += $"User: {key},  Participation Score: {value}\n";
            }
            Console.WriteLine(allUsers);

            await sm.Channel.SendMessageAsync("Done!");
        }
        #endregion

        #region Preconditions
        private static void RequireRole(SocketMessage sm, params ulong[] roles)
        {
            if (!(sm.Author is SocketGuildUser socketUser)) throw new PreconditionFailedException($"Failed to find roles of user.");
            if (socketUser.Roles.All(role => !roles.Contains(role.Id))) throw new PreconditionFailedException("Insufficient permissions.");
        }

        private static void RequireOfficerRole(SocketMessage sm) => RequireRole(sm, Config.OfficerRoles);

        private static string GetRequiredStringArgument(SocketMessage sm, string name)
        {
            var arg = sm.Content.GetArgument(name);

            if (arg is null) throw new PreconditionFailedException($"Missing required argument: `{name}`");
            return arg;
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
