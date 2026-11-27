#pragma once
#include <string>
#include <SFML/Graphics/Image.hpp>
#include <SFML/Graphics/Sprite.hpp>
#include <SFML/Graphics/Texture.hpp>

#include "Events.h"
#include "Script.h"

struct Actor
{
    struct CostumesStorage;
    
    char* name;
    double x;
    double y;
    Events* events;
    Script* scripts;
    unsigned int script_count;
    unsigned int costume_index;
    double scale;
    CostumesStorage* costumes;
    double direction;
    
    void __cdecl set_x(double x);
    void __cdecl set_y(double y);
    void __cdecl set_direction(double dir);
    
    struct alignas(8) Costume   // NOLINT(clang-diagnostic-padded)
    {
        sf::Texture texture;
        sf::Sprite sprite;
        std::string name;
        float imageScale;
        
        Costume(const void* content, size_t size, const char* name, float scale): texture(content, size), sprite(texture), name(name) { imageScale = scale; }
        Costume(const sf::Image& image, const char* name, float scale): texture(image), sprite(texture), name(name) { imageScale = scale; }
    };

    struct alignas(Costume) CostumesStorage {   // NOLINT(clang-diagnostic-padded)
        unsigned int count;
        Costume& operator[](unsigned int index);
    };
};

extern "C" {
    void __cdecl construct_actor(Actor& actor, char* name, Events* script_events, double x, double y, unsigned int script_count, Actor::CostumesStorage* costumes, unsigned int costume_index, double scale, double direction);
    Actor::CostumesStorage* __cdecl create_costume_storage(unsigned int count);    
    void __cdecl load_costume_data(Actor::CostumesStorage* storage, unsigned int index, const void* data, size_t size, const char* name);
    void __cdecl load_costume_svg(Actor::CostumesStorage* storage, unsigned int index, const char* svg, const char* name);
}