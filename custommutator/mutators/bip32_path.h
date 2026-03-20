#ifndef BITCOINFUZZ_CUSTOMMUTATOR_BIP32_H
#define BITCOINFUZZ_CUSTOMMUTATOR_BIP32_H

#include <algorithm>
#include <cstdint>
#include <cstring>
#include <string>

extern "C" size_t LLVMFuzzerMutate(uint8_t *Data, size_t Size, size_t MaxSize);

// Fallback input
static size_t initial_input(uint8_t *data, size_t max_size) {
    const std::string dummy = "m/0";
    size_t len = std::min(dummy.size(), max_size);
    std::memcpy(data, dummy.data(), len);
    return len;
}

// Allow only: m / 0-9
static void apply_whitelist(uint8_t *data, size_t size) {
    for (size_t i = 0; i < size; i++) {
        char &c = reinterpret_cast<char *>(data)[i];

        if (c == 'm' || c == '/') continue;
        if (c >= '0' && c <= '9') continue;

        c = '0' + (c % 10);
    }
}

// Remove // and trailing /
static size_t normalize(uint8_t *data, size_t size) {
    std::string in(reinterpret_cast<char *>(data), size);
    std::string out;
    out.reserve(in.size());

    bool last_slash = false;

    for (char c : in) {
        if (c == '/') {
            if (out.empty()) continue;
            if (last_slash) continue;
            last_slash = true;
        } else {
            last_slash = false;
        }
        out.push_back(c);
    }

    if (!out.empty() && out.back() == '/') {
        out.pop_back();
    }

    std::memcpy(data, out.data(), out.size());
    return out.size();
}

// Ensure minimal valid form
static size_t enforce(uint8_t *data, size_t size, size_t max_size) {
    std::string s(reinterpret_cast<char *>(data), size);

    if (s.empty() || s[0] != 'm') {
        s = "m/0";
    }

    if (std::count(s.begin(), s.end(), '/') == 0) {
        s += "/0";
    }

    size = std::min(s.size(), max_size);
    std::memcpy(data, s.data(), size);
    return size;
}

// Entry point
extern "C" size_t LLVMFuzzerCustomMutator(uint8_t *data,
                                          size_t size,
                                          size_t max_size,
                                          unsigned int seed) {

    if (size == 0)
        return initial_input(data, max_size);

    size = LLVMFuzzerMutate(data, size, max_size);

    apply_whitelist(data, size);
    size = normalize(data, size);
    size = enforce(data, size, max_size);

    // small deterministic change to avoid duplicates
    if (size > 2) {
        size_t pos = (seed % (size - 1)) + 1;
        char &c = reinterpret_cast<char *>(data)[pos];
        if (c >= '0' && c <= '9') {
            c = '0' + ((c - '0' + 1) % 10);
        }
    }

    return size;
}

#endif // BITCOINFUZZ_CUSTOMMUTATOR_BIP32_H