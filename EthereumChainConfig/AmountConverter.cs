using System;
using System.Linq;
using System.Numerics;

namespace NokitaKaze.EthereumChainConfig
{
    /// Hint: decimals can't contain numbers like 1_000_000_000_000_000_000.000_000_000_000_000_001
    public static class AmountConverter
    {
        public static decimal GetPoweredFromWei(int decimals, BigInteger wei)
        {
            if (wei == BigInteger.Zero)
            {
                return 0m;
            }

            if (wei.Sign == -1)
            {
                throw new Exception("Value lesser than 0");
            }

            var precisionPower = Enumerable
                .Repeat(0, decimals)
                .Aggregate(1L, (a, _) => a * 10);
            var precisionPowerR = 1m / precisionPower;

            var aTail = wei % precisionPower;
            var aEntier = (wei - aTail) / precisionPower;

            var balanceReal = (decimal)aEntier + (decimal)aTail * precisionPowerR;

            return balanceReal;
        }

        public static decimal GetPoweredFromWei(int decimals, decimal wei)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (wei == 0)
            {
                return 0m;
            }

            if (wei < 0)
            {
                throw new Exception("Value lesser than 0");
            }

            var precisionPower = Enumerable
                .Repeat(0, decimals)
                .Aggregate(1L, (a, _) => a * 10);
            var precisionPowerR = 1m / precisionPower;

            var aTail = wei % precisionPower;
            var aEntier = (wei - aTail) / precisionPower;

            var balanceReal = aEntier + aTail * precisionPowerR;

            return balanceReal;
        }

        public static BigInteger GetBigIntegerWeiFromPowered(int decimals, decimal poweredValue)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (poweredValue == 0m)
            {
                return BigInteger.Zero;
            }

            if (poweredValue < 0)
            {
                throw new Exception("Value lesser than 0");
            }

            var precisionPower = Enumerable
                .Repeat(0, decimals)
                .Aggregate(1L, (a, _) => a * 10);

            var powEntier = (long)poweredValue;
            var powTail = poweredValue - powEntier;

            var bEntier = new BigInteger(powEntier);
            var bEntierMultiplied = bEntier * precisionPower;
            var bTail = new BigInteger(powTail * precisionPower);

            var bFullValue = bEntierMultiplied + bTail;

            return bFullValue;
        }
    }
}