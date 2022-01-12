using System.IO;

namespace TSGDiscord
{
    public static class Config
    {
        public static readonly string RaidsSignupDataPath = @$"{Directory.GetCurrentDirectory()}\signups.dat";
        public static readonly string ParticipationTrackingDataPath = @$"{Directory.GetCurrentDirectory()}\participation.dat";
    }
}
