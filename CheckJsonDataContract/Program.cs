using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NokitaKaze.EthereumChainConfig.Models;
using System.Text.Json.Serialization;

namespace NokitaKaze.EthereumChainConfig.CheckJsonDataContract
{
    internal static class Program
    {
        private static async Task Main()
        {
            var jsonText = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,
                EthereumChainConfigService.DefaultTornadoConfigFilename));
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(jsonText)!;
            foreach (var (chainKey, rootObject) in dictionary)
            {
                Console.WriteLine("Check root object {0}", chainKey);
                CompareRealItemWithDataContract(rootObject, typeof(EthereumSingleChainConfig), string.Empty);
                Console.WriteLine();
            }
        }

        // ReSharper disable once RedundantNameQualifier
        private static void CompareRealItemWithDataContract(JToken realItem, System.Type dataContract, string path)
        {
            switch (realItem.Type)
            {
                case JTokenType.Integer:
                    // todo check
                    break;
                case JTokenType.Float:
                    // todo check
                    break;
                case JTokenType.String:
                    // todo check
                    break;
                case JTokenType.Boolean:
                    // todo check
                    break;
                case JTokenType.Array:
                    throw new NotImplementedException();
                case JTokenType.Object:
                    CompareRealItemWithDataContract_Object(
                        (JObject)realItem,
                        dataContract,
                        path
                    );
                    break;
                default:
                    return;
            }
        }

        // ReSharper disable once RedundantNameQualifier
        // ReSharper disable once SuggestBaseTypeForParameter
        private static void CompareRealItemWithDataContract_Object(
            JObject realItem,
            System.Type dataContract,
            string path)
        {
            var nameDictionary = dataContract
                .GetProperties()
                .Select(property =>
                {
                    var attrs = property
                        .GetCustomAttributes(typeof(JsonPropertyNameAttribute), false)
                        .Cast<JsonPropertyNameAttribute>()
                        .ToArray();
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (!attrs.Any())
                    {
                        return (property.Name, property.Name);
                    }

                    return (attrs.First().Name, property.Name);
                })
                .ToDictionary(t => t.Item1, t => t.Item2);

            foreach (var child in realItem.Children().Cast<JProperty>())
            {
                var name = child.Name;
                if (!nameDictionary.ContainsKey(name))
                {
                    Console.WriteLine("Item of type [{1}] {0}/ does not contain key {2}",
                        path, dataContract.Name, name);
                    continue;
                }

                var dataContractName = nameDictionary[name];
                var contractType = dataContract.GetProperty(dataContractName)!.PropertyType;

                if (IsTypeADictionary(contractType))
                {
                    if (child.Value.Type != JTokenType.Object)
                    {
                        Console.WriteLine("Item {0}/{1} in real item is not an object", path, name);
                        return;
                    }

                    var dataContractTypeValue = contractType.GenericTypeArguments[1];
                    var dictionaryValue = (JObject)child.Value;
                    foreach (var subChild in dictionaryValue.Children().Cast<JProperty>())
                    {
                        CompareRealItemWithDataContract(
                            subChild.Value,
                            dataContractTypeValue,
                            path + "/" + dataContractName + "{}/" + subChild.Name
                        );
                    }

                    continue;
                }

                CompareRealItemWithDataContract(
                    child.Value,
                    contractType,
                    path + "/" + dataContractName
                );
            }
        }

        private static bool IsTypeADictionary(Type type)
        {
            if (type.AssemblyQualifiedName!.StartsWith("System.Collections.Generic.IReadOnlyDictionary`") ||
                type.AssemblyQualifiedName!.StartsWith("System.Collections.Generic.IDictionary`"))
            {
                return true;
            }

            return (type.BaseType != null) && IsTypeADictionary(type.BaseType);
        }
    }
}