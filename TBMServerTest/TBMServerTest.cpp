// TBMServerTest.cpp : �������̨Ӧ�ó������ڵ㡣
//

#include "stdafx.h"
#include"../TBMServer/TBMServer.h"
#pragma comment (lib,"../debug/TBMServer")

#include <Windows.h>

int _tmain(int argc, _TCHAR* argv[])
{
	init_tbmServer();
	start_tbmServer(9090);
	Sleep(600000);
	fini_tbmServer();
	return 0;
}

