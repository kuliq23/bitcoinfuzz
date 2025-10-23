#include <span>
#include "module.h"
#include "NBitcoin/nbitcoin_lib.h"

namespace bitcoinfuzz
{
    namespace module
    {
        NBitcoin::NBitcoin(void) : BaseModule("NBitcoin") {}
        std::optional<bool> NBitcoin::miniscript_parse(std::string str) const
        {
            return nbitcoin_miniscript_parse(str.c_str());
        }
        std::optional<bool> NBitcoin::descriptor_parse(std::string str) const
        {
            return nbitcoin_descriptor_parse(str.c_str());
        }
        std::optional<std::string> NBitcoin::bip32_deserialize_key_xpub_xprv(std::span<const uint8_t> buffer) const
        {
            char* p = nbitcoin_bip32_deserialize_key_xpub_xprv(buffer.data(), buffer.size());
            if (p == nullptr) return std::nullopt;
            std::string s(p);
            nbitcoin_free_c_string(p);   
            return s;
        }

    }
}
