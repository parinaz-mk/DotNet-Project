using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControlTeacher : UserControl
    {
      
        List<Teacher> listTeacher = new List<Teacher>();
        public UserControlTeacher()
        {
            try
            {
                InitializeComponent();
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
                changeBackBorderColor();
                tbFirstNameT.Clear();
                tbLastNameT.Clear();
                dateHireDate.SelectedDate = null;
                tbContact.Clear();
                tbEmail.Clear();
                tbNumber.Clear();
                tbStreet.Clear();
                tbCity.Clear();
                comboProvince.SelectedIndex = -1;
                btDelete.IsEnabled = false;
                btUpdate.IsEnabled = false;
                tbCodePostal.Clear();
                listTeacher = (from t in Globals.ctx.Teachers orderby t.Id select t).ToList<Teacher>();
                lvTeachers.ItemsSource = listTeacher;
                Utils.AutoResizeColumns(lvTeachers);
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error! " + ex.Message);
                return;
            }
        }

        private bool VerifyFields()
        {
            List<String> errorsList = new List<string>();
            if (tbFirstNameT.Text.Length < 1 || tbFirstNameT.Text.Length > 25)
            {
                errorsList.Add("FirstName length must be 1 to 25 characters");
                Utils.ChangeBorderControl(tbFirstNameT);
            }
            
            if (tbLastNameT.Text.Length < 1 || tbLastNameT.Text.Length > 25)
            {
                errorsList.Add("LastName length must be 1 to 25 characters");
                Utils.ChangeBorderControl(tbLastNameT);
            }
           
            Regex regexPhone = new Regex(Globals.phonePattern);
            if (!regexPhone.IsMatch(tbContact.Text))
            {
                errorsList.Add("Phone number must be match (123) 456-7890");
                Utils.ChangeBorderControl(tbContact);
            }
           
            Regex regexEmail = new Regex(Globals.emailPattern);
            if (!regexEmail.IsMatch(tbEmail.Text))
            {
                errorsList.Add("Email is not valid");
                Utils.ChangeBorderControl(tbEmail);
            }
                 
            if (comboProvince.Text == "")
            {
                errorsList.Add("Province must be selected");
                Utils.ChangeBorderControl(comboProvince);
            }
            if (dateHireDate.SelectedDate == null)
            {
                errorsList.Add("Hire Date must be selected");
                Utils.ChangeBorderControl(comboProvince);
            }

            if (tbCity.Text.Length < 1 || tbCity.Text.Length > 20)
            {
                errorsList.Add("City length must be 1 to 20 characters");
                Utils.ChangeBorderControl(tbCity);
            }
           
            if (tbStreet.Text.Length < 1 || tbStreet.Text.Length > 20)
            {
                errorsList.Add("Street length must be 1 to 20 characters");
                Utils.ChangeBorderControl(tbStreet);
            }
           
            if (tbNumber.Text.Length < 1 || tbNumber.Text.Length > 20)
            {
                errorsList.Add("Number length must be 1 to 10 characters");
                Utils.ChangeBorderControl(tbNumber);
            }
           
            Regex regexCodePostal = new Regex(Globals.codePostalPattern);
            if (!regexCodePostal.IsMatch(tbCodePostal.Text))
            {
                errorsList.Add("CodePostal is not valid");
                Utils.ChangeBorderControl(tbCodePostal);
            }
            
            if (errorsList.Count != 0)
            {
                MessageBox.Show(string.Join("\n", errorsList), "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return (errorsList.Count == 0);
        }
      
        void changeBackBorderColor()
        {
            Utils.ChangeBackBorderControl(tbFirstNameT);
            Utils.ChangeBackBorderControl(dateHireDate);
            Utils.ChangeBackBorderControl(tbLastNameT);
            Utils.ChangeBackBorderControl(tbContact);
            Utils.ChangeBackBorderControl(tbEmail);
            Utils.ChangeBackBorderControl(tbNumber);
            Utils.ChangeBackBorderControl(tbStreet);
            Utils.ChangeBackBorderControl(tbCity);
            Utils.ChangeBackBorderControl(comboProvince);
            Utils.ChangeBackBorderControl(tbCodePostal);
            
        }

        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            changeBackBorderColor();
            if (!VerifyFields()) { return; }
            try
            {   
                string first = tbFirstNameT.Text;
                string last = tbLastNameT.Text;
                DateTime hire = (DateTime)dateHireDate.SelectedDate;
                string contact = tbContact.Text;
                string email = tbEmail.Text;
                string number = tbNumber.Text;
                string street = tbStreet.Text;
                string city = tbCity.Text;
                string Province = comboProvince.Text;
                string codePostal = tbCodePostal.Text.ToUpper();
                Teacher teacher = new Teacher() { FirstName = first, LastName = last, Phone = contact, Email = email, HireDate = hire, Province = Province, City = city, Street = street, No = number, CodePostal = codePostal };

                Globals.ctx.Teachers.Add(teacher);
                Globals.ctx.SaveChanges();
                clearFieldRefresh();
                MessageBox.Show("Teacher info successfully Added. ", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                lvTeachers.SelectedIndex = -1;
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error" + ex.Message);
            }
            catch (InvalidValueException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void btUpdate_Click(object sender, RoutedEventArgs e)
        {
               changeBackBorderColor();
                if (lvTeachers.SelectedIndex < 0)
                {
                    MessageBox.Show("Select an item first.");
                    return;
                }
                if (!VerifyFields()) { return; }
            try
            {

                if (MessageBox.Show("Are you sure to Update?", "Update", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                {
                    return;
                }
                DateTime hire = (DateTime)dateHireDate.SelectedDate;
                int id = listTeacher[lvTeachers.SelectedIndex].Id;
                Teacher t1 = (from t in Globals.ctx.Teachers where t.Id == id select t).FirstOrDefault<Teacher>();
                if (t1 != null)
                {
                    t1.FirstName = tbFirstNameT.Text;
                    t1.LastName = tbLastNameT.Text;
                    t1.Phone = tbContact.Text;
                    t1.Email = tbEmail.Text;
                    t1.No = tbNumber.Text;
                    t1.Street = tbStreet.Text;
                    t1.City = tbCity.Text;
                    t1.Province = comboProvince.Text;
                    t1.CodePostal = tbCodePostal.Text;
                }
                Globals.ctx.SaveChanges();
                clearFieldRefresh();
                 MessageBox.Show("Selected record successfully Updated. ", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error! " + ex.Message);
            }
            catch (InvalidValueException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void lvTeachers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvTeachers.SelectedIndex >= 0)
            {

                try
                {
                    btUpdate.IsEnabled = true;
                    btDelete.IsEnabled = true;
                    Teacher t = listTeacher[lvTeachers.SelectedIndex];
                    tbFirstNameT.Text = t.FirstName;
                    tbLastNameT.Text = t.LastName;
                    dateHireDate.SelectedDate = t.HireDate;
                    tbContact.Text = t.Phone;
                    tbEmail.Text = t.Email;
                    tbNumber.Text = t.No;
                    tbStreet.Text = t.Street;
                    tbCity.Text = t.City;
                    comboProvince.Text = t.Province;
                    tbCodePostal.Text = t.CodePostal;
                }
                catch (ArgumentOutOfRangeException)
                {
                    return;
                }
            }
        }

        private void btDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                changeBackBorderColor();
                if (lvTeachers.SelectedIndex < 0)
                {
                    MessageBox.Show("Select an item to delete.");
                    return;
                }
                int id = listTeacher[lvTeachers.SelectedIndex].Id;
                var sec = (from s in Globals.ctx.Sections where s.TeacherId == id select s).FirstOrDefault();//check for association
                if (sec != null)
                {

                    MessageBox.Show(" The Teacher is registered in section(s) and can not be deleted", "error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (MessageBox.Show("Are you sure to delete?", "Deletion", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                {
                    return;
                }
      
                Teacher t1 = (from t in Globals.ctx.Teachers where t.Id == id select t).FirstOrDefault<Teacher>();
                if (t1 != null)
                {
                    Globals.ctx.Teachers.Remove(t1);
                    Globals.ctx.SaveChanges();
                    clearFieldRefresh();
                    MessageBox.Show("Selected record successfully Deleted. ", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error!  " + ex.Message);
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
                        csv.WriteRecords(listTeacher);
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
    }
}