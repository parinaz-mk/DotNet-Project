using CsvHelper;


using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CmsProject
{
    /// <summary>
    /// Interaction logic for StudentReportDialog.xaml
    /// </summary>
    public partial class StudentReportDialog : Window
    {
        List<Course> listCourse = new List<Course>();
        readonly List<StudentTemp> studentTempList = new List<StudentTemp>();
        List<Section> sectionList = new List<Section>();
        public StudentReportDialog()
        {
            InitializeComponent();
            try
            {
                Globals.ctx = new CmsprojectDbConnection();
               
                LoadData();

                Globals.ctx = new CmsprojectDbConnection();

            }
            catch (SystemException ex)
            {
                System.Windows.MessageBox.Show("Fatal Error Connecting to Database:\n" + ex.Message, "Database Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }

        void LoadData()
        {
            listCourse = (from c in Globals.ctx.Courses select c).ToList<Course>();

            foreach (Course course in listCourse)
            {
                comboCourse.Items.Add(course.Id + "- " + course.CourseTitle);
            }
            List<Student> studentList = new List<Student>();
            studentList = (from s in Globals.ctx.Students select s).ToList<Student>();
            foreach (Student s in studentList)
            {
                StudentTemp studentTemp = new StudentTemp
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Phone = s.Phone,
                    Email = s.Email,
                    Address = s.No + " " + s.Street + ", " + s.City + ", " + s.Province + " " + s.CodePostal,
                };
                studentTempList.Add(studentTemp);
            }
            lvStudent.ItemsSource = studentTempList;
        }



        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

      
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            comboSection.Text = "";
            comboCourse.Text = "";
            LoadData();

        }

        private void ComboCourse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            {
                // refreshDataGrid();
                try
                {
                    if (comboCourse.SelectedIndex >= 0)
                    {
                        //sectionList.Clear();
                        //comboSection.ItemsSource = sectionList;
                        //comboSection.Items.Refresh();
                        int courseId = listCourse[comboCourse.SelectedIndex].Id;
                        comboSection.Items.Clear();
                        sectionList = (from s in Globals.ctx.Sections.Include("Teacher") where s.CourseId == courseId select s).ToList();
                        foreach (Section s in sectionList)
                        {
                            comboSection.Items.Add(s.Id + "-" + s.Day + ", " + s.StartDate.ToShortDateString() + ", " + s.Teacher.TeacherName);
                        }
                    }
                }
                catch (SystemException ex)
                {
                    System.Windows.Forms.MessageBox.Show("Database Error! " + ex.Message);
                }
            }
        }

        private void ComboSection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (comboSection.SelectedIndex >= 0)
                {
                    List<Enrollment> enrollList = new List<Enrollment>();
                    int sectionId = sectionList[comboSection.SelectedIndex].Id;
                    enrollList = (from en in Globals.ctx.Enrollments.Include("Student") where en.SectionId == sectionId select en).ToList();
                    studentTempList.Clear();
                    foreach (Enrollment enroll in enrollList)
                    {
                        StudentTemp studentTemp = new StudentTemp
                        {
                            Id = enroll.Student.Id,
                            FirstName = enroll.Student.FirstName,
                            LastName = enroll.Student.LastName,
                            Phone = enroll.Student.Phone,
                            Email = enroll.Student.Email,
                            Address = enroll.Student.No + " " + enroll.Student.Street + ", " + enroll.Student.City + ", " + enroll.Student.Province + " " + enroll.Student.CodePostal,

                        };

                        studentTempList.Add(studentTemp);
                    }
                    lvStudent.ItemsSource = studentTempList;
                    lvStudent.Items.Refresh();
                }
            }
            catch (SystemException ex)
            {
                System.Windows.Forms.MessageBox.Show("Database Error! " + ex.Message);
            }
        }
        private void BtnCSV_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv",
                Title = "Export to file"
            };
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Configuration.Delimiter = ",";
                        csv.WriteRecords(studentTempList);
                    }
                }
                catch (IOException ex)
                {
                    System.Windows.MessageBox.Show("Error writing file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }//end btnCSV_Click

    }
    }

