using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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
            Deserialize();
            DeserializeParticipation();
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

        public void Serialize()
        {
            using var stream = File.Open(Config.RaidsSignupDataPath, FileMode.Create);
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, RaidSignups.Values.ToArray());
        }

        public void SerializeParticipation()
        {
            using var stream = File.Open(Config.ParticipationTrackingDataPath, FileMode.Create);
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, Participation);
        }

        public void Deserialize()
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

        public void DeserializeParticipation()
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
                //Commands Go Here

                await Commands.RemovePaps(sm, this);

                await Commands.RaidSignup(sm, this);

                await Commands.AddOnePaP(sm, this);

                await Commands.SetUserPaps(sm, this);

                await Commands.PraiseJoko(sm, this);

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
                Serialize();
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
                Serialize();
                await this.EditRaidSignup(signup);
            });
            return Task.CompletedTask;
        }
    }
}
