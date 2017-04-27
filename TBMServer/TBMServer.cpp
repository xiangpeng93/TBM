// TBMServer.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"
#include "TBMServer.h"
#pragma comment(lib,"WS2_32.lib")
#include <WinSock2.h>
#include <iostream>
#include <thread>
#include <map>

#include "markup.h"
#include "sqlite3.h"

using namespace std;

#define EXPORT_API __stdcall
#define MAX_CONNECT 100

#define USER_DB "user.db"
#define CREATE_USER_DB_TABLE  "CREATE TABLE IF NOT EXISTS USER_INFO (id INTEGER PRIMARY KEY, USERNAME TEXT, USERCOUNT TEXT, USERPHONE TEXT, PASSWD TEXT, DB_PATH TEXT, REV TEXT)"
#define INSERT_USER_INO "INSERT INTO USER_INFO(USERNAME,PASSWD) VALUES('%s' , '%s');"
#define DELETE_USER_INO "DELETE FROM USER_INFO WHERE USERNAME='%s' and PASSWD='%s';"

//注册用户，添加使用
#define CREATE_COMMON_DB_TABLE "CREATE TABLE IF NOT EXISTS USER_DATA_INFO (id INTEGER PRIMARY KEY, USERNAME TEXT, USERCOUNT TEXT, USERPHONE TEXT, PASSWD TEXT, REV TEXT)"

#define SELECT_FROM_USER_DB_TABLE "select * from user_table order by name"

struct user_info_struct{
	string id;
	string name;
	string count;
	string phone;
	string pswd;
	string dbpath;
	string recv;
};

map<string, user_info_struct> g_user_info_map;

int g_serverSock = -1;
bool g_isexit = false;

sqlite3 * g_db;
char* g_errMsg = 0;

thread* g_processThread;


void init_db();
sqlite3 *init_db_by_name(string name);
int DoRegister(string name , string pswd);


string ProcessSelect(const char *cmd)
{
	cout << "cmd value : " << cmd << endl;
	return "";
}

string ProcessCommonCmd(const char *buffer)
{
	cout << "ProcessCommonCmd " << endl;
	CMarkupSTL cXml;
	cXml.SetDoc(buffer);

	string user_name = "";
	string user_pswd = "";
	if (cXml.FindElem("info", true))
	{
		cXml.IntoElem();
		if (cXml.FindElem("user_name", true))
		{
			user_name = cXml.GetData();
		}
		if (cXml.FindElem("user_pswd", true))
		{
			user_pswd = cXml.GetData();
		}
		if (cXml.FindElem("user_cmd", true))
		{
			if (cXml.GetData() == "register")
			{
				DoRegister(user_name, user_pswd);
			}
		}
	}
	return "";
}

void ProcessMsg()
{
	fd_set freads, temps;

	int  fd_max, fd_num;
	timeval timeout;
	FD_ZERO(&freads);
	FD_ZERO(&temps);

	FD_SET(g_serverSock, &freads);
	fd_max = g_serverSock;
	timeout.tv_sec = 5;
	timeout.tv_usec = 5000;
	while (!g_isexit)
	{
		temps = freads;
		if ((fd_num = select(fd_max + 1, &temps, 0, 0, &timeout)) == -1)
		{
			cout << "select error -1" << endl;
			break;
		}
		if (fd_num == 0)
			continue;
		for (int i = 0; i < fd_max + 1; i++)
		{
			if (FD_ISSET(i, &temps))
			{
				if (i == g_serverSock) //connection 
				{
					SOCKADDR_IN clntAddr;
					int clntAddrSz = sizeof(SOCKADDR_IN);
					int clnt_sock = accept(g_serverSock, (sockaddr *)&clntAddr, &clntAddrSz);
					FD_SET(clnt_sock, &freads);
					if (fd_max < clnt_sock)
					{
						fd_max = clnt_sock;
					}

					string ipAddr = inet_ntoa(clntAddr.sin_addr);
					cout << "new client . Ip : " << ipAddr.c_str() << " Port : " << htons(clntAddr.sin_port) << endl;
				}
				else
				{
					char buffer[1024*100] = { 0 };
					int str_len = recv(i, buffer, 100*1024 - 1, 0);
					if (str_len == 0)//disconnect
					{
						FD_CLR(i, &freads);
						closesocket(i);
						cout << "disconnect client " << i << endl;
					}
					else
					{
						cout << "recv msg : " << buffer << endl;
						CMarkupSTL cXml;
						cXml.SetDoc(buffer);
						if (cXml.FindElem("info", true))
						{
							cXml.IntoElem();
							if (cXml.FindElem("cmd", true))
							{
								if (cXml.GetData() == "select")
								{
									ProcessSelect(buffer);
								}
								else if (cXml.GetData() == "common")
								{
									ProcessCommonCmd(buffer);
								}
							}
						}
						send(i, buffer, str_len, 0);
					}
				}
			}
		}
	}
}

int __stdcall init_tbmServer()
{
	int nRet = -1;
	WSAData wsData;
	nRet = WSAStartup(MAKEWORD(2, 2), &wsData);
	if (nRet != 0)
	{
		cout << "last error " << GetLastError() << endl;
	}
	init_db();
	return nRet;
}

