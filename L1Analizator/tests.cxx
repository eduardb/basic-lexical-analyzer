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
	

	if (a == b)
	{
		a = a - b;
	}
	else
		b = b - a;

	while ( i < n)
	{
		std::cin >> numbers[i];
		i = i+1;

		if (a>b)
			a = a - b;
		else
		{
			b = b - a;
		}

	}
	
	i = 0;
	while ( i < n)
	{
		sum = sum + numbers[i];
		i = i+1;
	}

	while ( i < n)
		i = i+1;
	
	std::cout << sum;
}