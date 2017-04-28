// TBMServerTest.cpp : 定义控制台应用程序的入口点。
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

