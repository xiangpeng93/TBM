using MahApps.Metro.Controls;
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
using System.Windows.Shapes;
using TBM_Client_Windows;

namespace TBM_Client_Windows
{
    /// <summary>
    /// SaveToXls.xaml 的交互逻辑
    /// </summary>
    public partial class SaveToXls : MetroWindow
    {
        ManalWindow g_mangerMoney;
        public SaveToXls(ManalWindow mangerMoney)
		{
			InitializeComponent();
            g_mangerMoney = mangerMoney;
		}
		
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = inputFileName.Text.ToString(); // Default file name
            dlg.DefaultExt = ".xls"; // Default file extension
            dlg.Filter = "Excel 工作薄 (.xls)|*.xls"; // Filter files by extension
			
            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();
           
            // Process save file dialog box results
            if (result == true)
            {
               
				var initByFileName = g_mangerMoney.getScope().GetVariable<Func<object,object>>("initByFileName");

				var tInfoList = initByFileName(dlg.FileName);

                var insertDataByFileName = g_mangerMoney.getScope().GetVariable<Func<object, object, object, object, object, object, object, object, object>>("insertInfoByFileName");

                for (int i = 0; i < g_mangerMoney.Users.Count; i++)
                {
                    insertDataByFileName(tInfoList, g_mangerMoney.Users[i].UserName, 
                        g_mangerMoney.Users[i].UserCount,
                        g_mangerMoney.Users[i].UserPhone,
                        g_mangerMoney.Users[i].ShopName,
                        g_mangerMoney.Users[i].CostMoney,
                        g_mangerMoney.Users[i].CostForUser,
                        g_mangerMoney.Users[i].DateTime);
                }
                var saveInfoListToFile = g_mangerMoney.getScope().GetVariable<Func<object,object,object>>("wireFileByList");
				saveInfoListToFile(dlg.FileName, tInfoList);
                // Save document
                outputFileName.Text = dlg.FileName;
            }
            else
            {
                return;
            }
            
        }
    }
}
