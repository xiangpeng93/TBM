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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TBM_Client_Windows
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		[DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern void Init();

		[DllImport("TBMClient", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		public static extern int Login(string ip, int port,string userName, string paswd);
		public MainWindow()
		{
			InitializeComponent();
			this.MouseLeftButtonDown += Window_Drop;
			//从数据库中读入用户信息
			Init();
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
				if (textBox_ip.Text.Equals("") || textBox_port.Text.Equals(""))
				{
					text_user_paswd_msg.Text = "服务器地址端口不能为空.";
					return;
				}

				int port = Int32.Parse(textBox_port.Text);
				
				if (0 == Login(textBox_ip.Text,port,textBox_user.Text.ToString(), passwdBox_passwd.Password.ToString()))
				{
					ManalWindow manal = new ManalWindow();
					manal.Show();
					this.Close();
				}
				else
				{
					text_user_paswd_msg.Text = "登录失败，用户名或密码错误.";
				}
				//MessageBox.Show(textBox_user.Text.ToString());
				//MessageBox.Show(passwdBox_passwd.Password.ToString());
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

		private void btn_con_Click(object sender, RoutedEventArgs e)
		{
			if(dockPanel_ip.Visibility == Visibility.Visible)
			{
				dockPanel_ip.Visibility = Visibility.Hidden;
				dockPanel_port.Visibility = Visibility.Hidden;
			}
			else
			{
				dockPanel_ip.Visibility = Visibility.Visible;
				dockPanel_port.Visibility = Visibility.Visible;
			}
		}



		//isDigit是否是数字
        public static bool isNumberic(string _string)
        {
            if (string.IsNullOrEmpty(_string))
                return false;
            foreach (char c in _string)
            {
                if (!char.IsDigit(c))
                    //if(c<'0' c="">'9')//最好的方法,在下面测试数据中再加一个0，然后这种方法效率会搞10毫秒左右
                    return false;
            }
            return true;
        }

		private void textBox_port_Pasting(object sender, DataObjectPastingEventArgs e)
		{
			if (e.DataObject.GetDataPresent(typeof(String)))
			{
				String text = (String)e.DataObject.GetData(typeof(String));
				if (!isNumberic(text))
				{
					e.CancelCommand();
				}
			}
            else { e.CancelCommand(); }
		}

		private void textBox_port_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space)
				e.Handled = true;
		}

		private void textBox_port_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!isNumberic(e.Text))
			{
				e.Handled = true;
			}
			else
				e.Handled = false;
		}
	}
}
