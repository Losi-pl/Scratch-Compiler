#include "pch.h"
#include "Actor.h"

#include <cstdlib>

void Actor::set_x(const float x)
{
    if (x < -240)
        this->x = -240;
    else if (x > 240)
        this->x = 240;
    else
        this->x = x;
}
    
void Actor::set_y(const float y)
{
    if (y < -180)
        this->y = -180;
    else if (y > 180)
        this->y = 180;
    else
        this->y = y;
}

Actor::Actor(char* name, void* script_engine, float x, float y, unsigned int script_count)
{
    this->name = name;
    this->x = x;
    this->y = y;
    this->scripts = static_cast<Script*>(malloc(sizeof(Script)*script_count));
    this->script_count = script_count;
    this->script_engine = script_engine;
}
