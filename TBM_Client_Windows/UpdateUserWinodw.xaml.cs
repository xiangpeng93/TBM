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

namespace TBM_Client_Windows
{
    /// <summary>
    /// UpdateUserWinodw.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateUserWinodw : MetroWindow
    {
        [DllImport("TBMClient.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void Select(string sql);

        [DllImport("TBMClient.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void Insert(string sql);

        [DllImport("TBMClient.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void Delete(string sql);

        [DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void GetMsg(StringBuilder userName, StringBuilder userCount, StringBuilder userPhone);


        AddUser g_addUser;
        string tUserName;
        string tUserCount;
        string tUserPhone;

public void UpdateUserNameList()
		{
			userParentName.Items.Clear();
			Select(g_addUser.m_manger.sqlUserInfoOrderByName);
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

        public UpdateUserWinodw(AddUser addUserWin)
        {
            InitializeComponent();
            g_addUser = addUserWin;
			UpdateUserNameList();
        }

        internal void SetLocalData(string name, string count, string phone)
        {
            tUserName = name;
            tUserCount = count;
            tUserPhone = phone;
            username.Text = name;
            usercount.Text = count;

            userParentName.Text = phone;
        }

        private void updateHistoryDataClick_Click(object sender, RoutedEventArgs e)
        {
            string temp;
            string HistoryDataTable = "UPDATE USERINFO ";
            temp = HistoryDataTable;
            temp += "SET ";

            temp += "USERNAME = \"";
            temp += username.Text;
            temp += "\" , ";

            temp += "USERCOUNT = \"";
            temp += usercount.Text;
            temp += "\" , ";

            temp += "USERPHONE = \"";
            temp += userParentName.Text;

            temp += "\" WHERE ";

            temp += "USERNAME = \"";
            temp += tUserName;
            temp += "\" and ";

            temp += "USERCOUNT = \"";
            temp += tUserCount;
            temp += "\" and ";

            temp += "USERPHONE = \"";
            temp += tUserPhone;
            temp += "\" ";
            Delete(temp);
            this.Close();
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            g_addUser.IsEnabled = true;
            g_addUser.Search_Click_public();
        }
    }
}
