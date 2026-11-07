#pragma once
#include "Events.h"
#include "Script.h"

struct Actor
{
public:
    char* name;
    float x;
    float y;
    Events* events;
    Script* scripts;
    unsigned int script_count;
    
    void __cdecl set_x(float x);
    
    void __cdecl set_y(float y);
    
   __cdecl Actor(char* name, Events* script_events, float x, float y, unsigned int script_count);
};

