#pragma once
#include <string>
#include <SFML/Graphics/Sprite.hpp>
#include <SFML/Graphics/Texture.hpp>

#include "Events.h"
#include "Script.h"

struct Actor
{
public:
    char* name;
    double x;
    double y;
    Events* events;
    Script* scripts;
    unsigned int script_count;
    void* costumes;
    unsigned int costume_index;
    
    void __cdecl set_x(double x);
    
    void __cdecl set_y(double y);
    
   __cdecl Actor(char* name, Events* script_events, double x, double y, unsigned int script_count, void* costumes, unsigned int costume_index);
    
    struct Costume
    {
        sf::Sprite sprite;
        std::string name;
        
        Costume(const sf::Texture& texture, const char* name) : sprite(texture) { this->name = name; }
    };
};

extern "C" {
    void* __cdecl create_costume_storage(unsigned int count);

    void __cdecl load_costume_data(void* storage, unsigned int index, const void* data, size_t size, const char* name);
    void __cdecl load_costume_svg(void* storage, unsigned int index, const char* svg, const char* name);
}