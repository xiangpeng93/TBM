// TBMServer.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"
#include "TBMServer.h"
#pragma comment(lib,"WS2_32.lib")
#include <WinSock2.h>
#include <iostream>
#include <thread>
#include <map>
#include <mutex>
#include <list>

#include "markup.h"
#include "sqlite3.h"

using namespace std;

#define EXPORT_API __stdcall
#define MAX_CONNECT 100

#define USER_DB "user.db"
#define CREATE_USER_DB_TABLE  "CREATE TABLE IF NOT EXISTS USER_INFO (id INTEGER PRIMARY KEY, USERNAME TEXT, USERCOUNT TEXT, USERPHONE TEXT, PASSWD TEXT, DB_PATH TEXT, REV TEXT)"
#define INSERT_USER_INO "INSERT INTO USER_INFO(USERNAME,PASSWD,DB_PATH) VALUES('%s','%s','%s');"
#define DELETE_USER_INO "DELETE FROM USER_INFO WHERE USERNAME='%s' and PASSWD='%s';"

//注册用户，添加使用
#define CREATE_COMMON_DB_TABLE "CREATE TABLE IF NOT EXISTS USER_DATA_INFO (id INTEGER PRIMARY KEY, USERNAME TEXT, USERCOUNT TEXT, USERPHONE TEXT, REV TEXT)"
#define CREATE_SHOP_INFO_TABLE "CREATE TABLE IF NOT EXISTS SHOP_DATA_INFO (id INTEGER PRIMARY KEY, USERNAME TEXT, USERCOUNT TEXT, USERPHONE TEXT)"
#define CREATE_HISTROY_DATA_TABLE "CREATE TABLE IF NOT EXISTS HISTORY_DATA_INFO (id INTEGER PRIMARY KEY, USERNAME TEXT, USERCOUNT TEXT, USERPHONE TEXT,SHOPNAME TEXT,COSTMONEY TEXT,COSTMONEYFORUSER TEXT,DATETIME datetime)"

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

struct HISTORY_DATA_INFO{
	string userName;
	string userCount;
	string userPhone;
	string SHOPNAME;
	string COSTMONEY;
	string COSTMONEYFORUSER;
	string DATETIME;
	string RECV;
};

map<string, user_info_struct> g_user_info_map;

int g_serverSock = -1;
bool g_isexit = false;

sqlite3 * g_db;
char* g_errMsg = 0;

thread* g_processThread;


void init_db();
sqlite3 *init_db_by_name(string name);
void close_db(sqlite3 *db);

bool isFindInUserInfoMap(string user_name, string user_pswd);


int DoRegister(string name, string pswd);
int DoCommonSql(string name, string pswd, string sql);
int get_all_user_info();

mutex g_mutex;


class AutoLock{
public:
	AutoLock(mutex *mex)
	{
		m_mutex = mex;
		m_mutex->lock();
	}
	~AutoLock()
	{
		m_mutex->unlock();
	}
private:
	mutex *m_mutex;
};

class UserConnect
{
public:
	UserConnect(const char* name)
	{
		m_user_name = name;
	};

	~UserConnect()
	{

	};
public:
	string GetUserConnectName()
	{
		return m_user_name;
	};

