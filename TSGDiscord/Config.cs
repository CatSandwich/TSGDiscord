using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSGDiscord
{
    public static class Config
    {
        public const ulong RaidsSignupChannel = 907037150711775262;
        public const ulong RaidsSignupMessage = 930380076103655455;
        public static readonly string RaidsSignupDataPath = @$"{System.IO.Directory.GetCurrentDirectory()}\signups.dat";
    }
}
