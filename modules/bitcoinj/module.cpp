#include "module.h"
#include <cstring>
#include <filesystem>
#include <iostream>
#include <jni.h>
#include <jvmloader.h>
#include <optional>
#include <sstream>
#include <string>
#include <thread>

namespace fs = std::filesystem;

static JavaVM *jvm = nullptr;
static bool jvm_initialized = false;
static jclass bitcoinJWrapperClass = nullptr;
static jmethodID createMasterKeyMethod = nullptr;
static jmethodID deserializeExtendedKeyMethod = nullptr;
static jmethodID pathParseMethod = nullptr;

static bool init_jvm() {
  if (jvm_initialized) {
    return true;
  }

  jvm = JvmLoader::get_jvm();
  if (!jvm) {
    printf("NB Failed to get JVM\n");
    return false;
  }

  JNIEnv *env = nullptr;
  jint ge = jvm->GetEnv((void **)&env, JNI_VERSION_1_8);
  if (ge != JNI_OK || !env) {
    printf("NB Failed to get JNIEnv (status=%d)\n", ge);
    return false;
  }

  bitcoinJWrapperClass = env->FindClass("wrapper/Wrapper");
  if (!bitcoinJWrapperClass) {
    printf("NB Failed to find class wrapper/Wrapper\n");
    return false;
  }
  bitcoinJWrapperClass =
      static_cast<jclass>(env->NewGlobalRef(bitcoinJWrapperClass));

  createMasterKeyMethod = env->GetStaticMethodID(
      bitcoinJWrapperClass, "createMasterKey", "([B)Ljava/lang/String;");
  if (!createMasterKeyMethod) {
    printf("NB Failed to find method createMasterKey\n");
    return false;
  }

  deserializeExtendedKeyMethod = env->GetStaticMethodID(
      bitcoinJWrapperClass, "deserializeExtendedKey", "([B)Ljava/lang/String;");
  if (!deserializeExtendedKeyMethod) {
    printf("NB Failed to find method deserializeExtendedKey\n");
    return false;
  }

  pathParseMethod = env->GetStaticMethodID(bitcoinJWrapperClass, "pathParse",
                                           "([B)Ljava/lang/String;");
  if (!pathParseMethod) {
    printf("NB Failed to find method pathParse\n");
    return false;
  }

  jvm_initialized = true;
  return true;
}

static std::optional<std::string>
call_wrapper_method(jmethodID &methodRef, std::span<const uint8_t> buffer) {
  if (!init_jvm() || !jvm) {
    printf("NB JVM init failed\n");
    return "JVM FAILED";
  }

  if (!methodRef) {
    printf("NB Method reference is null\n");
    return "INVALID";
  }

  JNIEnv *env = nullptr;
  jint status = jvm->GetEnv((void **)&env, JNI_VERSION_1_8);
  if (status != JNI_OK || !env) {
    printf("NB Failed to get JNIEnv in call (status=%d)\n", status);
    return "JVM FAILED";
  }

  jbyteArray jBytes = env->NewByteArray(static_cast<jsize>(buffer.size()));
  if (!jBytes) {
    printf("NB Failed to allocate jbyteArray\n");
    return "JVM FAILED";
  }

  env->SetByteArrayRegion(jBytes, 0, static_cast<jsize>(buffer.size()),
                          reinterpret_cast<const jbyte *>(buffer.data()));

  jstring jResult = static_cast<jstring>(
      env->CallStaticObjectMethod(bitcoinJWrapperClass, methodRef, jBytes));

  env->DeleteLocalRef(jBytes);

  if (!jResult) {
    printf("NB Method returned null\n");
    return "FAILED";
  }

  const char *resultChars = env->GetStringUTFChars(jResult, nullptr);
  if (!resultChars) {
    printf("NB Failed to get UTF chars from result string\n");
    env->DeleteLocalRef(jResult);
    return "FAILED";
  }

  std::string result(resultChars);
  env->ReleaseStringUTFChars(jResult, resultChars);
  env->DeleteLocalRef(jResult);

  if (result == "skip error") {
    return std::nullopt;
  }
  return result;
}

namespace bitcoinfuzz {
namespace module {
BitcoinJ::BitcoinJ(void) : BaseModule("BitcoinJ") {}

std::optional<std::string>
BitcoinJ::bip32_master_keygen(std::span<const uint8_t> buffer) const {
  return call_wrapper_method(createMasterKeyMethod, buffer);
}

std::optional<std::string> BitcoinJ::bip32_deserialize_extended_key(
    std::span<const uint8_t> buffer) const {
  return call_wrapper_method(deserializeExtendedKeyMethod, buffer);
}

std::optional<std::string>
BitcoinJ::bip32_path_parse(std::span<const uint8_t> buffer) const {
  return call_wrapper_method(pathParseMethod, buffer);
}
} // namespace module
} // namespace bitcoinfuzz