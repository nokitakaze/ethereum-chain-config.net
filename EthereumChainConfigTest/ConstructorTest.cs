using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NokitaKaze.EthereumChainConfig.Models;
using Xunit;

namespace NokitaKaze.EthereumChainConfig.Test
{
    public class ConstructorTest
    {
        private static readonly PropertyInfo[] FieldsNotEmptyProps;
        private static readonly PropertyInfo[] FieldsNotMinusOneProps;
        private static readonly PropertyInfo[] FieldsOnlyNullOrNotEmptyProps;

        static ConstructorTest()
        {
            var fieldsNotEmpty = new[]
            {
                "gasPrices",
                "explorerUrl",
                "rpcUrls",
                "tokens",
                "relayers",
                "constants",
            };

            FieldsNotEmptyProps = typeof(EthereumSingleChainConfig)
                .GetProperties()
                .Where(x => fieldsNotEmpty.Contains(x.Name))
                .ToArray();

            var fieldsNotMinusOne = new[]
            {
                "merkleTreeHeight",
                "merkleTreeFarmerHeight",
                "deployedBlock",
                "pollInterval",
            };

            FieldsNotMinusOneProps = typeof(EthereumSingleChainConfig)
                .GetProperties()
                .Where(x => fieldsNotMinusOne.Contains(x.Name))
                .ToArray();

            var fieldsOnlyNullOrNotEmpty = new[]
            {
                "torn_contract_tornadocash_eth",
                "governance_contract_tornadocash_eth",
                "reward_swap_contract_tornadocash_eth",
                "tornado_proxy_contract_tornadocash_eth",
                "tornado_trees_contract_tornadocash_eth",
                "mining_v2_contract_tornadocash_eth",
                "voucher_contract_tornadocash_eth",
            };

            FieldsOnlyNullOrNotEmptyProps = typeof(EthereumSingleChainConfig)
                .GetProperties()
                .Where(x => fieldsOnlyNullOrNotEmpty.Contains(x.Name))
                .ToArray();
        }

        #region Assert Valid Address

        private static readonly Regex rValidAddress = new Regex("^0x[a-fA-F0-9]{40,40}$");

        private void AssertValidAddressNullEmpty(string? address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return;
            }

            AssertValidAddress(address);
        }

        private void AssertValidAddress(string address)
        {
            Assert.Matches(rValidAddress, address);
        }

        #endregion

        public static IEnumerable<object[]> MainTestData()
        {
            var defaultFilename = Path.Combine(AppContext.BaseDirectory,
                EthereumChainConfigService.DefaultTornadoConfigFilename);
            var json = File.ReadAllText(defaultFilename);

            var services = new[]
            {
                EthereumChainConfigService.CreateConfigFromDefaultFile(),
                EthereumChainConfigService.CreateConfigFromDefaultFileAsync().GetAwaiter().GetResult(),
                EthereumChainConfigService.CreateConfigFromFile(defaultFilename),
                EthereumChainConfigService.CreateConfigFromFileAsync(defaultFilename).GetAwaiter().GetResult(),
                EthereumChainConfigService.CreateConfigFromJson(json),
            };

            return services.Select(service => new[] { service });
        }

