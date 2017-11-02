using Hcdz.ModulePcie.Models;
using Microsoft.Practices.ServiceLocation;
using Pvirtech.Framework.Common;
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

namespace Hcdz.ModulePcie.Views
{
    /// <summary>
    /// HardDriveView.xaml 的交互逻辑
    /// </summary>
    public partial class HardDriveView : UserControl
    {
        public HardDriveView()
        {
            InitializeComponent();
        }

		private async void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			var menuItem = (sender as MenuItem).DataContext as DriveInfoModel;
			if (menuItem == null)
				return;
            var hcdzClient = ServiceLocator.Current.GetInstance<IHcdzClient>();
            try
            {
                var result = await hcdzClient.FormatDrive(menuItem.DriveLetter);
                if (result)
                {
                    MessageBox.Show("格式化成功!");
                }
                else
                {
                    MessageBox.Show("格式化失败!");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog(ex);
                MessageBox.Show("格式化磁盘出现异常,请重新连接，重试!");
            }
           
        }
    }
}
