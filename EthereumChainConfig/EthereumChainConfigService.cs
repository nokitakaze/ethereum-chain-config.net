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
        public const string DefaultTornadoConfigFilename = "tornado-config.json";
        public const int ETHEREUM_ID = 1;

        public const int ROPSTEN_ID = 3;

        public const int RINKEBY_ID = 4;

        public const int GOERLI_ID = 5;

        public const int KOVAN_ID = 6;

        /// <summary>
        /// Binance Smart Chain / BNB Smart Chain
        /// </summary>
        public const int BNB_SMART_CHAIN_ID = 56;

        /// <summary>
        /// Test Binance Smart Chain / Test BNB Smart Chain
        /// </summary>
        public const int TEST_BNB_SMART_CHAIN_ID = 97;

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
            // todo check file
            var jsonText = File.ReadAllText(filename);

            return CreateConfigFromJson(jsonText);
        }

        public static EthereumChainConfigService CreateConfigFromDefaultFile()
        {
            var filename = Path.Combine(AppContext.BaseDirectory, DefaultTornadoConfigFilename);

            return CreateConfigFromFile(filename);
        }

        public static async Task<EthereumChainConfigService> CreateConfigFromFileAsync(string filename)
        {
            // todo check file
            var jsonText = await File.ReadAllTextAsync(filename);

            return CreateConfigFromJson(jsonText);
        }

        public static Task<EthereumChainConfigService> CreateConfigFromDefaultFileAsync()
        {
            var filename = Path.Combine(AppContext.BaseDirectory, DefaultTornadoConfigFilename);

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