#include <iostream>

int main()
{
	int i;
	int n;
	float sum;
	float numbers[100];
	i = 0;
	sum = 0;
	
	std::cin >> n;
	
	while ( i < n)
	{
		std::cin >> numbers[i];
		i = i+1;
	}
	
	i = 0;
	while ( i < n)
	{
		sum = sum + numbers[i];
		i = i+1;
	}
	
	std::cout << sum;
}