        [Theory]
        [MemberData(nameof(MainTestData))]
        public void MainTest(EthereumChainConfigService service)
        {
            Assert.NotNull(service);

            {
                var chainIds = new[]
                {
                    EthereumChainConfigService.ETHEREUM_ID,
                    EthereumChainConfigService.GOERLI_ID,
                    EthereumChainConfigService.BNB_SMART_CHAIN_ID,
                    EthereumChainConfigService.TEST_BNB_SMART_CHAIN_ID,
                };

                var existedChainIds = service.GetChainIds();
                foreach (var chainId in chainIds)
                {
                    Assert.Contains(chainId, existedChainIds);
                }
            }

            foreach (var chainId in service.GetChainIds())
            {
                var config = service.GetChainConfig(chainId);

                AssertValidAddressNullEmpty(config.echoContract);
                AssertValidAddressNullEmpty(config.echoContractAccount);
                AssertValidAddressNullEmpty(config.multicall);
                AssertValidAddressNullEmpty(config.aggregatorContract);

                foreach (var prop in FieldsNotEmptyProps)
                {
                    var value = prop.GetValue(config);
                    Assert.NotNull(value);
                }

                foreach (var prop in FieldsNotMinusOneProps)
                {
                    var value = prop.GetValue(config);
                    Assert.NotEqual(-1, value);
                }

                foreach (var prop in FieldsOnlyNullOrNotEmptyProps)
                {
                    var value = prop.GetValue(config);
                    if (value != null)
                    {
                        Assert.NotEqual(string.Empty, value);
                    }
                }

                config
                    .gasPrices!
                    .Values
                    .ToList()
                    .ForEach(value => { Assert.True(value > 0); });

                config
                    .tokens!
                    .Select(t => (key: t.Key, value: t.Value.symbol))
                    .ToList()
                    .ForEach(t => Assert.Equal(t.key.ToLowerInvariant(), t.value.ToLowerInvariant()));

                config
                    .tokens!
                    .Select(t => t.Value.decimals)
                    .ToList()
                    .ForEach(value => Assert.InRange(value, 1, 36));
            }
        }

        [Fact]
        public void CheckEthereum()
        {
            var service = EthereumChainConfigService.CreateConfigFromDefaultFile();
            var config = service.GetChainConfig(EthereumChainConfigService.ETHEREUM_ID);

            Assert.True(config.EIP1559Enabled);
            Assert.NotNull(config.tornado_proxy_contract_tornadocash_eth);
            Assert.NotNull(config.tornado_proxy_contract_projected);
            AssertValidAddress(config.tornado_proxy_contract_tornadocash_eth!);
            AssertValidAddress(config.tornado_proxy_contract_projected!);
            Assert.Equal("ETH", config.currencyName);
            Assert.Equal("eth", config.nativeCurrency);
            Assert.Equal(18, config.nativeCurrencyDecimals);

            Assert.Contains("eth", config.tokens!.Keys);
            Assert.Contains("dai", config.tokens!.Keys);

            Assert.True(config.tokens!["eth"].miningEnabled);

            var amounts = config.tokens["eth"].GetAmounts();
            Assert.Contains(0.1m, amounts);
            Assert.Contains(1m, amounts);
            Assert.Contains(10m, amounts);
            Assert.Contains(100m, amounts);

            amounts = config.tokens["dai"].GetAmounts();
            Assert.Contains(100m, amounts);
            Assert.Contains(1000m, amounts);
            Assert.Contains(10_000m, amounts);
            Assert.Contains(100_000m, amounts);
        }

        [Fact]
        public void CheckGoerli()
        {
            var service = EthereumChainConfigService.CreateConfigFromDefaultFile();
            var config = service.GetChainConfig(EthereumChainConfigService.GOERLI_ID);

            Assert.True(config.EIP1559Enabled);
            Assert.NotNull(config.tornado_proxy_contract_tornadocash_eth);
            Assert.NotNull(config.tornado_proxy_contract_projected);
            AssertValidAddress(config.tornado_proxy_contract_tornadocash_eth!);
            AssertValidAddress(config.tornado_proxy_contract_projected!);
            Assert.Equal("gETH", config.currencyName);
            Assert.Equal("eth", config.nativeCurrency);
            Assert.Equal(18, config.nativeCurrencyDecimals);

            Assert.Contains("eth", config.tokens!.Keys);

            var amounts = config.tokens["eth"].GetAmounts();
            Assert.Contains(0.1m, amounts);
            Assert.Contains(1m, amounts);
            Assert.Contains(10m, amounts);
            Assert.Contains(100m, amounts);

            amounts = config.tokens["dai"].GetAmounts();
            Assert.Contains(100m, amounts);
            Assert.Contains(1000m, amounts);
            Assert.Contains(10_000m, amounts);
            Assert.Contains(100_000m, amounts);
        }

