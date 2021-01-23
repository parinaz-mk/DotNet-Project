using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CmsProject
{
    /// <summary>
    /// Interaction logic for UserControlEnrollment.xaml
    /// </summary>
    public partial class UserControlEnrollment : UserControl
    {
        Window parentWindow = Application.Current.MainWindow;
        List<Course> courseList;
        public UserControlEnrollment()
        {
            InitializeComponent();
            try
            {
                Globals.ctx = new CmsprojectDbConnection();
                LoadData();
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Fatal Error Connecting to Database:\n" + ex.Message, "Database Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }

        private void LoadData()
        {
            try
            {
                courseList = (from course in Globals.ctx.Courses select course).ToList<Course>();
                lvCourse.ItemsSource = courseList;
                lvCourse.Items.Refresh();
                Utils.AutoResizeColumns(lvCourse);
                tbCourseCode.Text = "";
                tbCourseName.Text = "";
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error!:\n" + ex.Message, "Database Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

            private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (lvCourse.SelectedIndex == -1)
            {
                MessageBox.Show("Select a course Please!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Course course = (Course)lvCourse.SelectedItem;
            EnrollementWizard enrollmentSectionDialog = new EnrollementWizard(course) { Owner = parentWindow };
            enrollmentSectionDialog.ShowDialog();
        }

        private void tbCourseName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (tbCourseName.Text == "")
                {
                    LoadData();
                }
                else
                {
                    courseList = (from course in Globals.ctx.Courses where course.CourseTitle.Contains(tbCourseName.Text) select course).ToList<Course>();
                    lvCourse.ItemsSource = courseList;
                    lvCourse.Items.Refresh();
                    Utils.AutoResizeColumns(lvCourse);
                }
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error!:\n" + ex.Message, "Database Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void tbCourseCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (tbCourseCode.Text == "")
                {
                    LoadData();
                }
                else
                {
                    courseList = (from course in Globals.ctx.Courses where course.Id.ToString() == tbCourseCode.Text select course).ToList<Course>();
                    lvCourse.ItemsSource = courseList;
                    lvCourse.Items.Refresh();
                    Utils.AutoResizeColumns(lvCourse);
                }
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error!:\n" + ex.Message, "Database Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
