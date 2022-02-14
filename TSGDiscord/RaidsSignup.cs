using System;
using System.Collections.Generic;
using System.Linq;
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
                new RaidSlot(CommanderBlue, "Commander"),
                new RaidSlot(Druid, "Druid"),
                new RaidSlot(BattleStandard, "Banner Slave"),
                new RaidSlot(Mirage, "Renegade - Alacrity"),
                new RaidSlot(Healbrand, "Healbrand"),
                new RaidSlot(Quickbrand, "Quickbrand"),
                new RaidSlot(Sub, "DPS", 4)
            },
            ["standardsubs"] = new[]
            {
                new RaidSlot(Chronomancer, "Chrono Tank / Quick"),
                new RaidSlot(Druid, "Druid"),
                new RaidSlot(BattleStandard, "Banner Slave"),
                new RaidSlot(Mirage, "Mirage / Alac"),
                new RaidSlot(Healbrand, "HB / Quick"),
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

        public RaidSlot(string emoji, string name, int size = 1)
        {
            Emoji = emoji;
            Name = name;
            Size = size;
            Users = new List<ulong>(size);
        }

        public override string ToString() => $"{Emoji} {Name} ({Size - Users.Count} open): {string.Join(", ", Users.Select(Utils.Mention))}";
    }
}
