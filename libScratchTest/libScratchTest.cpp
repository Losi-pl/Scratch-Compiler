#include <iostream>
#include "../libScratch/pch.h"

int main(int argc, char* argv[])
{
    char str1[] = "Ala ma ";
    char str2[] = "kota";
    auto combined = combineStrings(str1, str2);
    
    std::cout << combined;
    return 0;
}
