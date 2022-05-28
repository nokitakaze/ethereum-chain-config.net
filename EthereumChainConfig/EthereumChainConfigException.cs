using System;

namespace NokitaKaze.EthereumChainConfig
{
    public class EthereumChainConfigException : Exception
    {
        public int ErrorCode { get; }

        internal EthereumChainConfigException(string message, int errorCode = 0) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}