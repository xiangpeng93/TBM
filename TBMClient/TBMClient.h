#include "stdafx.h"

void __stdcall Init();
int __stdcall Login(char* ip, int port,char *userName, char *userPaswd);
void __stdcall Insert(char *sql);
void __stdcall Select(char *sql);
void __stdcall Delete(char *sql);
void __stdcall CommonSql(char *sql);
void __stdcall GetMsg(char *userName, char *userCount, char *userPhone);
void __stdcall Fini();
void __stdcall GetMsg2(char *userName, char *userCount, char *userPhone, char *shopName, char *costMoney, char *costMoneyForUser, char * dataTime);