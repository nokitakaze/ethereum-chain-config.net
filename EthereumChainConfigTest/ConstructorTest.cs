﻿using System.Linq;
using System.Reflection;
using NokitaKaze.EthereumChainConfig;
using NokitaKaze.EthereumChainConfig.Models;
using Xunit;

namespace EthereumChainConfigTest
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

        [Fact]
        public void MainTest()
        {
            var service = EthereumChainConfigService.CreateConfigFromDefaultFile();
            Assert.NotNull(service);

            foreach (var chainId in service.GetChainIds())
            {
                var config = service.GetChainConfig(chainId);
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
            Assert.Equal("ETH", config.currencyName);
            Assert.Equal("eth", config.nativeCurrency);
            Assert.Equal(18, config.nativeCurrencyDecimals);

            Assert.Contains("eth", config.tokens!.Keys);
            Assert.Contains("dai", config.tokens!.Keys);

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
    }
}