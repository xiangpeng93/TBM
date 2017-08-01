// TBMServer.cpp : ���� DLL Ӧ�ó���ĵ���������
//

#include "stdafx.h"
#include "TBMServer.h"

#ifdef WIN32
#pragma comment(lib,"WS2_32.lib")
#include <WinSock2.h>
#else
#include <sys/types.h>
#include <sys/socket.h>
#include <unistd.h>
#define SOCKET_ERROR -1
#endif

#include <iostream>
#include <thread>
#include <map>
#include <mutex>
#include <list>

#include "markup.h"
#include "sqlite3.h"

using namespace std;

//#define DEBUG 1
#define EXPORT_API __stdcall
#define MAX_CONNECT 100

#define USER_DB "user.db"
#define CREATE_USER_DB_TABLE  "CREATE TABLE IF NOT EXISTS USER_INFO (id INTEGER PRIMARY KEY, USERNAME TEXT, USERCOUNT TEXT, USERPHONE TEXT, PASSWD TEXT, DB_PATH TEXT, REV TEXT)"
#define INSERT_USER_INO "INSERT INTO USER_INFO(USERNAME,PASSWD,DB_PATH,REV) VALUES('%s','%s','%s','%s');"
#define DELETE_USER_INO "DELETE FROM USER_INFO WHERE USERNAME='%s' and PASSWD='%s';"

//ע���û������ʹ��
#define CREATE_COMMON_DB_TABLE "CREATE TABLE IF NOT EXISTS USERINFO (id INTEGER PRIMARY KEY, USERNAME TEXT, USERCOUNT TEXT, USERPHONE TEXT)"
#define CREATE_SHOP_INFO_TABLE "CREATE TABLE IF NOT EXISTS SHOPINFO  (id INTEGER PRIMARY KEY, USERNAME TEXT, USERCOUNT TEXT, USERPHONE TEXT)"
#define CREATE_HISTROY_DATA_TABLE "CREATE TABLE IF NOT EXISTS HISTORYDATA  (id INTEGER PRIMARY KEY, USERNAME TEXT, USERCOUNT TEXT, USERPHONE TEXT,SHOPNAME TEXT,COSTMONEY TEXT,COSTMONEYFORUSER TEXT,DATETIME datetime)"

#define SELECT_FROM_USER_DB_TABLE "select * from user_table order by name"

void closeSockById(int i);

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
void ProcessSelectThread(void * uscn, string name, string pswd, string sql);

mutex g_mutex;
class UserConnect;

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
		m_isSelectSuccess = false;
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
		//ProcessSelectThread(this,name, pswd, sql);
		thread tmpProcessSelect(ProcessSelectThread, this, name, pswd, sql);
		tmpProcessSelect.detach();
		return 0;
	};

	int GetMsg(char *userName, char *userCount, char *userPhone, char *shopName, char *costMoney, char *costMoneyForUser, char * dataTime)
	{
		//cout << "start search msg." << endl;
		AutoLock autoLock(&m_user_connect_mutex);
		time_t tbegin = time(0);
		static int tNum = 0;
		while (m_isSelectSuccess == false)
		{
			Sleep(10);
			tNum++;
			if (tNum > 100)
			{
				tNum = 0;
				return 0;

			}
		}
		if (m_list_callBack_info.size() > 0)
		{
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
#ifdef DEBUG
			cout << m_list_callBack_info.size() << endl;
#endif
		}
		else
		{
			m_isSelectSuccess = false; 
		}

		time_t tend = time(0);
		//cout << "get msg cost : " << tend - tbegin << " second" << endl;

		return 0;
	};
private:
	
public:
	string m_user_name;
	list<HISTORY_DATA_INFO> m_list_callBack_info;
	mutex m_user_connect_mutex;
	bool m_isSelectSuccess;
};

void FormatTime(time_t time1 , char *szTime)
{
	struct tm tm1;


#ifdef WIN32  
	tm1 = *localtime(&time1);
#else  
	localtime_r(&time1 , &tm1);
#endif  
	sprintf(szTime , "%4.4d-%2.2d-%2.2d %2.2d:%2.2d:%2.2d " ,
		tm1.tm_year + 1900 , tm1.tm_mon + 1 , tm1.tm_mday ,
		tm1.tm_hour , tm1.tm_min , tm1.tm_sec);
}



