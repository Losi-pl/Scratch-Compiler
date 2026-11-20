// libScratch.cpp : Defines functions of the static library.
//

#include "pch.h"
#include "libScratch.h"
#include <charconv>
#include <list>
#include <iostream>
#include <ostream>

void looks_say(const char* target, const char* text)
{
    std::cout << target << " is saying: " << text << '\n';
}

void looks_think(const char* target, const char* text)
{
    std::cout << target << " is thinking: " << text << '\n';
}

void __cdecl motion_goto_xy(Actor& actor, double x, double y)
{
    actor.set_x(x);
    actor.set_y(y);
}

static std::list<void*> actors;  // NOLINT(bugprone-throwing-static-initialization, misc-use-anonymous-namespace)

std::list<void*>* get_actors() { return &actors; }
void scratch_init() { actors = std::list<void*>(); }
void register_new_actor(Actor* actor) { actors.push_back(actor); }
std::list<void*>::iterator begin_actor_iteration() { return actors.begin(); }
unsigned int actor_count() { return static_cast<unsigned int>(actors.size()); }
char* operator_join(const char* left, const char* right) {
    unsigned int leftC = 0, rightC = 0;

    while (left[leftC] != 0) ++leftC;
    while (right[rightC] != 0) ++rightC;
    
    auto result = static_cast<char*>(malloc(leftC + rightC + 1));
    
    for (unsigned int i = 0; i < leftC; ++i) 
        result[i] = left[i];
    for (unsigned int i = 0; i < rightC; ++i) 
        result[leftC + i] = right[i];
    result[leftC + rightC] = 0;
    
    return result;
}
char* combineStrings_3(const char* left, const char* center, const char* right) {
    unsigned int leftC = 0, centerC = 0, rightC = 0;

    while (left[leftC] != 0) ++leftC;
    while (center[centerC] != 0) ++centerC;
    while (right[rightC] != 0) ++rightC;
    
    auto result = static_cast<char*>(malloc(leftC + centerC + rightC + 1));
    
    for (unsigned int i = 0; i < leftC; ++i) 
        result[i] = left[i];
    for (unsigned int i = 0; i < centerC; ++i) 
        result[leftC + i] = center[i];
    for (unsigned int i = 0; i < rightC; ++i) 
        result[leftC + centerC + i] = right[i];
    result[leftC + centerC + rightC] = 0;
    
    return result;
}
char* cast_double_to_string(const double value)
{
    if (value == 0.0) 
    {
        auto buffer = static_cast<char*>(malloc(2));
        if(buffer == nullptr) std::abort();
        std::memcpy(buffer, "0", 2);
        return buffer;
    }
    if (std::isnan(value))
    {
        auto buffer = static_cast<char*>(malloc(4));
        if(buffer == nullptr) std::abort();
        std::memcpy(buffer, "NaN", 4);
        return buffer;
    }
    if (std::isinf(value))
    {
        auto isNeg = std::signbit(value);
        auto buffer = static_cast<char*>(malloc(isNeg ? 10 : 9));
        if(buffer == nullptr) std::abort();
        std::memcpy(buffer, isNeg ? "-Infinity" : "Infinity", isNeg ? 10 : 9);
        return buffer;
    }
    
    {
        unsigned int bufferSize = 32;
        bool isScientific = std::abs(value) < 1e-6 || std::abs(value) >= 1e21;
        char* buffer = static_cast<char*>(malloc(bufferSize));
        if (buffer == nullptr) std::abort();
        while (true)
        {
            auto [ptr, ec] = std::to_chars(buffer, &buffer[bufferSize], value, isScientific ? std::chars_format::scientific :std::chars_format::fixed);
            if (ec != std::errc())
            {
                bufferSize *= 2;
                free(buffer);
                buffer = static_cast<char*>(malloc(bufferSize));
                if (buffer == nullptr) std::abort();
                continue;
            }
            *ptr = '\0';
            return buffer;
        }
    }
}
void* get_iteration_next(std::list<void*>::iterator& iterator, std::list<void*>& list, bool removeLast)
{
    if (iterator == list.end())
    {
        if (removeLast && iterator != list.begin())
            list.erase(std::prev(iterator));
        
        return nullptr;
    }

    auto value = *iterator;
    
    if (removeLast && iterator != list.begin())
    {
        auto rem = list.erase(std::prev(iterator)); 
        iterator = std::next(rem);
    }
    else
        iterator = std::next(iterator);
    
    return value;
}
void run_actor_script(Script* scripts, unsigned int index)
{
    auto script = &scripts[index];
    
    script->scriptPoint = 1; // TODO Not all scripts are reset at double trigger
    if (script->stackSize > 0)
        std::memset(script->methodStack, 0, script->stackSize);
}