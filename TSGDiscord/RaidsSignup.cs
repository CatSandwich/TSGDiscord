using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;

namespace TSGDiscord
{
    [Serializable]
    public class RaidsSignup
    {
        public static Dictionary<string, RaidSlot[]> Presets => new Dictionary<string, RaidSlot[]>
        {
            ["standard"] = new[]
            {
                new RaidSlot("<:commander_blue:935248843057025114>", "Commander", 1),
                new RaidSlot("<:Druid:931401803856244797>", "Druid", 1),
                new RaidSlot("<:BattleStandard:931401803604578326>", "Banner Slave", 1),
                new RaidSlot("<:Mirage:931401803583615007>", "Renegade - Alacrity", 1),
                new RaidSlot("<:Firebrand:931401803810082826>", "Healbrand - Quickness", 1),
                new RaidSlot("<:Quickbrand:936346814418919546>", "Firebrand - Quickness", 1),
                new RaidSlot("⚔️", "DPS", 4)
            },
            ["standardsubs"] = new[]
            {
                new RaidSlot("<:Chronomancer:931401803793326080>", "Chrono Tank / Quick", 1),
                new RaidSlot("<:Druid:931401803856244797>", "Druid", 1),
                new RaidSlot("<:BattleStandard:931401803604578326>", "Banner Slave", 1),
                new RaidSlot("<:Mirage:931401803583615007>", "Mirage / Alac", 1),
                new RaidSlot("<:Firebrand:931401803810082826>", "HB / Quick", 1),
                new RaidSlot("⚔️", "DPS", 5),
                new RaidSlot("➕", "Sub", 3)
            }
        };

        public ulong ChannelId;
        public ulong MessageId;
        public ulong StartTime;
        public ulong EndTime;
        public RaidSlot[] Slots;

        public RaidsSignup(ulong channelId, ulong messageId, ulong startTime, ulong endTime, RaidSlot[] slots)
        {
            ChannelId = channelId;
            MessageId = messageId;
            StartTime = startTime;
            EndTime = endTime;
            Slots = slots;
        }

        public Embed CreateEmbed() =>
            new EmbedBuilder
            {
                Title = "Raid Signup",
                Description = $"{StartTime.Timestamp()} to {EndTime.Timestamp()}\n\n{string.Join<RaidSlot>('\n', Slots)}"
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

        public override string ToString() => $"{Emoji} {Name} ({Size - Users.Count} open): {string.Join(", ", Users.Select(Utils.Mention))}";
    }
}
