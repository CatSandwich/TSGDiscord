using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;

using static TSGDiscord.Config.Emoji;

namespace TSGDiscord
{
    [Serializable]
    public class RaidsSignup
    {
        public static Dictionary<string, RaidSlot[]> Presets => new Dictionary<string, RaidSlot[]>
        {
            ["standard"] = new[]
            {
                new RaidSlot(CommanderBlue, "Commander", 1),
                new RaidSlot(Druid, "Druid", 1),
                new RaidSlot(BattleStandard, "Banner Slave", 1),
                new RaidSlot(Mirage, "Renegade - Alacrity", 1),
                new RaidSlot(Firebrand, "Healbrand - Quickness", 1),
                new RaidSlot(Quickbrand, "Firebrand - Quickness", 1),
                new RaidSlot("⚔️", "DPS", 4)
            },
            ["standardsubs"] = new[]
            {
                new RaidSlot(Chronomancer, "Chrono Tank / Quick", 1),
                new RaidSlot(Druid, "Druid", 1),
                new RaidSlot(BattleStandard, "Banner Slave", 1),
                new RaidSlot(Mirage, "Mirage / Alac", 1),
                new RaidSlot(Firebrand, "HB / Quick", 1),
                new RaidSlot(Dps, "DPS", 5),
                new RaidSlot(Sub, "Sub", 3)
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
