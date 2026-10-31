#pragma once

struct Actor
{
public:
    float x;
    float y;
    
    void __cdecl set_x(float x);
    
    void __cdecl set_y(float y);
    
   __cdecl Actor(float x, float y);
};

