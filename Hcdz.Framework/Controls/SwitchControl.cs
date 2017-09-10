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

namespace Hcdz.Framework.Controls
{
	/// <summary>
	/// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
	///
	/// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
	/// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
	/// 元素中: 
	///
	///     xmlns:MyNamespace="clr-namespace:Hcdz.Framework.Controls"
	///
	///
	/// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
	/// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
	/// 元素中: 
	///
	///     xmlns:MyNamespace="clr-namespace:Hcdz.Framework.Controls;assembly=Hcdz.Framework.Controls"
	///
	/// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
	/// 并重新生成以避免编译错误: 
	///
	///     在解决方案资源管理器中右击目标项目，然后依次单击
	///     “添加引用”->“项目”->[浏览查找并选择此项目]
	///
	///
	/// 步骤 2)
	/// 继续操作并在 XAML 文件中使用控件。
	///
	///     <MyNamespace:SwitchControl/>
	///
	/// </summary>
	 
	public class SwitchControl : Control
	{
		public static DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(string), typeof(SwitchControl));
		public static DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(SwitchControl), new UIPropertyMetadata(false, new PropertyChangedCallback(OnValueChanged)));
		public static DependencyProperty SelectedBackgroundProperty = DependencyProperty.Register("SelectedBackground", typeof(Brush), typeof(SwitchControl));

		public Brush SelectedBackground
		{
			get
			{
				return (Brush)GetValue(SelectedBackgroundProperty);
			}
			set
			{
				SetValue(SelectedBackgroundProperty, value);
			}
		}

		private const string SwitchBorder = "switchBorder";
		private Border bottomBorder;
		static SwitchControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SwitchControl), new FrameworkPropertyMetadata(typeof(SwitchControl)));
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			var _toggleBorder = GetTemplateChild(SwitchBorder) as Border;
			bottomBorder = GetTemplateChild("border") as Border;
			if (_toggleBorder != null)
			{
				//_toggleBorder.MouseLeftButtonDown += ToggleBorderMouseLeftButtonDown;
				//_toggleBorder.MouseEnter += _toggleBorder_MouseEnter;
				//SBorder = _toggleBorder; 
			}
		}

		void _toggleBorder_MouseEnter(object sender, MouseEventArgs e)
		{
			//   SelectedBackground=random.Next(0,3)
		}




		public bool IsSelected
		{
			get { return (bool)GetValue(IsSelectedProperty); }
			set { SetValue(IsSelectedProperty, value); }
		}
		public string Content
		{
			get
			{
				return (string)GetValue(ContentProperty);
			}
			set
			{
				SetValue(ContentProperty, value);
			}
		}
		private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SwitchControl border = d as SwitchControl;
			if (border.IsSelected)
			{
				 border.SelectedBackground = new SolidColorBrush(Color.FromRgb(34,113,172));
			}
		}

	}
}
