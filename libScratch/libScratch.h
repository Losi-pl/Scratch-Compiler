#ifndef SCRATCH_H
#define SCRATCH_H
#include <list>

#include "Actor.h"

std::list<Actor*>::iterator __cdecl begin_actor_iteration();

#ifdef __cplusplus
extern "C" {
#endif


__declspec(dllexport) char* __cdecl combineStrings_2(const char* left, const char* right);
__declspec(dllexport) char* __cdecl combineStrings_3(const char* left, const char* center, const char* right);

__declspec(dllexport) void __cdecl looks_say(const char* target, const char* text);
__declspec(dllexport) void __cdecl looks_think(const char* target, const char* text);

__declspec(dllexport) void __cdecl scratch_init();

__declspec(dllexport) void __cdecl register_new_actor(Actor* actor);
__declspec(dllexport) unsigned int __cdecl actor_count();

__declspec(dllexport) Actor* __cdecl next_actor(std::list<Actor*>::iterator* iterator);

#ifdef __cplusplus
}
#endif

#endif //SCRATCH_H