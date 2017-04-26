// TBMServer.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"
#include "TBMServer.h"
#pragma comment(lib,"WS2_32.lib")
#include <WinSock2.h>
#include <iostream>
#include <thread>
#include "markup.h"

using namespace std;

#define EXPORT_API __stdcall
#define MAX_CONNECT 100

int g_serverSock = -1;
bool g_isexit = false;

thread* g_processThread;

string ProcessSelect(const char *cmd)
{
	cout << "cmd value : " << cmd << endl;
	return "";
}

string ProcessCommonCmd(const char *cmd)
{
	cout << "cmd value : " << cmd << endl;
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
									if (cXml.FindElem("value", true))
									{
										ProcessSelect(cXml.GetData().c_str());
									}
								}
								else if (cXml.GetData() == "common")
								{
									if (cXml.FindElem("value", true))
									{
										ProcessCommonCmd(cXml.GetData().c_str());
									}
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