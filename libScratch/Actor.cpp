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

void __cdecl Actor::set_direction(double dir)
{
    // For whatever reason the range of direction in Scratch is <-179;181)
    this->direction = std::fmod(dir + 179, 380) - 179;
    if (this->direction < -179)
        this->direction += 360;
}

void construct_actor(Actor& actor, char* name, Events* script_events, const double x, const double y, const unsigned int script_count, Actor::CostumesStorage* costumes, unsigned int costume_index, double scale, double direction)
{
    actor.name = name;
    actor.x = x;
    actor.y = y;
    actor.scripts = static_cast<Script*>(_aligned_malloc(sizeof(Script) * script_count, alignof(Script)));
    actor.script_count = script_count;
    actor.events = script_events;
    actor.costumes = costumes;
    actor.costume_index = costume_index;
    actor.scale = scale;
    actor.direction = direction;
}

Actor::Costume& Actor::CostumesStorage::operator[](unsigned int index) { return *reinterpret_cast<Costume*>(reinterpret_cast<uint8_t*>(this) + sizeof(CostumesStorage) + (sizeof(Costume) * index)); }
Actor::CostumesStorage* create_costume_storage(const unsigned int count)
{
    auto size = sizeof(Actor::CostumesStorage) + sizeof(Actor::Costume) * count;
    auto memory = _aligned_malloc(size, alignof(Actor::CostumesStorage));
    auto* storage = ::new(memory) Actor::CostumesStorage{count};
    
    return storage;
}
void load_costume_data(Actor::CostumesStorage* storage, unsigned int index, const void* data, size_t size, const char* name)
{
    auto& costume = (*storage)[index];
    ::new (&costume) Actor::Costume(data, size, name, 1.f);
    costume.sprite.setOrigin({static_cast<float>(costume.texture.getSize().x) / 2.f, static_cast<float>(costume.texture.getSize().y) / 2.f});
}
void load_costume_svg(Actor::CostumesStorage* storage, unsigned int index, const char* svg, const char* name)
{
    auto& costume = (*storage)[index];
    
    try
    {
        auto document = lunasvg::Document::loadFromData(svg);
        if (!document) throw std::runtime_error("Failed to load icon");
        {
            auto width = static_cast<unsigned int>(round(document->width() * 4));
            auto height = static_cast<unsigned int>(round(document->height() * 4));
            auto bitmap = document->renderToBitmap(static_cast<int>(width), static_cast<int>(height));
            const auto srcPixels = bitmap.data();
            
            uint8_t* rgbaPixels = new uint8_t[static_cast<size_t>(width) * height * 4];
            for (size_t i = 0; i < static_cast<size_t>(width) * height * 4; i += 4) {
                rgbaPixels[i + 0] = srcPixels[i + 2]; // Red   <- Blue
                rgbaPixels[i + 1] = srcPixels[i + 1]; // Green <- Green
                rgbaPixels[i + 2] = srcPixels[i + 0]; // Blue  <- Red
                rgbaPixels[i + 3] = srcPixels[i + 3]; // Alpha <- Alpha
            }
            
            sf::Image img({width, height}, rgbaPixels);
            ::new (&costume) Actor::Costume(img, name, .5f);
            delete[] rgbaPixels;
            costume.sprite.setOrigin({static_cast<float>(costume.texture.getSize().x) / 2.f, static_cast<float>(costume.texture.getSize().y) / 2.f});
        }
    }
    catch (std::exception& e)
    {
        std::cout << "Error: " << e.what() << '\n';
        _exit(-1);
    }
}