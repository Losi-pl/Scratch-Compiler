#pragma once

#ifdef __cplusplus
extern "C" {
#endif

void __cdecl create_window(const char* title);
void __cdecl start_app();
void __cdecl test_load_image(const void* data, size_t size);

#ifdef __cplusplus
}
#endif