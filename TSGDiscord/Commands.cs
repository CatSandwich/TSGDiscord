using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace TSGDiscord
{
    public static class Commands
    {
        public static async Task TestArgs(Bot bot, SocketMessage sm)
        {
            var s = string.Join("\n", sm.Content.GetArguments().Select(arg => $"{arg.Key}={arg.Value}"));
            await sm.Channel.SendMessageAsync(s == "" ? "No args found" : s);
        }

        public static async Task ReturnTimeToDailyReset(Bot bot, SocketMessage sm)
        {
            var now = DateTime.UtcNow;
            var reset = new DateTime(now.Year, now.Month, now.Day + 1, 0, 0, 0);

            var timeRemaining = reset - now;

            await sm.Channel.SendMessageAsync($"The Time Remaining Until Daily Reset Is: {timeRemaining.Hours}:{timeRemaining.Minutes}");
        }

        public static async Task RemovePaps(Bot bot, SocketMessage sm)
        {
            if (!sm.IsFromOfficer())
            {
                await sm.Channel.SendMessageAsync("Only officers may use this command.");
                return;
            }

            foreach (var id in sm.Content.GetMentions())
            {
                bot.Participation[id] = 0;
                await sm.Channel.SendMessageAsync($"{id.Mention()}'s Participation Score is: {bot.Participation[id]}");
                bot.SerializeParticipation();
            }
        }

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

        public static async Task SetUserPaps(Bot bot, SocketMessage sm)
        {
            if (!sm.IsFromOfficer())
            {
                await sm.Channel.SendMessageAsync("Only officers may use this command.");
                return;
            }

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

        public static async Task RaidSignup(Bot bot, SocketMessage sm)
        {
            var args = sm.Content.GetArguments();

            #region Arguments
            if (!args.TryGetValue("preset", out var preset))
            {
                await sm.Channel.SendMessageAsync("'preset' argument required.");
                return;
            }

            if (!RaidsSignup.Presets.TryGetValue(preset, out var slots))
            {
                await sm.Channel.SendMessageAsync($"Unknown preset. Valid presets are: {string.Join(", ", RaidsSignup.Presets.Keys)}");
                return;
            }

            if (!args.TryGetValue("start", out var startArg))
            {
                await sm.Channel.SendMessageAsync("'start' argument required.");
                return;
            }

            if (!ulong.TryParse(startArg, out var start))
            {
                await sm.Channel.SendMessageAsync("Invalid start time. Must be unix timestamp.");
                return;
            }

            if (!args.TryGetValue("end", out var endArg))
            {
                await sm.Channel.SendMessageAsync("'start' argument required.");
                return;
            }

            if (!ulong.TryParse(endArg, out var end))
            {
                await sm.Channel.SendMessageAsync("Invalid end time. Must be unix timestamp.");
                return;
            }
            #endregion

            var message = await sm.Channel.SendMessageAsync("Creating...");
            var signup = new RaidsSignup(sm.Channel.Id, message.Id, start, end, slots);
            bot.RaidSignups.Add(signup.MessageId, signup);
            bot.Serialize();
            await bot.EditRaidSignup(signup);
        }

        public static async Task PrintAllParticipationScores(Bot bot, SocketMessage sm)
        {
            if (!sm.IsFromOfficer())
            {
                await sm.Channel.SendMessageAsync("Only officers may use this command.");
                return;
            }

            var allUsers = "";
            foreach (var (key, value) in bot.Participation)
            {
                allUsers += $"User: {key},  Participation Score: {value}\n";
            }
            Console.WriteLine(allUsers);

            await sm.Channel.SendMessageAsync("Done!");
        }
    }
}
