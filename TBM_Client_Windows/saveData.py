# -*- coding: utf-8 -*- 
import sys
sys.path.append('C:\\Python27\\lib')
sys.path.append('C:\\Python27\\lib\\site-packages\\setuptools-26.1.1-py2.7.egg')
sys.path.append('C:\\Python27\\lib\\site-packages\\xlrd-1.0.0-py2.7.egg')
sys.path.append('C:\\Python27\\lib\\site-packages\\xlwt-1.2.0-py2.7.egg')

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
    ws.write(0 , 2, "手机号")
    ws.write(0 , 3, "商户")
    ws.write(0 , 4, "刷单金额")
    ws.write(0 , 5, "佣金")
    ws.write(0 , 6, "时间")
    
    wb.save(fileName)

def wireFileByList(fileName,listInfo):
    wb = xlwt.Workbook(encoding='utf-8')
    ws = wb.add_sheet('Sheet1',cell_overwrite_ok=True)
    print len(listInfo)
    temp = listInfo[0]
    print len(temp)
    print temp[0]
    for i in range (0,len(listInfo)):
        for j in range (0,len(listInfo[i])):
            print str(i) + " " + str(j) + " " + listInfo[i][j]
            
            ws.write(i,j,listInfo[i][j])
    wb.save(fileName)
    
def insertInfoByFileName(fileName,p1,p2,p3,p4,p5,p6,p7):
    createFileIfNotExit(fileName);
    listInfo = readByFileName(fileName)
    listTemp = [p1,p2,p3,p4,p5,p6,p7]
    listInfo.append(listTemp)
    
    wireFileByList(fileName,listInfo)

def main():
    print u"脚本名：", sys.argv[0]
    paramLen = len(sys.argv)
    if paramLen != 9:
        print u"参数不足 %d" % paramLen
        return
    for i in range(1, ):
        print u"参数", i, sys.argv[i]
    insertInfoByFileName(sys.argv[1],sys.argv[2],
                         sys.argv[3],sys.argv[4],
                         sys.argv[5],sys.argv[6],
                         sys.argv[7],sys.argv[8])