int __stdcall start_tbmServer(int port)
{
	int nRet = -1;
	g_serverSock = socket(PF_INET, SOCK_STREAM, 0);
	if (g_serverSock == INVALID_SOCKET)
	{
		cout << "invalid socket" << endl;
		nRet = INVALID_SOCKET;

		return nRet;
	}
	SOCKADDR_IN serverAddr;
	serverAddr.sin_family = PF_INET;
	serverAddr.sin_addr.s_addr = htonl(INADDR_ANY);
	serverAddr.sin_port = htons(port);

	if (SOCKET_ERROR == ::bind(g_serverSock, (const sockaddr*)&serverAddr, sizeof(SOCKADDR_IN)))
	{
		cout << "bind SockError" << endl;
		nRet = SOCKET_ERROR;

		return nRet;
	}
	if (SOCKET_ERROR == listen(g_serverSock, MAX_CONNECT))
	{
		cout << "listen SockError" << endl;
		nRet = SOCKET_ERROR;

		return nRet;
	}
	nRet = 0;
	g_processThread = new thread(ProcessMsg);
	g_processThread->detach();
	return nRet;
}

int __stdcall fini_tbmServer()
{
	int nRet = -1;
	nRet = WSACleanup();
	if (nRet != 0)
	{
		cout << "last error " << GetLastError() << endl;
	}
	g_isexit = true;
	delete g_processThread;
	return nRet;
}

void init_db()
{
	char chpath[256];
	GetModuleFileNameA(NULL, (char*)chpath, sizeof(chpath));
	string dataPath = chpath;
	int status = dataPath.find_last_of("\\");
	dataPath.erase(status);

	dataPath += "\\";
	dataPath += USER_DB;
	sqlite3_open(dataPath.c_str(), &g_db);
	sqlite3_exec(g_db, CREATE_USER_DB_TABLE, NULL, NULL, &g_errMsg);
	
	sqlite3_exec(g_db, "PRAGMA synchronous = OFF", NULL, NULL, &g_errMsg);
	sqlite3_exec(g_db, "PRAGMA journal_mode = MEMORY", NULL, NULL, &g_errMsg);
	DoRegister("1234","456");
}

sqlite3 *init_db_by_name(string name)
{
	sqlite3 *db;
	char chpath[256];
	GetModuleFileNameA(NULL, (char*)chpath, sizeof(chpath));
	string dataPath = chpath;
	int status = dataPath.find_last_of("\\");
	dataPath.erase(status);

	dataPath += "\\";
	dataPath += name;
	sqlite3_open(dataPath.c_str(), &db);
	sqlite3_exec(db, CREATE_COMMON_DB_TABLE, NULL, NULL, &g_errMsg);

	sqlite3_exec(db, "PRAGMA synchronous = OFF", NULL, NULL, &g_errMsg);
	sqlite3_exec(db, "PRAGMA journal_mode = MEMORY", NULL, NULL, &g_errMsg);
	return db;
}

void close_db(sqlite3 *db)
{
	sqlite3_close(db);
};

int get_all_user_info()
{
	char** pResult;
	int nRow;
	int nCol;
	int nResult = sqlite3_get_table(g_db, "select * from USER_INFO;", &pResult, &nRow, &nCol, &g_errMsg);
	if (nResult != SQLITE_OK)
	{
		sqlite3_close(g_db);
		cout << g_errMsg << endl;
		sqlite3_free(g_errMsg);
		return -1;
	}


	int nIndex = nCol;
	for (int i = 0; i < nRow; i++)
	{
		user_info_struct tempStruct;
		for (int j = 0; j < nCol; j++)
		{
			string strOut;
			strOut += pResult[j];
			strOut += ":";
			if (pResult[nIndex] != NULL)
				strOut += pResult[nIndex];
			strOut += "  ";

			if (strcmp(pResult[j], "id") == 0)
			{
				if (pResult[nIndex] != NULL)
					tempStruct.id = pResult[nIndex];
			}
			else if (strcmp(pResult[j], "USERNAME") == 0)
			{
				if (pResult[nIndex] != NULL)
					tempStruct.name = pResult[nIndex];
			}
			else if (strcmp(pResult[j], "USERCOUNT") == 0)
			{
				if (pResult[nIndex] != NULL)
					tempStruct.count = pResult[nIndex];
			}
			else if (strcmp(pResult[j], "USERPHONE") == 0)
			{
				if (pResult[nIndex] != NULL)
					tempStruct.phone = pResult[nIndex];
			}
			else if (strcmp(pResult[j], "PASSWD") == 0)
			{
				if (pResult[nIndex] != NULL)
					tempStruct.pswd = pResult[nIndex];
			}
			else if (strcmp(pResult[j], "DB_PATH") == 0)
			{
				if (pResult[nIndex] != NULL)
					tempStruct.dbpath = pResult[nIndex];
			}
			else if (strcmp(pResult[j], "REV") == 0)
			{
				if (pResult[nIndex] != NULL)
					tempStruct.recv = pResult[nIndex];
			}

			++nIndex;
			cout << strOut.c_str();
		}

		g_user_info_map[tempStruct.name] = tempStruct;
		cout << endl;
	}
	sqlite3_free_table(pResult);  //使用完后务必释放为记录分配的内存，否则会内存泄漏
}

int DoRegister(string name,string pswd)
{
	get_all_user_info();
	if (name.empty())
	{
		cout << "name is empty." << endl;
		return -100;
	}
	if (g_user_info_map.find(name) != g_user_info_map.end())
	{
		cout << "this user was register!" << endl;
		return -1;
	}
	else
	{
		char cmd[1024] = { 0 };
		sprintf(cmd, INSERT_USER_INO, name.c_str(), pswd.c_str());
		sqlite3_exec(g_db, cmd, NULL, NULL, &g_errMsg);
		cout << "register success ." << name.c_str() << " " << pswd.c_str();
	}
	
	return 0;
}