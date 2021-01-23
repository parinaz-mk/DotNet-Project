using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace CmsProject
{
    /// <summary>
    /// Interaction logic for CourseSectionDialog.xaml
    /// </summary>
    public partial class CourseSectionDialog : Window
    {
        Course currCourse;
        List<Teacher> listTeacher = new List<Teacher>();

        public CourseSectionDialog(Course course)
        {
            InitializeComponent();
            try
            {
                Globals.ctx = new CmsprojectDbConnection();
                currCourse = course;
                refreshFields();
                listTeacher = (from t in Globals.ctx.Teachers select t).ToList<Teacher>();
                foreach (Teacher t in listTeacher)
                {
                    comboTeacher.Items.Add(t.Id + "-" + t.FirstName + " " + t.LastName);
                }
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Fatal error connecting to database:\n" + ex.Message);
                Environment.Exit(1);
            }
        }
        void refreshFields()
        {
            lblCourseId.Content = "";
            lblCourseName.Content = "";
            dateStart.Text = "";
            dateEnd.Text = "";
            comboStartTime.Text = "";
            comboEndTime.Text = "";
            comboTeacher.Text = "";
            chMonday.IsChecked = false;
            chTuesday.IsChecked = false;
            chWednesday.IsChecked = false;
            chThursday.IsChecked = false;
            chFriday.IsChecked = false;
            chSaturday.IsChecked = false;
            chSunday.IsChecked = false;
            chWeekdays.IsChecked = false;
            tbRoom.Clear();
            lblCourseName.Content = currCourse.CourseTitle;
            lblCourseId.Content = currCourse.Id;
            btAdd.Content = "Add";
            lvSection.SelectedIndex = -1;
            changeBackBorderColor();
            LoadSectionsList();
        }

        private void btBack_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        Section s1;

        private bool VerifyFields()
        {
            //List<String> errorsList = new List<string>();
            int count = 0;
            if (dateStart.SelectedDate == null)
            {
                Utils.ChangeBorderControl(dateStart);
                count++;
            }
            if (dateEnd.SelectedDate == null)
            {
                Utils.ChangeBorderControl(dateEnd);
                count++;
            }


            if (comboStartTime.Text == "")
            {

                Utils.ChangeBorderControl(comboStartTime);
                count++;
            }
            if (comboEndTime.Text == "")
            {

                Utils.ChangeBorderControl(comboEndTime);
                count++;
            }
            if (comboTeacher.Text == "")
            {

                Utils.ChangeBorderControl(comboTeacher);
                count++;
            }
            int countDay = 0;
            if (chMonday.IsChecked == true) { countDay++; }
            if (chTuesday.IsChecked == true) { countDay++; }
            if (chWednesday.IsChecked == true) { countDay++; }
            if (chThursday.IsChecked == true) { countDay++; }
            if (chFriday.IsChecked == true) { countDay++; }
            if (chSaturday.IsChecked == true) { countDay++; }
            if (chSunday.IsChecked == true) { countDay++; }
            if (countDay == 0)
            {
                Utils.ChangeBorderControl(grpDay);
                count++;
            }

            if (tbRoom.Text == "")
            {
                Utils.ChangeBorderControl(tbRoom);
                count++;
            }
           return (count == 0);
        }
        void changeBackBorderColor()
        {
            Utils.ChangeBackBorderControl(tbRoom);
            Utils.ChangeBackBorderControl(dateStart);
            Utils.ChangeBackBorderControl(dateEnd);
            Utils.ChangeBackBorderControl(comboStartTime);
            Utils.ChangeBackBorderControl(comboEndTime);
            Utils.ChangeBackBorderControl(comboTeacher);
            Utils.ChangeBackBorderControl(grpDay);
        }
        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                changeBackBorderColor();
                if (!VerifyFields())
                {
                    throw new InvalidValueException("Empty fields are not allowed!");
                }
                int courseId = currCourse.Id;
                ComboBoxItem sTime = (ComboBoxItem)comboStartTime.SelectedItem;
                ComboBoxItem eTime = (ComboBoxItem)comboEndTime.SelectedItem;
                var t1 = DateTime.Parse(sTime.Content.ToString());
                var t2 = DateTime.Parse(eTime.Content.ToString());
                string startTime = t1.ToShortTimeString();
                string endTime = t2.ToShortTimeString();

                DateTime startDate = (DateTime)dateStart.SelectedDate;
                DateTime endDate = (DateTime)dateEnd.SelectedDate;

                int room = Int32.Parse(tbRoom.Text);

                List<string> daylist = new List<string>();
                if (chWeekdays.IsChecked == true) { daylist.Add("Weekdays"); }
                else
                {
                    if (chMonday.IsChecked == true) { daylist.Add("Monday"); }
                    if (chTuesday.IsChecked == true) { daylist.Add("Tuesday"); }
                    if (chWednesday.IsChecked == true) { daylist.Add("Wednesday"); }
                    if (chThursday.IsChecked == true) { daylist.Add("Thursday"); }
                    if (chFriday.IsChecked == true) { daylist.Add("Friday"); }
                }
                if (chSaturday.IsChecked == true) { daylist.Add("Saturday"); }
                if (chSunday.IsChecked == true) { daylist.Add("Sunday"); }

                string dayStr = string.Join(",", daylist);

                if (dateEnd.SelectedDate < dateStart.SelectedDate)
                {
                    throw new InvalidValueException("End date should be after Start Date");
                }
                if (comboEndTime.SelectedIndex < comboStartTime.SelectedIndex)
                {
                    throw new InvalidValueException("End Time should be after Start time");
                }
                int teacherId = listTeacher[comboTeacher.SelectedIndex].Id;
                //check teacher availablity in days and times

                List<Section> teacherhasSection = new List<Section>();
                teacherhasSection = (from s in Globals.ctx.Sections where s.TeacherId == teacherId select s).ToList();//Teacher already has courses to teach
                                                                                                                    
                foreach (Section s in teacherhasSection)
                {
                    if (btAdd.Content.ToString() == "Update")// in case update it must not compare teacher availibility with  itself
                    {
                        if (s.Id != ((Section)lvSection.SelectedItem).Id)
                        {
                            if (((startDate >= s.StartDate) && (startDate <= s.EndDate)) || ((endDate >= s.StartDate) && (endDate <= s.EndDate)) || ((startDate <= s.StartDate) && (endDate >= s.EndDate)))
                            {//check for course duration 
                                for (int i = 0; i < daylist.Count(); i++)
                                {
                                    if (s.Day.Contains(daylist[i]))// check if the teacher is available on chosen days

                                    {
                                        DateTime starttime = DateTime.Parse(s.StartTime);
                                        DateTime endtime = DateTime.Parse(s.EndTime);
                                        if ((starttime <= t1 && t1 <= endtime) || (starttime <= t2 && t2 <= endtime))// check if teacher availabe on chosen times
                                        {
                                            List<string> notAvailableList = new List<String>();
                                            foreach (Section t in teacherhasSection)
                                            {
                                                notAvailableList.Add(t.Day + "  From " + t.StartTime + " To " + t.EndTime);//Display the days and times that the teacher                                                                            is not availabe  
                                            }
                                            string str = string.Join("\n", notAvailableList);
                                            throw new InvalidValueException("This teacher is not available on \n" + str);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (btAdd.Content.ToString() == "Add")
                    {
                        if (((startDate >= s.StartDate) && (startDate <= s.EndDate)) || ((endDate >= s.StartDate) && (endDate <= s.EndDate)) || ((startDate <= s.StartDate) && (endDate >= s.EndDate)))
                        {//check for course duration 
                            for (int i = 0; i < daylist.Count(); i++)
                            {
                                if (s.Day.Contains(daylist[i]))// check if the teacher is available on chosen days

                                {
                                    DateTime starttime = DateTime.Parse(s.StartTime);
                                    DateTime endtime = DateTime.Parse(s.EndTime);
                                    if ((starttime <= t1 && t1 <= endtime) || (starttime <= t2 && t2 <= endtime))// check if teacher availabe on chosen times
                                    {
                                        List<string> notAvailableList = new List<String>();
                                        foreach (Section t in teacherhasSection)
                                        {
                                            notAvailableList.Add(t.Day + "  From " + t.StartTime + " To " + t.EndTime);//Display the days and times that the teacher                                                                            is not availabe  
                                        }
                                        string str = string.Join("\n", notAvailableList);
                                        throw new InvalidValueException("This teacher is not available on \n" + str);
                                    }
                                }
                            }
                        }
                    }
                }

                s1 = new Section() { CourseId = courseId, TeacherId = teacherId, StartDate = startDate, EndDate = endDate, StartTime = startTime, EndTime = endTime, Room = room, Day = dayStr };
                if (btAdd.Content.ToString() == "Update")
                {
                    if (MessageBox.Show("Changes will save.Confirm?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        int sectionId = ((Section)lvSection.SelectedItem).Id;
                        Section s2 = (from s in Globals.ctx.Sections where s.Id == sectionId select s).FirstOrDefault<Section>();
                        s2.CourseId = courseId;
                        s2.StartDate = startDate;
                        s2.EndDate = endDate;
                        s2.StartTime = startTime;
                        s2.EndTime = endTime;
                        s2.TeacherId = teacherId;
                        s2.Day = dayStr;
                        s2.Room = room;
                        Globals.ctx.SaveChanges();
                        refreshFields();
                    }
                }
                else
                {
                    if (MessageBox.Show("Section info will save.Confirm?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        Globals.ctx.Sections.Add(s1);
                        Globals.ctx.SaveChanges();
                        // DialogResult = true; 
                        refreshFields();
                    }
                }
            }
            catch (InvalidValueException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (SystemException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            refreshFields();
        }
        List<Section> sectionList = new List<Section>();
        void LoadSectionsList()
        {
            sectionList = (from s in Globals.ctx.Sections.Include("Teacher") where s.CourseId == currCourse.Id select s).ToList();
            lvSection.ItemsSource = sectionList;
            Utils.AutoResizeColumns(lvSection);
        }
        private void itemDelete_Click(object sender, RoutedEventArgs e)
        {
            int sectionId = ((Section)lvSection.SelectedItem).Id;
            var sec = (from enroll in Globals.ctx.Enrollments where enroll.SectionId == sectionId select enroll).FirstOrDefault();//check for association
            if (sec != null)
            {

                MessageBox.Show("There are students enrolled in this Section and can not be deleted", "error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Section s1 = (from s in Globals.ctx.Sections where s.Id == sectionId select s).FirstOrDefault<Section>();
            try
            {
                if (s1 != null)
                {
                    if (MessageBox.Show("Are you sure to delete?", "Deletion", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                    {
                        return;
                    }

                    Globals.ctx.Sections.Remove(s1);
                    Globals.ctx.SaveChanges();
                    refreshFields();
                    MessageBox.Show("Selected record successfully Deleted. ", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error! " + ex.Message);
            }
        }

        void checkDays(string[] day)
        {
            foreach (string s in day)
            {
                switch (s)
                {
                    case "Weekdays": chMonday.IsChecked = true; chTuesday.IsChecked = true; chWednesday.IsChecked = true; chThursday.IsChecked = true; chFriday.IsChecked = true; chWeekdays.IsChecked = true; break;
                    case "Monday": chMonday.IsChecked = true; break;
                    case "Tuesday": chTuesday.IsChecked = true; break;
                    case "Wednesday": chWednesday.IsChecked = true; break;
                    case "Thursday": chThursday.IsChecked = true; break;
                    case "Friday": chFriday.IsChecked = true; break;
                    case "Saturday": chSaturday.IsChecked = true; break;
                    case "Sunday": chSunday.IsChecked = true; break;
                    default: break;
                }

            }
        }

        private void lvSection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvSection.SelectedIndex < 0)
            { return; }

            try
            {
                Section cs = (Section)lvSection.SelectedItem;
                var section = (from s in Globals.ctx.Sections.Include("Teacher") where s.Id == cs.Id select s).FirstOrDefault();
                dateStart.SelectedDate = cs.StartDate;
                dateEnd.SelectedDate = cs.EndDate;
                comboStartTime.Text = cs.StartTime;
                comboEndTime.Text = cs.EndTime;
                comboTeacher.Text = section.Teacher.Id + "-" + section.Teacher.FirstName + " " + section.Teacher.LastName;
                tbRoom.Text = cs.Room.ToString();
                string[] day = cs.Day.Split(',');
                checkDays(day);
                btAdd.Content = "Update";
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error! " + ex.Message);

            }
        }

        private void chWeekdays_Checked(object sender, RoutedEventArgs e)
        {

            chMonday.IsChecked = true;
            chTuesday.IsChecked = true;
            chWednesday.IsChecked = true;
            chThursday.IsChecked = true;
            chFriday.IsChecked = true;
        }
        private void chWeekdays_Unchecked(object sender, RoutedEventArgs e)
        {
            chMonday.IsChecked = false;
            chTuesday.IsChecked = false;
            chWednesday.IsChecked = false;
            chThursday.IsChecked = false;
            chFriday.IsChecked = false;
        }
    }
}
