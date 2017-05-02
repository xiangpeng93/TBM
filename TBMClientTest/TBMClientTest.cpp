// TBMClientTest.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include"../TBMClient/TBMClient.h"
#pragma comment (lib,"../debug/TBMClient")

#include <Windows.h>
#include <iostream>
using namespace std;

int _tmain(int argc, _TCHAR* argv[])
{
	Init("127.0.0.1",9090);
	Login("xp335", "2");
	//Select("select * from HISTORY_DATA_INFO");
	//Select("select * from SHOP_DATA_INFO");
	Select("select * from USER_DATA_INFO");

	char tempInfo[7][1024] = { 0 };
	do
	{
		memset(tempInfo, 0, sizeof(tempInfo));
		GetMsg2(tempInfo[0], tempInfo[1], tempInfo[2], tempInfo[3], tempInfo[4], tempInfo[5], tempInfo[6]);
	} while (strcmp(tempInfo[0] , "") != 0);
	cout << "getmsg ednl" << endl;
	Sleep(600000);
	return 0;
}

