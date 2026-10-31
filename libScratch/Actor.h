#pragma once

struct Actor
{
public:
    char* name;
    float x;
    float y;
    
    void __cdecl set_x(float x);
    
    void __cdecl set_y(float y);
    
   __cdecl Actor(char* name, float x, float y);
};

