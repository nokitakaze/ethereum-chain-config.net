using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;

namespace NokitaKaze.EthereumChainConfig.Models
{
    /// <summary>
    /// Configuration for a singe chain like Ethereum or Binance or Goerli
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [DebuggerDisplay("Chain {currencyName} in {explorerUrl.mainUrlPrefix}")]
    public class EthereumSingleChainConfig
    {
        [JsonInclude]
        public int rpcCallRetryAttempt { get; private set; }

        [JsonInclude]
        public IReadOnlyDictionary<string, decimal>? gasPrices { get; private set; }

        /// <summary>
        /// Is EIP-1559 enabled for this chain
        /// </summary>
        /// https://github.com/ethereum/EIPs/blob/master/EIPS/eip-1559.md
        [JsonInclude]
        public bool EIP1559Enabled { get; private set; }

        [JsonInclude]
        public string nativeCurrency { get; private set; } = string.Empty;

        [JsonIgnore]
        public int nativeCurrencyDecimals => tokens![nativeCurrency].decimals;

        [JsonInclude]
        public string currencyName { get; private set; } = string.Empty;

        [JsonInclude]
        public ExplorerUrl? explorerUrl { get; private set; }

        [JsonInclude]
        public string updateTreesBackend { get; private set; } = string.Empty;

        [JsonInclude]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int merkleTreeHeight { get; private set; } = -1;

        [JsonInclude]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int merkleTreeFarmerHeight { get; private set; } = -1;

        [JsonInclude]
        public string emptyElement { get; private set; } = string.Empty;

        [JsonInclude]
        public string networkName { get; private set; } = string.Empty;

        [JsonInclude]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int deployedBlock { get; private set; } = -1;

        [JsonInclude]
        public IReadOnlyDictionary<string, RpcUrl>? rpcUrls { get; private set; }

        [JsonInclude]
        public string multicall { get; private set; } = string.Empty;

        [JsonInclude]
        public string echoContract { get; private set; } = string.Empty;

        [JsonInclude]
        public string echoContractAccount { get; private set; } = string.Empty;

        [JsonInclude]
        public string aggregatorContract { get; private set; } = string.Empty;

        [JsonInclude]
        public IReadOnlyDictionary<string, Token>? tokens { get; private set; }

        [JsonInclude]
        public IReadOnlyDictionary<string, Relayer>? relayers { get; private set; }

        [JsonInclude]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int pollInterval { get; private set; } = -1;

        [JsonInclude]
        public IReadOnlyDictionary<string, long>? constants { get; private set; }

        [JsonPropertyName("torn.contract.tornadocash.eth")]
        [JsonInclude]
        public string? torn_contract_tornadocash_eth { get; private set; }

        [JsonPropertyName("governance.contract.tornadocash.eth")]
        [JsonInclude]
        public string? governance_contract_tornadocash_eth { get; private set; }

        [JsonPropertyName("reward-swap.contract.tornadocash.eth")]
        [JsonInclude]
        public string? reward_swap_contract_tornadocash_eth { get; private set; }

        [JsonPropertyName("tornado-proxy.contract.tornadocash.eth")]
        [JsonInclude]
        public string? tornado_proxy_contract_tornadocash_eth { get; private set; }

        [JsonPropertyName("tornado-proxy-light.contract.tornadocash.eth")]
        [JsonInclude]
        public string? tornado_proxy_light_contract_tornadocash_eth { get; private set; }

        public string? tornado_proxy_contract_projected =>
            tornado_proxy_contract_tornadocash_eth ?? tornado_proxy_light_contract_tornadocash_eth;

        [JsonPropertyName("tornado-trees.contract.tornadocash.eth")]
        [JsonInclude]
        public string? tornado_trees_contract_tornadocash_eth { get; private set; }

        [JsonPropertyName("mining-v2.contract.tornadocash.eth")]
        [JsonInclude]
        public string? mining_v2_contract_tornadocash_eth { get; private set; }

        [JsonPropertyName("voucher.contract.tornadocash.eth")]
        [JsonInclude]
        public string? voucher_contract_tornadocash_eth { get; private set; }

        public ICollection<string> GetRPCUrls()
        {
            return rpcUrls!
                .Values
                .Select(t => t.url)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(t => t.TrimEnd('/') + "/")
                .ToArray();
        }
    }
}