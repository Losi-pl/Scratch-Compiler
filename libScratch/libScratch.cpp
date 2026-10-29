// libScratch.cpp : Definiuje funkcje biblioteki statycznej.
//

#include "pch.h"

#include <cstdlib>

#include "framework.h"

char* combineStrings(const char* left, const char* right) {
    unsigned int leftC = 0, rightC = 0;

    for (; left[leftC] != 0; ++leftC) { }
    for (; right[rightC] != 0; ++rightC) { }
    
    auto result = static_cast<char*>(malloc(leftC + rightC + 1));
    
    for (unsigned int i = 0; i < leftC; ++i) 
        result[i] = left[i];
    for (unsigned int i = 0; i < rightC; ++i) 
        result[leftC + i] = right[i];
    result[leftC + rightC] = 0;
    
    return result;
}
