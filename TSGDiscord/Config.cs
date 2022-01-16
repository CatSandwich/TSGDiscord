using System.IO;

namespace TSGDiscord
{
    public static class Config
    {
        public const string Prefix = "!";
        public const ulong GuildId = 907035760945938463;

        public static readonly string RaidsSignupDataPath = @$"{Directory.GetCurrentDirectory()}\signups.dat";
        public static readonly string ParticipationTrackingDataPath = @$"{Directory.GetCurrentDirectory()}\participation.dat";

        public static ulong[] OfficerRoles = {907036197816238081, 930346276107718726, 930346208331968553, 907036129298112582};
        public static ulong[] GuildMasterRoles = {907036129298112582, 930332334971035708};
    }
}
