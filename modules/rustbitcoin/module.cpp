
#include <algorithm>
#include <span>

#include "module.h"
#include "rust_bitcoin_lib/rust_bitcoin_lib.h"

namespace bitcoinfuzz {
namespace module {
Rustbitcoin::Rustbitcoin(void) : BaseModule("Rustbitcoin") {}

std::optional<std::string>
Rustbitcoin::script_parse(std::span<const uint8_t> buffer) const {
  auto script{rust_bitcoin_script(buffer.data(), buffer.size())};
  std::string result(script);
  free_c_string(script);
  return result;
}

std::optional<std::string>
Rustbitcoin::deserialize_block(std::span<const uint8_t> buffer) const {
  auto pointer{rust_bitcoin_des_block(buffer.data(), buffer.size())};
  std::string result(pointer);
  free_c_string(pointer);
  if (result == "skip error") {
    return std::nullopt;
  }
  return result;
}

std::optional<std::string> Rustbitcoin::address_parse(std::string str) const {
  auto result_ptr = rust_bitcoin_address_parse(str.c_str());
  if (result_ptr == nullptr)
    return std::nullopt;

  std::string result(result_ptr);
  free_c_string(result_ptr);
  return result;
}

std::optional<std::string>
Rustbitcoin::psbt_parse(std::span<const uint8_t> buffer) const {
  auto result_ptr = rust_bitcoin_psbt_parse(buffer.data(), buffer.size());
  if (result_ptr == nullptr)
    return std::nullopt;

  std::string result(result_ptr);
  free_c_string(result_ptr);
  return result;
}

std::optional<std::string>
Rustbitcoin::addrv2_parse(std::span<const uint8_t> buffer) const {
  auto result_ptr = rust_bitcoin_addrv2(buffer.data(), buffer.size());
  if (result_ptr == nullptr)
    return std::nullopt;

  std::string result(result_ptr);
  free_c_string(result_ptr);
  return result;
}

std::optional<int>
Rustbitcoin::cmpctblocks_parse(std::span<const uint8_t> buffer) const {
  int32_t result = rust_bitcoin_cmpctblocks_parse(buffer.data(), buffer.size());
  if (result == -1 || result == -2) {
    return result; // Return error codes as is
  }
  return result; // Return the count, or handle as needed
}

std::optional<std::string>
Rustbitcoin::parse_p2p_message(std::span<const uint8_t> buffer) const {
  auto message{rust_bitcoin_parse_p2p_message(buffer.data(), buffer.size())};
  if (message == nullptr)
    return std::nullopt;
  std::string result(message);
  free_c_string(message);
  return result;
}

std::optional<std::string>
Rustbitcoin::bip32_master_keygen(std::span<const uint8_t> buffer) const {
  auto result_ptr =
      rust_bitcoin_bip32_master_keygen(buffer.data(), buffer.size());
  if (result_ptr == nullptr)
    return std::nullopt;
  std::string result(result_ptr);
  free_c_string(result_ptr);
  return result;
}

std::optional<std::string> Rustbitcoin::bip32_deserialize_extended_key(
    std::span<const uint8_t> buffer) const {
  auto result_ptr =
      rust_bitcoin_bip32_deserialize_extended_key(buffer.data(), buffer.size());
  if (result_ptr == nullptr)
    return std::nullopt;
  std::string result(result_ptr);
  free_c_string(result_ptr);
  return result;
}

std::optional<std::string>
Rustbitcoin::bip32_derive_from_path(std::span<const uint8_t> buffer) const {
  // PATH FILTERING BEGINS
  //  if buffer contains a '+', return nullopt
  if (std::find(buffer.begin(), buffer.end(), static_cast<uint8_t>('+')) !=
      buffer.end())
    return std::nullopt;
  // if trailing slash, return nullopt
  if (!buffer.empty() && buffer.back() == static_cast<uint8_t>('/'))
    return std::nullopt;
  // if whitespace present, return nullopt
  if (std::find_if(buffer.begin(), buffer.end(), ::isspace) != buffer.end())
    return std::nullopt;
  // if index too large (> 0x7FFFFFFF), return nullopt
  std::string path_str(buffer.begin(), buffer.end());
  size_t start = 0;
  while (start < path_str.size()) {
    size_t end = path_str.find('/', start);
    std::string part = path_str.substr(start, end - start);
    if (!part.empty() && (part.back() == '\'' || part.back() == 'h'))
      part.pop_back();
    if (!part.empty()) {
      try {
        unsigned long long val = std::stoull(part);
        if (val > 0x7FFFFFFF)
          return std::nullopt;
      } catch (...) {
      }
    }
    if (end == std::string::npos)
      break;
    start = end + 1;
  }
  // PATH FILTERING ENDS
  auto result_ptr =
      rust_bitcoin_bip32_derive_from_path(buffer.data(), buffer.size());
  if (result_ptr == nullptr)
    return std::nullopt;
  std::string result(result_ptr);
  free_c_string(result_ptr);
  return result;
}

} // namespace module
} // namespace bitcoinfuzz
