using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CmsProject
{
    /// <summary>
    /// Interaction logic for EnrollementWisardStudent.xaml
    /// </summary>
    public partial class EnrollementWizard : Window
    {
        List<Section> sectionList;
        List<Student> totalStudentList, studentList, selectedStudentList;
        readonly Course currCourse;
        Section currSection;

        public EnrollementWizard(Course course)
        {
            InitializeComponent();
            currCourse = course;
            gridConfimation.Visibility = Visibility.Hidden;
            gridStudent.Visibility = Visibility.Hidden;
            lblCourseId.Content = "ID: " + currCourse.Id;
            lblCourseTitle.Content = "Title: " + currCourse.CourseTitle;
            lblCourseDesc.Content = "Description: " + currCourse.Description;
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
                sectionList = (from section in Globals.ctx.Sections.Include("Teacher")
                               where section.CourseId == currCourse.Id
                               select section).ToList<Section>();
                lvSection.ItemsSource = sectionList;
                lvSection.Items.Refresh();
                Utils.AutoResizeColumns(lvSection);
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error!:\n" + ex.Message, "Database Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void ButtonSectionBack_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonSectionNext_Click(object sender, RoutedEventArgs e)
        {
            if (lvSection.SelectedIndex == -1)
            {
                MessageBox.Show("Select a Section Please!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            currSection = (Section)lvSection.SelectedItem;
            if (currSection.Room == currSection.Spot)
            {
                MessageBox.Show("This Section is already Full!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            gridSection.Visibility = Visibility.Hidden;
            gridStudent.Visibility = Visibility.Visible;
            try
            {
                totalStudentList = (from student in Globals.ctx.Students select student).ToList<Student>();
                selectedStudentList = (from s in Globals.ctx.Students
                                       join en in Globals.ctx.Enrollments
                                       on s.Id equals en.StudentId
                                       where en.SectionId == currSection.Id
                                       select s
                                       ).ToList<Student>();
                studentList = totalStudentList.Except<Student>(selectedStudentList).ToList<Student>();
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error!:\n" + ex.Message, "Database Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            lvStudent.ItemsSource = studentList;
            lvStudent.Items.Refresh();
            Utils.AutoResizeColumns(lvStudent);
        }


        private void ButtonStudentBack_Click(object sender, RoutedEventArgs e)
        {
            gridStudent.Visibility = Visibility.Hidden;
            gridSection.Visibility = Visibility.Visible;
        }

        private void RefreshAndResize()
        {
            lvStudent.ItemsSource = studentList;
            lvStudent.Items.Refresh();
            lvSelectedStudents.Items.Refresh();
            Utils.AutoResizeColumns(lvSelectedStudents);
            Utils.AutoResizeColumns(lvStudent);
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (lvStudent.SelectedIndex == -1)
            {
                MessageBox.Show("Select a student Please!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (lvStudent.HasItems && lvStudent.SelectedItems != null)
            {
                try
                {
                    selectedStudentList = lvStudent.SelectedItems.Cast<Student>().ToList();
                    foreach (var item in selectedStudentList)
                    {
                        lvSelectedStudents.Items.Add(item);
                        studentList.Remove(item);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show("Error in Casting" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                RefreshAndResize();
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lvSelectedStudents.SelectedIndex == -1)
            {
                MessageBox.Show("Select a student Please!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (lvStudent.HasItems && lvStudent.SelectedItems != null)
            {
                try
                {
                    selectedStudentList = lvSelectedStudents.SelectedItems.Cast<Student>().ToList();
                    foreach (var item in selectedStudentList)
                    {
                        studentList.Add(item);
                        lvSelectedStudents.Items.Remove(item);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show("Error in Casting" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                RefreshAndResize();
            }
        }

        private void ButtonAddSelection_Click(object sender, RoutedEventArgs e)
        {
            if (lvSelectedStudents.Items.Count == 0)
            {
                MessageBox.Show("Please Select Student(s) to enroll ", "Warning ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            gridStudent.Visibility = Visibility.Hidden;
            gridConfimation.Visibility = Visibility.Visible;
            lblCourseIdConf.Content = "ID: " + currCourse.Id;
            lblCourseTitleConf.Content = "Title: " + currCourse.CourseTitle;
            lblCourseDescConf.Content = "Description: " + currCourse.Description;
            lblSectionId.Content = "Id: " + currSection.Id;
            lblSectionDays.Content = "Days: " + currSection.Day;
            lblSectionDate.Content = "From: " + currSection.StartDate.ToShortDateString() + " To: " + currSection.EndDate.ToShortDateString();
            lblSectionTime.Content = "From: " + currSection.StartTime + " To: " + currSection.EndTime;
            lblTeacherName.Content = currSection.Teacher.FirstName + " " + currSection.Teacher.LastName;
            lvStdConfirm.ItemsSource = lvSelectedStudents.Items;
            Utils.AutoResizeColumns(lvStdConfirm);
        }

        private void BtnConfirmBack_Click(object sender, RoutedEventArgs e)
        {
            gridConfimation.Visibility = Visibility.Hidden;
            gridStudent.Visibility = Visibility.Visible;
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                List<int> studentsIdList = (from s in Globals.ctx.Enrollments
                                            where s.SectionId == currSection.Id
                                            select s.StudentId).ToList();

                int totalStudent = studentsIdList.Count();
                int countEnrollStd = lvStdConfirm.Items.Count;
                int availableSpot = currSection.Room - totalStudent;

                if (totalStudent + countEnrollStd > currSection.Room)
                {
                    MessageBox.Show("Availabile Spots are not enough for selected Student \n " +
                        "There are only " + availableSpot + " available spot(s) ", "Warning ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool hasMatch = selectedStudentList.Select(x => x.Id)
                          .Intersect(studentsIdList)
                          .Any();

                if (hasMatch)
                {
                    MessageBox.Show("Student is already registered in this Section Check the student List: ", "Warning ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (lvStdConfirm.Items.Count == 0)
                {
                    MessageBox.Show("Please select Student(s) to enroll ", "Warning ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show("Enrollment will save.Confirm?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    foreach (Student student in lvStdConfirm.Items)
                    {
                        Attendance attendance = new Attendance
                        {
                            StudentId = student.Id,
                            SectionId = currSection.Id,
                            IsAttend = "true",
                        };
                        Enrollment enrollment = new Enrollment
                        {
                            DateEnrolled = DateTime.Today,
                            StudentId = student.Id,
                            SectionId = currSection.Id,
                        };
                        Globals.ctx.Enrollments.Add(enrollment);
                        Globals.ctx.Attendances.Add(attendance);

                    }
                    Globals.ctx.SaveChanges();
                    MessageBox.Show("Enrollment is Successfully Done for " + countEnrollStd + " Student(s)");
                    DialogResult = true;
                }
            }
            catch (SystemException ex)
            {
                MessageBox.Show(ex.Message, "Database operation failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure to Cancel?", "Cancel Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DialogResult = true;
            }
        }
    }
}
