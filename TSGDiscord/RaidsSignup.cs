using System;
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
        public ulong? User;

        public RaidSlot(string emoji, string name)
        {
            Emoji = emoji;
            Name = name;
        }

        public override string ToString() => $"{Emoji} {Name}: {User?.Mention() ?? "Open"}";
    }
}