        [Fact]
        public void CheckBinance()
        {
            var service = EthereumChainConfigService.CreateConfigFromDefaultFile();
            var config = service.GetChainConfig(EthereumChainConfigService.BNB_SMART_CHAIN_ID);

            Assert.False(config.EIP1559Enabled);
            Assert.NotNull(config.tornado_proxy_contract_projected);
            AssertValidAddress(config.tornado_proxy_contract_projected!);
            Assert.Equal("BNB", config.currencyName);
            Assert.Equal("bnb", config.nativeCurrency);
            Assert.Equal(18, config.nativeCurrencyDecimals);

            Assert.Contains("bnb", config.tokens!.Keys);

            var amounts = config.tokens["bnb"].GetAmounts();
            Assert.Contains(0.1m, amounts);
            Assert.Contains(1m, amounts);
            Assert.Contains(10m, amounts);
            Assert.Contains(100m, amounts);
        }

        [Fact]
        public void ExplorerTest()
        {
            var service = EthereumChainConfigService.CreateConfigFromDefaultFile();
            Assert.NotNull(service);

            {
                var ethereum = service.GetChainConfig(EthereumChainConfigService.ETHEREUM_ID);
                Assert.Equal(
                    "https://etherscan.io/address/0x00000000219ab540356cbb839cbe05303d7705fa",
                    ethereum.explorerUrl!.GetAddressURL("0x00000000219ab540356cbb839cbe05303d7705fa")
                );
                Assert.Equal(
                    "https://etherscan.io/address/0x00000000219ab540356cbb839cbe05303d7705fa",
                    ethereum.explorerUrl!.GetAddressURL("00000000219ab540356cbb839cbe05303d7705fa")
                );
                Assert.Equal(
                    "https://etherscan.io/token/0xdac17f958d2ee523a2206206994597c13d831ec7",
                    ethereum.explorerUrl!.GetTokenURL("dac17f958d2ee523a2206206994597c13d831ec7")
                );
                Assert.Equal(
                    "https://etherscan.io/token/0xdac17f958d2ee523a2206206994597c13d831ec7",
                    ethereum.explorerUrl!.GetTokenURL("0xdac17f958d2ee523a2206206994597c13d831ec7")
                );

                Assert.Equal(
                    "https://etherscan.io/address/0x00000000219ab540356cbb839cbe05303d7705fa",
                    ethereum.explorerUrl!.GetBalanceURL("0x00000000219ab540356cbb839cbe05303d7705fa")
                );
                Assert.Equal(
                    "https://etherscan.io/address/0x00000000219ab540356cbb839cbe05303d7705fa",
                    ethereum.explorerUrl!.GetBalanceURL("00000000219ab540356cbb839cbe05303d7705fa")
                );
                Assert.Equal(
                    "https://etherscan.io/address/0x00000000219ab540356cbb839cbe05303d7705fa",
                    ethereum.explorerUrl!.GetBalanceURL("0x00000000219ab540356cbb839cbe05303d7705fa", string.Empty)
                );
                Assert.Equal(
                    "https://etherscan.io/address/0x00000000219ab540356cbb839cbe05303d7705fa",
                    ethereum.explorerUrl!.GetBalanceURL("00000000219ab540356cbb839cbe05303d7705fa", string.Empty)
                );

                Assert.Equal(
                    "https://etherscan.io/token/0xdac17f958d2ee523a2206206994597c13d831ec7?a=0x00000000219ab540356cbb839cbe05303d7705fa",
                    ethereum.explorerUrl!.GetBalanceURL("0x00000000219ab540356cbb839cbe05303d7705fa",
                        "0xdac17f958d2ee523a2206206994597c13d831ec7")
                );
                Assert.Equal(
                    "https://etherscan.io/token/0xdac17f958d2ee523a2206206994597c13d831ec7?a=0x00000000219ab540356cbb839cbe05303d7705fa",
                    ethereum.explorerUrl!.GetBalanceURL("00000000219ab540356cbb839cbe05303d7705fa",
                        "0xdac17f958d2ee523a2206206994597c13d831ec7")
                );
                Assert.Equal(
                    "https://etherscan.io/token/0xdac17f958d2ee523a2206206994597c13d831ec7?a=0x00000000219ab540356cbb839cbe05303d7705fa",
                    ethereum.explorerUrl!.GetBalanceURL("0x00000000219ab540356cbb839cbe05303d7705fa",
                        "dac17f958d2ee523a2206206994597c13d831ec7")
                );
                Assert.Equal(
                    "https://etherscan.io/token/0xdac17f958d2ee523a2206206994597c13d831ec7?a=0x00000000219ab540356cbb839cbe05303d7705fa",
                    ethereum.explorerUrl!.GetBalanceURL("00000000219ab540356cbb839cbe05303d7705fa",
                        "dac17f958d2ee523a2206206994597c13d831ec7")
                );

                Assert.Equal(
                    "https://etherscan.io/block/1337",
                    ethereum.explorerUrl!.GetBlockURL(1337)
                );

                Assert.Equal(
                    "https://etherscan.io/tx/0x3ae84d941086860d2be4b97e3e530198c098e966431c7b06253fb7fca62be3a9",
                    ethereum.explorerUrl!.GetTransactionURL(
                        "0x3ae84d941086860d2be4b97e3e530198c098e966431c7b06253fb7fca62be3a9")
                );
                Assert.Equal(
                    "https://etherscan.io/tx/0x3ae84d941086860d2be4b97e3e530198c098e966431c7b06253fb7fca62be3a9",
                    ethereum.explorerUrl!.GetTransactionURL(
                        "3ae84d941086860d2be4b97e3e530198c098e966431c7b06253fb7fca62be3a9")
                );
            }

            {
                var goerli = service.GetChainConfig(EthereumChainConfigService.GOERLI_ID);
                Assert.Equal(
                    "https://goerli.etherscan.io/address/0x00000000219ab540356cbb839cbe05303d7705fa",
                    goerli.explorerUrl!.GetAddressURL("0x00000000219ab540356cbb839cbe05303d7705fa")
                );
                Assert.Equal(
                    "https://goerli.etherscan.io/address/0x00000000219ab540356cbb839cbe05303d7705fa",
                    goerli.explorerUrl!.GetAddressURL("00000000219ab540356cbb839cbe05303d7705fa")
                );

                Assert.Equal(
                    "https://goerli.etherscan.io/tx/0xcd95f75381910ae995de7d8a31a4289a89c0d45ebb498760571a83edc5b6aa84",
                    goerli.explorerUrl!.GetTransactionURL(
                        "0xcd95f75381910ae995de7d8a31a4289a89c0d45ebb498760571a83edc5b6aa84")
                );
                Assert.Equal(
                    "https://goerli.etherscan.io/tx/0xcd95f75381910ae995de7d8a31a4289a89c0d45ebb498760571a83edc5b6aa84",
                    goerli.explorerUrl!.GetTransactionURL(
                        "cd95f75381910ae995de7d8a31a4289a89c0d45ebb498760571a83edc5b6aa84")
                );
            }
        }

