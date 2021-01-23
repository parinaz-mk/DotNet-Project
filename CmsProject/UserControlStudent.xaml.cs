using CsvHelper;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CmsProject
{
    /// <summary>
    /// Interaction logic for UserControlStudent.xaml
    /// </summary>
    public partial class UserControlStudent : UserControl
    {
        byte[] currStudentImage = null;
        List<Student> studentList;
        public UserControlStudent()
        {
            InitializeComponent();
            try
            {
                Globals.ctx = new CmsprojectDbConnection();
                ClearFieldsAndRefresh();
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Fatal Error Connecting to Database:\n" + ex.Message, "Database Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }


        private void ClearFieldsAndRefresh()
        {
            try
            {
                tbName.Clear();
                Utils.ChangeBackBorderControl(tbName);

                tbLname.Clear();
                Utils.ChangeBackBorderControl(tbLname);

                tbPhone.Clear();
                Utils.ChangeBackBorderControl(tbName);

                tbEmail.Clear();
                Utils.ChangeBackBorderControl(tbEmail);

                tbNumber.Clear();
                Utils.ChangeBackBorderControl(tbNumber);

                tbStreet.Clear();
                Utils.ChangeBackBorderControl(tbStreet);

                tbCity.Clear();
                Utils.ChangeBackBorderControl(tbCity);

                comboProvince.SelectedIndex = -1;
                Utils.ChangeBackBorderControl(comboProvince);

                tbCodePostal.Clear();
                Utils.ChangeBackBorderControl(tbCodePostal);

                imageViewer.Source = null;
                tbImage.Visibility = Visibility.Visible;
                Utils.ChangeBackBorderControl(btnPhoto);

                btnDelete.IsEnabled = false;
                btnUpdate.IsEnabled = false;

                studentList = (from studenet in Globals.ctx.Students select studenet).ToList<Student>();
                lvStudent.ItemsSource = studentList;
                lvStudent.Items.Refresh();
                Utils.AutoResizeColumns(lvStudent);
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error!:\n" + ex.Message, "Database Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!VerifyFields()) { return; }
            try
            {
                string name = tbName.Text;
                string lname = tbLname.Text;
                string phone = tbPhone.Text;
                string email = tbEmail.Text;
                string no = tbNumber.Text;
                string street = tbStreet.Text;
                string city = tbCity.Text;
                string province = comboProvince.Text;
                string codePostal = tbCodePostal.Text.ToUpper();
                byte[] photo = currStudentImage;

                Student student = new Student { FirstName = name, LastName = lname, Phone = phone, Email = email, No = no, Street = street, City = city, Province = province, CodePostal = codePostal, Photo = photo };
                Globals.ctx.Students.Add(student);
                Globals.ctx.SaveChanges();
                MessageBox.Show("Student info successfully Added. ", "Add Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFieldsAndRefresh();
            }
            catch (SystemException ex)
            {
                MessageBox.Show(ex.Message, "Loading image failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!VerifyFields()) { return; }
            try
            {
                Student student = (Student)lvStudent.SelectedItem;
                if (student != null)
                {
                    student.FirstName = tbName.Text;
                    student.LastName = tbLname.Text;
                    student.Phone = tbPhone.Text;
                    student.Email = tbEmail.Text;
                    student.No = tbNumber.Text;
                    student.Street = tbStreet.Text;
                    student.City = tbCity.Text;
                    student.Province = comboProvince.Text;
                    student.CodePostal = tbCodePostal.Text.ToUpper();
                    student.Photo = currStudentImage;

                    Globals.ctx.SaveChanges();
                    MessageBox.Show("Student info successfully Updated. ", "Update Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearFieldsAndRefresh();
                }
                else
                {
                    MessageBox.Show("Unable to find record to update","Internal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database operation failed: " + ex.Message,"Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                Student student = (Student)lvStudent.SelectedItem;
                if (student != null)
                {
                    Enrollment enrol1 = (from enrollment in Globals.ctx.Enrollments where enrollment.StudentId == student.Id select enrollment).FirstOrDefault();//check for association
                    if (enrol1 != null)
                    {

                        MessageBox.Show("Student was already enrolled in other courses and can't be deleted", "error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (MessageBox.Show("Are you sure to delete?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        Globals.ctx.Students.Remove(student);
                        Globals.ctx.SaveChanges();
                        MessageBox.Show("Student info successfully Deleted. ", "Delete Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                        ClearFieldsAndRefresh();
                    }
                }
                else
                {
                    MessageBox.Show("Unable to find record to delete","error",MessageBoxButton.OK,MessageBoxImage.Error);
                }
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database operation failed: " + ex.Message);
            }
        }

        private void btnPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Image files (*.jpg;*.jpeg;*.gif;*.png)|*.jpg;*.jpeg;*.gif;*.png|All Files (*.*)|*.*";
            openFile.RestoreDirectory = true;

            if (openFile.ShowDialog() == true)
            {
                try
                {
                    currStudentImage = File.ReadAllBytes(openFile.FileName);
                    tbImage.Visibility = Visibility.Hidden;
                    BitmapImage bitmap = Utils.ByteArrayToBitmapImage(currStudentImage);
                    imageViewer.Source = bitmap;
                }
                catch (Exception ex) when (ex is SystemException || ex is IOException)
                {
                    MessageBox.Show(ex.Message, "File reading failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private void lvStudent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvStudent.SelectedIndex == -1)
            {
                ClearFieldsAndRefresh();
                return;
            }
            try
            {
                Student student = (Student)lvStudent.SelectedItem;
                btnDelete.IsEnabled = true;
                btnUpdate.IsEnabled = true;
                tbName.Text = student.FirstName;
                tbLname.Text = student.LastName;
                tbPhone.Text = student.Phone;
                tbEmail.Text = student.Email;
                tbNumber.Text = student.No;
                tbStreet.Text = student.Street;
                tbCity.Text = student.City;
                comboProvince.Text = student.Province;
                tbCodePostal.Text = student.CodePostal;
                currStudentImage = student.Photo;
                imageViewer.Source = Utils.GetBitmapImageV2(student.Photo); // ex
            }
            catch (SystemException ex)
            {
                MessageBox.Show(ex.Message, "Loading image failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ClearFieldsAndRefresh();
        }

        private bool VerifyFields()
        {
            List<String> errorsList = new List<string>();
            if (tbName.Text.Length < 1 || tbName.Text.Length > 25)
            {
                errorsList.Add("FirstName length must be 1 to 25 characters");
                Utils.ChangeBorderControl(tbName);
            }
            else
            {
                Utils.ChangeBackBorderControl(tbName);
            }
            if (tbLname.Text.Length < 1 || tbLname.Text.Length > 25)
            {
                errorsList.Add("LastName length must be 1 to 25 characters");
                Utils.ChangeBorderControl(tbLname);
            }
            else
            {
                Utils.ChangeBackBorderControl(tbLname);
            }
            Regex regexPhone = new Regex(Globals.phonePattern);
            if (!regexPhone.IsMatch(tbPhone.Text))
            {
                errorsList.Add("Phone number must be match (123) 456-7890");
                Utils.ChangeBorderControl(tbPhone);
            }
            else
            {
                Utils.ChangeBackBorderControl(tbPhone);
            }
            Regex regexEmail = new Regex(Globals.emailPattern);
            if (!regexEmail.IsMatch(tbEmail.Text))
            {
                errorsList.Add("Email is not valid");
                Utils.ChangeBorderControl(tbEmail);
            }
            else
            {
                Utils.ChangeBackBorderControl(tbEmail);
            }
            if (currStudentImage == null)
            {
                errorsList.Add("Choose a picture , Image must not be null");
                Utils.ChangeBorderControl(btnPhoto);
            }
            else
            {
                Utils.ChangeBackBorderControl(btnPhoto);
            }
            if (comboProvince.Text == "")
            {
                errorsList.Add("Province must be selected");
                Utils.ChangeBorderControl(comboProvince);
            }
            else
            {
                Utils.ChangeBackBorderControl(comboProvince);
            }
            if (tbCity.Text.Length < 1 || tbCity.Text.Length > 20)
            {
                errorsList.Add("City length must be 1 to 20 characters");
                Utils.ChangeBorderControl(tbCity);
            }
            else
            {
                Utils.ChangeBackBorderControl(tbCity);
            }
            if (tbStreet.Text.Length < 1 || tbStreet.Text.Length > 20)
            {
                errorsList.Add("Street length must be 1 to 20 characters");
                Utils.ChangeBorderControl(tbStreet);
            }
            else
            {
                Utils.ChangeBackBorderControl(tbStreet);
            }
            if (tbNumber.Text.Length < 1 || tbNumber.Text.Length > 20)
            {
                errorsList.Add("Number length must be 1 to 10 characters");
                Utils.ChangeBorderControl(tbNumber);
            }
            else
            {
                Utils.ChangeBackBorderControl(tbNumber);
            }
            Regex regexCodePostal = new Regex(Globals.codePostalPattern);
            if (!regexCodePostal.IsMatch(tbCodePostal.Text))
            {
                errorsList.Add("CodePostal is not valid");
                Utils.ChangeBorderControl(tbCodePostal);
            }
            else
            {
                Utils.ChangeBackBorderControl(tbCodePostal);
            }
            if (errorsList.Count != 0)
            {
                MessageBox.Show(string.Join("\n", errorsList), "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return (errorsList.Count == 0);
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
                        csv.WriteRecords(studentList);
                    }
                }
                catch (IOException ex)
                {
                    System.Windows.MessageBox.Show("Error writing file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}
