#include "pch.h"
#include "Actor.h"

#include <cstdlib>
#include <iostream>
#include <lunasvg.h>
#include <SFML/Graphics/Image.hpp>

void Actor::set_x(const double x)  // NOLINT(clang-diagnostic-shadow)
{
    if (x < -240)
        this->x = -240;
    else if (x > 240)
        this->x = 240;
    else
        this->x = x;
}
    
void Actor::set_y(const double y)  // NOLINT(clang-diagnostic-shadow)
{
    if (y < -180)
        this->y = -180;
    else if (y > 180)
        this->y = 180;
    else
        this->y = y;
}

Actor::Actor(char* name, Events* script_events, const double x, const double y, const unsigned int script_count, void* costumes, unsigned int costume_index)
{
    this->name = name;
    this->x = x;
    this->y = y;
    this->scripts = static_cast<Script*>(malloc(sizeof(Script) * script_count));
    this->script_count = script_count;
    this->events = script_events;
    this->costumes = costumes;
    this->costume_index = costume_index;
}



void* create_costume_storage(const unsigned int count)
{
    auto storage = malloc(sizeof(unsigned int) * 2 + sizeof(Actor::Costume) * count);
    if (storage == nullptr)
        std::abort();
    
    static_cast<unsigned int*>(storage)[0] = count;
    
    return storage;
}
void load_costume_data(void* storage, unsigned int index, const void* data, size_t size, const char* name)
{
    auto costumes = reinterpret_cast<Actor::Costume*>(&static_cast<unsigned int*>(storage)[2]);
    
    auto texture = new sf::Texture();
    if (!texture->loadFromMemory(data, size))
        std::abort();
    
    memset(reinterpret_cast<void*>(&costumes[index]), 0, sizeof(Actor::Costume));
    costumes[index] = Actor::Costume(*texture, name);
    costumes[index].sprite.setOrigin({static_cast<float>(texture->getSize().x) / 2.f, static_cast<float>(texture->getSize().y) / 2.f});
}
void load_costume_svg(void* storage, unsigned int index, const char* svg, const char* name)
{
    auto costumes = reinterpret_cast<Actor::Costume*>(&static_cast<unsigned int*>(storage)[2]);
     
    try
    {
        auto document = lunasvg::Document::loadFromData(svg);
        if (!document)
            throw std::runtime_error("Failed to load icon");
 
        {
            auto width = static_cast<unsigned int>(round(document->width())) * 2;
            auto height = static_cast<unsigned int>(round(document->height())) * 2;
            auto bitmap = document->renderToBitmap(static_cast<int>(width), static_cast<int>(height));
            const auto srcPixels = bitmap.data();
             
            std::vector<uint8_t> rgbaPixels(std::size_t{width} * height * 4);  // NOLINT(bugprone-implicit-widening-of-multiplication-result)
             
            for (unsigned long long i = 0; i < std::size_t{width} * height * 4; i += 4) {
                rgbaPixels[i + 0] = srcPixels[i + 2]; // Red   <- Blue
                rgbaPixels[i + 1] = srcPixels[i + 1]; // Green <- Green
                rgbaPixels[i + 2] = srcPixels[i + 0]; // Blue  <- Red
                rgbaPixels[i + 3] = srcPixels[i + 3]; // Alpha <- Alpha
            }
            sf::Image img({width, height}, rgbaPixels.data());
            memset(reinterpret_cast<void*>(&costumes[index]), 0, sizeof(Actor::Costume));
            costumes[index] = Actor::Costume(*(new sf::Texture(img)), name);
            costumes[index].sprite.setScale({.5f, .5f});
            costumes[index].sprite.setOrigin({static_cast<float>(width) / 2.f, static_cast<float>(height) / 2.f});
        }
    }
    catch (std::exception& e)
    {
        std::cout << "Error: " << e.what() << '\n';
        _exit(-1);
    }
}