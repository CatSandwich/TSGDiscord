﻿using System.IO;

namespace TSGDiscord
{
    public static class Config
    {
        public static readonly string RaidsSignupDataPath = @$"{Directory.GetCurrentDirectory()}\signups.dat";
        public static readonly string ParticipationTrackingDataPath = @$"{Directory.GetCurrentDirectory()}\participation.dat";

        public static ulong[] OfficerRoles = {907036197816238081, 930346276107718726, 930346208331968553, 907036129298112582};
    }
}