	int DoCommonSql(string name, string pswd, string sql)
	{
		if (isFindInUserInfoMap(name, pswd) == false)
			return -1;
		sqlite3 *db = init_db_by_name(name);
		int nRet = sqlite3_exec(db, sql.c_str(), NULL, NULL, &g_errMsg);
		if (nRet != SQLITE_OK)
		{
			cout << g_errMsg << endl;
			sqlite3_free(g_errMsg);
		}
		close_db(db);
		return 0;
	}
	int DoSelectSql(string name, string pswd, string sql)
	{
		if (isFindInUserInfoMap(name, pswd) == false)
			return -1;
		sqlite3 *db = init_db_by_name(name);
		char** pResult;
		int nRow;
		int nCol;
		int nResult = sqlite3_get_table(db, sql.c_str(), &pResult, &nRow, &nCol, &g_errMsg);
		if (nResult != SQLITE_OK)
		{
			cout << g_errMsg << endl;
			sqlite3_free(g_errMsg);
			return -1;
		}

		int nIndex = nCol;
		for (int i = 0; i < nRow; i++)
		{
			HISTORY_DATA_INFO tempInfo;
			for (int j = 0; j < nCol; j++)
			{
				string strOut;
				strOut += pResult[j];
				strOut += ":";
				if (pResult[nIndex] != NULL)
				{
					strOut += pResult[nIndex];
					
					if (strcmp(pResult[j], "USERNAME") == 0)
					{
						tempInfo.userName = pResult[nIndex];
					}
					else if (strcmp(pResult[j], "USERCOUNT") == 0)
					{
						tempInfo.userCount = pResult[nIndex];
					}
					else if (strcmp(pResult[j], "USERPHONE") == 0)
					{
						tempInfo.userPhone = pResult[nIndex];
					}
					else if (strcmp(pResult[j], "SHOPNAME") == 0)
					{
						tempInfo.SHOPNAME = pResult[nIndex];
					}
					else if (strcmp(pResult[j], "COSTMONEY") == 0)
					{
						tempInfo.COSTMONEY = pResult[nIndex];
					}
					else if (strcmp(pResult[j], "COSTMONEYFORUSER") == 0)
					{
						tempInfo.COSTMONEYFORUSER = pResult[nIndex];
					}
					else if (strcmp(pResult[j], "DATETIME") == 0)
					{
						tempInfo.DATETIME = pResult[nIndex];
					}
				}
				strOut += " ";

				++nIndex;
				cout << strOut.c_str();
			}
			cout << endl;

			AutoLock autoLock(&m_user_connect_mutex);
			m_list_callBack_info.push_back(tempInfo);
			cout << m_list_callBack_info.size() << endl;
		}
		sqlite3_free_table(pResult);  //使用完后务必释放为记录分配的内存，否则会内存泄漏
		close_db(db);
		return 0;
	};

	int GetMsg(char *userName, char *userCount, char *userPhone, char *shopName, char *costMoney, char *costMoneyForUser, char * dataTime)
	{
		if (m_list_callBack_info.size() > 0)
		{
			AutoLock autoLock(&m_user_connect_mutex);
			list<HISTORY_DATA_INFO>::iterator iter = m_list_callBack_info.begin();
			if (iter->userName.length() > 0)
				strcpy_s(userName, iter->userName.length() + 1, iter->userName.c_str());
			if (iter->userCount.length() > 0)
				strcpy_s(userCount, iter->userCount.length() + 1, iter->userCount.c_str());
			if (iter->userPhone.length() > 0)
				strcpy_s(userPhone, iter->userPhone.length() + 1, iter->userPhone.c_str());
			if (iter->SHOPNAME.length() > 0)
				strcpy_s(shopName, iter->SHOPNAME.length() + 1, iter->SHOPNAME.c_str());
			if (iter->COSTMONEY.length() > 0)
				strcpy_s(costMoney, iter->COSTMONEY.length() + 1, iter->COSTMONEY.c_str());
			if (iter->COSTMONEYFORUSER.length() > 0)
				strcpy_s(costMoneyForUser, iter->COSTMONEYFORUSER.length() + 1, iter->COSTMONEYFORUSER.c_str());
			if (iter->DATETIME.length() > 0)
				strcpy_s(dataTime, iter->DATETIME.length() + 1, iter->DATETIME.c_str());
			
			m_list_callBack_info.erase(iter);
			cout << m_list_callBack_info.size() << endl;
		}
		return 0;
	};
private:
	
private:
	string m_user_name;
	list<HISTORY_DATA_INFO> m_list_callBack_info;
	mutex m_user_connect_mutex;
	
};

class UserConnectManger
{
public:
	static UserConnectManger* GetInstance()
	{
		static UserConnectManger userManger;
		return &userManger;
	}
public:
	int AddUserConnect(int id, string name)
	{
		AutoLock autoLock(&m_user_connect_manger_mutex);
		if (m_mapUserConnect.find(id) == m_mapUserConnect.end())
		{
			UserConnect *usc = new UserConnect(name.c_str());
			m_mapUserConnect[id] = usc;
			return 0;
		}
		cout << "id is exist!" << id<< endl;
		return -1;
	};

