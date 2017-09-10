using System;
using System.Collections.Concurrent;
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
using Telerik.Windows.Controls;

namespace Hcdz.ModulePcie.Views
{
    /// <summary>
    /// FilesView.xaml 的交互逻辑
    /// </summary>
    public partial class FilesView : UserControl
    {
        public FilesView()
        {
            InitializeComponent();
            //StyleManager.ApplicationTheme = StyleManager.
            ConcurrentQueue<Byte[]> concurrentQueue = new ConcurrentQueue<byte[]>();
            byte[] result;
            var flag=concurrentQueue.TryDequeue(out result);
            if (flag)
            {
                var s = 8;
            }
            concurrentQueue.Enqueue(new byte[16]);
            
        }
    }
}
