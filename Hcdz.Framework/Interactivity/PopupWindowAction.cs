using System;
using System.Windows;
using System.Windows.Interactivity;
using Microsoft.Practices.ServiceLocation;
using Pvirtech.Framework.Interactivity.DefaultPopupWindows;
using Pvirtech.Metro.Controls;
using Pvirtech.Framework.Common;

namespace Pvirtech.Framework.Interactivity
{
    /// <summary>
    /// Shows a popup window in response to an <see cref="InteractionRequest"/> being raised.
    /// </summary>
    public class PopupWindowAction : TriggerAction<FrameworkElement>
    {
        /// <summary>
        /// The content of the child window to display as part of the popup.
        /// </summary>
        public static readonly DependencyProperty WindowContentProperty =
            DependencyProperty.Register(
                "WindowContent",
                typeof(FrameworkElement),
                typeof(PopupWindowAction),
                new PropertyMetadata(null));

        /// <summary>
        /// The type of content of the child window to display as part of the popup.
        /// </summary>
        public static readonly DependencyProperty WindowContentTypeProperty =
            DependencyProperty.Register(
                "WindowContentType",
                typeof(Type),
                typeof(PopupWindowAction),
                new PropertyMetadata(null));

        /// <summary>
        /// Determines if the content should be shown in a modal window or not.
        /// </summary>
        public static readonly DependencyProperty IsModalProperty =
            DependencyProperty.Register(
                "IsModal",
                typeof(bool),
                typeof(PopupWindowAction),
                new PropertyMetadata(null));

		public static readonly DependencyProperty HasOwnerProperty =
		  DependencyProperty.Register(
			  "HasOwner",
			  typeof(bool),
			  typeof(PopupWindowAction),
			  new PropertyMetadata(null));
		public static readonly DependencyProperty IsMaximizedProperty =
		  DependencyProperty.Register(
			  "IsMaximized",
			  typeof(bool),
			  typeof(PopupWindowAction),
			  new PropertyMetadata(null));
		
		/// <summary>
		/// Determines if the content should be initially shown centered over the view that raised the interaction request or not.
		/// </summary>
		public static readonly DependencyProperty CenterOverAssociatedObjectProperty =
            DependencyProperty.Register(
                "CenterOverAssociatedObject",
                typeof(bool),
                typeof(PopupWindowAction),
                new PropertyMetadata(null));

        /// <summary>
        /// If set, applies this WindowStartupLocation to the child window.
        /// </summary>
        public static readonly DependencyProperty WindowStartupLocationProperty =
            DependencyProperty.Register(
                "WindowStartupLocation",
                typeof(WindowStartupLocation?),
                typeof(PopupWindowAction),
                new PropertyMetadata(null));

