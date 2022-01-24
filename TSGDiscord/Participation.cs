using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Discord;
using Discord.WebSocket;

namespace TSGDiscord
{
    static class Participation
    {
        public static readonly Dictionary<ulong, int> Scores;

        static Participation()
        {
            // Deserialize
            try
            {
                using var stream = File.Open(Config.ParticipationTrackingDataPath, FileMode.Open);
                var binaryFormatter = new BinaryFormatter();
                Scores = (Dictionary<ulong, int>)binaryFormatter.Deserialize(stream);
                Console.WriteLine($"{Scores.Count} participation entries loaded.");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("No participation data found.");
                Scores = new Dictionary<ulong, int>();
            }
        }

        public static void Serialize()
        {
            using var stream = File.Open(Config.ParticipationTrackingDataPath, FileMode.Create);
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, Scores);
        }

        public static int GetPoints(ulong userId)
        {
            if (!Scores.ContainsKey(userId)) Scores[userId] = 0;
            return Scores[userId];
        }

        public static IRole? GetRole(SocketGuildUser user)
        {
            if (user.IsOfficer()) return GetOfficerRole(GetPoints(user.Id));
            if (user.IsNCM()) return GetNcmRole(GetPoints(user.Id));
            return null;
        }

        public static IRole GetOfficerRole(int points)
        {
            var earned = Config.OfficerTuples.Last(tuple => points >= tuple.ppoints);
            return Bot.Instance.GetRole(earned.roleid)!;
        }

        public static IRole GetNcmRole(int points)
        {
            var earned = Config.NcmTuples.Last(tuple => points >= tuple.ppoints);
            return Bot.Instance.GetRole(earned.roleid)!;
        }

        public static bool AddPap(SocketGuildUser user, out IRole promotedTo)
        {
            // Get current track role
            var currentRole = GetRole(user);
            if (currentRole == null)
            {
                promotedTo = null!;
                return false;
            }

            Scores[user.Id]++;

            promotedTo = GetRole(user)!;
            return currentRole != promotedTo;
        }
    }
}
