// libScratch.cpp : Defines functions of the static library.
//

#include "pch.h"
#include "libScratch.h"
#include <charconv>
#include <list>
#include <iostream>
#include <numbers>
#include <ostream>
#include <random>

#include "Window.h"

extern "C" {
    Events* stage_events = nullptr;
    void* stage_scripts = nullptr;
    unsigned int stage_script_count = 0;
    Actor::CostumesStorage* stage_backdrops = nullptr;
    unsigned int stage_backdrop_index = 0;
}
namespace
{
    std::list<void*> actors;  // NOLINT(bugprone-throwing-static-initialization, misc-use-anonymous-namespace)
    std::mt19937_64 random_engine{std::random_device{}()};  // NOLINT(bugprone-throwing-static-initialization)
    std::uniform_real_distribution dist_double;
    std::uniform_int_distribution dist_int;
}

void looks_say(const char* target, const char* text)
{
    std::cout << target << " is saying: " << text << '\n';
}
void looks_think(const char* target, const char* text)
{
    std::cout << target << " is thinking: " << text << '\n';
}

void motion_goto_xy(Actor& actor, double x, double y)
{
    actor.set_x(x);
    actor.set_y(y);
}
void motion_goto_random(Actor& actor)
{
    actor.set_x(random_int(-240, 240));
    actor.set_y(random_int(-180, 180));
}
void motion_move_steps(Actor& actor, const double steps)
{
    auto deltaX = std::sin((actor.direction / 180) * std::numbers::pi) * steps;
    auto deltaY = std::cos((actor.direction / 180) * std::numbers::pi) * steps;
    actor.set_x(actor.x + deltaX);
    actor.set_y(actor.y + deltaY);
}

void motion_begin_glide_to(const Actor& actor, double* args, double desX, double desY, double time)
{
    args[0] = actor.x;
    args[1] = actor.y;
    args[2] = desX - actor.x;
    args[3] = desY - actor.y;
    args[4] = global_timer();
    args[5] = time;
}
bool motion_glide_to(Actor& actor, const double* args)
{
    auto now = global_timer();
    if(args[4] + args[5] <= now)
    {
        actor.set_x(args[0] + args[2]);
        actor.set_y(args[1] + args[3]);
        return true;
    }
    else
    {
        auto time = (now - args[4]) / args[5];
        actor.set_x(args[0] + (args[2] * time));
        actor.set_y(args[1] + (args[3] * time));
        return false;
    }
}

double random_double(const double min, const double max) {
    dist_double.param(std::uniform_real_distribution<>::param_type(min, max));
    return dist_double(random_engine);
}
double random_int(const int min, const int max) {
    dist_int.param(std::uniform_int_distribution<>::param_type(min, max));
    return dist_int(random_engine);
}
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