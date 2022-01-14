using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TSGDiscord
{
    public static class DpsReport
    {
        private const string Url = "https://dps.report/uploadContent";

        private static readonly HttpClient Client = new HttpClient();

        public static async Task<Response> Upload(byte[] log)
        {
            var res = await Client.PostAsync(Url, new MultipartFormDataContent
            {
                { new StreamContent(new MemoryStream(log)), "file", "log.zevtc" },
                { new StringContent("1"), "json" }
            });
            return JsonSerializer.Deserialize<Response>(await res.Content.ReadAsStringAsync());
        }

        public class Response
        {
            [JsonPropertyName("permalink")]
            public string Permalink { get; set; }
            [JsonPropertyName("encounterTime")]
            public ulong EncounterTime { get; set; }

            [JsonPropertyName("encounter")]
            public Encounter Encounter { get; set; }

            public override string ToString() => $"Report: {Permalink}\nBoss: {Encounter.Boss}{(Encounter.IsChallengeMode ? " (CM)" : "")}\nTime: {EncounterTime.Timestamp()}\nComp DPS: {Encounter.CompDps}";
        }

        public class Encounter
        {
            [JsonPropertyName("compDps")]
            public int CompDps { get; set; }
            [JsonPropertyName("boss")]
            public string Boss { get; set; }
            [JsonPropertyName("isCm")]
            public bool IsChallengeMode { get; set; }
        }
    }
}
