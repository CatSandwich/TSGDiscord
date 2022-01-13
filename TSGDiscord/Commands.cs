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
            var message = await sm.Channel.SendMessageAsync("Creating...");
            var signup = new RaidsSignup(sm.Channel.Id, message.Id, new[]
            {
                new RaidSlot("1️⃣", "Chrono Tank / Quick", 1),
                new RaidSlot("2️⃣", "Druid", 1),
                new RaidSlot("3️⃣", "Banner Slave", 1),
                new RaidSlot("4️⃣", "Mirage / Alac", 1),
                new RaidSlot("5️⃣", "HB / Quick", 1),
                new RaidSlot("6️⃣", "DPS", 5)
            });
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
