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

namespace Lackluster
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Boolean createAccount = false;
        public static Employee emptemp;
        public static String newpass = "";
        public static String secq = "";
        public static String answer = "";

        Employee _employee = new Employee();
        public Window1()
        {
            InitializeComponent();
            emptemp = null;
            empGrid.DataContext = _employee;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (username.Text == "" || String.IsNullOrWhiteSpace(passwordBox.Password) || firstname.Text == "" || lastname.Text == "" || email.Text == "" || secquestion.Text == "" || String.IsNullOrWhiteSpace(answerbox.Password))
            {
                MessageBox.Show("One or more fields are Blank!");
            }
            else if (passwordBox.Password != passwordBox_Confirm.Password || answerbox.Password != answerconfirm.Password)
            {
                MessageBox.Show("Password or Answers do not match!");
            }
            else if (!DB.Employees.Public_IsUsernameAvailable(username.Text.ToLower()))
            {
                MessageBox.Show("User already exists! Please choose a new Username!");
            }
            else
            {
                emptemp = new Employee(username.Text.ToLower(), firstname.Text, lastname.Text, email.Text, true, false);

                

                answer = answerbox.Password;
                secq = secquestion.Text;

                newpass = passwordBox.Password.ToString();
                ManagerLogin log = new ManagerLogin(this);
                log.ShowDialog();
               // this.Close();
            }
        }

        private void secquestion_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
//
        private int _noOfErrorsOnAddUser = 0;
        private void AddUserValidation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                _noOfErrorsOnAddUser++;
            else
                _noOfErrorsOnAddUser--;
        }

        private void AddUser_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _noOfErrorsOnAddUser == 0;
            e.Handled = true;
        }
        #region TrimText
        private void username_LostFocus(object sender, RoutedEventArgs e)
        {
            username.Text = username.Text.Trim();
        }

        private void firstname_LostFocus(object sender, RoutedEventArgs e)
        {
            firstname.Text = firstname.Text.Trim();

        }

        private void lastname_LostFocus(object sender, RoutedEventArgs e)
        {
            lastname.Text = lastname.Text.Trim();

        }

        private void email_LostFocus(object sender, RoutedEventArgs e)
        {
            email.Text = email.Text.Trim();
        }
        #endregion
        
    }
}
