using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NokitaKaze.EthereumChainConfig.Models
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [DebuggerDisplay("ExplorerUrl {mainUrlPrefix}")]
    public class ExplorerUrl
    {
        /// <summary>
        /// Transaction prefix URL
        /// </summary>
        [JsonInclude]
        public string tx { get; private set; }

        /// <summary>
        /// Address prefix URL
        /// </summary>
        [JsonInclude]
        public string address { get; private set; }

        /// <summary>
        /// Block prefix URL
        /// </summary>
        [JsonInclude]
        public string block { get; private set; }

        /// <summary>
        /// Main prefix URL
        /// </summary>
        [JsonIgnore]
        public string mainUrlPrefix
        {
            get
            {
                const string defaultAddressPostfix = "/address/";
                if (!address.EndsWith(defaultAddressPostfix))
                {
                    throw new Exception("Unknown protocol");
                }

                return address[..^defaultAddressPostfix.Length];
            }
        }

        /// <summary>
        /// Token prefix URL
        /// </summary>
        [JsonIgnore]
        public string token => $"{mainUrlPrefix}/token/";

        public string GetTxURL(string txId)
        {
            return tx + txId;
        }

        public string GetAddressURL(string address)
        {
            return this.address + address;
        }

        public string GetBlockURL(int blockId)
        {
            return block + blockId;
        }

        public string GetBalanceURL(string address, string? token = null)
        {
            if (token == string.Empty)
            {
                token = null;
            }

            return (token != null)
                ? $"{token}{token.ToLowerInvariant()}?a={address.ToLowerInvariant()}"
                : $"{address}{address.ToLowerInvariant()}";
        }
    }
}