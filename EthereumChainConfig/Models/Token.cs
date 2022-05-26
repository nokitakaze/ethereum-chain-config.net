using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace NokitaKaze.EthereumChainConfig.Models
{
    /// <summary>
    /// Token existing in Tornado in current Ethereum chain
    /// </summary>
    [DebuggerDisplay("Token {symbol} (dec = {decimals})")]
    public class Token
    {
        [JsonInclude]
        public IReadOnlyDictionary<string, string>? mixerAddress { get; private set; }

        [JsonInclude]
        public bool miningEnabled { get; private set; }

        [JsonInclude]
        public string symbol { get; private set; } = string.Empty;

        [JsonInclude]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int decimals { get; private set; } = -1;

        [JsonInclude]
        public string? tokenAddress { get; private set; }

        [JsonInclude]
        [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
        public int? gasLimit { get; private set; }

        public decimal[] GetAmounts()
        {
            return mixerAddress!
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .Select(t => decimal.Parse(t.Key))
                .ToArray();
        }
    }
}