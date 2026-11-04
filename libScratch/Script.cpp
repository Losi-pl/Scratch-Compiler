#include "pch.h"
#include "Script.h"

#include <cstring>
#include <iostream>
#include <ostream>

Script::Script(unsigned int stackSize)
{ 
    this->scriptPoint = 0;
    this->stackSize = stackSize;
    if (stackSize > 0)
    {
        this->methodStack = malloc(stackSize);
        std::memset(this->methodStack, 0, stackSize);
    }
    else
        this->methodStack = nullptr;
}
