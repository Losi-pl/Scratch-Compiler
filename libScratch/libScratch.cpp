// libScratch.cpp : Defines functions of the static library.
//

#include "pch.h"
#include "libScratch.h"

#include <list>
#include <iostream>
#include <ostream>

char* combineStrings_2(const char* left, const char* right) {
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

void looks_say(const char* target, const char* text)
{
    std::cout << target << " is saying: " << text << '\n';
}

void looks_think(const char* target, const char* text)
{
    std::cout << target << " is thinking: " << text << '\n';
}

static std::list<Actor*> actors;

void scratch_init()
{
    actors = std::list<Actor*>();
}

void register_new_actor(Actor* actor)
{
    actors.push_back(actor);
}
