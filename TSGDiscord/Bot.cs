using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace TSGDiscord
{
    public class Bot : DiscordSocketClient
    {
        public Dictionary<ulong, RaidsSignup> RaidSignups = new Dictionary<ulong, RaidsSignup>();
        public Dictionary<ulong, int> Participation = new Dictionary<ulong, int>();

        public Bot()
        {
            Log += _log;
            MessageReceived += _messageReceivedHandler;
            ReactionAdded += _reactionAddedHandler;
            ReactionRemoved += _reactionRemovedHandler;
            _deserialize();
            _deserializeParticipation();
        }

        public async Task Run(string token)
        {
            await LoginAsync(TokenType.Bot, token);
            await StartAsync();
            await Task.Delay(-1);
        }

        private static Task _log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private void _serialize()
        {
            using var stream = File.Open(Config.RaidsSignupDataPath, FileMode.Create);
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, RaidSignups.Values.ToArray());
        }
        private void _serializeParticipation()
        {
            using var stream = File.Open(Config.ParticipationTrackingDataPath, FileMode.Create);
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, Participation);
        }

        private void _deserialize()
        {
            try
            {
                using var stream = File.Open(Config.RaidsSignupDataPath, FileMode.Open);
                var binaryFormatter = new BinaryFormatter();
                var signups = (RaidsSignup[])binaryFormatter.Deserialize(stream);
                RaidSignups = new Dictionary<ulong, RaidsSignup>(signups.Select(signup => new KeyValuePair<ulong, RaidsSignup>(signup.MessageId, signup)));
                Console.WriteLine($"{RaidSignups.Count} signups loaded.");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("No signup data found.");
            }
        }

        private void _deserializeParticipation()
        {
            try
            {
                using var stream = File.Open(Config.ParticipationTrackingDataPath, FileMode.Open);
                var binaryFormatter = new BinaryFormatter();
                Participation = (Dictionary<ulong, int>)binaryFormatter.Deserialize(stream);
                Console.WriteLine($"{Participation.Count} participation entries loaded.");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("No participation data found.");
            }
        }

        private Task _messageReceivedHandler(SocketMessage sm)
        {
            Task.Run<Task>(async () =>
            {
                if (sm.Content == "sendmessage")
                {
                    await sm.Channel.SendMessageAsync("Message");
                    return;
                }

                if (sm.Content == "raidsignup")
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
                    RaidSignups.Add(signup.MessageId, signup);
                    _serialize();
                    await this.EditRaidSignup(signup);
                    return;
                }

                if (sm.Content.ToLower().StartsWith("!pap"))
                {
                    await sm.Channel.SendMessageAsync("Message Recieved");

                    foreach (var id in sm.Content.GetMentions())
                    {
                        if (!Participation.ContainsKey(id)) Participation[id] = 0;

                        Participation[id]++;
                        Console.WriteLine("Here");
                        await sm.Channel.SendMessageAsync($"{id.Mention()}'s Participation Score is: {Participation[id]}");
                    }

                    _serializeParticipation();

                    await sm.Channel.SendMessageAsync("Message Recieved");
                }
            });

            return Task.CompletedTask;
        }

        private Task _reactionAddedHandler(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            Task.Run<Task>(async () =>
            {
                if (!reaction.User.IsSpecified || reaction.User.Value.IsBot) return;
                if (!RaidSignups.TryGetValue((await message.GetOrDownloadAsync()).Id, out var signup)) return;

                // Get slot of corresponding emoji
                var slot = signup.Slots.FirstOrDefault(slot => slot.Emoji == reaction.Emote.Name);

                // If slot doesn't exist, ignore
                // If slot is full, ignore
                // If user is already signed up, ignore
                if (slot is null || slot.Users.Count >= slot.Size || signup.Slots.Any(s => s.Users.Contains(reaction.UserId)))
                {
                    var msg = await message.GetOrDownloadAsync();
                    await msg.RemoveReactionAsync(reaction.Emote, reaction.UserId);
                    return;
                }

                slot.Users.Add(reaction.UserId);
                _serialize();
                await this.EditRaidSignup(signup);
            });
            return Task.CompletedTask;
        }

        private Task _reactionRemovedHandler(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            Task.Run(async () =>
            {
                if (!reaction.User.IsSpecified || reaction.User.Value.IsBot) return;
                if (!RaidSignups.TryGetValue((await message.GetOrDownloadAsync()).Id, out var signup)) return;

                var slot = signup.Slots.FirstOrDefault(slot => slot.Emoji == reaction.Emote.Name);

                // If no corresponding slot, ignore
                // If user not in slot, ignore
                if (slot == null || !slot.Users.Contains(reaction.UserId))
                {
                    return;
                };

                slot.Users.Remove(reaction.UserId);
                _serialize();
                await this.EditRaidSignup(signup);
            });
            return Task.CompletedTask;
        }
    }
}
