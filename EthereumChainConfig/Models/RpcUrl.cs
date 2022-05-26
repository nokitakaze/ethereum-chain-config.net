using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NokitaKaze.EthereumChainConfig.Models
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [DebuggerDisplay("RpcUrl {url}")]
    public class RpcUrl
    {
        [JsonInclude]
        public string name { get; private set; }

        [JsonInclude]
        public string url { get; private set; }
    }
}