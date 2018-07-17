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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lackluster
{
    /// <summary>
    /// Interaction logic for WindowLogIn.xaml
    /// </summary>
    public partial class WindowLogIn : Window
    {
        public WindowLogIn()
        {
            InitializeComponent();
            txtUser.Focus();
        }
        private Employee emp;
        private void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
            if (!DB.GetConnectionString(txtUser.Text.ToLower(), pbxPassword.Password.ToString()))
            {
                MessageBox.Show("Please enter a correct employee id or password.");
                return;
            }

            //Create instance of Manager window to control objects
            Manager manager = new Manager();
            if (txtUser.Text != "" && (DB.Employees.GetByUsername(txtUser.Text.ToLower())  != null) ) //&& DB.Employee)
            {
                
                emp = DB.Employees.GetByUsername(txtUser.Text.ToLower());

                //Determine if user is Manager or Employee

                if (emp.isActive && emp.username == txtUser.Text.ToLower() && true == emp.VerifyPassword(pbxPassword.Password.ToString()))
                {
                    if (emp.isManager == true)
                    {
                        //Set lblRole to Manager
                        manager.lblRole.Content = "MANAGER";

                        //Create a LinearGradientBrush to set header colors
                        LinearGradientBrush green = new LinearGradientBrush();

                        //Set GradientStops
                        green.GradientStops.Add(new GradientStop(Color.FromArgb(255, 26, 226, 99), 0));
                        green.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 0, 0), 1));

                        //Assign gradient to Manager's recHeader
                        manager.recHeader.Fill = green;

                        //Open the manager window, pass the current user, and close the login window
                        manager.Show();
                        Manager.currentUser = emp;
                        this.Hide();

                    }
                    else
                    {

                        //Set lblRole to Employee
                        manager.lblRole.Content = "CLERK";

                        //Create a LinearGradientBrush to set header colors
                        LinearGradientBrush green = new LinearGradientBrush();

                        //Set GradientStops
                        green.GradientStops.Add(new GradientStop(Color.FromArgb(255, 26, 99, 226), 0));
                        green.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 0, 0), 1));

                        //Assign gradient to Manager's recHeader
                        manager.recHeader.Fill = green;

                        //Turn off tab and buttons not appropriate for an Employee
                        //manager.tbReports.Visibility = Visibility.Hidden;
                        manager.tbEmployee.Visibility = Visibility.Hidden;
                        manager.btnMovieAdd.Visibility = Visibility.Hidden;
                        manager.btnEditMovie.Visibility = Visibility.Hidden;
                        manager.btnMovieUpdate.Visibility = Visibility.Hidden;
                        manager.btnMovieUpdateCancel.Visibility = Visibility.Hidden;

                        //Open the manager window, pass the current user, and close the login window
                        manager.Show();
                        Manager.currentUser = emp;
                        this.Hide();

                    }
                }
                else
                {
                    MessageBox.Show("Please enter a correct employee id or password.");
                }
            }
            else
            {
                MessageBox.Show("Please enter a correct employee id or password.");
            }

        }
        private void btnForgot_Password_Click_1(object sender, RoutedEventArgs e)
        {
            Forgot_Password fp = new Forgot_Password();
            fp.ShowDialog();
        }
        private void newuser_Click(object sender, RoutedEventArgs e)
        {
            Window1 newuser = new Window1();
            newuser.ShowDialog();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();

        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            Help help = new Help();
            help.ShowDialog();
        }

        private void pbxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter || e.Key == Key.Return)
            {
                btnLogIn_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }
    }
}
