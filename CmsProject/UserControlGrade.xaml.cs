using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CmsProject
{
    /// <summary>
    /// Interaction logic for UserControlGrade.xaml
    /// </summary>
    public partial class UserControlGrade : UserControl
    {
        List<Enrollment> enrollmentList;
        public UserControlGrade()
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
                enrollmentList = (from enrollment in Globals.ctx.Enrollments select enrollment).ToList<Enrollment>();
                dgGrade.ItemsSource = enrollmentList;
                tbSectionId.Text = "";
                tbStudentId.Text = "";
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error!:\n" + ex.Message, "Database Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {

            if (MessageBox.Show("Grades will save.Confirm?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
            {
                try
                {

                    Globals.ctx.SaveChanges();
                    MessageBox.Show("Grade(s) is(are) successfully Submited. ", "Update Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (SystemException ex)
                {
                    MessageBox.Show("Database operation failed: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        private void TbSectionId_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbSectionId.Text == "")
            {
                LoadData();
            }
            else
            {
                try
                {
                    enrollmentList = (from enrollment in Globals.ctx.Enrollments.Include("Student") where enrollment.SectionId.ToString() == tbSectionId.Text select enrollment).ToList<Enrollment>();
                    dgGrade.ItemsSource = enrollmentList;
                    dgGrade.Items.Refresh();

                }
                catch (SystemException ex)
                {
                    MessageBox.Show("Database operation failed: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TbStudentId_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbStudentId.Text == "")
            {
                LoadData();
            }
            else
            {
                try
                {
                    enrollmentList = (from enrollment in Globals.ctx.Enrollments.Include("Student") where enrollment.StudentId.ToString() == tbStudentId.Text select enrollment).ToList<Enrollment>();

                    dgGrade.ItemsSource = enrollmentList;
                    dgGrade.Items.Refresh();

                }
                catch (SystemException ex)
                {
                    MessageBox.Show("Database operation failed: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }


        private void Bntprint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDlg = new PrintDialog();
            printDlg.PrintVisual(dgGrade, "Grid Printing.");
        }

        private void BtnCsv_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog exportDialog = new SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv",
                Title = "Export to file"
            };
            if (exportDialog.ShowDialog() == true)
            {
                try
                {
                    dgGrade.SelectAllCells();
                    dgGrade.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                    ApplicationCommands.Copy.Execute(null, dgGrade);
                    dgGrade.UnselectAllCells();
                    String result = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
                    File.AppendAllText(exportDialog.FileName, result, UnicodeEncoding.UTF8);
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Error exporting to csv: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}
