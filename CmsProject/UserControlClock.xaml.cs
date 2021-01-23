using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CmsProject
{
    /// <summary>
    /// Interaction logic for UserControlClock.xaml
    /// </summary>
    public partial class UserControlClock : UserControl
    {
        public UserControlClock()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }


        void timer_Tick(object sender, EventArgs e)
        {
            lblTime.Content = DateTime.Now.ToString("dddd , MMM dd yyyy,   hh:mm:ss");
        }
    }
}
