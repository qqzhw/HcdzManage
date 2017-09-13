using Hcdz.ModulePcie.Models;
using Pvirtech.Framework.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Hcdz.ModulePcie.ViewModels
{
    
    public class ContextMenuBehavior
    {
        private readonly RadGridView gridView = null;
        private readonly FrameworkElement contextMenu = null;

        public static readonly DependencyProperty ContextmenuPropery =
        DependencyProperty.RegisterAttached("ContextMenu", typeof(FrameworkElement), typeof(ContextMenuBehavior),
            new PropertyMetadata(new PropertyChangedCallback(OnIsEnabledPropertyChanged)));

        public static void SetContextMenu(DependencyObject dependencyObject, FrameworkElement contextmenu)
        {
            dependencyObject.SetValue(ContextmenuPropery, contextmenu);
        }

        public static FrameworkElement GetContextMenu(DependencyObject dependencyObject)
        {
            return (FrameworkElement)dependencyObject.GetValue(ContextmenuPropery);
        }

        public static void OnIsEnabledPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            RadGridView grid = dependencyObject as RadGridView;
            FrameworkElement contextMenu = e.NewValue as FrameworkElement;

            if (grid != null && contextMenu != null)
            {
                ContextMenuBehavior behavior = new ContextMenuBehavior(grid, contextMenu);
            }
        }

        public ContextMenuBehavior(RadGridView grid, FrameworkElement contextMenu)
        {
            this.gridView = grid;
            this.contextMenu = contextMenu;

            (contextMenu as RadContextMenu).Opened += RadContextMenu_Opened;
            (contextMenu as RadContextMenu).ItemClick += RadContextMenu_ItemClick;
        }

        private void RadContextMenu_ItemClick(object sender, RadRoutedEventArgs e)
        {
            RadContextMenu menu = (RadContextMenu)sender;
            RadMenuItem clickedItem = e.OriginalSource as RadMenuItem;
            GridViewRow row = menu.GetClickedElement<GridViewRow>();

            if (clickedItem != null && row != null)
            {
                string header = Convert.ToString(clickedItem.Header);

                switch (header)
                {
                    case "新建":
                        gridView.BeginInsert();
                        break;
                    case "编辑":
                        gridView.BeginEdit();
                        break;
                    case "删除":
                        DirectoryInfoModel directoryInfoModel = row.DataContext as DirectoryInfoModel;
                        gridView.Items.Remove(row.DataContext);
                        if (File.Exists(directoryInfoModel.FullName))
                        {
                            try
                            {
                                File.Delete(directoryInfoModel.FullName);
                                Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                                    RadWindow.Alert(new DialogParameters
                                    {
                                        Content = "删除成功！",
                                        DefaultPromptResultValue = directoryInfoModel.Name,
                                        Theme = new Windows8Theme(),
                                        Header = "提示",
                                        TopOffset = 30,
                                        OkButtonContent="关闭",                                        
                                    });
                                }));
                               
                            }                            
                            catch (Exception ex)
                            {
                                LogHelper.ErrorLog(ex,"删除文件");
                                Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                                    RadWindow.Alert(new DialogParameters
                                    {
                                        Content = "删除失败！",
                                        DefaultPromptResultValue = directoryInfoModel.Name,
                                        Theme = new Windows8Theme(),
                                        Header = "提示",
                                        TopOffset = 30,
                                        OkButtonContent = "关闭",
                                    });
                                }));
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void RadContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            RadContextMenu menu = (RadContextMenu)sender;
            GridViewRow row = menu.GetClickedElement<GridViewRow>();

            if (row != null)
            {
                row.IsSelected = row.IsCurrent = true;
                GridViewCell cell = menu.GetClickedElement<GridViewCell>();
                if (cell != null)
                {
                    cell.IsCurrent = true;
                }
            }
            else
            {
                menu.IsOpen = false;
            }
        }
    }
}
