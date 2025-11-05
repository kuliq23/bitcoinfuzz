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
            if (string.IsNullOrEmpty(input))
            {
                return Marshal.StringToCoTaskMemUTF8("could not convert to string");
            }
        }
        catch
        {
            return Marshal.StringToCoTaskMemUTF8("could not convert to string");
        }
        Console.WriteLine("Innbi string: " + input);
        NBitcoin.ExtKey ext;
        try
        {
            ext = NBitcoin.ExtKey.Parse(input, Network.Main);
            Console.WriteLine("Parsed as ExtKey");
            Console.WriteLine("Fingerprint: " + ext.GetPublicKey().GetHDFingerPrint().ToString());
            Console.WriteLine("Depth: " + ext.Depth);
            Console.WriteLine("Chain code: " + ext.ChainCode);
            Console.WriteLine("Private key: " + ext.PrivateKey.ToString(Network.Main));
            Console.WriteLine("Public key: " + ext.GetPublicKey().ToString());
            Console.WriteLine("Serialized: " + ext.ToString(Network.Main));
        }
        catch
        {
            Console.WriteLine("UNABLE TO PARSE");
            return Marshal.StringToCoTaskMemUTF8("UNABLE TO PARSE");
        }
        try 
        {
            Console.WriteLine("Parsed key: " + ext.ToString(Network.Main));
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