void ProcessSelectThread(void * uscn, string name, string pswd, string sql)
{
	//cout << "start search msg." << endl;
	time_t tbegin = time(0);
	UserConnect *usercnt = (UserConnect *)uscn;
	AutoLock autoLock(&usercnt->m_user_connect_mutex);
	usercnt->m_list_callBack_info.clear();

	if (isFindInUserInfoMap(name, pswd) == false)
		return;
	sqlite3 *db = init_db_by_name(name);
	char** pResult;
	int nRow;
	int nCol;
	int nResult = sqlite3_get_table(db, sql.c_str(), &pResult, &nRow, &nCol, &g_errMsg);
	if (nResult != SQLITE_OK)
	{
		//cout << g_errMsg << endl;
		sqlite3_free(g_errMsg);
		return;
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
#ifdef DEBUG
			cout << strOut.c_str();
#endif
		}
#ifdef DEBUG
		cout << endl;
#endif
		usercnt->m_list_callBack_info.push_back(tempInfo);
		usercnt->m_isSelectSuccess = true;
#ifdef DEBUG
		cout << usercnt->m_list_callBack_info.size() << endl;
#endif
	}
	sqlite3_free_table(pResult);  //ʹ���������ͷ�Ϊ��¼������ڴ棬������ڴ�й©
	close_db(db);

	time_t tend = time(0);
	//cout << "select cost : " << tend - tbegin << " second" << endl;
}

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
		cout << "m_mapUserConnect size " << m_mapUserConnect.size() << endl;
		if (m_mapUserConnect.find(id) == m_mapUserConnect.end())
		{
			UserConnect *usc = new UserConnect(name.c_str());
			m_mapUserConnect.insert(pair<int, UserConnect *>(id, usc));
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
	
	void CheckConnectAlive()
	{
		AutoLock autoLock(&m_user_connect_manger_mutex);

		map<int, UserConnect*>::iterator iter = m_mapUserConnect.begin();

		for (; iter != m_mapUserConnect.end(); )
		{
			if (-1 == send(iter->first, " ", 1, 0))
			{
				closeSockById(iter->first);
				delete m_mapUserConnect [ iter->first ];
				m_mapUserConnect [ iter->first ] = NULL;
				iter = m_mapUserConnect.erase(iter);
			}
			else
			{
				iter++;
			}
		}
	}
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
					UserConnect *temp = UserConnectManger::GetInstance()->FindUserConnectById(clnt_sock);
					if (temp != NULL)
					{
						temp->DoSelectSql(user_name, user_pswd, cmd_value);
						send(clnt_sock, "success", strlen("success")+1, 0);
						return "success";
					}
				}
			}
			//get select msg 
			if (cXml.GetData() == "getmsg")
			{
				UserConnect *temp = UserConnectManger::GetInstance()->FindUserConnectById(clnt_sock);
				if (temp != NULL)
				{
					
					CMarkupSTL cXmlRsp;
					cXmlRsp.AddElem("info");
					cXmlRsp.IntoElem();
					int sendNum = 0;
					do
					{
						sendNum++;
						char tempInfo[7][256] = { 0 };
						temp->GetMsg(tempInfo[0], tempInfo[1], tempInfo[2], tempInfo[3], tempInfo[4], tempInfo[5], tempInfo[6]);
						//if (strcmp(tempInfo[0], "") != 0)
							cXmlRsp.AddElem("username", tempInfo[0]);
						//if (strcmp(tempInfo[1], "") != 0)
							cXmlRsp.AddElem("usercount", tempInfo[1]);
						//if (strcmp(tempInfo[2], "") != 0)
							cXmlRsp.AddElem("userphone", tempInfo[2]);
						//if (strcmp(tempInfo[3], "") != 0)
							cXmlRsp.AddElem("shopname", tempInfo[3]);
						//if (strcmp(tempInfo[4], "") != 0)
							cXmlRsp.AddElem("costmoney", tempInfo[4]);
						//if (strcmp(tempInfo[5], "") != 0)
							cXmlRsp.AddElem("costmoneyforuser", tempInfo[5]);
						//if (strcmp(tempInfo[6], "") != 0)
							cXmlRsp.AddElem("datetime", tempInfo[6]);
					} while (temp->m_list_callBack_info.size() > 0);
					//cout << "getmsg send num:" << sendNum << " length :" << cXmlRsp.GetDoc().length() + 1 << endl;
					cXmlRsp.AddElem("username");
					send(clnt_sock, cXmlRsp.GetDoc().c_str(), cXmlRsp.GetDoc().length() + 1, 0);
					return "success";
				}
			}
		}
	}
	send(clnt_sock, "failed", strlen("failed") + 1, 0);
	return "failed";
}

