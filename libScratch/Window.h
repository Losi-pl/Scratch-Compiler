#pragma once

#ifdef __cplusplus
extern "C" {
#endif

void __cdecl create_window(const char* title);
void __cdecl start_app();

double __cdecl sensing_timer();
void __cdecl sensing_reset_timer();
double __cdecl global_timer();

#ifdef __cplusplus
}
#endif