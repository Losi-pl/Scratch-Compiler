#include "pch.h"
#include "Actor.h"

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