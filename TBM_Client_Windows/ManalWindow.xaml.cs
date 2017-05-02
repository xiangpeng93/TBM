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
using 刷单管理;

namespace TBM_Client_Windows
{
    /// <summary>
    /// ManalWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ManalWindow : Window
    {
       public string sqlUserInfoOrderByName = "select * from USERINFO order by USERNAME";
        public string sqlUserInfo = "select * from USERINFO ";
        public string sqlShop = "select * from SHOPINFO order by USERNAME";

        public string searchHistoryData = "select * from HISTORYDATA ";
        public string searchHistoryDataUser = "select * from USERDATAHISTORY ";

        public string InsertHistoryData = "INSERT INTO HISTORYDATA (USERNAME , USERCOUNT , USERPHONE ,SHOPNAME, COSTMONEY ,COSTMONEYFORUSER ,DATETIME) VALUES";
        public string InsertHistoryDataUser = "INSERT INTO USERDATAHISTORY (USERNAME , USERCOUNT , USERPHONE ,SHOPNAME, COSTMONEY ,COSTMONEYFORUSER ,DATETIME) VALUES";
        public string updateHistoryDataUser = "UPDATE USERDATAHISTORY ";

        string cmdDeleteHistoryData = "delete from HISTORYDATA where ";
        string cmdDeleteHistoryDataUser = "delete from USERDATAHISTORY where ";
        [DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void Insert(string sql);

        [DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void Delete(string sql);

        [DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void Init(string ip,int port);

        [DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Login(string userName, string paswd);

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
            string sUserPhone ,
            string sShopName ,
            string sCostMoney ,
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
            public string IDNumber { get; set; }
            public string UserName { get; set; }
            public string UserCount { get; set; }
            public string UserPhone { get; set; }
            public string ShopName { get; set; }
            public string CostMoney{ get; set; }
            public string CostForUser { get; set; }
            public string DateTime { get; set; }
        }
        public ManalWindow()
        {
            InitializeComponent();
            ListCollectionView cs = new ListCollectionView(Users);
            infoList.ItemsSource = cs;

            //从数据库中读入用户信息
            Init("127.0.0.1",9090);

            userName.Items.Clear();
            Select(sqlUserInfoOrderByName);
            string Name = "";

            ComboBoxItem comboxIten1 = new ComboBoxItem();
            comboxIten1.Content = "";
            userName.Items.Add(comboxIten1);

            do
            {
                StringBuilder TuserName = new StringBuilder(2048);
                StringBuilder TuserCount = new StringBuilder(2048);
                StringBuilder TuserPhone = new StringBuilder(2048);
                GetMsg(TuserName, TuserCount, TuserPhone);
                Name = TuserName.ToString();
                if(Name.Equals("") == false)
                {
                    ComboBoxItem comboxIten = new ComboBoxItem();
                    comboxIten.Content = Name;
                    userName.Items.Add(comboxIten);
                }
            }
            while (Name.Equals("") == false);
            userName.SelectedIndex = -1;

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
        public void Update()
        {
            userCount.Clear();
            userPhone.Clear();
            userName.Items.Clear();
            Select(sqlUserInfoOrderByName);
            string Name = "";
            ComboBoxItem comboxIten1 = new ComboBoxItem();
            comboxIten1.Content = "";
            userName.Items.Add(comboxIten1);
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
                }
            }
            while (Name.Equals("") == false);
            userName.SelectedIndex = -1;

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
            public string Name { get; set; }
            public int Age { get; set; }
        }
        private void addItem_Click(object sender, RoutedEventArgs e)
        {
            g_tableType = 0;
            if(isFirstClicAddList)
            {
                Users.Clear();
                isFirstClicAddList = false;
                g_number = 0;
            }
            g_number++;


            string sUserName = userName.Text;
            string sUserCount = userCount.Text;
            string sUserPhone = userPhone.Text;
            string sShopName = shopName.Text;
            string sCostMoney = costMoney.Text.ToString();
            if(sCostMoney.Length > 8)
			{
				MessageBox.Show("输入金额过大,最大99999999!");
				return;
			}
            string sCostForUser = costForUser.Text.ToString();
			if(sCostForUser.Length > 8)
			{
				MessageBox.Show("输入金额过大,最大99999999!");
				return;

			}
            string sDatetime = DateTime.Now.ToLocalTime().ToString("u");

            Users.Add(new CInfoList(g_number.ToString(),sUserName, sUserCount, sUserPhone, sShopName, sCostMoney, sCostForUser, sDatetime));
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
                sql += "USERNAME = '";
                sql += item.UserName.ToString();
                sql += "'";
				if (!item.UserCount.Equals(""))
				{
					sql += " and ";
					sql += "USERCOUNT='";
					sql += item.UserCount.ToString();
					sql += "'";
				}
				if (!item.UserPhone.Equals(""))
				{
					sql += " and ";
					sql += " USERPHONE = '";
					sql += item.UserPhone.ToString();
					sql += "'";
				}
				if (!item.ShopName.Equals(""))
				{
					sql += " and ";
					sql += " SHOPNAME = '";
					sql += item.ShopName.ToString();
					sql += "'";
				}
				if (!item.DateTime.Equals(""))
				{
					sql += " and ";
					sql += " DATETIME = '";
					sql += item.DateTime.ToString();
					sql += "'";
				}

                Delete(sql);
            }

            string sqlDeleteUser = cmdDeleteHistoryDataUser;
            if (!item.UserName.Equals(""))
            {
                sqlDeleteUser += "USERNAME = '";
                sqlDeleteUser += item.UserName.ToString();
                sqlDeleteUser += "'";
                if (!item.UserCount.Equals(""))
                {
                    sqlDeleteUser += " and ";
                    sqlDeleteUser += "USERCOUNT='";
                    sqlDeleteUser += item.UserCount.ToString();
                    sqlDeleteUser += "'";
                }
                if (!item.UserPhone.Equals(""))
                {
                    sqlDeleteUser += " and ";
                    sqlDeleteUser += " USERPHONE = '";
                    sqlDeleteUser += item.UserPhone.ToString();
                    sqlDeleteUser += "'";
                }
                if (!item.ShopName.Equals(""))
                {
                    sqlDeleteUser += " and ";
                    sqlDeleteUser += " SHOPNAME = '";
                    sqlDeleteUser += item.ShopName.ToString();
                    sqlDeleteUser += "'";
                }
                if (!item.DateTime.Equals(""))
                {
                    sqlDeleteUser += " and ";
                    sqlDeleteUser += " DATETIME = '";
                    sqlDeleteUser += item.DateTime.ToString();
                    sqlDeleteUser += "'";
                }

                Delete(sqlDeleteUser);
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
				updateHistory.SetLocalData(item.UserName, item.UserCount, item.UserPhone, item.ShopName,item.CostMoney, item.CostForUser, item.DateTime,g_tableType);
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
            int AllMoney  = 0;
            if (!countMoney.Text.Equals(""))
            {
                AllMoney = Int32.Parse(countMoney.Text);
            }
            else
            {
                MessageBox.Show("输入金额");
            }
            int CostMoney = 0;
            for (int i = 0; i < Users.Count;i++ )
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
                string searchUser = sqlUserInfo + "where USERNAME = " + "'" + item.Content.ToString() + "'";
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
                    userPhone.Text = TuserPhone.ToString();
                }
                else
                {
                    userCount.Clear();
                    userPhone.Clear();
                }
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
            isFirstClicAddList = true;
            string search = searchHistoryData;
            string sUserName = userName.Text;
            string sShopName = shopName.Text;
            bool isSelectName = false;
            bool isSelectSuccess = false;
            if (!sUserName.Equals(""))
            {
                search += "where USERNAME='";
                search += sUserName;
                search += "' ";
                isSelectName = true;
            }
            else if (sShopName.Equals(""))
            {

                if (!beginDataTime.ToString("u").Equals("0001-01-01 00:00:00Z") && !lastDataTime.ToString("u").Equals("0001-01-01 00:00:00Z"))
                {
                    search += " where datetime > '";
                    search += beginDataTime.ToString("u");
                    search += "' and datetime < '";
                    search += lastDataTime.ToString("u");
                    search += "'";

                }
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
                search += "SHOPNAME='";
                search += sShopName;
                search += "' ";
                if (!beginDataTime.ToString("u").Equals("0001-01-01 00:00:00Z") && !lastDataTime.ToString("u").Equals("0001-01-01 00:00:00Z"))
                {
                    search += " and datetime > '";
                    search += beginDataTime.ToString("u");
                    search += "' and datetime < '";
                    search += lastDataTime.ToString("u");
                    search += "'";

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
                    search += " and datetime > '";
                    search += beginDataTime.ToString("u");
                    search += "' and datetime < '";
                    search += lastDataTime.ToString("u");
                    search += "'";
                }

                search += "order by DATETIME";
                Select2(search);
            }

            string TempUserName;
            string TempShopName;
            string TempDataTime;
            g_number = 0;
            if (isSelectSuccess)
            {
                Users.Clear();
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
                    if (TempUserName.Equals("") == false || TempShopName.Equals("") == false || TempDataTime.Equals("") == false )
                    {
						if (TempDataTime.Equals("0001-01-01 00:00:00Z") == false)
                        {
                            g_number++;
                            Users.Add(new CInfoList(g_number.ToString(),TuserName.ToString(), TuserCount.ToString(), TuserPhone.ToString(), ShopName.ToString(), COSTMONEY.ToString(), COSTMONEYForUser.ToString(), sDateTime.ToString()));
                        }
					}
                }
                while (TempUserName.Equals("") == false || TempShopName.Equals("") == false || TempDataTime.Equals("") == false);
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
        public void saveItemDyInput(string sUserName,string sUserCount, string sUserPhone,string sShopName,string sCostMoney,string sCostForUser,string sDateTime)
        {
            string search = searchHistoryData;
            if (!sUserName.Equals(""))
            {
            search += "where USERNAME='";
            search += sUserName;
            search += "' ";
            }
            if (!sShopName.Equals(""))
            {
            search += "and ";
            search += "SHOPNAME='";
            search += sShopName;
            search += "' ";
            }
            if (!sDateTime.Equals(""))
            {
            search += "and ";
            search += "DATETIME='";
            search += sDateTime;
            search += "' ";
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
            temp += "('";
            temp += sUserName;
            temp += "','";
            temp += sUserCount;
            temp += "','";
            temp += sUserPhone;
            temp += "','";
            temp += sShopName;
            temp += "','";
            temp += sCostMoney;
            temp += "','";
            temp += sCostForUser;
            temp += "','";
            temp += sDateTime;            // 2008-9-4 20:02:10;
            temp += "')";

            string sql = temp;
            Insert(sql);
            }
            //end

            //start
            string searchForUser = searchHistoryDataUser;

            if (!sUserName.Equals(""))
            {
            searchForUser += "where USERNAME='";
            searchForUser += sUserName;
            searchForUser += "' ";
            }
            if (!sShopName.Equals(""))
            {
            searchForUser += "and ";
            searchForUser += "SHOPNAME='";
            searchForUser += sShopName;
            searchForUser += "' ";
            }
            if (!sUserCount.Equals(""))
            {
            searchForUser += "and ";
            searchForUser += "USERCOUNT='";
            searchForUser += sUserCount;
            searchForUser += "' ";
            }

            Select2(searchForUser);
            TuserName.Clear();
            TuserCount.Clear();
            TuserPhone.Clear();
            ShopName.Clear();
            COSTMONEY.Clear();
            COSTMONEYForUser.Clear();
            DateTimeData.Clear();

            GetMsg2(TuserName, TuserCount, TuserPhone, ShopName, COSTMONEY, COSTMONEYForUser, DateTimeData);
            TempUserName = TuserName.ToString();
            TempShopName = ShopName.ToString();
            TempDateTime = DateTimeData.ToString();
            if (TempUserName.Equals("") && TempShopName.Equals("") && TempDateTime.Equals("") &&
            !sUserName.Equals("") && !sShopName.Equals(""))
            {
            string temp = InsertHistoryDataUser;
            temp += "('";
            temp += sUserName;
            temp += "','";
            temp += sUserCount;
            temp += "','";
            temp += sUserPhone;
            temp += "','";
            temp += sShopName;
            temp += "','";
            temp += sCostMoney;
            temp += "','";
            temp += sCostForUser;
            temp += "','";
            temp += sDateTime;            // 2008-9-4 20:02:10;
            temp += "')";

            Insert(temp);
            }
            else
            {

            DateTime nowDate = Convert.ToDateTime(sDateTime);
            DateTime lastDate = Convert.ToDateTime(TempDateTime);
            if (DateTime.Compare(nowDate, lastDate) >= 0)
            {


                string temp = updateHistoryDataUser;
                temp += "SET ";

                temp += "COSTMONEY = '";
                temp += sCostMoney;
                temp += "', ";

                temp += "COSTMONEYFORUSER = '";
                temp += sCostForUser;
                temp += "', ";

                temp += "DATETIME = '";
                temp += sDateTime;
                temp += "' WHERE ";

                temp += "USERNAME = '";
                temp += sUserName;
                temp += "' and ";

                temp += "USERCOUNT = '";
                temp += sUserCount;
                temp += "' and ";

                temp += "USERPHONE = '";
                temp += sUserPhone;
                temp += "' and ";

                temp += "SHOPNAME = '";
                temp += sShopName;
                temp += "' ";

                Delete(temp);
            }
            //"SET USERNAME , USERCOUNT , USERPHONE ,SHOPNAME, COSTMONEY ,COSTMONEYFORUSER ,DATETIME) VALUES";
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
			if(g_tableType == 1)
			{
				//infoList.ContextMenu.Visibility = Visibility.Hidden;
			}
            Users.Clear();

            string search = "select * from (select * from (";

			search += searchHistoryData;
			if (!shopName.Text.Equals(""))
			{
				search += " where SHOPNAME='";
				search += shopName.Text.ToString();
				search += "'";
			}
			else
			{
				MessageBox.Show("请输入商户!");
				return ;
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
                    Users.Add(new CInfoList(g_number.ToString(),TuserName.ToString(), TuserCount.ToString(), TuserPhone.ToString(), ShopName.ToString(), COSTMONEY.ToString(), COSTMONEYForUser.ToString(), sDateTime.ToString()));
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
    }
}
