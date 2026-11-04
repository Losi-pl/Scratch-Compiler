#pragma once
#include "Script.h"

struct Actor
{
public:
    char* name;
    float x;
    float y;
    void* script_engine;
    Script* scripts;
    unsigned int script_count;
    
    void __cdecl set_x(float x);
    
    void __cdecl set_y(float y);
    
   __cdecl Actor(char* name, void* script_engine, float x, float y, unsigned int script_count);
};

