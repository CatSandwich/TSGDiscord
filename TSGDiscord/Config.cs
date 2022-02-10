using System.Collections.Generic;
using System.IO;

namespace TSGDiscord
{
    public static class Config
    {
        public const string Prefix = "!";
        public const ulong GuildId = 907035760945938463;

        public static readonly string RaidsSignupDataPath = @$"{Directory.GetCurrentDirectory()}\signups.dat";
        public static readonly string ParticipationTrackingDataPath = @$"{Directory.GetCurrentDirectory()}\participation.dat";

        //The Order Of These Roles Now Matters - DO NOT CHANGE
        public static ulong[] GuildMasterRoles = { 907036129298112582, 930332334971035708 };
        public static ulong[] OfficerRoles = { 932048274687406140 , 930346208331968553 , 930346276107718726 , 932048508788310027 , 907036197816238081, 907036129298112582, 930332334971035708 };
        public static ulong[] NCMRoles =
        {
            907036334894514237, 930345159835676702, 932047500586651719, 911031416945520660, 930345764247457863,
            932047858717294693, 932047941907152947
        };

        public static (string name, ulong roleid, int ppoints)[] NcmTuples =
        {
            ("Guild Member", 907036334894514237, 0),
            ("Corporal", 930345159835676702, 5),
            ("Master Corporal", 932047500586651719, 20),
            ("Sergeant", 911031416945520660, 40),
            ("Warrant Officer", 930345764247457863, 60),
            ("Master Warrant Officer", 932047858717294693, 80),
            ("Chief Warrant Officer", 932047941907152947, 150)
        };

        public static (string name, ulong roleid, int ppoints)[] OfficerTuples =
        {
            ("Lieutenant", 932048274687406140, 0),
            ("Captain", 930346208331968553, 30),
            ("Major", 930346276107718726, 80),
            ("Lieutenant Colonel", 932048508788310027, 150),
            ("Colonel", 907036197816238081, 200)
        };


        public static class Emoji
        {
            public static readonly Dictionary<string, string> Dictionary = new Dictionary<string, string>
            {
                ["commander_blue"] = CommanderBlue,

                ["chronomancer"] = Chronomancer,
                ["chrono"] = Chronomancer,
                ["druid"] = Druid,
                ["firebrand"] = Firebrand,
                ["healbrand"] = Firebrand,
                ["mirage"] = Mirage,
                ["quickbrand"] = Quickbrand,

                ["banner"] = BattleStandard,
                ["banner_slave"] = BattleStandard,
                ["battle_standard"] = BattleStandard,

                ["dps"] = Dps,
                ["sub"] = Sub,
                ["subs"] = Sub
            };

            #region Commander
            public const string CommanderBlue = "<:commander_blue:935248843057025114>";
            #endregion

            #region Classes
            public const string Chronomancer = "<:Chronomancer:931401803793326080>";
            public const string Druid = "<:Druid:931401803856244797>";
            public const string Firebrand = "<:Firebrand:931401803810082826>";
            public const string Mirage = "<:Mirage:931401803583615007>";
            public const string Quickbrand = "<:Quickbrand:936346814418919546>";
            #endregion

            #region Skills
            public const string BattleStandard = "<:BattleStandard:931401803604578326>";
            #endregion

            #region Role
            public const string Dps = "⚔️";
            public const string Sub = "➕";
            #endregion
        }
    }
}
