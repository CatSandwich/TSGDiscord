using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace TSGDiscord
{
    public static class Utils
    {
        public static string Mention(this ulong id) => $"<@!{id}>";

        public static async Task EditRaidSignup(this DiscordSocketClient client, RaidsSignup signup)
        {
            var channel = (ITextChannel) client.GetChannel(signup.ChannelId);
            var message = (RestUserMessage) await channel.GetMessageAsync(signup.MessageId);

            if (message == null)
            {
                Console.WriteLine($"[Error] Message could not be cast to {nameof(RestUserMessage)}.");
                return;
            }

            await message.ModifyAsync(prop =>
            {
                prop.Content = null;
                prop.Embed = signup.CreateEmbed();
            });

            foreach (var emoji in signup.Slots.Select(slot => slot.Emoji))
            {
                await message.AddReactionAsync(new Emoji(emoji));
            }
        }
    }
}
