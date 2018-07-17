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
    /// Interaction logic for ManagerLogin.xaml
    /// </summary>
    public partial class ManagerLogin : Window
    {
        private Employee emp;
        private int count;
        Window1 newUser;

        public ManagerLogin(Window1 newUser)
        {
            InitializeComponent();
            this.newUser = newUser;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ManagerConfirm confirm = new ManagerConfirm(newUser);

            //See if this is called with current user or using open api
            //This is only used for public new user button
            if(Manager.currentUser == null && count <3)
            {
                if (string.IsNullOrWhiteSpace(username.Text) || string.IsNullOrWhiteSpace(password.Password.ToString()) || !DB.GetConnectionString(username.Text.ToLower(), password.Password.ToString()))
                {
                    MessageBox.Show("Error Wrong Username, Password, or the User is not a manager!");
                    count++;
                    return;
                }
                else
                {
                    Manager.currentUser = DB.Employees.GetByUsername(username.Text.ToLower());
                    if(Manager.currentUser == null)
                    {
                        return;
                    }

                    if (Manager.currentUser.isManager)
                    {
                        confirm.ShowDialog();
                        
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Error Wrong Username, Password, or the User is not a manager!");
                        count++;
                    }

                    //Clear connection and user
                    Manager.currentUser = null;
                    DB.ClearConnectionString();

                    return;
                }
            }

            if (username.Text != "" && (DB.Employees.GetByUsername(username.Text.ToLower()) != null))
            {
                emp = DB.Employees.GetByUsername(username.Text.ToLower());
                if (emp.isManager == true && username.Text.ToLower() == emp.username && true == emp.VerifyPassword(password.Password.ToString()))
                {
                    confirm.ShowDialog();
                    this.Close();
                }
                else if (count < 3)
                {
                    MessageBox.Show("Error Wrong Username, Password, or the User is not a manager!");
                    count++;
                }
                else
                {
                    this.Close();
                }
            }
            else if (count < 3)
            {
                MessageBox.Show("Error Wrong Username, Password, or the User is not a manager!");
                count++;
            }
            else
            {
                this.Close();
            }
        }
    }
}