time_t StringToDatetime(const char *str)
{
	tm tm_; 
	int year, month, day, hour, minute,second; 
	sscanf(str,"%d-%d-%d %d:%d:%d", &year, &month, &day, &hour, &minute, &second); 
	tm_.tm_year = year - 1900;
	tm_.tm_mon = month - 1;
	tm_.tm_mday = day;
	tm_.tm_hour = hour;
	tm_.tm_min = minute;
	tm_.tm_sec = second;
	tm_.tm_isdst = 0;

	time_t t_ = mktime(&tm_); //�Ѿ�����8��ʱ�� 
	return t_; //��ʱ�� 
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
					if( 200 == DoRegister(user_name, user_pswd))
						send(clnt_sock, "success", strlen("success") + 1, 0);
					else
						send(clnt_sock, "failed", strlen("failed") + 1, 0);
				}

				if (cXml.GetData() == "login")
				{
					get_all_user_info();
					if (isFindInUserInfoMap(user_name, user_pswd))
					{
						time_t t = time(NULL);
						if (StringToDatetime(g_user_info_map[user_name].recv.c_str()) < t)
						{
							send(clnt_sock, "timeout", strlen("timeout") + 1, 0);
							return "timeout";
						}
						if (-1 == UserConnectManger::GetInstance()->AddUserConnect(clnt_sock, user_name))
						{
							send(clnt_sock, "this user is login.", strlen("this user is login.") + 1, 0);
							return "success";
						}
						send(clnt_sock, "success", strlen("success") + 1, 0);

						char strDate[MAX_PATH] = { 0 };
						FormatTime(time(0) , strDate);

						cout << "1. login success. data :" << strDate << "username :" << user_name.c_str()<<  endl;

						return "success";
					}
				}

				if (cXml.GetData() == "logout")
				{
					UserConnectManger::GetInstance()->DeleteUserConnectById(clnt_sock);
					send(clnt_sock, "success", strlen("success") + 1, 0);

					char strDate[MAX_PATH] = { 0 };
					FormatTime(time(0) , strDate);

					cout << "2. logout success. data :" << strDate << "username :" << user_name.c_str() << endl;
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
						UserConnect *temp = UserConnectManger::GetInstance()->FindUserConnectById(clnt_sock);
						if (temp != NULL)
						{
							temp->DoCommonSql(user_name, user_pswd, cmd_value);
							send(clnt_sock, "success", strlen("success") + 1, 0);
							return "success";
						}
					}
				}


			}
		}
	} while (0);
	
	send(clnt_sock, "failed", strlen("failed") + 1, 0);
	return "failed";
}
map<int, string> g_mapRecvMsg;
fd_set freads;
void closeSockById(int i)
{
	AutoLock autoLock(&g_mutex);

	
	if (FD_ISSET(i, &freads))
	{
		FD_CLR(i, &freads);
		closesocket(i);
	}
	cout << "disconnect client " << i << endl;
}

