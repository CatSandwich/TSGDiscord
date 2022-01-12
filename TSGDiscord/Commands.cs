using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace TSGDiscord
{
    public static class Commands
    {
        public static async Task ReturnTimeToDailyReset(Bot bot, SocketMessage sm)
        {
            var now = DateTime.UtcNow;
            var reset = new DateTime(now.Year, now.Month, now.Day + 1, 0, 0, 0);

            var timeRemaining = reset - now;

            await sm.Channel.SendMessageAsync($"The Time Remaining Until Daily Reset Is: {timeRemaining.Hours}:{timeRemaining.Minutes}");
        }

        public static async Task RemovePaps(Bot bot, SocketMessage sm)
        {
            if (sm.Author is SocketGuildUser user)
            {
                if (user.Roles.Select(role => role.Id).Any(id => Config.OfficerRoles.Contains(id)))
                {
                    foreach (var id in sm.Content.GetMentions())
                    {
                        if (bot.Participation.ContainsKey(id))
                        {
                            bot.Participation[id] = 0;

                            await sm.Channel.SendMessageAsync(
                                $"{id.Mention()}'s Participation Score is: {bot.Participation[id]}");

                            bot.SerializeParticipation();
                        }
                    }

                }
            }
        }

        public static async Task AddOnePaP(Bot bot, SocketMessage sm)
        {
            if (sm.Author is SocketGuildUser user)
            {
                if (user.Roles.Select(role => role.Id).Any(id => Config.OfficerRoles.Contains(id)))
                {
                    foreach (var id in sm.Content.GetMentions())
                    {
                        if (!bot.Participation.ContainsKey(id)) bot.Participation[id] = 0;

                        bot.Participation[id]++;
                        Console.WriteLine("Here");
                        await sm.Channel.SendMessageAsync(
                            $"{id.Mention()}'s Participation Score is: {bot.Participation[id]}");
                    }

                    bot.SerializeParticipation();

                }
            }
        }

        public static async Task SetUserPaps(Bot bot, SocketMessage sm)
        {
            int newPaP = Utils.ReturnIntBetweenBrackets(sm.Content);

            if (Utils.IsUserOfficer(sm))
            {
                if (newPaP >= 0)
                {
                    foreach (var id in sm.Content.GetMentions())
                    {
                        if (!bot.Participation.ContainsKey(id)) bot.Participation[id] = 0;

                        bot.Participation[id] = newPaP;
                        await sm.Channel.SendMessageAsync(
                            $"{id.Mention()}'s Participation Score is: {bot.Participation[id]}");

                        bot.SerializeParticipation();
                    }
                }
                else
                {
                    await sm.Channel.SendMessageAsync("Invalid Format");
                }
            }
            else
            {
                await sm.Channel.SendMessageAsync("Only Officers May Use This Command");
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
            if (Utils.IsUserOfficer(sm))
            {
                string allUsers = "";

                foreach (var id in bot.Participation)
                {
                    allUsers += ("User: {0},  Participation Score: {1} \n", id.Key, id.Value);
                }

                Console.WriteLine(allUsers);

                await sm.Channel.SendMessageAsync("Done!");
            }
        }
    }
}
