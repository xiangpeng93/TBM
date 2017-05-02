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


/*
for Test

<info>
<user_name>xp335</user_name>
<user_pswd>2</user_pswd>
<cmd>common</cmd>
<user_cmd>register</user_cmd>
<cmd_msg></cmd_msg>
</info>

<info>
<user_name>xp335</user_name>
<user_pswd>2</user_pswd>
<cmd>common</cmd>
<user_cmd>login</user_cmd>
<cmd_msg></cmd_msg>
</info>


<info>
<user_name>xp335</user_name>
<user_pswd>2</user_pswd>
<cmd>select</cmd>
<user_cmd>selectsql</user_cmd>
<cmd_msg>select * from HISTORY_DATA_INFO</cmd_msg>
</info>

<info>
<user_name>xp335</user_name>
<user_pswd>2</user_pswd>
<cmd>select</cmd>
<user_cmd>getmsg</user_cmd>
</info>

*/