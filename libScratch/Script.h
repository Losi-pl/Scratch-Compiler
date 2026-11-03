#pragma once
#include <cstdlib>

struct Script
{
    void* method;
    unsigned int scriptPoint;
    unsigned int stackSize;
    void* methodStack;
    
    __cdecl Script(void* method, unsigned int stackSize);
};
