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
        public string tx { get; private set; } = string.Empty;

        /// <summary>
        /// Address prefix URL
        /// </summary>
        [JsonInclude]
        public string address { get; private set; } = string.Empty;

        /// <summary>
        /// Block prefix URL
        /// </summary>
        [JsonInclude]
        public string block { get; private set; } = string.Empty;

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
                    throw new Exception("Unknown url template");
                }

                return address[..^defaultAddressPostfix.Length];
            }
        }

        /// <summary>
        /// Token prefix URL
        /// </summary>
        [JsonIgnore]
        public string token => $"{mainUrlPrefix}/token/";

        public static string EnsurePrefix(string rawString)
        {
            return rawString.StartsWith("0x") ? rawString : "0x" + rawString;
        }

        public string GetTransactionURL(string viewTxId)
        {
            return tx + EnsurePrefix(viewTxId);
        }

        public string GetAddressURL(string viewAddress)
        {
            return address + EnsurePrefix(viewAddress);
        }

        public string GetTokenURL(string viewToken)
        {
            return token + EnsurePrefix(viewToken);
        }

        public string GetBlockURL(int viewBlockId)
        {
            return block + viewBlockId;
        }

        public string GetBalanceURL(string viewAddress, string? viewToken = null)
        {
            if (viewToken == string.Empty)
            {
                viewToken = null;
            }

            return (viewToken != null)
                ? $"{token}{EnsurePrefix(viewToken).ToLowerInvariant()}?a={EnsurePrefix(viewAddress).ToLowerInvariant()}"
                : $"{address}{EnsurePrefix(viewAddress).ToLowerInvariant()}";
        }
    }
}