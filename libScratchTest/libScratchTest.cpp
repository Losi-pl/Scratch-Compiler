#include <iostream>
#include "../libScratch/libScratch.h"

int main(int argc, char* argv[])
{
    char str1[] = "Ala ma ";
    char str2[] = "kota";
    auto combined = combineStrings_2(str1, str2);
    
    
    char str3[] = "Ala ";
    char str4[] = "ma ";
    char str5[] = "kota";
    auto combined2 = combineStrings_3(str3, str4, str5);
    
    std::cout << combined << '\n' << combined2;
    
    scratch_init();
    
    auto iter = begin_actor_iteration();
    auto iter_size = sizeof(std::list<Actor*>::iterator);
    
    std::cout << "sizeof(std::list<Actor*>::iterator) = " << iter_size << '\n';
    
    return 0;
}
