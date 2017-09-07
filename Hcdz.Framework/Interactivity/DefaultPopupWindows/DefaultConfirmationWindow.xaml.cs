using Pvirtech.Framework.Interactivity;
using System.Windows;

namespace Pvirtech.Framework.Interactivity.DefaultPopupWindows
{
    /// <summary>
    /// Interaction logic for ConfirmationChildWindow.xaml
    /// </summary>
    public partial class DefaultConfirmationWindow 
    {
        /// <summary>
        /// Creates a new instance of ConfirmationChildWindow.
        /// </summary>
        public DefaultConfirmationWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets or gets the <see cref="IConfirmation"/> shown by this window./>
        /// </summary>
        public IConfirmation Confirmation
        {
            get
            {
                return this.DataContext as IConfirmation;
            }
            set
            {
                this.DataContext = value;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Confirmation.Confirmed = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Confirmation.Confirmed = false;
            this.Close();
        }
    }
}
