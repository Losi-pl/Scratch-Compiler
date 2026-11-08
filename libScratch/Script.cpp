#include "pch.h"
#include "Script.h"

#include <ostream>

Script::Script(unsigned int stackSize)
{ 
    this->scriptPoint = 0;
    this->stackSize = stackSize;
    if (stackSize > 0)
        this->methodStack = malloc(stackSize);
    else
        this->methodStack = nullptr;
}
