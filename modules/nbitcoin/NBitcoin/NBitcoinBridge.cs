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
                return Marshal.StringToCoTaskMemUTF8("INVALID");
            }
        }
        catch
        {
            return Marshal.StringToCoTaskMemUTF8("INVALID");
        }
        //get environment variable EXTKEYTYPE
        string extKeyType = Environment.GetEnvironmentVariable("EXTKEYTYPE");
        if (extKeyType != "xpub" && extKeyType != "xprv" &&
            extKeyType != "tpub" && extKeyType != "tprv")
        {
            extKeyType = "xpub";
        }
        //try parse based on extKeyType
        if (extKeyType == "xprv")
        {
            if (TryParseXprv(input, Network.Main, out string xprvResult)){
                Console.WriteLine("Parsed as xprv");
                Console.WriteLine(xprvResult);
                return Marshal.StringToCoTaskMemUTF8(xprvResult);
            }
            else{
                return Marshal.StringToCoTaskMemUTF8("INVALID");;
            }
        }
        else if (extKeyType == "tprv")
        {
            if (TryParseXprv(input, Network.TestNet, out string tprvResult)){
                Console.WriteLine("Parsed as tprv");
                Console.WriteLine(tprvResult);
                return Marshal.StringToCoTaskMemUTF8(tprvResult);
            }
            else{
                return Marshal.StringToCoTaskMemUTF8("INVALID");;
            }
        }
        else if (extKeyType == "xpub")
        {
            if (TryParseXpub(input, Network.Main, out string xpubResult)){
                Console.WriteLine("Parsed as xpub");
                Console.WriteLine(xpubResult);
                return Marshal.StringToCoTaskMemUTF8(xpubResult);
            }
            else{
                return Marshal.StringToCoTaskMemUTF8("INVALID");;
            }
        }
        else //tpub
        {
            if (TryParseXpub(input, Network.TestNet, out string tpubResult)){
                Console.WriteLine("Parsed as tpub");
                Console.WriteLine(tpubResult);
                return Marshal.StringToCoTaskMemUTF8(tpubResult);
            }
            else{
                return Marshal.StringToCoTaskMemUTF8("INVALID");;
            }
        }
    }
    // Helper methods for BIP32 deserialization...

    private static string Hex(byte[] data) =>
        Convert.ToHexString(data).ToLower();

    // REFACTOR to cleaner code?
    private static bool TryParseXprv(string input,Network network, out string result)
    {
        result = null;
        try
        {
            var ext = NBitcoin.ExtKey.Parse(input, network);

            string depthHex = ext.Depth.ToString("x2");
            string fingerprintHex = Hex(ext.ParentFingerprint.ToBytes());
            string childHex = ext.Child.ToString("x8");
            string chainHex = Hex(ext.ChainCode);
            string keyHex = ext.PrivateKey.ToHex().ToLower();

            result = $"depth={depthHex};fp={fingerprintHex};child={childHex};chaincode={chainHex};key={keyHex}";
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseXpub(string input,Network network,out string result)
    {
        result = null;
        try
        {
            var exp = NBitcoin.ExtPubKey.Parse(input, network);

            string depthHex = exp.Depth.ToString("x2");
            string fingerprintHex = Hex(exp.ParentFingerprint.ToBytes());
            string childHex = exp.Child.ToString("x8");
            string chainHex = Hex(exp.ChainCode);
            string pubKeyHex = exp.PubKey.ToHex().ToLower();

            result = $"depth={depthHex};fp={fingerprintHex};child={childHex};chaincode={chainHex};pub={pubKeyHex}";
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    [UnmanagedCallersOnly(EntryPoint = "nbitcoin_free_c_string")]
    public static void FreeString(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
    }
}