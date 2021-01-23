using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for UserControlAttendance.xaml
    /// </summary>
    public partial class UserControlAttendance : UserControl
    {
        List<Course> listCourse = new List<Course>();
        List<Section> sectionList = new List<Section>();
        public UserControlAttendance()
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
        void LoadData()
        {
            listCourse = (from c in Globals.ctx.Courses select c).ToList<Course>();

            foreach (Course course in listCourse)
            {
                comboCourse.Items.Add(course.Id + "- " + course.CourseTitle);
            }
        }

        private void comboCourse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            {
                refreshDataGrid();
                if (comboCourse.SelectedIndex>=0)
                {
                    int courseId = listCourse[comboCourse.SelectedIndex].Id;
                    sectionList = (from s in Globals.ctx.Sections.Include("Teacher") where s.CourseId == courseId select s).ToList();
                    lvSection.ItemsSource = sectionList;
                    Utils.AutoResizeColumns(lvSection);
                }
            }
        }
        class TempClass
        {
            public int StudentId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public bool Attendance { get; set; }
        }
        List<Attendance> attendList = new List<Attendance>();

        List<TempClass> tempList = new List<TempClass>();
        private void lvSection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lvSection.SelectedIndex >= 0)
                {
                    int sectionId = sectionList[lvSection.SelectedIndex].Id;
                    var attendList1 = (from a in Globals.ctx.Attendances.Include("Student") where a.SectionId == sectionId && a.DateAttended == null select a).ToList<Attendance>();
                    if (attendList1.Count() == 0)
                    {
                        throw new InvalidValueException("There is no student registered in this section.");
                    }
                    if (dateAttandance.SelectedDate == null)
                    {
                        Utils.ChangeBorderControl(dateAttandance);
                        lvSection.SelectedItem = null;
                        throw new InvalidValueException("Please choose a date first.");
                    }
                    DateTime attendDate = (DateTime)dateAttandance.SelectedDate;
                    DateTime startDate = sectionList[lvSection.SelectedIndex].StartDate;
                    DateTime endDate = sectionList[lvSection.SelectedIndex].EndDate;
                   
                    refreshDataGrid();
                   

                    if (startDate > attendDate || (attendDate > endDate))
                    {
                        Utils.ChangeBorderControl(dateAttandance);
                        lvSection.SelectedItem = null;
                        throw new InvalidValueException("Date is not Valid.\ndate should be in course duration.");
                    }

                   
                    List<Attendance> attendList = new List<Attendance>();
                    List<Attendance> registeredStudentList = new List<Attendance>();
                    attendList = (from a in Globals.ctx.Attendances.Include("Student") where a.SectionId == sectionId && (a.DateAttended == attendDate || a.DateAttended == null) select a).ToList<Attendance>();
                    if(attendList.Count()==0)
                    {
                        throw new InvalidValueException("There is no student registered in this section.");
                    }
                    registeredStudentList = (from a in Globals.ctx.Attendances.Include("Student") where a.SectionId == sectionId && (a.DateAttended == null) select a).ToList<Attendance>();
                    foreach (Attendance attendR in registeredStudentList)
                    {
                        Attendance tempAttent = new Attendance();
                        tempAttent = attendR;
                        foreach (Attendance att in attendList)
                        {
                            if (attendR.StudentId == att.StudentId)
                            {
                                if (att.DateAttended != null)
                                {
                                    tempAttent = att;
                                }
                            }
                        }
                        TempClass en = new TempClass();
                        en.StudentId = tempAttent.StudentId;
                        //en.StudentName = enroll.Student.StudentName;
                        en.FirstName = tempAttent.Student.FirstName;
                        en.LastName = tempAttent.Student.LastName;
                        if (tempAttent.IsAttend.Contains("True"))
                            en.Attendance = true;
                        else
                            en.Attendance = false;
                        tempList.Add(en);
                    }
                    dgAttendance.ItemsSource = tempList;
                    dgAttendance.Items.Refresh();
                }
            }
            catch (SystemException ex)
            {
                MessageBox.Show(ex.Message);

            }
            catch (InvalidValueException ex)
            {
                MessageBox.Show(ex.Message, "Validation Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    void refreshDataGrid()
        {
            tempList.Clear();
            dgAttendance.ItemsSource = tempList;
            dgAttendance.Items.Refresh();
            Utils.ChangeBackBorderControl(dateAttandance);
        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgAttendance.Items.Count == 0)
                {
                    throw new InvalidValueException(" Attendence form is Empty.");
                }

                if (dateAttandance.SelectedDate == null)
                {
                    Utils.ChangeBorderControl(dateAttandance);
                    lvSection.SelectedItem = null;
                    throw new InvalidValueException("Please choose a date first.");
                }

                DateTime attendDate = (DateTime)dateAttandance.SelectedDate;
                DateTime startDate = sectionList[lvSection.SelectedIndex].StartDate;
                DateTime endDate = sectionList[lvSection.SelectedIndex].EndDate;

                if (startDate > attendDate || (attendDate > endDate))
                {
                    Utils.ChangeBorderControl(dateAttandance);
                    lvSection.SelectedItem = null;
                    throw new InvalidValueException("Date is not Valid.\ndate should be in course duration.");
                }

                int sectionId = sectionList[lvSection.SelectedIndex].Id;
                attendList = (from a in Globals.ctx.Attendances.Include("Student") where a.SectionId == sectionId && (a.DateAttended == attendDate || a.DateAttended == null) select a).ToList<Attendance>();
                Attendance attendance = new Attendance();
                foreach (TempClass ea in tempList)
                {
                    int f = 0;//check if the transition is add or update
                    foreach (Attendance attend in attendList)
                    {
                       
                        if (ea.StudentId == attend.StudentId)
                        { 
                            f++;
                            attendance = attend;
                            }

                    }
                  if (f == 2)
                    {
                        attendance.IsAttend = ea.Attendance.ToString();
                        attendance.DateAttended = attendDate;
                           
                        
                    }else{ if (f==1) 
                        {
                                    Attendance at = new Attendance();
                                    at.DateAttended = attendDate;
                                    at.IsAttend = ea.Attendance.ToString();
                                    at.StudentId = attendance.StudentId;
                                    at.SectionId = sectionId;
                                    Globals.ctx.Attendances.Add(at);
                           
                            }
                            }
                   Globals.ctx.SaveChanges();
                }
                MessageBox.Show("Attendance info is successfully updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                refreshDataGrid();
               }
                        catch (InvalidValueException ex)
                {
                    MessageBox.Show(ex.Message, "Validation Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
                private void refreshfields_Click(object sender, RoutedEventArgs e)
        {
            dateAttandance.Text = "";
           comboCourse.Text="" ;
            sectionList.Clear();
            lvSection.Items.Refresh();
            refreshDataGrid();
        }

        private void dateAttandance_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
           
        }

        private void dateAttandance_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            refreshDataGrid();
            // if(comboCourse.Text=="") {return; }
            sectionList.Clear();
            lvSection.ItemsSource = sectionList;
            lvSection.Items.Refresh();
            if (comboCourse.Text!="")
            {
                int courseId = listCourse[comboCourse.SelectedIndex].Id;

                sectionList = (from s in Globals.ctx.Sections.Include("Teacher") where s.CourseId == courseId select s).ToList();

                lvSection.ItemsSource = sectionList;

                Utils.AutoResizeColumns(lvSection);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //var rows = from o in Globals.ctx.Attendances
            //           select o;
            //foreach (var row in rows)
            //{
            //    Globals.ctx.Attendances.Remove(row);
            //}
            //Globals.ctx.SaveChanges();
            var enroll = (from e1 in Globals.ctx.Enrollments select e1).ToList<Enrollment>();
            foreach(Enrollment en in enroll )
            {
                Attendance atten = new Attendance();
                atten.StudentId = en.StudentId;
                atten.SectionId = en.SectionId;
                atten.DateAttended = null;
                atten.IsAttend = "True";
                Globals.ctx.Attendances.Add(atten);
                Globals.ctx.SaveChanges();
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
                        csv.WriteRecords(tempList);
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
