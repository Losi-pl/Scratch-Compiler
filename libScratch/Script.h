#pragma once
#include <cstdlib>

struct Script
{
    unsigned int scriptPoint;
    unsigned int stackSize;
    void* methodStack;
    
    __cdecl Script(unsigned int stackSize);
};
