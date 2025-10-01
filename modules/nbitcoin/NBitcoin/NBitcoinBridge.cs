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
    [UnmanagedCallersOnly(EntryPoint = "nbitcoin_bip32_master_keygen")]
    public static IntPtr BIP32MasterKeygen(IntPtr dataPtr, UIntPtr len)
    {
        var seed = new byte[(int)len];
        Marshal.Copy(dataPtr, seed, 0, (int)len);
        ExtKey sk = ExtKey.CreateFromSeed(seed);
        Console.WriteLine("Master key NBIT: " + sk.GetWif(Network.Main).ToString());
        IntPtr strPtr = Marshal.StringToHGlobalAnsi(sk.GetWif(Network.Main).ToString());
        return strPtr;
        
    }
    [UnmanagedCallersOnly(EntryPoint = "nbitcoin_bip32_parse_random_path")]
    public static IntPtr BIP32ParseRandomPath(IntPtr dataPtr, UIntPtr len)
    {
        var input = new byte[(int)len];
        Marshal.Copy(dataPtr, input, 0, (int)len);
        //input to utf8 string
        string path = System.Text.Encoding.UTF8.GetString(input);
        //string path = "m/44'/0'/0'/0/0"; // goes through
        try
        {
            var parsed_path = new KeyPath(path);
            IntPtr strPtr = Marshal.StringToHGlobalAnsi(parsed_path.ToString());
            //Console.WriteLine("Parsed path NBIT: " + parsed_path.ToString());
            return strPtr;
        }
        catch
        {
            return IntPtr.Zero;
        }
    }
    [UnmanagedCallersOnly(EntryPoint = "nbitcoin_free_c_string")]
    public static void FreeString(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
    }
}