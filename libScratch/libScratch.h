#ifndef SCRATCH_H
#define SCRATCH_H
#include <list>

#include "Actor.h"
#include "Script.h"

__declspec(dllexport) std::list<void*>::iterator __cdecl begin_actor_iteration();

#ifdef __cplusplus
extern "C" {
#endif

extern Events* stage_events;
extern void* stage_scripts;
extern unsigned int stage_script_count;
extern Actor::CostumesStorage* stage_backdrops;
extern unsigned int stage_backdrop_index;

__declspec(dllexport) void __cdecl scratch_init();

__declspec(dllexport) char* __cdecl combineStrings_3(const char* left, const char* center, const char* right);
__declspec(dllexport) double __cdecl random_double(double min, double max);
__declspec(dllexport) double __cdecl random_int(int min, int max);

__declspec(dllexport) void __cdecl register_new_actor(Actor* actor);
__declspec(dllexport) void* __cdecl get_iteration_next(std::list<void*>::iterator& iterator, std::list<void*>& list, bool removeLast);
__declspec(dllexport) void __cdecl run_actor_script(Script* scripts, unsigned int index);
__declspec(dllexport) std::list<void*>* __cdecl get_actors();
__declspec(dllexport) unsigned int __cdecl actor_count();

__declspec(dllexport) char* __cdecl cast_double_to_string(double value);

__declspec(dllexport) void __cdecl looks_say(const char* target, const char* text);
__declspec(dllexport) void __cdecl looks_think(const char* target, const char* text);

__declspec(dllexport) void __cdecl motion_goto_xy(Actor& actor, double x, double y);
__declspec(dllexport) void __cdecl motion_goto_random(Actor& actor);

__declspec(dllexport) void __cdecl motion_begin_glide_to(const Actor& actor, double* args, double desX, double desY, double time);
__declspec(dllexport) bool __cdecl motion_glide_to(Actor& actor, const double* args);

__declspec(dllexport) void __cdecl say_direction(Actor& actor);

__declspec(dllexport) char* __cdecl operator_join(const char* left, const char* right);
#ifdef __cplusplus
}
#endif

#endif //SCRATCH_H