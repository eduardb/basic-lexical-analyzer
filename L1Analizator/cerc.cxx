#include <iostream>

int main()
{
	float r;
	float pi;
	float aria;
	float perim;
	pi = 3.1415;
	std::cin >> r;
	aria = pi * r * r;
	perim = 2 * pi * r;
	std::cout << perim;
	std::cout << aria;
}
	