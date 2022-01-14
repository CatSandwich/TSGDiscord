using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace TSGDiscord
{
    public static class Utils
    {
        private static readonly Regex ArgWithQuotes;
        private static readonly Regex ArgWithoutQuotes;

        static Utils()
        {
            // Capture characters that aren't '='
            var argName = "([^=]+)";
            // Capture any number of characters inside quotes
            var inQuotes = "\"([^\"]*)\"";
            // Capture characters that don't start with a quote and aren't whitespace
            var notInQuotes = "([^\"][^\\s]*)";

            ArgWithQuotes = new Regex($"-{argName}={inQuotes}", RegexOptions.Multiline | RegexOptions.Compiled);
            ArgWithoutQuotes = new Regex($"-{argName}={notInQuotes}", RegexOptions.Multiline | RegexOptions.Compiled);
        }

        public static string Mention(this ulong id) => $"<@!{id}>";
        public static string Timestamp(this ulong id) => $"<t:{id}>";

        public static Dictionary<string, string> GetArguments(this string content)
        {
            var dict = new Dictionary<string, string>();
            foreach (var hit in ArgWithQuotes.Matches(content).Concat(ArgWithoutQuotes.Matches(content)))
            {
                dict[hit.Groups[1].Value] = hit.Groups[2].Value;
            }
            return dict;
        }

        public static string? GetArgument(this string content, string arg) => content.GetArguments().TryGetValue(arg, out var val) ? val : null;

        public static bool TryParseMention(string mention, out ulong id)
        {
            if (!mention.StartsWith("<@!"))
            {
                id = 0;
                return false;
            }

            if (!mention.EndsWith(">"))
            {
                id = 0;
                return false;
            }

            return ulong.TryParse(mention[3..^1], out id);
        }

        public static ulong[] GetMentions(this string msg)
        {
            var regex = new Regex("<@!(\\d+)>");
            return regex.Matches(msg)
                .Select(match => ulong.Parse(match.Groups[1].Value))
                .ToArray();
        }

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

        public static int ReturnIntBetweenBrackets(string msg)
        {
            int stringFrom = msg.IndexOf("(") + "(".Length;
            int stringTo = msg.LastIndexOf(")");

            string input = msg.Substring(stringFrom, stringTo - stringFrom);

            if (int.TryParse(input, out int num))
            {
                return num;
            }
            else
            {
                num = -1;
                return num;
            }
        }

        public static bool IsFromOfficer(this SocketMessage sm)
        {
            if (!(sm.Author is SocketGuildUser user)) return false;
            return user.Roles.Select(role => role.Id).Any(id => Config.OfficerRoles.Contains(id));
        }
    }
}
