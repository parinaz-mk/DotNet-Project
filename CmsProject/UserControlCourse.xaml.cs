using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

namespace CmsProject
{
    /// <summary>
    /// Interaction logic for UserControlCourse.xaml
    /// </summary>
    public partial class UserControlCourse : UserControl
    {
        Window parentWindow = Application.Current.MainWindow;
       
        List<Course> listCourse = new List<Course>();
        public UserControlCourse()
        {
            InitializeComponent();
            try
            {
                Globals.ctx = new CmsprojectDbConnection();
                clearFieldRefresh();
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Fatal error connecting to database:\n" + ex.Message);
                Environment.Exit(1);
            }
        }
        void clearFieldRefresh()
        {
            try
            {
                var converter = new System.Windows.Media.BrushConverter();
                tbTitle.Clear();
                Utils.ChangeBackBorderControl(tbTitle);
                tbDescription.Clear();
                btnUpdate.IsEnabled = false;
                btnDelete.IsEnabled = false;
                btnAddSection.IsEnabled = false;
                //   listCourse = (from c in Globals.ctx.Courses orderby c.CourseTitle select c).ToList<Course>();
                lvCourses.ItemsSource = Globals.ctx.Courses.ToList();
                listCourse= Globals.ctx.Courses.ToList();
                Utils.AutoResizeColumns(lvCourses);
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error!" + ex.Message);
                return;
            }
        }
        void isFieldEmpty()
        {
            if (tbTitle.Text == "")
            {
                Utils.ChangeBorderControl(tbTitle);
                throw new InvalidValueException("Empty fields are not allowed!");
               
            }
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            changeBackBorderColor();
            try
            {
                isFieldEmpty();
                string title = tbTitle.Text;
                string description = tbDescription.Text;
                Course course = new Course() { CourseTitle = title, Description = description };
                Globals.ctx.Courses.Add(course);
                Globals.ctx.SaveChanges();
                clearFieldRefresh();
                MessageBox.Show("Course info successfully Added. ", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                lvCourses.SelectedIndex = -1;
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error! " + ex.Message);
            }
            catch (InvalidValueException ex)
            {
                MessageBox.Show( ex.Message, "Validation Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        void changeBackBorderColor()
        {
            var converter = new System.Windows.Media.BrushConverter();
            var brush = (Brush)converter.ConvertFromString("#FFE3E9EF");
            tbTitle.BorderBrush = brush;
        }
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            changeBackBorderColor();
            try
            {
                isFieldEmpty();
                if (MessageBox.Show("Are you sure to Update?", "Update", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                {
                    return;
                }
                Course c1 = (Course)lvCourses.SelectedItem;
                c1.CourseTitle = tbTitle.Text;
                c1.Description = tbDescription.Text;
                Globals.ctx.SaveChanges();
                clearFieldRefresh();
                lvCourses.SelectedIndex = -1;
                MessageBox.Show("Selected record successfully Updated. ", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error" + ex.Message);
            }
            catch (InvalidValueException ex)
            {
                MessageBox.Show(ex.Message, "Validation Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void lvCourses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvCourses.SelectedIndex >= 0)
            {
                try
                {
                    Course c = (Course)lvCourses.SelectedItem;
                    btnUpdate.IsEnabled = true;
                    btnDelete.IsEnabled = true;
                    btnAddSection.IsEnabled = true;
                    tbDescription.Text = c.Description;
                    tbTitle.Text = c.CourseTitle;
                }
                catch (ArgumentOutOfRangeException)
                {
                    return;
                }
            }
            else
            {
                btnUpdate.IsEnabled = false;
                btnDelete.IsEnabled = false;
                btnAddSection.IsEnabled = false;
                tbTitle.Clear();
                tbDescription.Clear();
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            changeBackBorderColor();
            try
            {
               Course c1 = (Course)lvCourses.SelectedItem;
               var sec= (from c in Globals.ctx.Sections where c.CourseId == c1.Id select c).FirstOrDefault();//check for association
                if(sec!=null)
                {
                                      
                    throw new InvalidValueException("This course has section(s) associated which are needed to be deleted first");
                  
                }
                if (MessageBox.Show("Are you sure to delete?", "Deletion", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                {
                    return;
                }
                Globals.ctx.Courses.Remove(c1);
                Globals.ctx.SaveChanges();
                clearFieldRefresh();
                MessageBox.Show("Selected record successfully Deleted. ", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }catch(InvalidValueException ex)
            {
                MessageBox.Show(ex.Message, "Validation Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error! " + ex.Message);
            }
        }
        private void btnAddSection_Click(object sender, RoutedEventArgs e)
        {
            try { 
            if (lvCourses.SelectedIndex >= 0)
            {
                Course course = (Course)lvCourses.SelectedItem;
                CourseSectionDialog courseSectionDialog = new CourseSectionDialog(course) { Owner = parentWindow };
                courseSectionDialog.ShowDialog();
                clearFieldRefresh();
            }
            }  catch (SystemException ex)
            {
                MessageBox.Show("Database Error! " + ex.Message);
            }
        }
        private void btnCSV_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "CSV file (*.csv)|*.csv";
            saveFileDialog.Title = "Export to file";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Configuration.Delimiter = ",";
                        csv.WriteRecords(listCourse);
                    }
                }
                catch (IOException ex)
                {
                    System.Windows.MessageBox.Show("Error writing file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }//end btnCSV_Click
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            clearFieldRefresh();
        }
        private void btnViewSections_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}