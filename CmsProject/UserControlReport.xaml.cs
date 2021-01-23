using System.Windows;
using System.Windows.Controls;

namespace CmsProject
{
    /// <summary>
    /// Interaction logic for UserControlReport.xaml
    /// </summary>
    public partial class UserControlReport : UserControl
    {
        Window parentWindow = Application.Current.MainWindow;


        public UserControlReport()
        {
            InitializeComponent();

        }

        private void btnPhoto_Click(object sender, RoutedEventArgs e)
        {
            StudentIdCardDialog studentIdCardDialog = new StudentIdCardDialog() { Owner = parentWindow };

            studentIdCardDialog.ShowDialog();
        }

        private void btnStudent_Click(object sender, RoutedEventArgs e)
        {
            StudentReportDialog studentReportDialog = new StudentReportDialog() { Owner = parentWindow };
           
            studentReportDialog.ShowDialog();
        }

        private void ButtonGradeReport_Click(object sender, RoutedEventArgs e)
        {
            GradeReportDialog gradeReportDialog = new GradeReportDialog() { Owner = parentWindow };
            gradeReportDialog.ShowDialog();
        }
    }
}
