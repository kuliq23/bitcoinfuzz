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
    [UnmanagedCallersOnly(EntryPoint = "nbitcoin_bip32_deserialize_extended_key")]
    public static IntPtr BIP32DeserializeExtendedKeyTarget(IntPtr inputPtr)
    {   
        string? input = null;
        try
        {
            input = Marshal.PtrToStringUTF8(inputPtr);
            if (string.IsNullOrEmpty(input))
            {
                return Marshal.StringToCoTaskMemUTF8("could not convert to string");
            }
        }
        catch
        {
            return Marshal.StringToCoTaskMemUTF8("could not convert to string");
        }
        NBitcoin.ExtKey ext;
        try
        {
            Console.WriteLine("Input   CS: " + input);
            ext = NBitcoin.ExtKey.Parse(input, Network.Main);
        }
        catch
        {
            return Marshal.StringToCoTaskMemUTF8("UNABLE TO PARSE");
        }
        try 
        {
            return Marshal.StringToCoTaskMemUTF8(ext.ToString(Network.Main));
        }
        catch
        {
            return Marshal.StringToCoTaskMemUTF8("could not convert to string");
        }


    }
    [UnmanagedCallersOnly(EntryPoint = "nbitcoin_free_c_string")]
    public static void FreeString(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
    }
}