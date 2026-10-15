#include <cstring>
#include <string>
#include <vector>
#include "wildcards.hpp"
#include "Listing1.cpp"
#define PCRE2_CODE_UNIT_WIDTH 8
#include "pcre2.h"
extern "C" {
#include "wildmatch.h"
}

struct Context {
    int library;
    std::string pattern;
    std::vector<std::string> inputs;
    pcre2_code* code = nullptr;
    pcre2_match_data* data = nullptr;
    ~Context() { pcre2_match_data_free(data); pcre2_code_free(code); }
};

// Prevent whole-batch folding or specialization to a benchmark fixture.
__declspec(noinline) bool krauss(Context* c, const std::string& s) {
    return FastWildCompare(c->pattern.data(), const_cast<char*>(s.c_str()));
}
__declspec(noinline) bool git_match(Context* c, const std::string& s) {
    return wildmatch(c->pattern.c_str(), s.c_str(), 0) == WM_MATCH;
}
__declspec(noinline) bool zema(Context* c, const std::string& s) {
    return wildcards::match(cx::string_view(s.data(), s.size()), cx::string_view(c->pattern.data(), c->pattern.size()));
}
__declspec(noinline) bool pcre(Context* c, const std::string& s) {
    int result = pcre2_jit_match(c->code, reinterpret_cast<PCRE2_SPTR>(s.data()), s.size(), 0, 0, c->data, nullptr);
    if (result < 0 && result != PCRE2_ERROR_NOMATCH) std::terminate();
    return result >= 0;
}
using Matcher = bool (*)(Context*, const std::string&);
static Matcher matchers[] = { krauss, git_match, zema, pcre };

extern "C" __declspec(dllexport) Context* create_context(int library, const char* pattern, const char** inputs, int count) {
    if (library < 0 || library >= 4 || count < 0) return nullptr;
    Context* c = new Context();
    c->library = library;
    c->pattern = pattern;
    for (int i = 0; i < count; ++i) c->inputs.emplace_back(inputs[i]);
    if (library == 3) {
        std::string regex = "\\A";
        for (char ch : c->pattern) {
            if (ch == '*') regex += ".*";
            else if (ch == '?') regex += '.';
            else {
                if (std::strchr("\\.^$|()[]{}+", ch)) regex += '\\';
                regex += ch;
            }
        }
        regex += "\\z";
        int error;
        PCRE2_SIZE offset;
        c->code = pcre2_compile(reinterpret_cast<PCRE2_SPTR>(regex.c_str()), regex.size(), PCRE2_DOTALL, &error, &offset, nullptr);
        if (!c->code || pcre2_jit_compile(c->code, PCRE2_JIT_COMPLETE) != 0) { delete c; return nullptr; }
        c->data = pcre2_match_data_create_from_pattern(c->code, nullptr);
        if (!c->data) { delete c; return nullptr; }
    }
    return c;
}

extern "C" __declspec(dllexport) int match_batch(Context* c, int count) {
    if (count < 0 || static_cast<size_t>(count) > c->inputs.size()) return -1;
    Matcher match = matchers[c->library];
    int matched = 0;
    for (int i = 0; i < count; ++i) matched += match(c, c->inputs[i]);
    return matched;
}
extern "C" __declspec(dllexport) int match_one(Context* c, int index) {
    return matchers[c->library](c, c->inputs.at(index));
}
extern "C" __declspec(dllexport) void destroy_context(Context* c) { delete c; }
