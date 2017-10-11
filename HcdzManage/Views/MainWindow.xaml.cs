using HcdzManage.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

namespace HcdzManage.Views
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow 
	{
		public MainWindow()
        {
            InitializeComponent();
           
        }

        private  async void NewMethod()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var buffer = new byte[80 * 1024];
                var response = await httpClient.GetAsync(new Uri("http://localhost:8080/api/download/getbackup?filepath=d:\\bar1\\20170925000817"));
                if (response.IsSuccessStatusCode)
                {
                    var stream = response.Content.ReadAsStreamAsync().Result;
                    var file = AppDomain.CurrentDomain.BaseDirectory + "dddd";
                    var finfo = new FileInfo(file);

                    if (finfo.Directory == null)
                    {
                        Console.WriteLine("Wrong file path!");
                        return;
                    }

                    if (!finfo.Directory.Exists) finfo.Directory.Create();

                    Console.WriteLine("Downloading data ...");
                    using (var wrtr = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length))
                    {
                        var read = 0;
                        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            wrtr.Write(buffer, 0, read);
                        }
                        wrtr.Flush();
                        wrtr.Close();
                    }

                    Console.WriteLine("Data downloaded!");

                    stream.Close();
                }
                else
                {
                    Console.WriteLine("Response Failed");
                }
            }
        }
    }
}
