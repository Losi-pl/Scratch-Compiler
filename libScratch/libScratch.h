#ifndef SCRATCH_H
#define SCRATCH_H
#include <list>

#include "Actor.h"
#include "Script.h"

__declspec(dllexport) std::list<void*>::iterator __cdecl begin_actor_iteration();

#ifdef __cplusplus
extern "C" {
#endif

__declspec(dllexport) void __cdecl scratch_init();

__declspec(dllexport) void __cdecl looks_say(const char* target, const char* text);
__declspec(dllexport) void __cdecl looks_think(const char* target, const char* text);

__declspec(dllexport) char* __cdecl combineStrings_2(const char* left, const char* right);
__declspec(dllexport) char* __cdecl combineStrings_3(const char* left, const char* center, const char* right);

__declspec(dllexport) void __cdecl register_new_actor(Actor* actor);
__declspec(dllexport) void* __cdecl get_iteration_next(std::list<void*>::iterator& iterator, std::list<void*>& list, bool removeLast);

__declspec(dllexport) std::list<void*>* __cdecl get_actors();
__declspec(dllexport) unsigned int __cdecl actor_count();

__declspec(dllexport) void __cdecl run_actor_script(Script* scripts, unsigned int index);

#ifdef __cplusplus
}
#endif

#endif //SCRATCH_H