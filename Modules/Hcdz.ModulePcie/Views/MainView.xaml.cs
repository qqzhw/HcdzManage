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
using System.Windows.Threading;

namespace Hcdz.ModulePcie.Views
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainView : UserControl
	{
        private DispatcherTimer dispatcherTimer;
        public MainView()
		{
			InitializeComponent();
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(10);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Start();
		}

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            int s = 8;
        }
    }
}
