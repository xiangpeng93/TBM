using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TBM_Client_Windows;

namespace TBM_Client_Windows
{
	/// <summary>
	/// updateHistoryData.xaml 的交互逻辑
	/// </summary>
	public partial class updateHistoryData : MetroWindow
	{
		[DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern void Delete(string sql);

		string tUserName;
		string tUserCount;
		string tUserPhone;
		string tShopName;
		string tMoney;
		string tCostMoney;
		string tDatetime;
        int tTableType;

		string HistoryDataTable = "UPDATE HISTORYDATA ";
        ManalWindow g_mangerMoney;
        public updateHistoryData(ManalWindow mangerMoney)
		{
			InitializeComponent();
            g_mangerMoney = mangerMoney;
		}
		

		private void updateHistoryData_Click(object sender, RoutedEventArgs e)
		{
            string temp;

            temp = HistoryDataTable;
			temp += "SET ";

			temp += "USERNAME = \"";
			temp += username.Text;
			temp += "\" , ";

			temp += "USERCOUNT = \"";
			temp += usercount.Text;
			temp += "\" , ";

			temp += "USERPHONE = \"";
			temp += userphone.Text;
			temp += "\" , ";

			temp += "SHOPNAME = \"";
			temp += shopName.Text;
			temp += "\" , ";

			temp += "COSTMONEY = \"";
			temp += usermoney.Text;
			temp += "\", ";

			temp += "COSTMONEYFORUSER = \"";
			temp += usercostMoney.Text;
			temp += "\", ";

			temp += "DATETIME = \"";
			temp += usertime.Text;

			temp += "\" WHERE ";

			temp += "USERNAME = \"";
			temp += tUserName;
			temp += "\" and ";

			temp += "USERCOUNT = \"";
			temp += tUserCount;
			temp += "\" and ";

			temp += "USERPHONE = \"";
			temp += tUserPhone;
			temp += "\" and ";

			temp += "SHOPNAME = \"";
			temp += tShopName;
			temp += "\" and ";

			temp += "COSTMONEY = \"";
			temp += tMoney;
			temp += "\" and ";

			temp += "COSTMONEYFORUSER = \"";
			temp += tCostMoney;
			temp += "\" and ";

			temp += "DATETIME = \"";
			temp += tDatetime;
			temp += "\" ";
			Delete(temp);


            this.Close();
            g_mangerMoney.IsEnabled = true;
            g_mangerMoney.saveItemDyInput(username.Text,usercount.Text,userphone.Text,shopName.Text,usermoney.Text,usercostMoney.Text,usertime.Text);
            g_mangerMoney.searchItemData();
		}

		internal void SetLocalData(string name,string count, string phone,string shop,string money,string costmoney,string time,int tableType)
		{
			tUserName = name;
			tUserCount = count;
			tShopName = shop;
			tUserPhone = phone;
			tMoney = money;
			tCostMoney = costmoney;
			tDatetime = time;
            tTableType = tableType;

			username.Text = name;
			usercount.Text = count;
			shopName.Text = shop;
			userphone.Text = phone;
			usermoney.Text = money;
			usercostMoney.Text = costmoney;
			usertime.Text = time;
		}

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            g_mangerMoney.IsEnabled = true;
        }
	}
}
