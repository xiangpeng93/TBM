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
using TBM_Client_Windows;

namespace TBM_Client_Windows
{
    /// <summary>
    /// AddUser.xaml 的交互逻辑
    /// </summary>
    public partial class AddUser : MetroWindow
    {
        string cmdInsert = "INSERT INTO USERINFO (USERNAME , USERCOUNT , USERPHONE ) VALUES ";
        string cmdInsertShop = "INSERT INTO SHOPINFO (USERNAME , USERCOUNT , USERPHONE ) VALUES ";
        string cmdDelete = "delete from USERINFO where ";
        string cmdDeleteShop = "delete from SHOPINFO where ";


        [DllImport("TBMClient.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void Init();

        [DllImport("TBMClient.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void Fini();

        [DllImport("TBMClient.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void Select(string sql);

        [DllImport("TBMClient.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void Insert(string sql);

        [DllImport("TBMClient.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void Delete(string sql);

        [DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void GetMsg(StringBuilder userName, StringBuilder userCount, StringBuilder userPhone);

		struct userInfo
		{
			public string userName;
			public string userCount;
			public string userPhone;
	
		}
        private static System.Windows.Threading.DispatcherTimer UpdateUserInfoTimer = new System.Windows.Threading.DispatcherTimer();  
		List<string> m_shopList = new List<string>();
		List<userInfo> m_userList = new List<userInfo>();
        static int g_number = 0;
        class CInfoList
        {
            public CInfoList(string sIdNumber, string sUserName,
            string sUserCount,
            string sUserPhone)
            {
                IDNumber = sIdNumber;
                UserName = sUserName;
                UserCount = sUserCount;
                UserPhone = sUserPhone;
            }
            public string UserName { get; set; }
            public string UserCount { get; set; }
            public string UserPhone { get; set; }
            public string IDNumber { get; set; }
        }

        public ManalWindow m_manger;
        private ObservableCollection<CInfoList> Users = new ObservableCollection<CInfoList>();
        bool g_isStartUpdate = false;
        List<string> UpdateInfoList = new List<string>();
        public void timerUpdateInfo(object sender, EventArgs e)
        {
            if (g_isStartUpdate)
            {
                if (UpdateInfoList.Count > 0)
                {
                    Insert(UpdateInfoList[0]);
                    UpdateInfoList.Remove(UpdateInfoList[0]);
                    UpdateProgressBar.Value++;
                    int status = (Int32)((UpdateProgressBar.Value / UpdateProgressBar.Maximum) * 100);
                    UpdateInfoProcessTextBlock.Text = "更新信息进度:" + status.ToString() + "%";
                    Console.WriteLine(UpdateInfoProcessTextBlock.Text);
                }
                else   
                g_isStartUpdate = false;
            }
        }

		public void UpdateUserNameList()
		{
			userParentName.Items.Clear();
			Select(m_manger.sqlUserInfoOrderByName);
			string Name = "";
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
					ComboBoxItem temp = new ComboBoxItem();
					temp.Content = Name;
					userParentName.Items.Add(temp);

				}
			}
			while (Name.Equals("") == false);
			userParentName.SelectedIndex = -1;
		}

        public AddUser(ManalWindow mangerMoney)
        {
            g_number = 0;
            m_manger = mangerMoney;
            InitializeComponent();

            ListCollectionView cs = new ListCollectionView(Users);
            infoList.ItemsSource = cs;

            Users.Clear();
            Select(m_manger.sqlUserInfoOrderByName);
            string Name = "";

            do
            {
                StringBuilder TuserName = new StringBuilder(2048);
                StringBuilder TuserCount = new StringBuilder(2048);
                StringBuilder TuserPhone = new StringBuilder(2048);
                GetMsg(TuserName, TuserCount, TuserPhone);
                Name = TuserName.ToString();
                if (Name.Equals("") == false)
                {
                    g_number++;
                    Users.Add(new CInfoList(g_number.ToString(),TuserName.ToString(), TuserCount.ToString(), TuserPhone.ToString()));
                }
            }
            while (Name.Equals("") == false);

            UpdateUserInfoTimer.Tick += new EventHandler(timerUpdateInfo);
            UpdateUserInfoTimer.Interval = new TimeSpan(0, 0, 0, 0);
            UpdateUserInfoTimer.Start();

			UpdateUserNameList();
        }

        private void backMainWindows_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            m_manger.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
            m_manger.Show();
            UpdateUserInfoTimer.Stop();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            object o = infoList.SelectedItem;
            if (o == null)
                return;
            CInfoList item = o as CInfoList;
            string sql = "";
			if (chooseUserOrShop.SelectionBoxItem.Equals("用户"))
			{
				sql = cmdDelete;
			}
			if (chooseUserOrShop.SelectionBoxItem.Equals("商户"))
			{
				sql = cmdDeleteShop;
			}

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
                    if (!item.UserPhone.Equals(""))
                    {
                        sql += " and ";
                        sql += " USERPHONE = \"";
                        sql += item.UserPhone.ToString();
                        sql += "\" ";
                    }
                }
                Delete(sql);
            }
            Users.Remove(item);
            m_manger.Update();
        }

        private void sqlAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!userName.Text.Equals("") && !userCount.Text.Equals(""))
            {
                string sql = cmdInsert;
                sql += "(\"";
                sql += userName.Text;
                sql += "\",\"";
                sql += userCount.Text;
                sql += "\",\"";
                sql += userParentName.Text;
                sql += "\")";
                Insert(sql);
            }
            m_manger.Update();

            Users.Clear();
            Select(m_manger.sqlUserInfoOrderByName);
            string Name = "";
            g_number = 0;
            do
            {
                StringBuilder TuserName = new StringBuilder(2048);
                StringBuilder TuserCount = new StringBuilder(2048);
                StringBuilder TuserPhone = new StringBuilder(2048);
                GetMsg(TuserName, TuserCount, TuserPhone);
                Name = TuserName.ToString();
                if (Name.Equals("") == false)
                {
                    g_number++;
                    Users.Add(new CInfoList(g_number.ToString(),TuserName.ToString(), TuserCount.ToString(), TuserPhone.ToString()));
                }
            }
            while (Name.Equals("") == false);
        }

		private void AddShop_Click(object sender, RoutedEventArgs e)
		{
            if (!userName.Text.Equals("") && !userCount.Text.Equals(""))
            {
                string sql = cmdInsertShop;
                sql += "(\"";
                sql += userName.Text;
                sql += "\",\"";
                sql += userCount.Text;
                sql += "\",\"";
                sql += userParentName.Text;
                sql += "\")";
                Insert(sql);
            }
            m_manger.Update();

            Users.Clear();
            Select(m_manger.sqlShop);
            string Name = "";
            g_number = 0;
            do
            {
                StringBuilder TuserName = new StringBuilder(2048);
                StringBuilder TuserCount = new StringBuilder(2048);
                StringBuilder TuserPhone = new StringBuilder(2048);
                GetMsg(TuserName, TuserCount, TuserPhone);
                Name = TuserName.ToString();
                if (Name.Equals("") == false)
                {
                    Users.Add(new CInfoList(g_number.ToString(),TuserName.ToString(), TuserCount.ToString(), TuserPhone.ToString()));
                }
            }
            while (Name.Equals("") == false);
		}

        private bool isInDb(string sql)
        {
            Select(sql);

            StringBuilder TuserName = new StringBuilder(2048);
            StringBuilder TuserCount = new StringBuilder(2048);
            StringBuilder TuserPhone = new StringBuilder(2048);
            GetMsg(TuserName, TuserCount, TuserPhone);
            string tmpName = TuserName.ToString();
            if (tmpName.Equals("") == false)
            {
                return true;
            }
            return false;
        }
		private void AddToDb_Click(object sender, RoutedEventArgs e)
		{
			string cmdSql = "";
			if (chooseUserOrShop.SelectionBoxItem.Equals("用户"))
			{
                string sqlCheckIndb = m_manger.sqlUserInfo + "where USERNAME=\"" + userName.Text + "\" and USERCOUNT=\"" + userCount.Text + "\"";
                if (isInDb(sqlCheckIndb))
                {
                    MessageBox.Show("插入失败,该用户已存在!");
                    return;
                }
				sqlAdd_Click(sender, e);
                cmdSql = m_manger.sqlUserInfoOrderByName;
			}
			if (chooseUserOrShop.SelectionBoxItem.Equals("商户"))
			{
                string sqlCheckIndb = m_manger.sqlShopNoOrder + " where USERNAME=\"" + userName.Text + "\" and USERCOUNT=\"" + userCount.Text + "\"";
                if (isInDb(sqlCheckIndb))
                {
                    MessageBox.Show("插入失败,该用户已存在!");
                    return;
                }
				AddShop_Click(sender, e);
				cmdSql = m_manger.sqlShop;
			}
            userName.Clear();
            userCount.Clear();
			userParentName.SelectedIndex = -1;
            Users.Clear();
            
			//updateAllInfo();

			Select(cmdSql);
			string Name = "";
            g_number = 0;
            do
            {
                StringBuilder TuserName = new StringBuilder(2048);
                StringBuilder TuserCount = new StringBuilder(2048);
                StringBuilder TuserPhone = new StringBuilder(2048);
                GetMsg(TuserName, TuserCount, TuserPhone);
                Name = TuserName.ToString();
                if (Name.Equals("") == false)
                {
                    g_number++;
                    Users.Add(new CInfoList(g_number.ToString(),TuserName.ToString(), TuserCount.ToString(), TuserPhone.ToString()));
                }
            }
            while (Name.Equals("") == false);
			UpdateUserNameList();
		}
        public void Search_Click_public()
        {
            string selectSql = "";
            if (chooseUserOrShop.SelectionBoxItem.Equals("用户"))
            {
                selectSql = m_manger.sqlUserInfoOrderByName;
            }
            if (chooseUserOrShop.SelectionBoxItem.Equals("商户"))
            {
                selectSql = m_manger.sqlShop;
            }

            m_manger.Update();

            Users.Clear();
            Select(selectSql);
            string Name = "";
            g_number = 0;
            do
            {
                StringBuilder TuserName = new StringBuilder(2048);
                StringBuilder TuserCount = new StringBuilder(2048);
                StringBuilder TuserPhone = new StringBuilder(2048);
                GetMsg(TuserName, TuserCount, TuserPhone);
                Name = TuserName.ToString();
                if (Name.Equals("") == false)
                {
                    g_number++;
                    Users.Add(new CInfoList(g_number.ToString(), TuserName.ToString(), TuserCount.ToString(), TuserPhone.ToString()));
                }
            }
            while (Name.Equals("") == false);
        }
		private void search_Click(object sender, RoutedEventArgs e)
		{
            Search_Click_public();
		}
    
		private void InsertIfNotExit(string name,string count,string phone,string shop)
		{
            string insertToHistoryData = "INSERT INTO HISTORYDATA (USERNAME , USERCOUNT , USERPHONE ,SHOPNAME, COSTMONEY ,COSTMONEYFORUSER ,DATETIME) VALUES";
            insertToHistoryData += "(\"";
            insertToHistoryData += name;
            insertToHistoryData += "\",\"";
            insertToHistoryData += count;
            insertToHistoryData += "\",\"";
            insertToHistoryData += phone;
            insertToHistoryData += "\",\"";

            insertToHistoryData += shop;
            insertToHistoryData += "\",\"";
            insertToHistoryData += "";
            insertToHistoryData += "\",\"";
            insertToHistoryData += "";
            insertToHistoryData += "\",\"";
            insertToHistoryData += "0001-01-01 00:00:00Z";
            insertToHistoryData += "\")";
            UpdateInfoList.Add(insertToHistoryData);
            

            return;
		}		

		private void updateAllInfo()
		{
            Delete("DELETE from HISTORYDATA where DATETIME=\"0001-01-01 00:00:00Z\"");
            Console.Write(m_userList.Count);
            Console.Write("\r\n");
            Console.Write(m_shopList.Count);
            Console.Write("\r\n");
            //UpdateProgressBar.Maximum = m_userList.Count * m_shopList.Count;
			foreach (var useritem in m_userList)
			{
				foreach (var shopitem in m_shopList)
				{
					InsertIfNotExit(useritem.userName, useritem.userCount, useritem.userPhone, shopitem.ToString());
                    //UpdateProgressBar.Value++;
                    //UpdateInfoProcessTextBlock.Text = "更新信息进度:" + ((UpdateProgressBar.Value / UpdateProgressBar.Maximum)*100).ToString() + "%";
                    Console.WriteLine("更新信息进度:" + "%");
				}
			}
           
        }

		private void SaveAllData_Click(object sender, RoutedEventArgs e)
		{
            UpdateProgressBar.Value = 0;
            string selectSql = "";
            selectSql = m_manger.sqlShop;
            m_manger.Update();
            Users.Clear();
            Select(selectSql);
            string Name = "";
            do
            {
                StringBuilder TuserName = new StringBuilder(2048);
                StringBuilder TuserCount = new StringBuilder(2048);
                StringBuilder TuserPhone = new StringBuilder(2048);
                GetMsg(TuserName, TuserCount, TuserPhone);
                Name = TuserName.ToString();
                if (Name.Equals("") == false)
                {
                    m_shopList.Add(Name);

                }
            }
            while (Name.Equals("") == false);


            selectSql = m_manger.sqlUserInfo;
            m_manger.Update();
            Users.Clear();
            Select(selectSql);
            do
            {
                StringBuilder TuserName = new StringBuilder(2048);
                StringBuilder TuserCount = new StringBuilder(2048);
                StringBuilder TuserPhone = new StringBuilder(2048);
                GetMsg(TuserName, TuserCount, TuserPhone);
                Name = TuserName.ToString();
                if (Name.Equals("") == false)
                {
                    userInfo UserInfoTemp = new userInfo();
                    UserInfoTemp.userName = TuserName.ToString();
                    UserInfoTemp.userCount = TuserCount.ToString();
                    UserInfoTemp.userPhone = TuserPhone.ToString();
                    m_userList.Add(UserInfoTemp);

                }
            }
            while (Name.Equals("") == false);
            Delete("DELETE from HISTORYDATA where DATETIME=\"0001-01-01 00:00:00Z\"");

            Console.Write(m_userList.Count);
            Console.Write("\r\n");
            Console.Write(m_shopList.Count);
            Console.Write("\r\n");
            UpdateProgressBar.Maximum = m_userList.Count * m_shopList.Count;
            foreach (var useritem in m_userList)
            {
                foreach (var shopitem in m_shopList)
                {
                    InsertIfNotExit(useritem.userName, useritem.userCount, useritem.userPhone, shopitem.ToString());
                   
                }
            }

            g_isStartUpdate = true;

		}

        private void UpdateData_Click(object sender, RoutedEventArgs e)
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
                UpdateUserWinodw updateHistory = new UpdateUserWinodw(this);
                updateHistory.SetLocalData(item.UserName, item.UserCount, item.UserPhone);
                updateHistory.Show();
                this.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("名称为空！");
            }
        }

		private void userParentName_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}
    }
}
