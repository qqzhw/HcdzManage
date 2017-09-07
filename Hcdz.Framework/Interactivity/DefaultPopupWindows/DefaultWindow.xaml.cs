using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
 

namespace Pvirtech.Framework.Interactivity.DefaultPopupWindows
{
    /// <summary>
    /// Interaction logic for DefaultWindow.xaml
    /// </summary>
    public partial class DefaultWindow 
    {
        public DefaultWindow()
        {
            InitializeComponent(); 
        }

        public INotification Notification
        {
            get
            {
                return this.DataContext as INotification;
            }
            set
            {
                this.DataContext = value;
            }
        }
    }
}
