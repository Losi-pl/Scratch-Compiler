#include "pch.h"
#include "Script.h"

#include <cstring>

Script::Script(void* method, unsigned int stackSize)
{
    this->method = method;
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
