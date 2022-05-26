using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Xunit;

namespace NokitaKaze.EthereumChainConfig.Test
{
    public class AmountConverterTest
    {
        public static IEnumerable<object[]> GetPowFromWeiTestData()
        {
            return new List<object[]>
            {
                new object[] { 18, BigInteger.Zero, 0 },
                new object[] { 18, new BigInteger(1_000_000_000m * 1_000_000_000m), 1 },
                new object[] { 18, new BigInteger(1_000_000_000m * 1_000_000_000m + 1m), 1.000_000_000_000_000_001m },

                new object[] { 6, BigInteger.Zero, 0 },
                new object[] { 6, new BigInteger(1_000_000m), 1 },

                new object[] { 8, BigInteger.Zero, 0 },
                new object[] { 8, new BigInteger(100_000_000m), 1 },
                new object[] { 8, new BigInteger(100_000_000m + 1m), 1.000_000_01m },
            };
        }

        [Theory]
        [MemberData(nameof(GetPowFromWeiTestData))]
        public void GetPowFromWeiTest(int decimals, BigInteger wei, decimal expectedValue)
        {
            var actual = AmountConverter.GetPoweredFromWei(decimals, wei);
            Assert.Equal(expectedValue, actual);

            actual = AmountConverter.GetPoweredFromWei(decimals, (decimal)wei);
            Assert.Equal(expectedValue, actual);
        }

        public static IEnumerable<object[]> GetPowSelfTestData()
        {
            return new[] { new object[] { 6 }, new object[] { 8 }, new object[] { 18 } };
        }

        [Theory]
        [MemberData(nameof(GetPowSelfTestData))]
        public void GetPowSelfTest(int decimals)
        {
            var rnd = new Random();
            var precisionPower = Enumerable
                .Repeat(0, decimals)
                .Aggregate(1L, (a, _) => a * 10);
            var precisionPowerR = Enumerable
                .Repeat(0, decimals)
                .Aggregate(1m, (a, _) => a * 0.1m);
            Assert.Equal(1m, precisionPower * precisionPowerR);

            var d2Decimals = new[] { 0m, 1m, 1_000_000_000m };

            for (var i = 0; i < 100; i++)
            {
                var v1Long = (long)(rnd.NextDouble() * precisionPower);
                var d1Dec = (decimal)v1Long;
                foreach (var d2Dec in d2Decimals)
                {
                    var fullValue = d1Dec * precisionPowerR + d2Dec;
                    var big = AmountConverter.GetBigIntegerWeiFromPowered(decimals, fullValue);
                    Assert.True(big.Sign >= 0);

                    var divMod = big % precisionPower;
                    Assert.Equal(v1Long, divMod);

                    var revert = AmountConverter.GetPoweredFromWei(decimals, big);
                    Assert.Equal(fullValue, revert);
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetPowSelfTestData))]
        public void GetReversePowSelfTest(int decimals)
        {
            var rnd = new Random();
            var lg = Math.Log(10, 256);

            foreach (var mod in new[] { 0.5d, 2d / 3, 1d, 1.5, 2d })
            {
                for (var i = 0; i < 100; i++)
                {
                    var byteCount = (int)Math.Ceiling(lg * decimals * mod);
                    if (byteCount > 13)
                    {
                        continue;
                    }

                    var bytes = new byte[byteCount];
                    rnd.NextBytes(bytes);

                    var big = new BigInteger(bytes, isUnsigned: true, isBigEndian: false);
                    if (big == BigInteger.Zero)
                    {
                        continue;
                    }

                    Assert.True(big.Sign >= 0);

                    var dec = AmountConverter.GetPoweredFromWei(decimals, big);
                    Assert.True(dec >= 0);

                    var bigRevert = AmountConverter.GetBigIntegerWeiFromPowered(decimals, dec);
                    Assert.Equal(big, bigRevert);
                }
            }
        }


        [Fact]
        public void HexBigIntegerTest()
        {
            var rnd = new Random();

            for (var byteCount = 2; byteCount <= 32; byteCount++)
            {
                for (var i = 0; i < 1000; i++)
                {
                    var bytes = new byte[byteCount];
                    rnd.NextBytes(bytes);

                    var big = new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
                    var hex = new HexBigInteger(big);

                    var bigRevert = (BigInteger)hex;
                    Assert.Equal(big, bigRevert);
                }
            }
        }
    }
}