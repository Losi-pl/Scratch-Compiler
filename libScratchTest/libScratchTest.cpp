#include <iostream>
#include "../libScratch/libScratch.h"
#include "../libScratch/Script.h"

int main(int argc, char* argv[])
{
    char str1[] = "Ala ma ";
    char str2[] = "kota";
    auto combined = combineStrings_2(str1, str2);
    
    
    char str3[] = "Ala ";
    char str4[] = "ma ";
    char str5[] = "kota";
    auto combined2 = combineStrings_3(str3, str4, str5);
    
    std::cout << combined << '\n' << combined2 << '\n';
    
    scratch_init();
    
    std::cout << "sizeof(std::list<void*>) = " << sizeof(std::list<void*>) << '\n';
    std::cout << "sizeof(std::list<void*>::iterator) = " << sizeof(std::list<void*>::iterator) << "\n\n";
    
    
    std::cout << "sizeof(Actor) = " << sizeof(Actor) << '\n';
    std::cout << "sizeof(std::list<Actor>) = " << sizeof(std::list<Actor>) << '\n';
    std::cout << "sizeof(std::list<Actor>::iterator) = " << sizeof(std::list<Actor>::iterator) << "\n\n";
    
    return 0;
}