        /// <summary>
        /// If set, applies this Style to the child window.
        /// </summary>
        public static readonly DependencyProperty WindowStyleProperty =
            DependencyProperty.Register(
                "WindowStyle",
                typeof(Style),
                typeof(PopupWindowAction),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the content of the window.
        /// </summary>
        public FrameworkElement WindowContent
        {
            get { return (FrameworkElement)GetValue(WindowContentProperty); }
            set { SetValue(WindowContentProperty, value); }
        }

        /// <summary>
        /// Gets or sets the type of content of the window.
        /// </summary>
        public Type WindowContentType
        {
            get { return (Type)GetValue(WindowContentTypeProperty); }
            set { SetValue(WindowContentTypeProperty, value); }
        }

        /// <summary>
        /// Gets or sets if the window will be modal or not.
        /// </summary>
        public bool IsModal
        {
            get { return (bool)GetValue(IsModalProperty); }
            set { SetValue(IsModalProperty, value); }
        }

		public bool HasOwner
		{
			get { return (bool)GetValue(HasOwnerProperty); }
			set { SetValue(HasOwnerProperty, value); }
		}
		public bool IsMaximized
		{
			get { return (bool)GetValue(IsMaximizedProperty); }
			set { SetValue(IsMaximizedProperty, value); }
		}
		
		/// <summary>
		/// Gets or sets if the window will be initially shown centered over the view that raised the interaction request or not.
		/// </summary>
		public bool CenterOverAssociatedObject
        {
            get { return (bool)GetValue(CenterOverAssociatedObjectProperty); }
            set { SetValue(CenterOverAssociatedObjectProperty, value); }
        }

        /// <summary>
        /// Gets or sets the startup location of the Window.
        /// </summary>
        public WindowStartupLocation? WindowStartupLocation
        {
            get { return (WindowStartupLocation?)GetValue(WindowStartupLocationProperty); }
            set { SetValue(WindowStartupLocationProperty, value); }
        }
		
		/// <summary>
		/// Gets or sets the Style of the Window.
		/// </summary>
		public Style WindowStyle
        {
            get { return (Style)GetValue(WindowStyleProperty); }
            set { SetValue(WindowStyleProperty, value); }
        }

        /// <summary>
        /// Displays the child window and collects results for <see cref="IInteractionRequest"/>.
        /// </summary>
        /// <param name="parameter">The parameter to the action. If the action does not require a parameter, the parameter may be set to a null reference.</param>
        protected override void Invoke(object parameter)
        {
            var args = parameter as InteractionRequestedEventArgs;
            if (args == null)
            {
                return;
            }

            // If the WindowContent shouldn't be part of another visual tree.
            if (this.WindowContent != null && this.WindowContent.Parent != null)
            {
                return;
            }

            Window wrapperWindow = this.GetWindow(args.Context); 
            // We invoke the callback when the interaction's window is closed.
            var callback = args.Callback;
            EventHandler handler = null;
            handler =
                (o, e) =>
                {
                    var findWindow = PopupWindows.CurrentWidows.Find(c => c.Uid == args.Context.Id);
                    if (findWindow != null)
                    {
                        PopupWindows.CurrentWidows.Remove(findWindow);
                    }
                    wrapperWindow.Closed -= handler;
                    wrapperWindow.Content = null;
                    callback?.Invoke();
                };
            wrapperWindow.Closed += handler;

            if (this.CenterOverAssociatedObject && this.AssociatedObject != null)
            {
                // If we should center the popup over the parent window we subscribe to the SizeChanged event
                // so we can change its position after the dimensions are set.
                SizeChangedEventHandler sizeHandler = null;
                sizeHandler =
                    (o, e) =>
                    {
                        wrapperWindow.SizeChanged -= sizeHandler;

                        // If the parent window has been minimized, then the poition of the wrapperWindow is calculated to be off screen
                        // which makes it impossible to activate and bring into view.  So, we want to check to see if the parent window
                        // is minimized and automatically set the position of the wrapperWindow to be center screen.
                        var parentWindow = wrapperWindow;
                        if (parentWindow != null && parentWindow.WindowState == WindowState.Minimized)
                        {
                            wrapperWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            return;
                        }
						if (IsMaximized)
						{
							wrapperWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
							return;
						}
						FrameworkElement view = this.AssociatedObject;

                        // Position is the top left position of the view from which the request was initiated.
                        // On multiple monitors, if the X or Y coordinate is negative it represent that the monitor from which
                        // the request was initiated is either on the left or above the PrimaryScreen
                        Point position = view.PointToScreen(new Point(0, 0));
                        PresentationSource source = PresentationSource.FromVisual(view);
                        position = source.CompositionTarget.TransformFromDevice.Transform(position);

                        // Find the middle of the calling view.
                        // Take the width and height of the view divided by 2 and add to the X and Y coordinates.
                        var middleOfView = new Point(position.X + (view.ActualWidth / 2),
                                                     position.Y + (view.ActualHeight / 2));

                        // Set the coordinates for the top left part of the wrapperWindow.
                        // Take the width of the wrapperWindow, divide it by 2 and substract it from 
                        // the X coordinate of middleOfView. Do the same thing for the Y coordinate.
                        // If the wrapper window is wider or taller than the view, it will be behind the view.
                        wrapperWindow.Left = middleOfView.X - (wrapperWindow.ActualWidth / 2);
                        wrapperWindow.Top = middleOfView.Y - (wrapperWindow.ActualHeight / 2);
                    };
                wrapperWindow.SizeChanged += sizeHandler;
            }

            if (this.IsModal)
            {
                wrapperWindow.ShowDialog();
            }
            else
            {
				if (IsMaximized)
				{ 
					wrapperWindow.WindowState = WindowState.Maximized; 
				} 
				wrapperWindow.Show(); 
                if (!wrapperWindow.IsActive)
                {
                    wrapperWindow.Activate();
                }
            }
        }

        /// <summary>
        /// Returns the window to display as part of the trigger action.
        /// </summary>
        /// <param name="notification">The notification to be set as a DataContext in the window.</param>
        /// <returns></returns>
        protected virtual Window GetWindow(INotification notification)
        {
            Window wrapperWindow;

            if (this.WindowContent != null || this.WindowContentType != null)
            {
                wrapperWindow = new DefaultWindow() { Notification = notification };

                if (wrapperWindow == null)
                    throw new NullReferenceException("CreateWindow cannot return null");

                // If the WindowContent does not have its own DataContext, it will inherit this one.
                wrapperWindow.DataContext = notification;
                wrapperWindow.Title = notification.Title;

                this.PrepareContentForWindow(notification, wrapperWindow);
            }
            else
            {
                wrapperWindow = this.CreateDefaultWindow(notification);
            }

            if (AssociatedObject != null)
                wrapperWindow.Owner = notification.HasOwner ? Window.GetWindow(AssociatedObject):null;
			//wrapperWindow.WindowState = IsMaximized ? WindowState.Maximized : WindowState.Normal;
			// If the user provided a Style for a Window we set it as the window's style.
			IsModal = notification.IsModal;
			if (WindowStyle != null)
                wrapperWindow.Style = WindowStyle;

            // If the user has provided a startup location for a Window we set it as the window's startup location.
            if (WindowStartupLocation.HasValue)
                wrapperWindow.WindowStartupLocation = WindowStartupLocation.Value;

            if (wrapperWindow == null)
                throw new NullReferenceException("CreateWindow cannot return null"); 

            return wrapperWindow;
        }

        /// <summary>
        /// Checks if the WindowContent or its DataContext implements <see cref="IInteractionRequestAware"/>.
        /// If so, it sets the corresponding values.
        /// </summary>
        /// <param name="notification">The notification to be set as a DataContext in the HostWindow.</param>
        /// <param name="wrapperWindow">The HostWindow</param>
        protected virtual void PrepareContentForWindow(INotification notification, Window wrapperWindow)
        {
            if (this.WindowContent != null)
            {
                // We set the WindowContent as the content of the window. 
                wrapperWindow.Content = this.WindowContent;
            }
            else if (this.WindowContentType != null)
            {
                wrapperWindow.Content = ServiceLocator.Current.GetInstance(this.WindowContentType);
            }
            else
            {
                return;
            }

            Action<IInteractionRequestAware> setNotificationAndClose = (iira) =>
            {
                iira.Notification = notification;
                iira.FinishInteraction = () => wrapperWindow.Close();
            };

            MvvmHelpers.ViewAndViewModelAction(wrapperWindow.Content, setNotificationAndClose);
        }

        /// <summary>
        /// Creates a Window that is used when providing custom Window Content
        /// </summary>
        /// <returns>The Window</returns>
        protected virtual Window CreateWindow()
        {
            return new DefaultWindow();
        }

        /// <summary>
        /// When no WindowContent is sent this method is used to create a default basic window to show
        /// the corresponding <see cref="INotification"/> or <see cref="IConfirmation"/>.
        /// </summary>
        /// <param name="notification">The INotification or IConfirmation parameter to show.</param>
        /// <returns></returns>
        protected MetroWindow CreateDefaultWindow(INotification notification)
        {
            MetroWindow window = null;
            if (notification is INormalNotification)
            {
                window = new DefaultWindow() { Notification = notification };
            }
            else if (notification is IConfirmation)
            {
                window = new DefaultConfirmationWindow() { Confirmation = (IConfirmation)notification };
            }
            else
            {
				var findItem = PopupWindows.CurrentWidows.Find(o => o.Uid == notification.Id);
				if (findItem != null)
				{
					window = findItem;
				}
				else
				{
					notification.Id = notification.Id ?? Guid.NewGuid().ToString();
					window = new DefaultNotificationWindow() { Notification = notification };
					window.Uid = notification.Id;
				}

                //notification.Id = notification.Id ?? Guid.NewGuid().ToString();
                //window = new DefaultNotificationWindow() { Notification = notification };
                //window.Uid = notification.Id;
            }

            var mainWindow = Application.Current.MainWindow;
             
              window.Uid = notification.Id; 
			 // notification.SecOwner = window;
			if (notification.SecondOwner)
			{
				// window.Owner = notification.SecOwner;
			}
			 else
			window.Owner =mainWindow;
             
            PopupWindows.CurrentWidows.Add(window);

            //  PopupWindows.CurrentWidows.Add(window);

            return window;
        }
    }
}
