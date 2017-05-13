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
		List<string> m_shopList = new List<string>();
		List<userInfo> m_userList = new List<userInfo>();

        class CInfoList
        {
            public CInfoList(string sUserName,
            string sUserCount,
            string sUserPhone)
            {
                UserName = sUserName;
                UserCount = sUserCount;
                UserPhone = sUserPhone;
            }
            public string UserName { get; set; }
            public string UserCount { get; set; }
            public string UserPhone { get; set; }
        }

        ManalWindow m_manger;
        private ObservableCollection<CInfoList> Users = new ObservableCollection<CInfoList>();
        public AddUser(ManalWindow mangerMoney)
        {
            m_manger = mangerMoney;
            InitializeComponent();

            ListCollectionView cs = new ListCollectionView(Users);
            infoList.ItemsSource = cs;

            Users.Clear();
            Select(m_manger.sqlUserInfo);
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
                    Users.Add(new CInfoList(TuserName.ToString(), TuserCount.ToString(), TuserPhone.ToString()));
                }
            }
            while (Name.Equals("") == false);
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
                sql += "USERNAME = '";
                sql += item.UserName.ToString();
                sql += "'";
                if (!item.UserCount.Equals(""))
                {
                    sql += " and ";
                    sql += "USERCOUNT='";
                    sql += item.UserCount.ToString();
                    sql += "'";
                    if (!item.UserPhone.Equals(""))
                    {
                        sql += " and ";
                        sql += " USERPHONE = '";
                        sql += item.UserPhone.ToString();
                        sql += "' ";
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
                sql += "('";
                sql += userName.Text;
                sql += "','";
                sql += userCount.Text;
                sql += "','";
                sql += userPhone.Text;
                sql += "')";
                Insert(sql);
            }
            m_manger.Update();

            Users.Clear();
            Select(m_manger.sqlUserInfo);
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
                    Users.Add(new CInfoList(TuserName.ToString(), TuserCount.ToString(), TuserPhone.ToString()));
                }
            }
            while (Name.Equals("") == false);
        }

		private void AddShop_Click(object sender, RoutedEventArgs e)
		{
            if (!userName.Text.Equals("") && !userCount.Text.Equals(""))
            {
                string sql = cmdInsertShop;
                sql += "('";
                sql += userName.Text;
                sql += "','";
                sql += userCount.Text;
                sql += "','";
                sql += userPhone.Text;
                sql += "')";
                Insert(sql);
            }
            m_manger.Update();

            Users.Clear();
            Select(m_manger.sqlShop);
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
                    Users.Add(new CInfoList(TuserName.ToString(), TuserCount.ToString(), TuserPhone.ToString()));
                }
            }
            while (Name.Equals("") == false);
		}

		private void AddToDb_Click(object sender, RoutedEventArgs e)
		{
			string cmdSql = "";
			if (chooseUserOrShop.SelectionBoxItem.Equals("用户"))
			{
				sqlAdd_Click(sender, e);
				cmdSql = m_manger.sqlUserInfo;
			}
			if (chooseUserOrShop.SelectionBoxItem.Equals("商户"))
			{
				AddShop_Click(sender, e);
				cmdSql = m_manger.sqlShop;
			}
            userName.Clear();
            userCount.Clear();
            userPhone.Clear();
            Users.Clear();
            
			//updateAllInfo();

			Select(cmdSql);
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
                    Users.Add(new CInfoList(TuserName.ToString(), TuserCount.ToString(), TuserPhone.ToString()));
                }
            }
            while (Name.Equals("") == false);
		}

		private void search_Click(object sender, RoutedEventArgs e)
		{
			string selectSql = "";
			if(chooseUserOrShop.SelectionBoxItem.Equals("用户"))
			{
				selectSql = m_manger.sqlUserInfo;
			}
			if (chooseUserOrShop.SelectionBoxItem.Equals("商户"))
			{
				selectSql = m_manger.sqlShop;
			}

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
                    Users.Add(new CInfoList(TuserName.ToString(), TuserCount.ToString(), TuserPhone.ToString()));
                }
            }
            while (Name.Equals("") == false);
		}
		private void InsertIfNotExit(string name,string count,string phone,string shop)
		{
            string insertToHistoryData = "INSERT INTO HISTORYDATA (USERNAME , USERCOUNT , USERPHONE ,SHOPNAME, COSTMONEY ,COSTMONEYFORUSER ,DATETIME) VALUES";
            insertToHistoryData += "('";
            insertToHistoryData += name;
            insertToHistoryData += "','";
            insertToHistoryData += count;
            insertToHistoryData += "','";
            insertToHistoryData += phone;
            insertToHistoryData += "','";

            insertToHistoryData += shop;
            insertToHistoryData += "','";
            insertToHistoryData += "";
            insertToHistoryData += "','";
            insertToHistoryData += "";
            insertToHistoryData += "','";
            insertToHistoryData += "0001-01-01 00:00:00Z";
            insertToHistoryData += "')";
            Insert(insertToHistoryData);

            return;
		}		

		private void updateAllInfo()
		{
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

            Delete("DELETE from HISTORYDATA where DATETIME='0001-01-01 00:00:00Z'");
            Console.Write(m_userList.Count);
            Console.Write("\r\n");
            Console.Write(m_shopList.Count);
            Console.Write("\r\n");
			foreach (var useritem in m_userList)
			{
				foreach (var shopitem in m_shopList)
				{
					InsertIfNotExit(useritem.userName, useritem.userCount, useritem.userPhone, shopitem.ToString());
				}
			}
}

		private void SaveAllData_Click(object sender, RoutedEventArgs e)
		{
			updateAllInfo();
		}
    }
}