        [Fact]
        public void ExplorerDistinctTest()
        {
            var service = EthereumChainConfigService.CreateConfigFromDefaultFile();
            Assert.NotNull(service);

            var chains = service.GetChainIds();
            var explorerUrls = chains
                .Select(chainId => service.GetChainConfig(chainId).explorerUrl!.mainUrlPrefix)
                .ToArray();

            Assert.Equal(
                explorerUrls.Length,
                explorerUrls.Select(t => t.ToLowerInvariant().TrimEnd('/')).Distinct().Count()
            );
        }

        #region Constructor exception tests

        private static string GetWrongFilename()
        {
            var rnd = new Random();

            // ReSharper disable once UseStringInterpolation
            return string.Format("{0}/nyanpasu-{1}.json", AppContext.BaseDirectory, rnd.Next(0, 1_000_000));
        }

        [Fact]
        public void CreateConfigFromFileExceptionTest()
        {
            try
            {
                var filename = GetWrongFilename();
                EthereumChainConfigService.CreateConfigFromFile(filename);
            }
            catch (EthereumChainConfigException e)
            {
                Assert.InRange(e.ErrorCode, 1, 999);
            }
        }

        [Fact]
        public void CreateConfigFromFileExceptionAsyncTest()
        {
            try
            {
                var filename = GetWrongFilename();
                EthereumChainConfigService.CreateConfigFromFileAsync(filename).GetAwaiter().GetResult();
            }
            catch (EthereumChainConfigException e)
            {
                Assert.InRange(e.ErrorCode, 1, 999);
            }
        }

