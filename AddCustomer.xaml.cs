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
    /// Interaction logic for AddCustomer.xaml
    /// </summary>
    public partial class AddCustomer : Window
    {
        Manager manager;
        private Customer _customer = new Customer();
        public AddCustomer(Manager callingWindow)
        {
            InitializeComponent();
            custGrid.DataContext = _customer;
            manager = callingWindow;
        }

        private int _noOfErrorsOnScreen = 0;
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                _noOfErrorsOnScreen++;
            else
                _noOfErrorsOnScreen--;
        }

        private void AddCustomer_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _noOfErrorsOnScreen == 0;
            e.Handled = true;
        }
        private void btnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (txtNewCustomerFirstName.Text != "" || txtNewCustomerLastName.Text != "" || txtNewCustomerPhoneNumber.Text != "" || txtNewCustomerEmail.Text != "")
            {

                if (DB.Customers.GetByNumber(txtNewCustomerPhoneNumber.Text) == null)
                {

                    Customer newCustomer = new Customer();
                    newCustomer.firstName = txtNewCustomerFirstName.Text;
                    newCustomer.lastName = txtNewCustomerLastName.Text;
                    newCustomer.phoneNumber = txtNewCustomerPhoneNumber.Text;
                    newCustomer.email = txtNewCustomerEmail.Text;
                    newCustomer.isActive = true;
                    Customer insertedCustomer = DB.Customers.Create(newCustomer);

                    if (insertedCustomer != null)
                    {

                        if(manager.tbCustomer.IsSelected)
                        {
                            manager.txtCustomerSearch.Text = insertedCustomer.phoneNumber;
                            manager.btnCustomerSearch_Click(sender, e);
                        }else
                        {
                            manager.txtCustomerPhoneNumberSearch.Text = insertedCustomer.phoneNumber;
                            manager.btnCustomerLookup_Click(sender, e);
                        }


                       

                        this.Close();
                    }else
                    {
                        MessageBox.Show("Can not create customer.", "Error");
                    }
                }
                else
                {
                    MessageBox.Show("Customer already exists with this phone number.");
                }
            }
            else
            {
                MessageBox.Show("You must complete all fields");
            }
        }

        #region TrimText
        private void txtNewCustomerFirstName_LostFocus(object sender, RoutedEventArgs e)
        {
            txtNewCustomerFirstName.Text = txtNewCustomerFirstName.Text.Trim();
        }

        private void txtNewCustomerLastName_LostFocus(object sender, RoutedEventArgs e)
        {
            txtNewCustomerLastName.Text = txtNewCustomerLastName.Text.Trim();

        }

        private void txtNewCustomerPhoneNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            txtNewCustomerPhoneNumber.Text = txtNewCustomerPhoneNumber.Text.Trim();

        }

        private void txtNewCustomerEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            txtNewCustomerEmail.Text = txtNewCustomerEmail.Text.Trim();

        }
        #endregion


    }
}
