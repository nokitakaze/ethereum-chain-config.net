using System.Diagnostics;
using System.Text.Json.Serialization;

namespace NokitaKaze.EthereumChainConfig.Models
{
    [DebuggerDisplay("Relayer {url}")]
    public class Relayer
    {
        [JsonInclude]
        public string url { get; private set; } = string.Empty;

        [JsonInclude]
        public string name { get; private set; } = string.Empty;

        [JsonInclude]
        public string cachedUrl { get; private set; } = string.Empty;
    }
}