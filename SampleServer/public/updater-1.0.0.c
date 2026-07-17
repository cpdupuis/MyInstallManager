#include <stdio.h>

int wmain(int argc, wchar_t **argv)
{
	wprintf(L"This is the latest iteration of the updater!\n"
		L"(i.e. the self-update process is complete)\n");

	for (int i = 1; i < argc; ++i)
		wprintf(L"argv[%d] = '%ls'\n", i, argv[i]);

	return 0;
}
