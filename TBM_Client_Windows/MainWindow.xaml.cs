using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TBM_Client_Windows
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			this.MouseLeftButtonDown += Window_Drop;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void passwdBox_passwd_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key.ToString().Equals("Return"))
			{
				btn_login_Click(null,null);
			}
		}

		private void textBox_user_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key.ToString().Equals("Return"))
			{
				btn_login_Click(null,null);
			}
		}

		private void btn_login_Click(object sender, RoutedEventArgs e)
		{
			if (passwdBox_passwd.Password.Equals("") || textBox_user.Text.Equals(""))
			{
				text_user_paswd_msg.Text = "用户名密码不能为空.";
			}
			else
			{
				MessageBox.Show(textBox_user.Text.ToString());
				MessageBox.Show(passwdBox_passwd.Password.ToString());
			}
				
		}

		private void img_min_btn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}

		private void img_close_btn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.Close();
		}

		private void Window_Drop(object sender, MouseButtonEventArgs e)
		{
			this.DragMove(); 
		}

		private void btn_register_Click(object sender, RoutedEventArgs e)
		{
			text_user_paswd_msg.Text = "请联系管理员.";
		}

		private void img_max_btn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (this.WindowState == WindowState.Maximized)
				this.WindowState = WindowState.Normal;
			else
				this.WindowState = WindowState.Maximized;

		}
	}
}
