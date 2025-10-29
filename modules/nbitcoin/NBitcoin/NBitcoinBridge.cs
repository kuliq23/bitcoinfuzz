using System.Runtime.InteropServices;
using NBitcoin.Scripting;
using NBitcoin.WalletPolicies;


namespace NBitcoin.CppBridge;

public static class Bridge
{
    [UnmanagedCallersOnly(EntryPoint = "nbitcoin_miniscript_parse")]
    public static bool MiniscriptParse(IntPtr miniscriptStringPtr)
    {
        if (miniscriptStringPtr == IntPtr.Zero)
            return false;

        string miniscriptString = Marshal.PtrToStringUTF8(miniscriptStringPtr) ?? "";
        if (string.IsNullOrEmpty(miniscriptString))
            return false;

        return TryParseMiniscript(miniscriptString, KeyType.Classic)
            || TryParseMiniscript(miniscriptString, KeyType.Taproot);
    }

    [UnmanagedCallersOnly(EntryPoint = "nbitcoin_descriptor_parse")]
    public static bool DescriptorParse(IntPtr descriptorStringPtr)
    {
        if (descriptorStringPtr == IntPtr.Zero)
            return false;

        string descriptorString = Marshal.PtrToStringUTF8(descriptorStringPtr) ?? "";
        if (string.IsNullOrEmpty(descriptorString))
            return false;

        try
        {
            _ = OutputDescriptor.Parse(descriptorString, Network.Main);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseMiniscript(string miniscript, KeyType keyType)
    {
        try
        {
            _ = Miniscript.Parse(miniscript, new MiniscriptParsingSettings(Network.Main)
            {
                Dialect = MiniscriptDialect.Strict,
                KeyType = keyType,
                AllowedParameters = ParameterTypeFlags.None
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
    [UnmanagedCallersOnly(EntryPoint = "nbitcoin_bip32_deserialize_key_xpub_xprv")]
    public static IntPtr Bip32DeserializeKeyXpubXprv(IntPtr inputPtr)
    {   
        string? input = null;
        try
        {
            input = Marshal.PtrToStringUTF8(inputPtr);
        }
        catch
        {
            return Marshal.StringToCoTaskMemUTF8("could not convert to string");
        }

        var xpub = NBitcoin.ExtPubKey.Parse(input, Network.Main);
        string result = xpub.ToString();
        return Marshal.StringToCoTaskMemUTF8(result);


    }
    [UnmanagedCallersOnly(EntryPoint = "nbitcoin_free_c_string")]
    public static void FreeString(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
    }
}