	int DeleteUserConnectById(int id)
	{
		AutoLock autoLock(&m_user_connect_manger_mutex);
		if (m_mapUserConnect.find(id) != m_mapUserConnect.end())
		{
			delete m_mapUserConnect[id];
			m_mapUserConnect[id] = NULL;
			m_mapUserConnect.erase(m_mapUserConnect.find(id));
			return 0;
		}
		cout << "id not exist! "<<id << endl;
		return -1;
	};


	int DeleteUserConnectByName(string name)
	{
		AutoLock autoLock(&m_user_connect_manger_mutex);
		map<int, UserConnect*>::iterator iter = m_mapUserConnect.begin();
		for (; iter != m_mapUserConnect.end(); iter++)
		{
			if (iter->second->GetUserConnectName() == name)
			{
				delete iter->second;
				iter->second = NULL;
				m_mapUserConnect.erase(iter);
				return 0;
			}
		}
		cout << "name not exist! "<<name.c_str() << endl;
		return -1;
	};

	UserConnect *FindUserConnectById(int id)
	{
		if (m_mapUserConnect.find(id) != m_mapUserConnect.end())
		{
			return m_mapUserConnect[id];
		}
		cout << "id not exist! " << id << endl;
		return NULL;
	};

	UserConnect * FindUserConnectByName(string name)
	{
		map<int, UserConnect*>::iterator iter = m_mapUserConnect.begin();

		for (; iter != m_mapUserConnect.end(); iter++)
		{
			if (iter->second->GetUserConnectName() == name)
			{
				return iter->second;
			}
		}
		cout << "name not exist! " << name.c_str() << endl;
		return NULL;
	};
	

private:
	map<int, UserConnect*> m_mapUserConnect;
	mutex m_user_connect_manger_mutex;
};

bool isFindInUserInfoMap(string user_name, string user_pswd)
{
	map<string, user_info_struct>::iterator iter = g_user_info_map.begin();

	for (; iter != g_user_info_map.end(); iter++)
	{
		if (iter->second.name == user_name)
		{
			if (g_user_info_map[user_name].pswd == user_pswd)
			{
				return true;
			}
		}
	}
	return false;
}

string ProcessSelect(int clnt_sock, const char *buffer)
{
	cout << "ProcessSelect " << endl;
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
			//select from db
			//do select msg
			if (cXml.GetData() == "selectsql")
			{
				string cmd_value = "";
				if (cXml.FindElem("cmd_msg", true))
				{
					cmd_value = cXml.GetData();
					UserConnect *temp = UserConnectManger::GetInstance()->FindUserConnectByName(user_name);
					if (temp != NULL)
					{
						temp->DoSelectSql(user_name, user_pswd, cmd_value);
						send(clnt_sock, "success", strlen("success"), 0);
						return "success";
					}
				}
			}
			//get select msg 
			if (cXml.GetData() == "getmsg")
			{
				UserConnect *temp = UserConnectManger::GetInstance()->FindUserConnectByName(user_name);
				if (temp != NULL)
				{
					char tempInfo[7][1024] = { 0 };
					temp->GetMsg(tempInfo[0], tempInfo[1], tempInfo[2], tempInfo[3], tempInfo[4], tempInfo[5], tempInfo[6]);
					CMarkupSTL cXmlRsp;
					cXmlRsp.AddElem("username", tempInfo[0]);
					cXmlRsp.AddElem("usercount", tempInfo[1]);
					cXmlRsp.AddElem("userphone", tempInfo[2]);
					cXmlRsp.AddElem("shopname", tempInfo[3]);
					cXmlRsp.AddElem("costmoney", tempInfo[4]);
					cXmlRsp.AddElem("costmoneyforuser", tempInfo[5]);
					cXmlRsp.AddElem("datetime", tempInfo[6]);

					send(clnt_sock, cXmlRsp.GetDoc().c_str(), cXmlRsp.GetDoc().length(), 0);
					return "success";
				}
			}
		}
	}
	send(clnt_sock, "failed", strlen("failed"), 0);
	return "failed";
}

