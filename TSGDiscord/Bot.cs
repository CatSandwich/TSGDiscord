using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TSGDiscord.Commands;
using TSGDiscord.Commands.Attributes;

namespace TSGDiscord
{
    public class Bot : DiscordSocketClient
    {
        public static Bot Instance;
        public Scheduler Scheduler = new Scheduler();

        public Dictionary<ulong, RaidsSignup> RaidSignups = new Dictionary<ulong, RaidsSignup>();
        public Dictionary<string, Command> Commands = new Dictionary<string, Command>();

        public Bot() : base(new DiscordSocketConfig {AlwaysDownloadUsers = true, GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers})
        {
            Instance = this;

            Log += _log;
            MessageReceived += _messageReceivedHandler;
            ReactionAdded += _reactionAddedHandler;
            ReactionRemoved += _reactionRemovedHandler;
            Deserialize();
            _initializeCommands();
            SetStatus();
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

        private async Task SetStatus()
        {
            await SetGameAsync("Type !help for a list of commands");
        }

        public void Serialize()
        {
            using var stream = File.Open(Config.RaidsSignupDataPath, FileMode.Create);
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, RaidSignups.Values.ToArray());
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

        public IRole? GetRole(ulong id) => GetGuild(Config.GuildId).GetRole(id);
        public new SocketGuildUser? GetUser(ulong id)
        {
            var guild = GetGuild(Config.GuildId);
            var user = guild.GetUser(id);
            return user;
        }

        private async Task _messageReceivedHandler(SocketMessage sm)
        {
            if (sm.Author.IsBot) return;

            // Command handling
            foreach (var (name, command) in Commands)
            {
                if (sm.Content.ToLower().StartsWith($"{Config.Prefix}{name}"))
                {
                    // Assign to variable to suppress warning
                    var task = Task.Run(async () =>
                    {
                        try
                        {
                            foreach (var precondition in command.Preconditions)
                            {
                                await precondition.Check(this, sm);
                            }
                            await command.Handler(this, sm);
                        }
                        catch (Commands.Commands.PreconditionFailedException ex)
                        {
                            await sm.Channel.SendMessageAsync(ex.Reason);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception in command {name}: {ex}");
                        }
                    });
                }
            }

            // Praise joko
            if (sm.Content.ToLower().Contains("praise joko") || sm.Content.ToLower().Contains("praise aurene"))
            {
                await sm.Channel.SendMessageAsync("Praise Joko!");
            }

            // Upload arcdps logs automatically
            foreach (var attachment in sm.Attachments)
            {
                if (!attachment.Filename.EndsWith(".zevtc")) continue;

                // Assign to variable to suppress warning
                var task = Task.Run(async () =>
                {
                    try
                    {
                        var result = await DpsReport.Upload(await attachment.Download());
                        await sm.Channel.SendMessageAsync(result.ToString());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Exception while uploading log: {e}");
                    }
                });
            }
        }

        private Task _reactionAddedHandler(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            Task.Run<Task>(async () =>
            {
                if (!reaction.User.IsSpecified || reaction.User.Value.IsBot) return;
                if (!RaidSignups.TryGetValue((await message.GetOrDownloadAsync()).Id, out var signup)) return;

                // Get slot of corresponding emoji
                var slot = signup.Slots.FirstOrDefault(slot => (Utils.TryParseEmote(slot.Emoji, out var value) ? value : null)?.Name == reaction.Emote.Name);

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

                var slot = signup.Slots.FirstOrDefault(slot => (Utils.TryParseEmote(slot.Emoji, out var value) ? value : null)?.Name == reaction.Emote.Name);

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

        private void _initializeCommands()
        {
            var commands = GetType()
                .Assembly
                .GetTypes()
                .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                .Where(method => method.IsDefined(typeof(CommandAttribute)))
                .Select(method => new Command(method));

            foreach (var command in commands)
            {
                foreach (var name in command.Names)
                {
                    Commands[name] = command;
                }
            }
        }
    }
}
