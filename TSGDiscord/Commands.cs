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

        public static async Task SetUserPaps(SocketMessage sm, Bot bot)
        {
            int newPaP = Utils.ReturnIntBetweenBrackets(sm.Content);

            if (sm.Content.ToLower().StartsWith("!setpap"))
            {
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

        }

        public static async Task RaidSignup(SocketMessage sm, Bot bot)
        {
            if (sm.Content == "!raidsignup")
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
                return;
            }
        }

        public static async Task PraiseJoko(SocketMessage sm, Bot bot)
        {
            if (sm.Content.ToLower().Contains("praise joko"))
            {
                Console.WriteLine("test");
                await sm.Channel.SendMessageAsync("Praise Joko!");
            }
        }

    }
}
