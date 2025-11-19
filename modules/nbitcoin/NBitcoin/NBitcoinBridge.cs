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

        if (TryParseXprv(input, out string xprvResult)){
            return Marshal.StringToCoTaskMemUTF8(xprvResult);
        }

        if (TryParseXpub(input, out string xpubResult)){
            return Marshal.StringToCoTaskMemUTF8(xpubResult);
        }

        return Marshal.StringToCoTaskMemUTF8("INVALID");

    }
    // Helper methods for BIP32 deserialization
    private static bool TryParseXprv(string input, out string result)
    {
        result = null;
        try
        {
            var ext = NBitcoin.ExtKey.Parse(input, Network.Main);

            string depthHex = ext.Depth.ToString("X2").ToLower();
            byte[] fingerprint = ext.ParentFingerprint.ToBytes();
            string fingerprintHex = BitConverter.ToString(fingerprint).Replace("-", "").ToLower();
            string childHex = ext.Child.ToString("X8").ToLower();
            string chainHex = BitConverter.ToString(ext.ChainCode).Replace("-", "").ToLower();
            string keyHex = BitConverter.ToString(ext.PrivateKey.ToBytes()).Replace("-", "").ToLower();

            result = $"depth={depthHex};fp={fingerprintHex};child={childHex};chaincode={chainHex};key={keyHex}";
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseXpub(string input, out string result)
    {
        result = null;
        try
        {
            var exp = NBitcoin.ExtPubKey.Parse(input, Network.Main);

            string depthHex = exp.Depth.ToString("X2").ToLower();
            byte[] fingerprint = exp.ParentFingerprint.ToBytes();
            string fingerprintHex = BitConverter.ToString(fingerprint).Replace("-", "").ToLower();
            string childHex = exp.Child.ToString("X8").ToLower();
            string chainHex = BitConverter.ToString(exp.ChainCode).Replace("-", "").ToLower();
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