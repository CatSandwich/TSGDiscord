using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace TSGDiscord
{
    public static class Commands
    {
        public static async Task RemovePaps(SocketMessage sm, Bot bot)
        {
            if (sm.Content.ToLower().StartsWith("!removepap"))
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
        }

        public static async Task RaidSignup(SocketMessage sm, Bot bot)
        {
            if (sm.Content == "raidsignup")
            {
                var message = await sm.Channel.SendMessageAsync("Creating...");
                var signup = new RaidsSignup(sm.Channel.Id, message.Id, new[]
                {
                    new RaidSlot("1️⃣", "Chrono Tank / Quick"),
                    new RaidSlot("2️⃣", "Druid"),
                    new RaidSlot("3️⃣", "Banner Slave"),
                    new RaidSlot("4️⃣", "DPS"),
                    new RaidSlot("5️⃣", "DPS"),

                    new RaidSlot("6️⃣", "Mirage / Alac"),
                    new RaidSlot("7️⃣", "HB / Quick"),
                    new RaidSlot("8️⃣", "DPS"),
                    new RaidSlot("9️⃣", "DPS"),
                    new RaidSlot("🔟", "DPS")
                });
                bot.RaidSignups.Add(signup.MessageId, signup);
                bot.Serialize();
                await bot.EditRaidSignup(signup);
                return;
            }
        }

        public static async Task AddOnePaP(SocketMessage sm, Bot bot)
        {
            if (sm.Content.ToLower().StartsWith("!pap"))
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
        }

    }
}
