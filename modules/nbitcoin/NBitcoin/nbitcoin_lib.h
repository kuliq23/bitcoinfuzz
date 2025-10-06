#include <cstdint>

extern "C" bool nbitcoin_miniscript_parse(const char* input);

extern "C" bool nbitcoin_descriptor_parse(const char* input);

extern "C" char* nbitcoin_bip32_deserialize_key_xpub_xprv(const uint8_t *data, size_t len);

extern "C" void nbitcoin_free_c_string(void* ptr);