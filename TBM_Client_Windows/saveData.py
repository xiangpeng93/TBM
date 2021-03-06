# -*- coding: utf-8 -*- 
import sys
import time
sys.path.append('C:\\Python27\\lib')
sys.path.append('C:\\Python27\\lib\\site-packages\\setuptools-26.1.1-py2.7.egg')
sys.path.append('C:\\Python27\\lib\\site-packages\\xlrd-1.0.0-py2.7.egg')
sys.path.append('C:\\Python27\\lib\\site-packages\\xlwt-1.2.0-py2.7.egg')
sys.path.append('C:\\Python27\\lib\\site-packages')

print sys.path
import xlwt
import xlrd
import os

from datetime import datetime

def open_excel(file):
    try:
        data = xlrd.open_workbook(file)
        return data
    except Exception,e:
        print str(e)
        
def readByFileName(fileName):
    data = open_excel(fileName)
    table = data.sheets()[0]
    nrows = table.nrows #行数
    print nrows
    ncols = table.ncols #列数
    print ncols

    list = []
    for rownum in range(0,nrows):
         #print rownum
         row = table.row_values(rownum)
         #print row
         if row:
             list.append(row)
    
    return list
    
def createFileIfNotExit(fileName):
    if(os.path.exists(fileName) == True):
        return
    wb = xlwt.Workbook(encoding='utf-8')
    ws = wb.add_sheet('Sheet1',cell_overwrite_ok=True)
    
    ws.write(0 , 0, "用户名")
    ws.write(0 , 1, "旺旺号")
    ws.write(0 , 2, "介绍人号")
    ws.write(0 , 3, "商户")
    ws.write(0 , 4, "刷单金额")
    ws.write(0 , 5, "佣金")
    ws.write(0 , 6, "时间")
    
    wb.save(fileName)
def createFileIfNotExitEx(fileName):
    if(os.path.exists(fileName) == True):
        return
    wb = xlwt.Workbook(encoding='utf-8')
    ws = wb.add_sheet('Sheet1',cell_overwrite_ok=True)
    
    ws.write(0 , 0, "序号")
    ws.write(0 , 1, "关键字")
    ws.write(0 , 2, "数量")
    
    wb.save(fileName)

def wireFileByList(fileName,listInfo):
    print "start writeFile"
    print time.time()
    print "\r\n"
    wb = xlwt.Workbook(encoding='utf-8')
    ws = wb.add_sheet('Sheet1',cell_overwrite_ok=True)
##    print len(listInfo)
    temp = listInfo[0]
##    print len(temp)
##    print temp[0]
    for i in range (0,len(listInfo)):
        for j in range (0,len(listInfo[i])):
##            print str(i) + " " + str(j) + " " + listInfo[i][j]
            ws.write(i,j,listInfo[i][j])
    wb.save(fileName)
    print "end writeFile"
    print time.time()
    print "\r\n"
 
def initByFileName(fileName):
    createFileIfNotExit(fileName);
    listInfo = readByFileName(fileName)
    print "init initByFileName"
    print time.time()
    print "\r\n"
    return listInfo

def initByFileNameEx(fileName):
    createFileIfNotExitEx(fileName);
    listInfo = readByFileName(fileName)
    print "init initByFileName"
    print time.time()
    print "\r\n"
    return listInfo

def insertInfoByFileName(listInfo,p1,p2,p3,p4,p5,p6,p7):
    listTemp = [p1,p2,p3,p4,p5,p6,p7]
    listInfo.append(listTemp)

def getInfoFromListInfo(listInfo,row,cloum):
    if row >= len(listInfo):
        return "";
    if cloum >= len(listInfo[row]):
        return "";
    return listInfo[row][cloum]

def getInfoNumFromListInfo(listInfo):
    return len(listInfo)


def main():
    infoList = readByFileName("test.xls")
    value = getInfoFromListInfo(infoList,0,0)
    print value
    print getInfoNumFromListInfo(infoList)
##    print u"脚本名：", sys.argv[0]
##    paramLen = len(sys.argv)
##    if paramLen != 9:
##        print u"参数不足 %d" % paramLen
##        return
##    for i in range(1, ):
##        print u"参数", i, sys.argv[i]
##    insertInfoByFileName(sys.argv[1],sys.argv[2],
##                         sys.argv[3],sys.argv[4],
##                         sys.argv[5],sys.argv[6],
##                         sys.argv[7],sys.argv[8])
