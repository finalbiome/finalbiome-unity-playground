using FinalBiome.Api.Tx;
using FinalBiome.Api.Types;
using FinalBiome.Api.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using FinalBiome.Api.Types.Primitive;
using FinalBiome.Api.Types.SpCore.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// <example>
/// How to create new dev accounts
/// <code>subkey inspect \/\/Ferdie</code>
/// </example>
/// </summary>
public static class AccountKeyring
{
    public static Account Ferdie()
    {
        return Account.FromSeed(FinalBiome.Api.Types.SpRuntime.InnerMultiSignature.Sr25519,
                                HexUtils.HexToBytes("0x42438b7883391c05512a938e36c2df0131e088b3756d6aa7a755fbff19d2f842"));
    }
    public static Account Alice()
    {
        return Account.FromSeed(FinalBiome.Api.Types.SpRuntime.InnerMultiSignature.Sr25519,
                                HexUtils.HexToBytes("0xe5be9a5092b81bca64be81d212e7f2f9eba183bb7a90954f7b76361f6edb5c0a"));
    }
    public static Account Bob()
    {
        return Account.FromSeed(FinalBiome.Api.Types.SpRuntime.InnerMultiSignature.Sr25519,
                                HexUtils.HexToBytes("0x398f0c28f98885e046333d4a41c19cee4c37368a9832c6502f6cfd182e2aef89"));
    }
    public static Account Charlie()
    {
        return Account.FromSeed(FinalBiome.Api.Types.SpRuntime.InnerMultiSignature.Sr25519,
                                HexUtils.HexToBytes("0xbc1ede780f784bb6991a585e4f6e61522c14e1cae6ad0895fb57b9a205a8f938"));
    }
    public static Account Dave()
    {
        return Account.FromSeed(FinalBiome.Api.Types.SpRuntime.InnerMultiSignature.Sr25519,
                                HexUtils.HexToBytes("0x868020ae0687dda7d57565093a69090211449845a7e11453612800b663307246"));
    }
    public static Account Eve()
    {
        return Account.FromSeed(FinalBiome.Api.Types.SpRuntime.InnerMultiSignature.Sr25519,
                                HexUtils.HexToBytes("0x786ad0e2df456fe43dd1f91ebca22e235bc162e0bb8d53c633e8c85b2af68b7a"));
    }
    public static Account Oscar()
    {
        return Account.FromSeed(FinalBiome.Api.Types.SpRuntime.InnerMultiSignature.Sr25519,
                                HexUtils.HexToBytes("0xc3b679c3ddfe58e98373b762ef53b0e350bce45ae6a5352453c42591af354d41"));
    }
    public static Account Mike()
    {
        return Account.FromSeed(FinalBiome.Api.Types.SpRuntime.InnerMultiSignature.Sr25519,
                                HexUtils.HexToBytes("0x38b1bd20895d5187c4944a101787bd2552fb7091e72ec0d6c8015b2a7f42d29a"));
    }
}

public static class StringifyExtension
{
    /// <summary>
    /// Serialize Codec value to readable format.
    /// </summary>
    /// <param name="that"></param>
    /// <param name="formatting"></param>
    /// <returns></returns>
    public static string ToHuman(this Codec that, Formatting formatting = Formatting.Indented)
    {
        if (that is null) return "null";
        var sOpt = new JsonSerializerSettings
        {
            //NullValueHandling = NullValueHandling.Ignore,
            Converters = {
                    new ApiTypesJsonConverter(),
                new StringEnumConverter(),
                }
        };

        return JsonConvert.SerializeObject(that, formatting, sOpt);
    }
}

public class ApiTypesJsonConverter : JsonConverter
{
  readonly Type[] _types =
        {
            typeof(Bool),
            typeof(U8),
            typeof(U16),
            typeof(U32),
            typeof(U64),
            typeof(U128),
            typeof(Str),
            typeof(FinalBiome.Api.Types.Primitive.Char),
            typeof(AccountId32),
            typeof(BoundedVecU8),
        };

    public override bool CanConvert(Type objectType)
    {
        return (_types.AsEnumerable().Contains(objectType) ||
            _types.AsEnumerable().Contains(objectType.BaseType) ||
            (objectType.GetInterface(nameof(FinalBiome.Api.Types.Codec)) != null && objectType.GetProperty("Value") != null));
    }

    public override bool CanRead
    {
        get { return false; }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        JToken token = JToken.FromObject(value);
        // write primitives
        Type vv = value.GetType();
        if (_types.AsEnumerable().Contains(vv))
        {
            switch (value)
            {
                case Bool v:
                    writer.WriteValue(v.Value);
                    break;
                case U8 v:
                    writer.WriteValue(v.Value);
                    break;
                case U16 v:
                    writer.WriteValue(v.Value);
                    break;
                case U32 v:
                    writer.WriteValue(v.Value);
                    break;
                case U64 v:
                    writer.WriteValue(v.Value);
                    break;
                case U128 v:
                    writer.WriteValue(v.Value);
                    break;
                case Str v:
                    writer.WriteValue(v.Value);
                    break;
                case FinalBiome.Api.Types.Primitive.Char v:
                    writer.WriteValue(v.Value);
                    break;
                case AccountId32 v:
                    List<byte> b = new();
                    foreach (var i in v.Value) b.Add(i.Value);
                    string e = AddressUtils.GetAddressFrom(b.ToArray());
                    writer.WriteValue(e);
                    break;
                //case VecU8:
                case BoundedVecU8 v:
                    List<byte> b1 = new();
                    foreach (var i in v.Value) b1.Add(i.Value);

                    string a = System.Text.Encoding.UTF8.GetString(b1.ToArray());
                    writer.WriteValue(a);
                    break;
                default:
                    //t.WriteTo(writer);
                    throw new NotImplementedException();
                    //break;
            }
            return;
        }

        // deref wrappers
        int childTokensNum = token.Count();
        var valueProp = value.GetType().GetProperty("Value");
        if (childTokensNum == 1 && valueProp != null)
        {
            // deref value from Value prop
            var val = valueProp.GetValue(value);
            serializer.Serialize(writer, val);
        }
        else // write object as is
        {
            if (token.Type != JTokenType.Object)
            {
                token.WriteTo(writer);
                return;
            }

            // it's composite object. writes it as an object
            writer.WriteStartObject();
            foreach (JToken childToken in token.AsEnumerable())
            {
                string propName = childToken.Path;
                var propVal = value.GetType().GetProperty(propName);
                if (propVal is not null)
                {
                    var val = propVal.GetValue(value);
                    writer.WritePropertyName(propName);
                    serializer.Serialize(writer, val);
                }
            }
            writer.WriteEndObject();
        }
    }
}
