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

namespace TBM_Client_Windows
{
	/// <summary>
	/// AnalyzeData.xaml 的交互逻辑
	/// </summary>
	public partial class AnalyzeData : MetroWindow
	{
		List<FileItem> g_fileNameItems  = new List<FileItem>();

		public AnalyzeData()
		{
			InitializeComponent();
		}
		protected class FileItem
		{
			public FileItem(string fileName)
			{
				this.m_fileName = fileName;
				
			}
			public string m_fileName{get;set;}
		};

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

				foreach (var filename in dlg.FileNames)
				{
					Console.WriteLine(filename);
					FileItem file = new FileItem(filename.ToString());
					
					listFileListBox.Items.Add(file);
					
				}

			}
		}

		private void btnReSelectFiles_Click(object sender, RoutedEventArgs e)
		{
			listFileListBox.Items.Clear();
		}

	}
}
