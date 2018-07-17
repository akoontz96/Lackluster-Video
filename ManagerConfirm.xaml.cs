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
    /// Interaction logic for ManagerConfirm.xaml
    /// </summary>
    public partial class ManagerConfirm : Window
    {
        Window1 newUser;
        public ManagerConfirm(Window1 newUser)
        {
            InitializeComponent();
            this.newUser = newUser;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(Manager.currentUser == null && !Manager.currentUser.isManager)
            {
                MessageBox.Show("Error");
                this.Close();
                return;
            }

            if (chkIsMan.IsChecked == true)
            {
                Window1.emptemp.isManager = true;
            }

            //Create Employee in db
            Employee ee = DB.Employees.Create(Window1.emptemp);
            if(ee == null)
            {
                MessageBox.Show("Error creating new user.");
                this.Close();
                return;
            }


            ee.SetPassword(Window1.newpass);
            ee.SetQA(Window1.secq, Window1.answer);

            ee.Save();

            MessageBox.Show("User Has Successfully been Created!");
            newUser.Close();
            this.Close();
        }

    }
}
