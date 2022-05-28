using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NokitaKaze.EthereumChainConfig.Models;

namespace NokitaKaze.EthereumChainConfig
{
    public class EthereumChainConfigService
    {
        public const string DefaultTornadoConfigFilename = "default-ethereum-config.json";

        // ReSharper disable UnusedMember.Global

        /// <summary>
        /// Main Ethereum network
        /// https://ethereum.org/
        /// </summary>
        public const int ETHEREUM_ID = 1;

        public const int ROPSTEN_ID = 3;

        /// <summary>
        /// Ethereum's Rinkeby test network.
        /// https://www.rinkeby.io/#stats
        /// </summary>
        public const int RINKEBY_ID = 4;

        /// <summary>
        /// Ethereum's Goerli/Görli test network.
        /// https://goerli.net/
        /// </summary>
        public const int GOERLI_ID = 5;

        /// <summary>
        /// Ethereum's Kovan test network.
        /// Kovan is a Proof of Authority (PoA) publicly accessible blockchain for Ethereum;
        /// created and maintained by a consortium of Ethereum developers, to aide the Ethereum developer community
        /// https://kovan-testnet.github.io/website/
        /// </summary>
        public const int KOVAN_ID = 6;

        /// <summary>
        /// Binance Smart Chain / BNB Smart Chain / BSC.
        ///
        /// Binance Smart Chain is an innovative solution to bring programmability and interoperability
        /// to Binance Chain. Binance Smart Chain relies on a system of 21 validators with
        /// Proof of Staked Authority (PoSA) consensus that can support short block time and lower fees
        /// https://www.bnbchain.org/
        /// https://docs.binance.org/smart-chain/guides/bsc-intro.html
        /// https://github.com/bnb-chain/whitepaper/blob/master/WHITEPAPER.md
        /// </summary>
        public const int BNB_SMART_CHAIN_ID = 56;

        /// <summary>
        /// Test Binance Smart Chain / Test BNB Smart Chain
        /// https://docs.binance.org/guides/testnet.html
        /// https://docs.binance.org/smart-chain/developer/rpc.html
        /// </summary>
        public const int TEST_BNB_SMART_CHAIN_ID = 97;
        // ReSharper restore UnusedMember.Global

        private protected readonly IReadOnlyDictionary<string, EthereumSingleChainConfig> Config;

        private protected EthereumChainConfigService(string jsonString)
        {
            Config = JsonSerializer.Deserialize<Dictionary<string, EthereumSingleChainConfig>>(jsonString)!;
        }

        #region Public constructors

        public static EthereumChainConfigService CreateConfigFromJson(string json)
        {
            return new EthereumChainConfigService(json);
        }

        public static EthereumChainConfigService CreateConfigFromFile(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new Exception("Config ethereum file " + filename + " not found");
            }

            var jsonText = File.ReadAllText(filename);

            return CreateConfigFromJson(jsonText);
        }

        public static EthereumChainConfigService CreateConfigFromDefaultFile()
        {
            var filename = Path.Combine(AppContext.BaseDirectory, DefaultTornadoConfigFilename);
            if (!File.Exists(filename))
            {
                throw new Exception("Can not find default config named " + DefaultTornadoConfigFilename);
            }

            return CreateConfigFromFile(filename);
        }

        public static async Task<EthereumChainConfigService> CreateConfigFromFileAsync(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new Exception("Config ethereum file " + filename + " not found");
            }

            var jsonText = await File.ReadAllTextAsync(filename);

            return CreateConfigFromJson(jsonText);
        }

        public static Task<EthereumChainConfigService> CreateConfigFromDefaultFileAsync()
        {
            var filename = Path.Combine(AppContext.BaseDirectory, DefaultTornadoConfigFilename);
            if (!File.Exists(filename))
            {
                throw new Exception("Can not find default config named " + DefaultTornadoConfigFilename);
            }

            return CreateConfigFromFileAsync(filename);
        }

        #endregion

        public EthereumSingleChainConfig GetChainConfig(int chainId = 1)
        {
            var key = "netId" + chainId;
            if (!Config.ContainsKey(key))
            {
                throw new Exception($"Can not find chain {chainId} in config dictionary");
            }

            return Config[key];
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Global
        public int[] GetChainIds()
        {
            return Config
                .Keys
                .Select(t =>
                {
                    if (!t.StartsWith("netId"))
                    {
                        // ReSharper disable once RedundantCast
                        return (int?)null;
                    }

                    if (!int.TryParse(t[5..], out var chainId))
                    {
                        // ReSharper disable once RedundantCast
                        return (int?)null;
                    }

                    return chainId;
                })
                .Where(x => x != null)
                .Cast<int>()
                .ToArray();
        }
    }
}