        #endregion

        [Fact]
        public void NotValidChain()
        {
            var service = EthereumChainConfigService.CreateConfigFromDefaultFile();
            Assert.NotNull(service);
            var chains = service.GetChainIds();
            var rnd = new Random();

            int chainId;
            do
            {
                chainId = rnd.Next(1, 65_535);
            } while (chains.Contains(chainId));

            try
            {
                service.GetChainConfig(chainId);
            }
            catch (EthereumChainConfigException e)
            {
                Assert.InRange(e.ErrorCode, 1, 999);
            }
        }

        public static IEnumerable<object[]> GetAvailableChains()
        {
            var service = EthereumChainConfigService.CreateConfigFromDefaultFile();
            return service
                .GetChainIds()
                .Select(chainId => new object[] { chainId })
                .ToArray();
        }

        [Theory]
        [MemberData(nameof(GetAvailableChains))]
        public async Task CheckTokens(int chainId)
        {
            var service = await EthereumChainConfigService.CreateConfigFromDefaultFileAsync();
            Assert.NotNull(service);

            var abiFilename = Path.Combine(AppContext.BaseDirectory, "erc20-token-abi.json");
            var abiText = await File.ReadAllTextAsync(abiFilename);


            var config = service.GetChainConfig(chainId);
            Assert.NotEmpty(config.nativeCurrency);

            if (config.tokens == null)
            {
                return;
            }

            var nativeCurrency = config.nativeCurrency.ToLowerInvariant();
            var web3 = new Nethereum.Web3.Web3(config.rpcUrls!.Values.First().url);
            Assert.Contains(nativeCurrency, config.tokens.Keys);
            foreach (var (tokenName, token) in config.tokens)
            {
                if (tokenName == nativeCurrency)
                {
                    Assert.Null(token.tokenAddress);
                    continue;
                }

                Assert.NotNull(token.tokenAddress);
                AssertValidAddress(token.tokenAddress!);
                var contract = web3.Eth.GetContract(abiText, token.tokenAddress);
                var function = contract.GetFunction("decimals");
                var realDecimal = await function.CallAsync<int>();
                Assert.Equal(realDecimal, token.decimals);

                function = contract.GetFunction("symbol");
                var realSymbol = await function.CallAsync<string>();
                Assert.Equal(realSymbol, token.symbol);

                // ReSharper disable once InvertIf
                if (token.mixerAddress != null)
                {
                    foreach (var (amount, value) in token.mixerAddress!)
                    {
                        Assert.True(decimal.TryParse(amount, out _));
                        AssertValidAddressNullEmpty(value);
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetAvailableChains))]
        public void CheckRPC(int chainId)
        {
            var service = EthereumChainConfigService.CreateConfigFromDefaultFile();
            Assert.NotNull(service);

            var config = service.GetChainConfig(chainId);
            Assert.NotEmpty(config.nativeCurrency);
            Assert.NotNull(config.rpcUrls);

            foreach (var rpcUrl in config.rpcUrls!.Values)
            {
                Assert.NotEmpty(rpcUrl.name);
                Assert.NotEmpty(rpcUrl.url);

                var url = new Uri(rpcUrl.url);
                Assert.Contains(url.Scheme, new[] { "http", "https" });
            }
        }

        [Theory]
        [MemberData(nameof(GetAvailableChains))]
        public void CheckRelayer(int chainId)
        {
            var service = EthereumChainConfigService.CreateConfigFromDefaultFile();
            Assert.NotNull(service);

            var config = service.GetChainConfig(chainId);
            Assert.NotEmpty(config.nativeCurrency);
            Assert.NotNull(config.relayers);

            foreach (var relayer in config.relayers!.Values)
            {
                Assert.NotEmpty(relayer.name);
                Assert.NotEmpty(relayer.url);
                Assert.NotEmpty(relayer.cachedUrl);

                var url = new Uri(relayer.cachedUrl);
                Assert.Contains(url.Scheme, new[] { "http", "https" });
            }
        }
    }
}