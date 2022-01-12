using System;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace TSGDiscord
{
    [Serializable]
    public class RaidsSignup
    {
        public ulong ChannelId;
        public ulong MessageId;
        public RaidSlot[] Slots;

        public RaidsSignup(ulong channelId, ulong messageId, RaidSlot[] slots)
        {
            ChannelId = channelId;
            MessageId = messageId;
            Slots = slots;
        }

        public Embed CreateEmbed() =>
            new EmbedBuilder
            {
                Title = "Raid Signup",
                Description = string.Join<RaidSlot>('\n', Slots)
            }.Build();
    }

    [Serializable]
    public class RaidSlot
    {
        public string Emoji;
        public string Name;
        public int Size;
        public List<ulong> Users;

        public RaidSlot(string emoji, string name, int size)
        {
            Emoji = emoji;
            Name = name;
            Size = size;
            Users = new List<ulong>(size);
        }

        public override string ToString()
        {
            var sb = new StringBuilder($"{Emoji} {Name} ");
            sb.Append($"({Size - Users.Count} open): ");

            var slots = new List<string>();
            for (var i = 0; i < Users.Count; i++)
            {
                slots.Add(Users[i].Mention());
            }
            for (var i = Users.Count; i < Size; i++)
            {
                slots.Add("Open");
            }
            sb.Append(string.Join(", ", slots));

            return sb.ToString();
        }
    }
}
