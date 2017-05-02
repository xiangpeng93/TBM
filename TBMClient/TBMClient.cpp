// TBMClient.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"
#include "TBMClient.h"
#pragma comment(lib,"WS2_32.lib")
#include <WinSock2.h>
#include <iostream>
#include <thread>

#include "markup.h"

using namespace std;

string g_userName;
string g_userPaswd;

int g_clntSock;

string assemblyMsg(const char *userName, const char *paswd, const char* cmd, const char *user_cmd, const char *cmd_msg)
{
	CMarkupSTL cXml;
	cXml.AddElem("info");
	cXml.IntoElem();
	cXml.AddElem("user_name", userName);
	cXml.AddElem("user_pswd", paswd);
	cXml.AddElem("cmd", cmd);
	cXml.AddElem("user_cmd", user_cmd);
	cXml.AddElem("cmd_msg", cmd_msg);
	return cXml.GetDoc();
}

/*
<info>
<user_name>xp335</user_name>
<user_pswd>2</user_pswd>
<cmd>common</cmd>
<user_cmd>login</user_cmd>
<cmd_msg></cmd_msg>
</info>
*/
int __stdcall Login(char *userName, char *userPaswd)
{
	g_userName = userName;
	g_userPaswd = userPaswd;
	string req = assemblyMsg(g_userName.c_str(), g_userPaswd.c_str(), "common", "login", "");
	int nRet = send(g_clntSock, req.c_str(), req.length(), 0);
	if (nRet != -1)
	{
		char buffer[1024 * 10] = { 0 };
		int bufferSize = 1024 * 10 - 1;
		nRet = recv(g_clntSock, buffer, bufferSize, 0);
		if(nRet > 0){
			cout << "buffer : " << buffer << endl;
			cout << "size : " << strlen(buffer) << endl;
		}
		else
		{
			cout << "nRet : " << nRet << endl;
		}
	}
	return 0;
};

int logout(char *userName, char *userPaswd)
{
	g_userName = "";
	g_userPaswd = "";
	return 0;
};

void __stdcall Init(char* ip,int port)
{
	int nRet = -1;
	WSAData wsData;
	nRet = WSAStartup(MAKEWORD(2, 2), &wsData);
	if (nRet != 0)
	{
		cout << "last error " << GetLastError() << endl;
	}
	
	g_clntSock = socket(PF_INET, SOCK_STREAM, 0);
	if (g_clntSock == INVALID_SOCKET)
	{
		cout << "invalid socket" << endl;
		nRet = INVALID_SOCKET;

		return;
	}
	SOCKADDR_IN clntAddr;
	clntAddr.sin_family = PF_INET;
	clntAddr.sin_addr.s_addr = inet_addr(ip);
	clntAddr.sin_port = htons(port);
	nRet = connect(g_clntSock, (sockaddr*)&clntAddr, sizeof(clntAddr));
	if (nRet != 0)
	{
		cout << "last error " << GetLastError() << endl;
	}
}

void __stdcall Fini()
{
	int nRet = -1;
	nRet = WSACleanup();
	if (nRet != 0)
	{
		cout << "last error " << GetLastError() << endl;
	}
	return;
}

void __stdcall Insert(char *sql)
{
	CommonSql(sql);
}

/*
<info>
<user_name>xp335</user_name>
<user_pswd>2</user_pswd>
<cmd>select</cmd>
<user_cmd>selectsql</user_cmd>
<cmd_msg>select * from HISTORY_DATA_INFO</cmd_msg>
</info>
*/
void  __stdcall Select(char *sql)
{
	string req = assemblyMsg(g_userName.c_str(), g_userPaswd.c_str(), "select", "selectsql", sql);
	int nRet = send(g_clntSock, req.c_str(), req.length(), 0);
	int bufferSize = 1024 * 10 - 1;
	if (nRet != -1)
	{
		char buffer[1024 * 10] = { 0 };
		nRet = recv(g_clntSock, buffer, bufferSize, 0);
		if (nRet > 0){
			cout << "buffer : " << buffer << endl;
			cout << "size : " << strlen(buffer) << endl;
		}
		else
		{
			cout << "nRet : " << nRet << endl;
		}
	}
}

void  __stdcall Select2(char *sql)
{
	Select(sql);
}

void __stdcall Delete(char *sql)
{
	CommonSql(sql);

}

void __stdcall Delete2(char *sql)
{
	CommonSql(sql);
}

void __stdcall CommonSql(char *sql)
{

}

void __stdcall GetMsg(char *userName, char *userCount, char *userPhone)
{

}

/*
<info>
<user_name>xp335</user_name>
<user_pswd>2</user_pswd>
<cmd>select</cmd>
<user_cmd>getmsg</user_cmd>
</info>
*/

void __stdcall GetMsg2(char *userName, char *userCount, char *userPhone, char *shopName, char *costMoney, char *costMoneyForUser, char * dataTime)
{
	string req = assemblyMsg(g_userName.c_str(), g_userPaswd.c_str(), "select", "getmsg", "");

	int nRet = send(g_clntSock, req.c_str(), req.length(), 0);
	int bufferSize = 1024 * 10 - 1;
	if (nRet != -1)
	{
		char buffer[1024 * 10] = { 0 };
		nRet = recv(g_clntSock, buffer, bufferSize, 0);
		if (nRet > 0){
			cout << "buffer : " << buffer << endl;
			cout << "size : " << strlen(buffer) << endl;

			CMarkupSTL cXml;
			cXml.SetDoc(buffer);
			if (cXml.FindElem("username"))
			{
				strcpy(userName, cXml.GetData().c_str()); 
			}
			if (cXml.FindElem("usercount"))
			{
				strcpy(userCount, cXml.GetData().c_str());
			}
			if (cXml.FindElem("userphone"))
			{
				strcpy(userPhone, cXml.GetData().c_str());
			}
			if (cXml.FindElem("shopname"))
			{
				strcpy(shopName, cXml.GetData().c_str());
			}
			if (cXml.FindElem("costmoney"))
			{
				strcpy(costMoney, cXml.GetData().c_str());
			}
			if (cXml.FindElem("costmoneyforuser"))
			{
				strcpy(costMoneyForUser, cXml.GetData().c_str());
			}
			if (cXml.FindElem("datetime"))
			{
				strcpy(dataTime, cXml.GetData().c_str());
			}
		}
		else
		{
			cout << "nRet : " << nRet << endl;
		}
	}
}

