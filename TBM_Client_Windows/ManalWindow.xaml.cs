using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace TBM_Client_Windows
{
	/// <summary>
	/// ManalWindow.xaml 的交互逻辑
	/// </summary>
	public partial class ManalWindow : MetroWindow
	{
		public string sqlUserInfoOrderByName = "select * from USERINFO order by USERNAME";
		public string sqlUserInfo = "select * from USERINFO ";
		public string sqlShop = "select * from SHOPINFO order by USERNAME";
		public string sqlShopNoOrder = "select * from SHOPINFO";

		public string searchHistoryData = "select * from HISTORYDATA ";

		public string InsertHistoryData = "INSERT INTO HISTORYDATA (USERNAME , USERCOUNT , USERPHONE ,SHOPNAME, COSTMONEY ,COSTMONEYFORUSER ,DATETIME) VALUES";

		string cmdDeleteHistoryData = "delete from HISTORYDATA where ";
		[DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern void Insert(string sql);

		[DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern void Delete(string sql);



		[DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern void Fini();

		[DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern void Select(string sql);

		[DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern void Select2(string sql);

		[DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern void GetMsg(StringBuilder userName, StringBuilder userCount, StringBuilder userPhone);

		[DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern void GetMsg2(StringBuilder userName, StringBuilder userCount, StringBuilder userPhone, StringBuilder shopName, StringBuilder costMoney, StringBuilder costMoneyForUser, StringBuilder dateTime);
		public ObservableCollection<CInfoList> Users = new ObservableCollection<CInfoList>();

		DateTime beginDataTime;
		DateTime lastDataTime;

		bool isFirstClicAddList = true;
		private static System.Windows.Threading.DispatcherTimer readDataTimer = new System.Windows.Threading.DispatcherTimer();


		public void timeCycle(object sender, EventArgs e)
		{
			string TempUserName;
			string TempShopName;
			string TempDataTime;
			StringBuilder TuserName = new StringBuilder(2048);
			StringBuilder TuserCount = new StringBuilder(2048);
			StringBuilder TuserPhone = new StringBuilder(2048);
			StringBuilder ShopName = new StringBuilder(2048);
			StringBuilder COSTMONEY = new StringBuilder(2048);
			StringBuilder COSTMONEYForUser = new StringBuilder(2048);
			StringBuilder sDateTime = new StringBuilder(2048);
			GetMsg2(TuserName, TuserCount, TuserPhone, ShopName, COSTMONEY, COSTMONEYForUser, sDateTime);
			TempUserName = TuserName.ToString();
			TempShopName = ShopName.ToString();
			TempDataTime = sDateTime.ToString();
			if (TempUserName.Equals("") == false || TempShopName.Equals("") == false || TempDataTime.Equals("") == false)
			{
				if (TempDataTime.Equals("0001-01-01 00:00:00Z") == false)
				{
					g_number++;
					Users.Add(new CInfoList(g_number.ToString(), TuserName.ToString(), TuserCount.ToString(), TuserPhone.ToString(), ShopName.ToString(), COSTMONEY.ToString(), COSTMONEYForUser.ToString(), sDateTime.ToString()));
				}
			}
			if (TempUserName.Equals("") && TempShopName.Equals("") && TempDataTime.Equals(""))
				readDataTimer.Stop();
		}

		// 0 history Table
		// 1 historyUser Table
		int g_tableType = 0;
		static int g_number = 0;
		public class CInfoList
		{
			public CInfoList(
				string sIdNumber,
				string sUserName,
			string sUserCount,
			string sUserPhone,
			string sShopName,
			string sCostMoney,
			string sCostForUser,
			string sDateTime)
			{
				IDNumber = sIdNumber;
				UserName = sUserName;
				UserCount = sUserCount;
				UserPhone = sUserPhone;
				ShopName = sShopName;
				CostMoney = sCostMoney;
				CostForUser = sCostForUser;
				DateTime = sDateTime;
			}
			public string IDNumber
			{
				get;
				set;
			}
			public string UserName
			{
				get;
				set;
			}
			public string UserCount
			{
				get;
				set;
			}
			public string UserPhone
			{
				get;
				set;
			}
			public string ShopName
			{
				get;
				set;
			}
			public string CostMoney
			{
				get;
				set;
			}
			public string CostForUser
			{
				get;
				set;
			}
			public string DateTime
			{
				get;
				set;
			}
		}
		public ManalWindow()
		{
			InitializeComponent();
			ListCollectionView cs = new ListCollectionView(Users);
			infoList.ItemsSource = cs;



			userName.Items.Clear();
			userParentName.Items.Clear();
			Select(sqlUserInfoOrderByName);
			string Name = "";

			ComboBoxItem comboxIten1 = new ComboBoxItem();
			comboxIten1.Content = "";
			userName.Items.Add(comboxIten1);
			ComboBoxItem comboxIten_userParent = new ComboBoxItem();
			comboxIten_userParent.Content = "";
			userParentName.Items.Add(comboxIten_userParent);
			do
			{
				StringBuilder TuserName = new StringBuilder(2048);
				StringBuilder TuserCount = new StringBuilder(2048);
				StringBuilder TuserPhone = new StringBuilder(2048);
				GetMsg(TuserName, TuserCount, TuserPhone);
				Name = TuserName.ToString();
				if (Name.Equals("") == false)
				{
					ComboBoxItem comboxIten = new ComboBoxItem();
					comboxIten.Content = Name;
					userName.Items.Add(comboxIten);
					ComboBoxItem temp = new ComboBoxItem();
					temp.Content = Name;
					userParentName.Items.Add(temp);

				}
			}
			while (Name.Equals("") == false);
			userName.SelectedIndex = -1;
			userParentName.SelectedIndex = -1;
			shopName.Items.Clear();
			Select(sqlShop);
			string NameShop = "";
			ComboBoxItem comboxIten2 = new ComboBoxItem();
			comboxIten2.Content = NameShop;
			shopName.Items.Add(comboxIten2);
			do
			{
				StringBuilder TuserName = new StringBuilder(2048);
				StringBuilder TuserCount = new StringBuilder(2048);
				StringBuilder TuserPhone = new StringBuilder(2048);
				GetMsg(TuserName, TuserCount, TuserPhone);
				NameShop = TuserName.ToString();
				if (NameShop.Equals("") == false)
				{
					ComboBoxItem comboxIten = new ComboBoxItem();
					comboxIten.Content = NameShop;
					shopName.Items.Add(comboxIten);
				}
			}
			while (NameShop.Equals("") == false);
			shopName.SelectedIndex = -1;

			readDataTimer.Tick += new EventHandler(timeCycle);
			readDataTimer.Interval = new TimeSpan(0, 0, 0, 0);
		}
		public void Update()
		{
			userCount.Clear();
			userParentName.Items.Clear();
			userName.Items.Clear();
			Select(sqlUserInfoOrderByName);
			string Name = "";
			ComboBoxItem comboxIten1 = new ComboBoxItem();
			comboxIten1.Content = "";
			userName.Items.Add(comboxIten1);
			ComboBoxItem comboxIten_userParent = new ComboBoxItem();
			comboxIten_userParent.Content = "";
			userParentName.Items.Add(comboxIten_userParent);
			do
			{
				StringBuilder TuserName = new StringBuilder(2048);
				StringBuilder TuserCount = new StringBuilder(2048);
				StringBuilder TuserPhone = new StringBuilder(2048);
				GetMsg(TuserName, TuserCount, TuserPhone);
				Name = TuserName.ToString();
				if (Name.Equals("") == false)
				{
					ComboBoxItem comboxIten = new ComboBoxItem();
					comboxIten.Content = Name;
					userName.Items.Add(comboxIten);
					ComboBoxItem temp = new ComboBoxItem();
					temp.Content = Name;
					userParentName.Items.Add(temp);
				}
			}
			while (Name.Equals("") == false);
			userName.SelectedIndex = -1;
			userParentName.SelectedIndex = -1;

			shopName.Items.Clear();
			Select(sqlShop);
			string NameShop = "";
			ComboBoxItem comboxIten2 = new ComboBoxItem();
			comboxIten2.Content = NameShop;
			shopName.Items.Add(comboxIten2);
			do
			{
				StringBuilder TuserName = new StringBuilder(2048);
				StringBuilder TuserCount = new StringBuilder(2048);
				StringBuilder TuserPhone = new StringBuilder(2048);
				GetMsg(TuserName, TuserCount, TuserPhone);
				NameShop = TuserName.ToString();
				if (NameShop.Equals("") == false)
				{
					ComboBoxItem comboxIten = new ComboBoxItem();
					comboxIten.Content = NameShop;
					shopName.Items.Add(comboxIten);
				}
			}
			while (NameShop.Equals("") == false);
			shopName.SelectedIndex = -1;

		}
		public class Person
		{
			public string Name
			{
				get;
				set;
			}
			public int Age
			{
				get;
				set;
			}
		}
		private void addItem_Click(object sender, RoutedEventArgs e)
		{
			g_tableType = 0;
			if (isFirstClicAddList)
			{
				Users.Clear();
				isFirstClicAddList = false;
				g_number = 0;
			}
			g_number++;


			string sUserName = userName.Text;
			string sUserCount = userCount.Text;
			string sUserPhone = userParentName.Text;
			string sShopName = shopName.Text;
			string sCostMoney = costMoney.Text.ToString();
			if (sCostMoney.Length > 8)
			{
				MessageBox.Show("输入金额过大,最大99999999!");
				return;
			}
			string sCostForUser = costForUser.Text.ToString();
			if (sCostForUser.Length > 8)
			{
				MessageBox.Show("输入金额过大,最大99999999!");
				return;

			}
			string sDatetime = DateTime.Now.ToLocalTime().ToString("u");

			Users.Add(new CInfoList(g_number.ToString(), sUserName, sUserCount, sUserPhone, sShopName, sCostMoney, sCostForUser, sDatetime));
			if (sUserName.Equals("") || sUserCount.Equals("") || sShopName.Equals(""))
				return;
			saveItemDyInput(sUserName, sUserCount, sUserPhone, sShopName, sCostMoney, sCostForUser, sDatetime);
		}


		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			object o = infoList.SelectedItem;
			if (o == null)
				return;
			CInfoList item = o as CInfoList;
			Users.Remove(item);
		}

		private void DeleteDbData_Click(object sender, RoutedEventArgs e)
		{
			object o = infoList.SelectedItem;
			if (o == null)
				return;
			CInfoList item = o as CInfoList;

			string sql = cmdDeleteHistoryData;
			if (!item.UserName.Equals(""))
			{
				sql += "USERNAME = \"";
				sql += item.UserName.ToString();
				sql += "\"";
				if (!item.UserCount.Equals(""))
				{
					sql += " and ";
					sql += "USERCOUNT=\"";
					sql += item.UserCount.ToString();
					sql += "\"";
				}
				if (!item.UserPhone.Equals(""))
				{
					sql += " and ";
					sql += " USERPHONE = \"";
					sql += item.UserPhone.ToString();
					sql += "\"";
				}
				if (!item.ShopName.Equals(""))
				{
					sql += " and ";
					sql += " SHOPNAME = \"";
					sql += item.ShopName.ToString();
					sql += "\"";
				}
				if (!item.DateTime.Equals(""))
				{
					sql += " and ";
					sql += " DATETIME = \"";
					sql += item.DateTime.ToString();
					sql += "\"";
				}

				Delete(sql);
			}

			Users.Remove(item);
		}

		private void EditHistoryData_Click(object sender, RoutedEventArgs e)
		{
			object o = infoList.SelectedItem;
			if (o == null)
			{
				MessageBox.Show("列表为空！");
				return;
			}

			CInfoList item = o as CInfoList;
			if (!item.UserName.Equals(""))
			{
				updateHistoryData updateHistory = new updateHistoryData(this);
				updateHistory.SetLocalData(item.UserName, item.UserCount, item.UserPhone, item.ShopName, item.CostMoney, item.CostForUser, item.DateTime, g_tableType);
				updateHistory.Show();
				this.IsEnabled = false;
			}
			else
			{
				MessageBox.Show("名称为空！");
			}
		}

		private void calcMoney_Click(object sender, RoutedEventArgs e)
		{
			int AllMoney = 0;
			if (!countMoney.Text.Equals(""))
			{
				AllMoney = Int32.Parse(countMoney.Text);
			}
			else
			{
				MessageBox.Show("输入金额");
			}
			int CostMoney = 0;
			for (int i = 0; i < Users.Count; i++)
			{
				if (!Users[i].CostMoney.Equals("") && Users[i].CostMoney.Length <= 8)
				{
					CostMoney += Int32.Parse(Users[i].CostMoney.ToString());
				}
				if (!Users[i].CostForUser.Equals("") && Users[i].CostForUser.Length <= 8)
				{
					CostMoney += Int32.Parse(Users[i].CostForUser.ToString());
				}

			}
			int income = AllMoney - CostMoney;
			incomeMoney.Text = income.ToString();
		}


		private void userName_Selected(object sender, RoutedEventArgs e)
		{
			ComboBoxItem combox = (ComboBoxItem)sender;

			string test = combox.Content.ToString();
			return;
		}

		private void shopName_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox combox = (ComboBox)sender;
			ComboBoxItem item = combox.SelectedItem as ComboBoxItem;
			if (item != null)
			{
				//MessageBox.Show(item.Content.ToString());
			}
		}

		private void userName_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox combox = (ComboBox)sender;
			ComboBoxItem item = combox.SelectedItem as ComboBoxItem;

			if (item != null)
			{
				//MessageBox.Show(item.Content.ToString());
				string searchUser = sqlUserInfo + "where USERNAME = " + "\"" + item.Content.ToString() + "\"";
				Select(searchUser);

				string Name = "";
				StringBuilder TuserName = new StringBuilder(2048);
				StringBuilder TuserCount = new StringBuilder(2048);
				StringBuilder TuserPhone = new StringBuilder(2048);
				GetMsg(TuserName, TuserCount, TuserPhone);
				Name = TuserName.ToString();

				if (Name.Equals("") == false)
				{
					userCount.Text = TuserCount.ToString();
					userParentName.Text = TuserPhone.ToString();
				}
				else
				{
					userCount.Clear();
					userParentName.SelectedIndex = -1;
				}
			}
			else
			{
				userCount.Clear();
				userParentName.SelectedIndex = -1;

			}
		}

		private void addUser_Click(object sender, RoutedEventArgs e)
		{
			AddUser addUser = new AddUser(this);
			//this.Visibility = (Visibility)1;
			addUser.Show();
		}

		public void searchItemData()
		{
			Users.Clear();
			bool isSelectSuccess = false;
			string search = searchHistoryData;
			string sUserName = userName.Text;
			string sShopName = shopName.Text;
			string sUserParent = userParentName.Text;
			if (sShopName.Equals("") && sUserName.Equals("") && sUserParent.Equals(""))
			{
				if (!beginDataTime.ToString("u").Equals("0001-01-01 00:00:00Z") && !lastDataTime.ToString("u").Equals("0001-01-01 00:00:00Z"))
				{
					search += " where datetime > \"";
					search += beginDataTime.ToString("u");
					search += "\" and datetime < \"";
					search += lastDataTime.ToString("u");
					search += "\"";
				}
				else
					search += " where DATETIME != '0001-01-01 00:00:00Z'";
				search += " order by DATETIME";

				Select2(search);
isSelectSuccess = true;
			}
			else
			{
				search += "where ";
				bool tIsFirst = true;
				if(!sUserName.Equals(""))
				{
					if(!tIsFirst)
					{
						search += " and ";
					}
					search += "USERNAME=\"";
					search += sUserName;
					search += "\"";
					tIsFirst = false;
				}
				if (!sShopName.Equals(""))
				{
					if(!tIsFirst)
					{
						search += " and ";
					}
					search += "SHOPNAME=\"";
					search += sShopName;
					search += "\"";
					tIsFirst = false;
				}
				if (!sUserParent.Equals(""))
				{
					if(!tIsFirst)
					{
						search += " and ";
					}
					search += "USERPHONE=\"";
					search += sUserParent;
					search += "\"";
					tIsFirst = false;
				}
				if (!beginDataTime.ToString("u").Equals("0001-01-01 00:00:00Z") && !lastDataTime.ToString("u").Equals("0001-01-01 00:00:00Z"))
				{
					search += " and datetime > \"";
					search += beginDataTime.ToString("u");
					search += "\" and datetime < \"";
					search += lastDataTime.ToString("u");
					search += "\"";
				}
				else
					search += " and DATETIME != '0001-01-01 00:00:00Z'";
				search += " order by DATETIME";
				Select2(search);
				isSelectSuccess = true;

			}
/*
			bool isSelectName = false;
			bool isSelectSuccess = false;
			if (!sUserName.Equals(""))
			{
				search += "where USERNAME=\"";
				search += sUserName;
				search += "\"";
				isSelectName = true;
			}
			else if (sShopName.Equals(""))
			{

				if (!beginDataTime.ToString("u").Equals("0001-01-01 00:00:00Z") && !lastDataTime.ToString("u").Equals("0001-01-01 00:00:00Z"))
				{
					search += " where datetime > \"";
					search += beginDataTime.ToString("u");
					search += "\" and datetime < \"";
					search += lastDataTime.ToString("u");
					search += "\"";
				}
				else
					search += "where DATETIME != '0001-01-01 00:00:00Z'";
				search += " order by DATETIME";

				Select2(search);
				isSelectSuccess = true;
			}
			if (!sShopName.Equals(""))
			{
				if (isSelectName)
				{
					search += "and ";
				}
				else
				{
					search += "where ";
				}
				search += "SHOPNAME=\"";
				search += sShopName;
				search += "\" ";
				if (!beginDataTime.ToString("u").Equals("0001-01-01 00:00:00Z") && !lastDataTime.ToString("u").Equals("0001-01-01 00:00:00Z"))
				{
					search += " and datetime > \"";
					search += beginDataTime.ToString("u");
					search += "\" and datetime < \"";
					search += lastDataTime.ToString("u");
					search += "\"";

				}

				isSelectName = false;
				isSelectSuccess = true;

				search += "order by DATETIME";
				Select2(search);
			}
			if (isSelectName)
			{
				isSelectSuccess = true;
				if (!beginDataTime.ToString("u").Equals("0001-01-01 00:00:00Z") && !lastDataTime.ToString("u").Equals("0001-01-01 00:00:00Z"))
				{
					search += " and datetime > \"";
					search += beginDataTime.ToString("u");
					search += "\" and datetime < \"";
					search += lastDataTime.ToString("u");
					search += "\"";
				}

				search += "order by DATETIME";
				Select2(search);
			}
*/
			g_number = 0;
			if (isSelectSuccess)
			{
				readDataTimer.Start();
			}
		}
		private void searchItem_Click(object sender, RoutedEventArgs e)
		{
			g_tableType = 0;
			searchItemData();
			if (g_tableType == 0)
			{
				infoList.ContextMenu.Visibility = Visibility.Visible;
			}
		}
		public void saveItemDyInput(string sUserName, string sUserCount, string sUserPhone, string sShopName, string sCostMoney, string sCostForUser, string sDateTime)
		{
			string search = searchHistoryData;
			if (!sUserName.Equals(""))
			{
				search += "where USERNAME=\"";
				search += sUserName;
				search += "\" ";
			}
			if (!sShopName.Equals(""))
			{
				search += "and ";
				search += "SHOPNAME=\"";
				search += sShopName;
				search += "\" ";
			}
			if (!sDateTime.Equals(""))
			{
				search += "and ";
				search += "DATETIME=\"";
				search += sDateTime;
				search += "\" ";
			}

			Select2(search);

			string TempUserName;
			string TempShopName;
			string TempDateTime;
			StringBuilder TuserName = new StringBuilder(2048);
			StringBuilder TuserCount = new StringBuilder(2048);
			StringBuilder TuserPhone = new StringBuilder(2048);
			StringBuilder ShopName = new StringBuilder(2048);
			StringBuilder COSTMONEY = new StringBuilder(2048);
			StringBuilder COSTMONEYForUser = new StringBuilder(2048);
			StringBuilder DateTimeData = new StringBuilder(2048);
			GetMsg2(TuserName, TuserCount, TuserPhone, ShopName, COSTMONEY, COSTMONEYForUser, DateTimeData);
			TempUserName = TuserName.ToString();
			TempShopName = ShopName.ToString();
			TempDateTime = DateTimeData.ToString();
			if (TempUserName.Equals("") && TempShopName.Equals("") && TempDateTime.Equals("") &&
			!sUserName.Equals("") && !sShopName.Equals(""))
			{
				string temp = InsertHistoryData;
				temp += "(\"";
				temp += sUserName;
				temp += "\",\"";
				temp += sUserCount;
				temp += "\",\"";
				temp += sUserPhone;
				temp += "\",\"";
				temp += sShopName;
				temp += "\",\"";
				temp += sCostMoney;
				temp += "\",\"";
				temp += sCostForUser;
				temp += "\",\"";
				temp += sDateTime;            // 2008-9-4 20:02:10;
				temp += "\")";

				string sql = temp;
				Insert(sql);
			}
		}
		public void saveItemData()
		{
			int dataCount = Users.Count();
			foreach (var item in Users)
			{
				string sUserName = item.UserName;
				string sUserCount = item.UserCount;
				string sUserPhone = item.UserPhone;
				string sShopName = item.ShopName;
				string sCostMoney = item.CostMoney;
				string sCostForUser = item.CostForUser;
				string sDateTime = item.DateTime;
				if (sUserName.Equals("") || sUserCount.Equals("") || sShopName.Equals(""))
					return;
				saveItemDyInput(sUserName, sUserCount, sUserPhone, sShopName, sCostMoney, sCostForUser, sDateTime);
			}
			Users.Clear();
		}

		private void SaveHistoryData_Click(object sender, RoutedEventArgs e)
		{
			saveItemData();
		}

		private void ClearData_Click(object sender, RoutedEventArgs e)
		{
			Users.Clear();
		}

		private void calendarCtlStart_SelectionModeChanged(object sender, EventArgs e)
		{
			int lastData = calendarCtlStart.SelectedDates.Count();
			beginDataTime = calendarCtlStart.SelectedDates.ElementAt(0);
			lastDataTime = calendarCtlStart.SelectedDates.ElementAt(lastData - 1);
			lastDataTime = lastDataTime.AddDays(1);
			Console.Write(beginDataTime);
			Console.Write("\r\n");

			Console.Write(lastDataTime);
			Console.Write("\r\n");
		}

		private void serachHistoryUserData_Click(object sender, RoutedEventArgs e)
		{
			g_tableType = 1;
			if (g_tableType == 1)
			{
				//infoList.ContextMenu.Visibility = Visibility.Hidden;
			}
			Users.Clear();

			string search = "select * from (select * from (";

			search += searchHistoryData;
			if (!shopName.Text.Equals(""))
			{
				search += " where SHOPNAME=\"";
				search += shopName.Text.ToString();
				search += "\"";
			}
			else
			{
				MessageBox.Show("请输入商户!");
				return;
			}
			search += " order by DATETIME)";
			search += " group by USERNAME)";
			search += " order by DATETIME ";

			Select2(search);
			string TempUserName;
			string TempShopName;
			string TempDataTime;
			g_number = 0;
			do
			{
				StringBuilder TuserName = new StringBuilder(2048);
				StringBuilder TuserCount = new StringBuilder(2048);
				StringBuilder TuserPhone = new StringBuilder(2048);
				StringBuilder ShopName = new StringBuilder(2048);
				StringBuilder COSTMONEY = new StringBuilder(2048);
				StringBuilder COSTMONEYForUser = new StringBuilder(2048);
				StringBuilder sDateTime = new StringBuilder(2048);
				GetMsg2(TuserName, TuserCount, TuserPhone, ShopName, COSTMONEY, COSTMONEYForUser, sDateTime);
				TempUserName = TuserName.ToString();
				TempShopName = ShopName.ToString();
				TempDataTime = sDateTime.ToString();
				if (TempUserName.Equals("") == false || TempShopName.Equals("") == false || TempDataTime.Equals("") == false)
				{
					g_number++;
					Users.Add(new CInfoList(g_number.ToString(), TuserName.ToString(), TuserCount.ToString(), TuserPhone.ToString(), ShopName.ToString(), COSTMONEY.ToString(), COSTMONEYForUser.ToString(), sDateTime.ToString()));
				}

			}
			while (TempUserName.Equals("") == false || TempShopName.Equals("") == false || TempDataTime.Equals("") == false);
			Users.OrderBy(p => p.DateTime);
		}

		private void SaveToXls_Click(object sender, RoutedEventArgs e)
		{
			SaveToXls updateHistory = new SaveToXls(this);

			updateHistory.Show();
		}

		private void windowAnalyzeData_Click(object sender, RoutedEventArgs e)
		{
			AnalyzeData winAnalyzeData = new AnalyzeData();
			winAnalyzeData.Show();
		}
	}
}