void ProcessMsg()
{
	fd_set temps;
	int  fd_max, fd_num;
	timeval timeout;
	FD_ZERO(&freads);
	FD_ZERO(&temps);

	FD_SET(g_serverSock, &freads);
	fd_max = g_serverSock;
	timeout.tv_sec = 3;
	timeout.tv_usec = 0;
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
					AutoLock autoLock(&g_mutex);

					SOCKADDR_IN clntAddr;
					int clntAddrSz = sizeof(SOCKADDR_IN);
					int clnt_sock = accept(g_serverSock, (sockaddr *)&clntAddr, &clntAddrSz);
					FD_SET(clnt_sock, &freads);
					if (fd_max < clnt_sock)
					{
						fd_max = clnt_sock;
					}
					g_mapRecvMsg[clnt_sock] = "";
					string ipAddr = inet_ntoa(clntAddr.sin_addr);
					cout << "new client . Ip : " << ipAddr.c_str() << " Port : " << htons(clntAddr.sin_port) << " sock id : " << clnt_sock << endl;
				}
				else
				{
					char buffer[1500] = { 0 };
					int str_len = recv(i, buffer, 1500 - 1, 0);
					
					if (str_len <= 0)//disconnect
					{
						if (NULL != UserConnectManger::GetInstance()->FindUserConnectById(i))
						{
							UserConnectManger::GetInstance()->DeleteUserConnectById(i);
						}
						g_mapRecvMsg.erase(g_mapRecvMsg.find(i));

						closeSockById(i);
					}
					else if (str_len > 0)
					{
						g_mapRecvMsg[i] += buffer;
						if (g_mapRecvMsg[i].find("<info>") != -1 && g_mapRecvMsg[i].find("</info>") != -1)
						{
#ifdef DEBUG
							cout << "recv msg : " << g_mapRecvMsg[i].c_str() << endl;
#endif
							CMarkupSTL cXml;
							cXml.SetDoc(g_mapRecvMsg[i].c_str());
							if (cXml.FindElem("info", true))
							{
								cXml.IntoElem();
								if (cXml.FindElem("cmd", true))
								{
									if (cXml.GetData() == "select")
									{
										ProcessSelect(i, g_mapRecvMsg[i].c_str()).c_str();
										//cout << ProcessSelect(i, g_mapRecvMsg[i].c_str()).c_str();
									}
									else if (cXml.GetData() == "common")
									{
										ProcessCommonCmd(i, g_mapRecvMsg[i].c_str()).c_str();
										//cout << ProcessCommonCmd(i, g_mapRecvMsg[i].c_str()).c_str();
									}
								}
							}
							g_mapRecvMsg[i] = "";
						}
						//send(i, buffer, str_len, 0);
					}
				}
			}
		}
	}
	cout << "ProcessMsg return ." << endl;
}

DWORD  CALLBACK CheckConnectTimer(PVOID pvoid)
{
	while (!g_isexit)
	{
#ifdef DEBUG
		cout << "start CheckConnectTimer" << endl;
#endif
		Sleep(5000);
		UserConnectManger::GetInstance()->CheckConnectAlive();
	}
	return 0;
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
	DWORD dwThreadId;
	// 创建线程  
	HANDLE hThread = ::CreateThread(NULL, 0, CheckConnectTimer, 0, 0, &dwThreadId);
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

	//��ȡһ����Ϣ
	get_all_user_info();

	int timeout = 500; //3s
	setsockopt(g_serverSock, SOL_SOCKET, SO_SNDTIMEO, (const char*)&timeout, sizeof(timeout));
	setsockopt(g_serverSock, SOL_SOCKET, SO_RCVTIMEO, (const char*)timeout, sizeof(timeout));
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
			//cout << strOut.c_str();
		}
		g_user_info_map[tempStruct.name] = tempStruct;
		//cout << endl;
	}
	sqlite3_free_table(pResult);  //ʹ���������ͷ�Ϊ��¼������ڴ棬������ڴ�й©
	return 0;
}

int DoRegister(string name,string pswd)
{
	//������Ϣ
	AutoLock autoLock(&g_mutex);
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
		time_t timeNowt = time(0);
		tm *timeNow = gmtime((const time_t*)&timeNowt);
		char strDate[MAX_PATH] = { 0 };
		sprintf(strDate, "%d-%d-%d %d:%d:%d", timeNow->tm_year + 1900, timeNow->tm_mon + 2, timeNow->tm_mday, timeNow->tm_hour, timeNow->tm_min, timeNow->tm_sec);
		cout << strDate << endl;
		char cmd[1024] = { 0 };
		sprintf(cmd, INSERT_USER_INO, name.c_str(), pswd.c_str(), name.c_str(), strDate);
		nResult = sqlite3_exec(g_db, cmd, NULL, NULL, &g_errMsg);
		if (nResult != SQLITE_OK)
		{
			cout << g_errMsg << endl;
			sqlite3_free(g_errMsg);
			return -1;
		}
		
		cout << "register success . " << name.c_str() << " " << pswd.c_str();
	}
	//������Ϣ
	get_all_user_info();
	return 200;
}