string ProcessCommonCmd(int clnt_sock, const char *buffer)
{
	cout << "ProcessCommonCmd " << endl;
	CMarkupSTL cXml;
	cXml.SetDoc(buffer);

	string user_name = "";
	string user_pswd = "";
	do{
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

				if (cXml.GetData() == "login")
				{
					if (isFindInUserInfoMap(user_name, user_pswd))
					{
						if (-1 == UserConnectManger::GetInstance()->AddUserConnect(clnt_sock, user_name))
						{
							send(clnt_sock, "this user is login.", strlen("this user is login."), 0);
							return "success";
						}
						send(clnt_sock, "success", strlen("success"), 0);
						return "success";
					}
				}

				if (cXml.GetData() == "logout")
				{
					UserConnectManger::GetInstance()->DeleteUserConnectByName(user_name);
					send(clnt_sock, "success", strlen("success"), 0);
					return "success";
				}


				//add user to db table
				//delete user to db table
				//add shop to db table 
				//delete shop to db table
				//add history data to db table
				//delete history data to db table
				if (cXml.GetData() == "commonsql")
				{
					string cmd_value = "";
					if (cXml.FindElem("cmd_msg", true))
					{
						cmd_value = cXml.GetData();
						UserConnect *temp = UserConnectManger::GetInstance()->FindUserConnectByName(user_name);
						if (temp != NULL)
						{
							temp->DoCommonSql(user_name, user_pswd, cmd_value);
							send(clnt_sock, "success", strlen("success"), 0);
							return "success";
						}
					}
				}


			}
		}
	} while (0);
	
	send(clnt_sock, "failed", strlen("failed"), 0);
	return "failed";
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
					if (str_len <= 0)//disconnect
					{
						FD_CLR(i, &freads);
						if (NULL != UserConnectManger::GetInstance()->FindUserConnectById(i))
						{
							UserConnectManger::GetInstance()->DeleteUserConnectById(i);
						}

						closesocket(i);
						cout << "disconnect client " << i << endl;
					}
					else if (str_len > 0)
					{
						//cout << "recv msg : " << buffer << endl;
						CMarkupSTL cXml;
						cXml.SetDoc(buffer);
						if (cXml.FindElem("info", true))
						{
							cXml.IntoElem();
							if (cXml.FindElem("cmd", true))
							{
								if (cXml.GetData() == "select")
								{
									cout << ProcessSelect(i, buffer).c_str();
								}
								else if (cXml.GetData() == "common")
								{
									cout << ProcessCommonCmd(i,buffer).c_str();
								}
							}
						}
						//send(i, buffer, str_len, 0);
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

	//获取一次信息
	get_all_user_info();
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
	sqlite3_close(g_db);

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
	sqlite3_exec(db, CREATE_SHOP_INFO_TABLE, NULL, NULL, &g_errMsg);
	sqlite3_exec(db, CREATE_HISTROY_DATA_TABLE, NULL, NULL, &g_errMsg);

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
		AutoLock autoLock(&g_mutex);
		g_user_info_map[tempStruct.name] = tempStruct;
		cout << endl;
	}
	sqlite3_free_table(pResult);  //使用完后务必释放为记录分配的内存，否则会内存泄漏
	return 0;
}

int DoRegister(string name,string pswd)
{
	int nResult = 0;
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
		sprintf(cmd, INSERT_USER_INO, name.c_str(), pswd.c_str(),name.c_str());
		nResult = sqlite3_exec(g_db, cmd, NULL, NULL, &g_errMsg);
		if (nResult != SQLITE_OK)
		{
			cout << g_errMsg << endl;
			sqlite3_free(g_errMsg);
			return -1;
		}
		cout << "register success . " << name.c_str() << " " << pswd.c_str();
	}
	//更新信息
	get_all_user_info();
	return 0;
}


