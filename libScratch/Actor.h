#pragma once
#include "Events.h"
#include "Script.h"

struct Actor
{
public:
    char* name;
    double x;
    double y;
    Events* events;
    Script* scripts;
    unsigned int script_count;
    
    void __cdecl set_x(double x);
    
    void __cdecl set_y(double y);
    
   __cdecl Actor(char* name, Events* script_events, double x, double y, unsigned int script_count);
    
    // TODO Proposed Graphics: SFML
};

