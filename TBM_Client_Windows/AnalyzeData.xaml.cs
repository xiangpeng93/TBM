using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace TBM_Client_Windows
{
	/// <summary>
	/// AnalyzeData.xaml 的交互逻辑
	/// </summary>
	public partial class AnalyzeData : MetroWindow
	{
		protected class FileItem
		{
			public FileItem(string fileName)
			{
				this.m_fileName = fileName;
				
			}
			public string m_fileName{get;set;}
		};

		public class resultInfo
		{
			public string IDNumber
			{
				get;
				set;
			}
			public string keyName
			{
				get;
				set;
			}
			public string viewNum
			{
				get;
				set;
			}
			public string otherNum1
			{
				get;
				set;
			}
			public string otherNum2
			{
				get;
				set;
			}
			public string otherNum3
			{
				get;
				set;
			}
			public string otherNum4
			{
				get;
				set;
			}
			public string otherNum5
			{
				get;
				set;
			}
			public resultInfo(
							string sIdNumber,
							string sUserName,
						string sViewNum
						)
			{
				IDNumber = sIdNumber;
				keyName = sUserName;
				viewNum = sViewNum;
			}
			public resultInfo()
			{

			}
		}

		private ObservableCollection<resultInfo> m_resultInfo = new ObservableCollection<resultInfo>();
		private ObservableCollection<resultInfo> m_searchResultInfo = new ObservableCollection<resultInfo>();

static int g_IDNumber = 0;

		public AnalyzeData()
		{
			InitializeComponent();
			ListCollectionView cs = new ListCollectionView(m_resultInfo);
			listFileResult.ItemsSource = cs;
			ListCollectionView csSearch = new ListCollectionView(m_searchResultInfo);
			listSearchResult.ItemsSource = csSearch;
		}

		private void btnSelectFiles_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".xls"; // Default file extension
			dlg.Multiselect = true;
            dlg.Filter = "Excel 工作薄 (.*)|*.*"; // Filter files by extension
            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();
			if (result == true)
			{
				string pyfilename = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
				pyfilename = pyfilename.Substring(0, pyfilename.LastIndexOf('\\'));
				pyfilename += "\\saveData.py";
				var engine = IronPython.Hosting.Python.CreateEngine();
				var scope = engine.CreateScope();
				var source = engine.CreateScriptSourceFromFile(pyfilename.ToString());
				source.Execute(scope);

				var readByFileName = scope.GetVariable<Func<object, object>>("readByFileName");
				var getInfoListLen = scope.GetVariable<Func<object, object>>("getInfoNumFromListInfo");
				var getInfoFromListInfo = scope.GetVariable<Func<object, object, object, object>>("getInfoFromListInfo");
			

				foreach (var filename in dlg.FileNames)
				{
					Console.WriteLine(filename);
					FileItem file = new FileItem(filename.ToString());
					
					listFileListBox.Items.Add(file);
					
					var tInfoList = readByFileName(filename.ToString());

					var InfoNum = getInfoListLen(tInfoList);
					for(int i = 5; i < Int32.Parse(InfoNum.ToString()); i++)
					{
g_IDNumber++;
						resultInfo resultMsg = new resultInfo();
						resultMsg.IDNumber = g_IDNumber.ToString();
						resultMsg.keyName = getInfoFromListInfo(tInfoList, i, 0).ToString();
						resultMsg.viewNum = getInfoFromListInfo(tInfoList, i, 6).ToString();
						Console.WriteLine(resultMsg.keyName);
						Console.WriteLine(resultMsg.viewNum);
						m_resultInfo.Add(resultMsg);
					}
				}

			}
		}

		private void btnReSelectFiles_Click(object sender, RoutedEventArgs e)
		{
			g_IDNumber = 0;
			m_resultInfo.Clear();
			listFileListBox.Items.Clear();
		}

		private void btnStartAnalyze_Click(object sender, RoutedEventArgs e)
		{
			if (textBoxKeyName.Text.Equals(""))
			{
				return;
			}
			string[] sArray;
			if (textBoxKeyName.Text.ToString().IndexOf(',') == -1)
			{
				sArray = textBoxKeyName.Text.ToString().Split('，');
			}
			else
			{
				sArray = textBoxKeyName.Text.ToString().Split(',');
			}
			
			m_searchResultInfo.Clear();
			int iIDNumber = 0;
			foreach (string i in sArray)
			{
				Console.WriteLine(i.ToString());
				int iKeyNameNum = 0;
				foreach (var item in m_resultInfo)
				{
					if (-1 != item.keyName.ToString().IndexOf(i.ToString()))
					{
						iKeyNameNum += Int32.Parse(item.viewNum);
					}
				}
				if (iKeyNameNum > 0)
				{
					iIDNumber++;
					m_searchResultInfo.Add(new resultInfo(iIDNumber.ToString(), i, iKeyNameNum.ToString()));
				}
			}
		}

		private void btnExportData_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".xls"; // Default file extension
            dlg.Filter = "Excel 工作薄 (.xls)|*.xls"; // Filter files by extension
			
            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            string filename = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            filename = filename.Substring(0, filename.LastIndexOf('\\'));
            filename += "\\saveData.py";
            // Process save file dialog box results
            if (result == true)
            {
                var engine = IronPython.Hosting.Python.CreateEngine();
                var scope = engine.CreateScope();
                var source = engine.CreateScriptSourceFromFile(filename);
                source.Execute(scope);
				var initByFileName = scope.GetVariable<Func<object,object>>("initByFileNameEx");

				var tInfoList = initByFileName(dlg.FileName);

                var insertDataByFileName = scope.GetVariable<Func<object, object, object, object, object, object, object, object, object>>("insertInfoByFileName");

                for (int i = 0; i < m_searchResultInfo.Count; i++)
                {
                    insertDataByFileName(tInfoList, m_searchResultInfo[i].IDNumber, 
                         m_searchResultInfo[i].keyName,
                        m_searchResultInfo[i].viewNum,
                        "",
                        "",
                        "",
                        "");
                }
                var saveInfoListToFile = scope.GetVariable<Func<object,object,object>>("wireFileByList");
				saveInfoListToFile(dlg.FileName, tInfoList);
                // Save document
                MessageBox.Show("保存成功.");
            }
            else
            {
                return;
            }
		}

	}
}
