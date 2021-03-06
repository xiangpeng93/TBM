// TBMClient.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"
#include "TBMClient.h"
#pragma comment(lib,"WS2_32.lib")
#include <WinSock2.h>
#include <iostream>
#include <thread>
#include <mutex>
#include <list>
#include "markup.h"

//#define DEBUG 1
using namespace std;

string g_userName;
string g_userPaswd;

string g_ip;
int g_port = 0;

int g_clntSock = -1;
mutex g_mutex;
bool g_isFirstConnect = true;

void initSocket();
typedef struct msgRecvStruct
{
	string userName;
	string userCount;
	string userPhone;
	string shopName;
	string costMoney;
	string costMoneyForUser;
	string dataTime;
};
list<msgRecvStruct> g_lRecvMsg;
string GetMsgRspBuffer;

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
<user_cmd>register</user_cmd>
<cmd_msg></cmd_msg>
</info>
*/
int _stdcall Register(char* ip, int port, char *userName, char *userPaswd,char *cmd)
{
	cout << "register " << endl;
	int nRet = 0;
	g_userName = userName;
	g_userPaswd = userPaswd;
	g_ip = ip;
	g_port = port;
	if (g_clntSock == -1)
	{
		initSocket();
		SOCKADDR_IN clntAddr;
		clntAddr.sin_family = PF_INET;
		clntAddr.sin_addr.s_addr = inet_addr(g_ip.c_str());
		clntAddr.sin_port = htons(g_port);
		nRet = connect(g_clntSock, (sockaddr*)&clntAddr, sizeof(clntAddr));
		if (nRet != 0)
		{
			cout << "last error " << GetLastError() << endl;
			return 400;
		}
	}

	string req = assemblyMsg(g_userName.c_str(), g_userPaswd.c_str(), "common", "register", "");
	nRet = send(g_clntSock, req.c_str(), req.length() + 1, 0);
	cout << "send buffer :" << req.c_str() << endl;
	if (nRet != -1)
	{
		char buffer[1500] = { 0 };
		int bufferSize = 1500 - 1;
		nRet = recv(g_clntSock, buffer, bufferSize, 0);
		if (nRet > 0){
			cout << "recv buffer : " << buffer << endl;
			if (strcmp(buffer, "success") == 0)
			{
				nRet = 200;
			}
			else
			{
				nRet = - 1;
			}
		}
		else
		{
			cout << "nRet : " << nRet << endl;
		}
	}
	closesocket(g_clntSock);
	g_clntSock = -1;
	return nRet;
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

int __stdcall Login(char* ip, int port,char *userName, char *userPaswd)
{
	int nRet = 400;
	g_userName = userName;
	g_userPaswd = userPaswd;
	g_ip = ip;
	g_port = port;
	if (g_clntSock == -1)
	{
		initSocket();
		SOCKADDR_IN clntAddr;
		clntAddr.sin_family = PF_INET;
		clntAddr.sin_addr.s_addr = inet_addr(g_ip.c_str());
		clntAddr.sin_port = htons(g_port);
		nRet = connect(g_clntSock, (sockaddr*)&clntAddr, sizeof(clntAddr));
		if (nRet != 0)
		{
			cout << "last error " << GetLastError() << endl;
			return 400;
		}
	}

	string req = assemblyMsg(g_userName.c_str(), g_userPaswd.c_str(), "common", "login", "");
	cout << req.c_str() << endl;
	nRet = send(g_clntSock, req.c_str(), req.length() + 1, 0);
	if (nRet != -1)
	{
		char buffer[1500] = { 0 };
		int bufferSize = 1500 - 1;
		nRet = recv(g_clntSock, buffer, bufferSize, 0);
		if (nRet > 0){
			cout << "recv :" << buffer<<endl;
			if (strcmp(buffer, "success") == 0)
			{
				return 200;
			}
			else if (strcmp(buffer, "timeout") == 0)
			{
				nRet  = 1;
			}
			else
			{
				nRet = - 1;
			}
		}
		else
		{
			cout << "nRet : " << nRet << endl;
		}
	}
	return nRet;
};

int logout(char *userName, char *userPaswd)
{
	g_userName = "";
	g_userPaswd = "";
	return 0;
};

void initSocket()
{
	g_clntSock = socket(PF_INET, SOCK_STREAM, 0);
	if (g_clntSock == INVALID_SOCKET)
	{
		cout << "invalid socket" << endl;
		return;
	}
	int timeout = 0; //3s
	setsockopt(g_clntSock, SOL_SOCKET, SO_SNDTIMEO, (const char*)&timeout, sizeof(timeout));
	setsockopt(g_clntSock, SOL_SOCKET, SO_RCVTIMEO, (const char*)timeout, sizeof(timeout));
}
void __stdcall Init()
{
	int nRet = -1;
	WSAData wsData;
	nRet = WSAStartup(MAKEWORD(2, 2), &wsData);
	if (nRet != 0)
	{
		cout << "last error " << GetLastError() << endl;
	}
	
}

void __stdcall Fini()
{
	int nRet = -1;
	if (g_clntSock != -1)
		closesocket(g_clntSock);
	nRet = WSACleanup();
	if (nRet != 0)
	{
		cout << "last error " << GetLastError() << endl;
	}
	return;
}

void reconnect()
{
	Fini();
	Init();
	Login((char*)g_ip.c_str(), g_port, (char*)g_userName.c_str(), (char*)g_userPaswd.c_str());
};


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
<cmd_msg>select * from HISTORYDATA</cmd_msg>
</info>
*/
void  __stdcall Select(char *sql)
{
	string req = assemblyMsg(g_userName.c_str(), g_userPaswd.c_str(), "select", "selectsql", sql);
	int nRet = send(g_clntSock, req.c_str(), req.length(), 0);
	int bufferSize = 1500 - 1;
	if (nRet != -1)
	{
		char buffer[1500] = { 0 };
		nRet = recv(g_clntSock, buffer, bufferSize, 0);
		if (nRet > 0){
#ifdef DEBUG
			cout << "buffer : " << buffer << endl;
			cout << "size : " << strlen(buffer) << endl;
#endif
		}
		else
		{
			cout << "nRet : " << nRet << endl;
		}
	}
	else
	{
		cout << "nRet : " << nRet << endl;
		reconnect();
		return ;
	}

	req = assemblyMsg(g_userName.c_str(), g_userPaswd.c_str(), "select", "getmsg", "");
	nRet = send(g_clntSock, req.c_str(), req.length(), 0);
	if (nRet == -1)
	{
		cout << "nRet : " << nRet << endl;
		reconnect();
		return ;
	}
	do{
		char buffer[1500] = { 0 };
		nRet = recv(g_clntSock, buffer, bufferSize, 0);
		if (nRet == -1)
		{
			cout << "nRet : " << nRet << endl;
			reconnect();
			return;
		}
		GetMsgRspBuffer += buffer;
#ifdef DEBUG

		cout << "buffer : " << GetMsgRspBuffer << endl;
		cout << "size : " << strlen(GetMsgRspBuffer.c_str()) << endl;
#endif
	} while (GetMsgRspBuffer.find("<info>") == -1 || GetMsgRspBuffer.find("</info>") == -1);
	//cout << "size : " << (GetMsgRspBuffer.c_str()) << endl;
	g_mutex.lock();

	CMarkupSTL cXml;
	cXml.SetDoc(GetMsgRspBuffer.c_str());
	GetMsgRspBuffer = "";
	cXml.FindElem("info");
	cXml.IntoElem();
	while (cXml.FindElem("username"))
	{
		msgRecvStruct tMsg;
		tMsg.userName = cXml.GetData().c_str();
		//std::cout << tMsg.userName.c_str() << endl;

		if (cXml.FindElem("usercount"))
		{
			tMsg.userCount = cXml.GetData().c_str();
		}
		if (cXml.FindElem("userphone"))
		{
			tMsg.userPhone = cXml.GetData().c_str();
		}
		if (cXml.FindElem("shopname"))
		{
			tMsg.shopName = cXml.GetData().c_str();
		}
		if (cXml.FindElem("costmoney"))
		{
			tMsg.costMoney = cXml.GetData().c_str();
		}
		if (cXml.FindElem("costmoneyforuser"))
		{
			tMsg.costMoneyForUser = cXml.GetData().c_str();
		}
		if (cXml.FindElem("datetime"))
		{
			tMsg.dataTime = cXml.GetData().c_str();
		}
		if (!tMsg.userName.empty())
			g_lRecvMsg.push_back(tMsg);

		//std::cout << g_lRecvMsg.size() << endl;
	};
	g_mutex.unlock();

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

/*
<info>
<user_name>xp335</user_name>
<user_pswd>2</user_pswd>
<cmd>common</cmd>
<user_cmd>commonsql</user_cmd>
<cmd_msg>select * from HISTORYDATA</cmd_msg>
</info>
*/
void __stdcall CommonSql(char *sql)
{
	string req = assemblyMsg(g_userName.c_str(), g_userPaswd.c_str(), "common", "commonsql", sql);
	int nRet = send(g_clntSock, req.c_str(), req.length(), 0);
	int bufferSize = 1500 - 1;
	if (nRet != -1)
	{
		char buffer[1500] = { 0 };
		nRet = recv(g_clntSock, buffer, bufferSize, 0);
		if (nRet > 0){
#if DEBUG
			cout << "buffer : " << buffer << endl;
			cout << "size : " << strlen(buffer) << endl;
#endif
		}
		else
		{
			cout << "nRet : " << nRet << endl;
		}
	}
	if (nRet == -1)
	{
		cout << "nRet : " << nRet << endl;
		reconnect();
		return;
	}
}

void __stdcall GetMsg(char *userName, char *userCount, char *userPhone)
{
	GetMsg2(userName, userCount, userPhone, NULL, NULL, NULL, NULL);
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
	g_mutex.lock();
	if (g_lRecvMsg.size() > 0)
	{
		if (userName != NULL)
		strcpy(userName, g_lRecvMsg.begin()->userName.c_str());
		if (userCount != NULL)
			strcpy(userCount, g_lRecvMsg.begin()->userCount.c_str());
		if (userPhone != NULL)
			strcpy(userPhone, g_lRecvMsg.begin()->userPhone.c_str());
		if (shopName != NULL)
			strcpy(shopName, g_lRecvMsg.begin()->shopName.c_str());
		if (costMoney != NULL)
			strcpy(costMoney, g_lRecvMsg.begin()->costMoney.c_str());
		if (costMoneyForUser != NULL)
			strcpy(costMoneyForUser, g_lRecvMsg.begin()->costMoneyForUser.c_str()); 
		if (dataTime != NULL)
		{
			strcpy(dataTime, g_lRecvMsg.begin()->dataTime.c_str());
			//cout << dataTime << endl;
		}
		g_lRecvMsg.erase(g_lRecvMsg.begin());

	}
	g_mutex.unlock();

	/*string req = assemblyMsg(g_userName.c_str(), g_userPaswd.c_str(), "select", "getmsg", "");

	int nRet = send(g_clntSock, req.c_str(), req.length(), 0);
	int bufferSize = 1500 - 1;
	if (nRet != -1)
	{*/
		/*do{
			char buffer[1500] = { 0 };
			nRet = recv(g_clntSock, buffer, bufferSize, 0);
			GetMsgRspBuffer += buffer;
#ifdef DEBUG

			cout << "buffer : " << GetMsgRspBuffer << endl;
			cout << "size : " << strlen(GetMsgRspBuffer.c_str()) << endl;
#endif
		} while (GetMsgRspBuffer.find("<info>") == -1 || GetMsgRspBuffer.find("</info>") == -1);
		

		CMarkupSTL cXml;
		cXml.SetDoc(GetMsgRspBuffer.c_str());
		GetMsgRspBuffer = "";
		cXml.FindElem("info");
		cXml.IntoElem();
		if (cXml.FindElem("username") && userName != NULL)
		{
			strcpy(userName, cXml.GetData().c_str());
		}
		if (cXml.FindElem("usercount") && userCount != NULL)
		{
			strcpy(userCount, cXml.GetData().c_str());
		}
		if (cXml.FindElem("userphone") && userPhone != NULL)
		{
			strcpy(userPhone, cXml.GetData().c_str());
		}
		if (cXml.FindElem("shopname") && shopName != NULL)
		{
			strcpy(shopName, cXml.GetData().c_str());
		}
		if (cXml.FindElem("costmoney") && costMoney != NULL)
		{
			strcpy(costMoney, cXml.GetData().c_str());
		}
		if (cXml.FindElem("costmoneyforuser") && costMoneyForUser != NULL)
		{
			strcpy(costMoneyForUser, cXml.GetData().c_str());
		}
		if (cXml.FindElem("datetime") && dataTime != NULL)
		{
			strcpy(dataTime, cXml.GetData().c_str());
		}*/
	//}
}

