#include <iostream>

int main()
{
	int a;
	int b;
	
	std::cin >> a;
	std::cin >> b;
	
	while (a != b)
	{
		if (a>b)
			a = a - b;
		else 
			b = b - a;
	}
	std::cout << a;
} 