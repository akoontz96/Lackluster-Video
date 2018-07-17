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
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Lackluster
{
    /// <summary>
    /// Interaction logic for Manager.xaml
    /// </summary>
    public partial class Manager : Window
    {
        #region Global Variables
        //Initialize total amount to zero
        double total = 0.00;
        double lateFeeTotal = 0.00;
        double rentalTotal = 0.00;


        public static Employee currentUser; //variable to hold current user
        public Customer currentCustomer; //Variable to hold the customer who was looked up
        private Movie selectedMovieInList; //Variable to hold selected movie in the movie search tab
        private Customer custToUpdate;
        private Employee empToUpdate;
        #endregion


        private Customer _customer = new Customer();
        private Movie _movie = new Movie();
        private Employee _employee = new Employee();
        public Manager()
        {
            InitializeComponent();
            txtCustomerPhoneNumberSearch.Focus();

            //Test Here
            custGrid.DataContext = _customer;
            movieGrid.DataContext = _movie;
            empGrid.DataContext = _employee;

            cboReports.Items.Add("Late Rentals");
            cboReports.Items.Add("Best Customers");
            cboReports.Items.Add("Best Movies");
            cboReports.Items.Add("Active Rentals");
            cboReports.SelectedIndex = 0;

            //Sets all customer tab fields disabled
            txtCustFirstName.IsEnabled = false;
            txtCustLastName.IsEnabled = false;
            txtCustPhone.IsEnabled = false;
            txtCustEmail.IsEnabled = false;
            txtPoints.IsEnabled = false;
            custActiveCheck.IsEnabled = false;

            //Disabled customer edit/use  buttons
            btnCustUpdate.IsEnabled = false;
            btnCustEdit.IsEnabled = false;
            btnCustUse.IsEnabled = false;
            btnCustCancel.IsEnabled = false;
            lstActiveRentals.IsEnabled = false;

            //Disable all the movie update fields
            cboMovieUPC.IsEnabled = false;
            cboMovieUPC.IsEnabled = false;
            txtMovieTitle.IsEnabled = false;
            txtMovieYear.IsEnabled = false;
            txtMovieRating.IsEnabled = false;
            txtMovieGenre.IsEnabled = false;
            txtMoviePrice.IsEnabled = false;
            chkMovieActive.IsEnabled = false;

            //Clear the checkboxes
            chkMovieRented.IsChecked = false;
            chkMovieActive.IsChecked = false;

            //Disable the movie update and cancel buttons
            btnMovieUpdateCancel.IsEnabled = false;
            btnMovieUpdate.IsEnabled = false;

            //Disable Employee edit fields
            txtEmpUserName.IsEnabled = false;
            txtEmpFName.IsEnabled = false;
            txtEmpLName.IsEnabled = false;
            txtEmpEmail.IsEnabled = false;
            chkEmpIsActive.IsEnabled = false;
            chkEmpIsManager.IsEnabled = false;

            //Disable employee edit, update, and and cancel buttons
            btnEmpEdit.IsEnabled = false;
            btnEmpUpdate.IsEnabled = false;
            btnEmpCancel.IsEnabled = false;

        }

        //tab changes event
       /* private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           //customer tab
           if (tabControl.SelectedIndex == 3)
            {
                //if customer lookedup on rent screen search for them
                if(currentCustomer!= null)
                {
                    txtCustomerSearch.Text = currentCustomer.phoneNumber;
                }
                else
                {
                    txtCustomerSearch.Text = "";
                }
                //calls customer serch function to get ubdated information
                btnCustomerSearch_Click(sender, e);
            }
        }*/

        //Logout Button
        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            //Sets all variables null
            total = 0.0;
            Manager.currentUser = null;
            currentCustomer = null;
            selectedMovieInList = null;
            custToUpdate = null;

            DB.ClearConnectionString();
            
            //Create instance of Manager window to control objects
            WindowLogIn login = new WindowLogIn();

            //Open the login window and close the manager window
            login.Show();
            this.Hide();
        }

        #region Rent Tab
        private void btnStartRental_Click(object sender, RoutedEventArgs e)
        {


            //Make sure there is a currentCustomer set
            if(currentCustomer != null)
            {
                //If customer is not active they cannot rent movies
                if (!currentCustomer.isActive)
                {
                    MessageBox.Show("Customer is inactive, they can not rent movies.", "Inactive");
                    return;
                }


                //Hide and lock fields
                btnCustomerLookup.IsEnabled = false;
                btnNoCustomerFoundAdd.IsEnabled = false;
                btnUpdateCustomerInfo.IsEnabled = false;
                txtCustomerPhoneNumberSearch.Focusable = false;
                txtCustomerFirstName.Focusable = false;
                txtCustomerLastName.Focusable = false;
                txtCustomerPhoneNumber.Focusable = false;
                txtCustomerEmail.Focusable = false;
                btnCustomerLookup.Focusable = false;
                btnCustomerAdd.Focusable = false;
                btnUpdateCustomerInfo.Focusable = false;
                tbReturn.Focusable = false;
                tbMovies.Focusable = false;
                tbCustomer.Focusable = false;
                tbReports.Focusable = false;
                tbEmployee.Focusable = false;

                //Show the txtRentalEntry Box and btnDelete
                txtRentalEntry.Visibility = Visibility.Visible;
                btnDeleteRentalEntry.Visibility = Visibility.Visible;
                btnCancelRental.Visibility = Visibility.Visible;
                btnCompleteRental.IsEnabled = true;


                //Set focus to the txtRentalEntry Box
                txtRentalEntry.Focus();

                //Change start rental box text
                btnStartRental.Content = "Continue Rental";
            }
            else
            {
                //Warn user they need to look up a customer
                MessageBox.Show("You need to look up a customer");
            }

        }

        private void txtScanEntry_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Look for the Return key press
            if (e.Key == Key.Return)
            {
                //Variable to hold whether movie is already in the list
                bool found = false;

                //Search each list entry
                foreach (Movie i in lstRent.Items)
                {
                    //Determine if the upc entered is already in the list
                    if (i.upc.ToString().Equals(txtRentalEntry.Text))
                    {
                        //Set found to true since the movie is already in the list
                        found = true;
                    }
                }

                //Check if the movie is already in the list
                if (found)
                {
                    MessageBox.Show("Movie already added");
                }
                else
                {

                    //Create a movie object by passing the scanned text
                    Movie scannedEntry = new Movie();
                    scannedEntry = DB.Movies.Get(txtRentalEntry.Text);


                    //Check if the movie is actually a movie in our database & not already rented out
                    if (scannedEntry != null && !scannedEntry.isRented)
                    {
                        //Add the movie object to the list
                        lstRent.Items.Add(scannedEntry);

                        //Increase the total
                        rentalTotal = rentalTotal + Convert.ToDouble(scannedEntry.price);
                        updateTotals();
                    } else
                    {
                        MessageBox.Show("This is not an active movie");
                    }

                    
                }

                //Reset the txtRentalEntry box
                txtRentalEntry.Text = "";
            }

        }

        private void btnCompleteRental_Click(object sender, RoutedEventArgs e)
        {
           
            btnStartRental.Content = "Start Rental";

            btnCompleteRental.IsEnabled = false;
            btnStartRental.IsEnabled = false;

            //Hide the txtRentalEntry Box and btnDelete
            txtRentalEntry.Visibility = Visibility.Hidden;
            btnDeleteRentalEntry.Visibility = Visibility.Hidden;
            btnCancelRental.Visibility = Visibility.Hidden;

            //Check if there are movies in the list
            if (lstRent.HasItems)
            {
                double rentTotal = lateFeeTotal;
                int totalMoviesRented = 0;
                string notRented = "";
                List<Movie> notRentedLst = new List<Movie>();

                //rent each movie in rental list
                foreach (Movie i in lstRent.Items)
                {
                    //if add rental to db is sucessful
                    if (DB.Rentals.Create(currentUser, currentCustomer, i))
                    {
                        //increase rent total and number of movies rented
                        rentTotal += Convert.ToDouble(i.price);
                        totalMoviesRented += 1;
                    }
                    // else if DB returns false meaning error behind the scenes
                    else
                    {
                        //add error movie to error list to display
                        notRentedLst.Add(i);
                    }
                }

                
                //foreach movie in the error list
                foreach (Movie i in notRentedLst)
                {
                    //add movie title and upc for display
                    notRented += i.title + " " + i.upc + " \n";
                }

                //Default box for rented movies
                string display = $"You rented {totalMoviesRented} movie(s)\nFor a total of {rentTotal:C}\nTo {txtCustomerFirstName.Text} {txtCustomerLastName.Text}.";

                //If error movies make sure to show them in first box
                if(notRentedLst.Count > 0)
                {
                    display += $"\n\nThere is {notRentedLst.Count} error(s.)";
                }

                //Show message showing how many movies were rented and the total
                MessageBox.Show(display);

                //if there was an error list of movies, display problem movies
                if (notRentedLst.Count > 0)
                {
                    MessageBox.Show("The following movies could not be rented:\n\n" + notRented);
                }

                //Clear the lists
                lstRent.Items.Clear();
                notRentedLst.Clear();


            }

            DB.Customers.ClearLateFee(currentCustomer);
            clearTotals();


            //UnHide and unlock fields 
            btnCustomerLookup.IsEnabled = true;
            btnNoCustomerFoundAdd.IsEnabled = true;
            btnUpdateCustomerInfo.IsEnabled = true;
            txtCustomerPhoneNumberSearch.Focusable = true;
            txtCustomerFirstName.Focusable = true;
            txtCustomerLastName.Focusable = true;
            txtCustomerPhoneNumber.Focusable = true;
            txtCustomerEmail.Focusable = true;
            btnCustomerLookup.Focusable = true;
            btnCustomerAdd.Focusable = true;
            btnUpdateCustomerInfo.Focusable = true;
            tbReturn.Focusable = true;
            tbMovies.Focusable = true;
            tbCustomer.Focusable = true;
            tbReports.Focusable = true;
            tbEmployee.Focusable = true;

            //Clear out the customer
            currentCustomer = null;
            txtCustomerPhoneNumberSearch.Text = "";
            txtCustomerFirstName.Text = "";
            txtCustomerLastName.Text = "";
            txtCustomerPhoneNumber.Text = "";
            txtCustomerEmail.Text = "";

            //Reenable customer lookup 
            btnCustomerLookup.IsEnabled = true;
            btnNoCustomerFoundAdd.IsEnabled = true;
            btnUpdateCustomerInfo.IsEnabled = false;
        }

        public void btnCustomerLookup_Click(object sender, RoutedEventArgs e)
        {          

            //Pass the phone number to the Customer object to create a new object
            Customer searchCustomer = new Customer();

            searchCustomer = DB.Customers.GetByNumber(txtCustomerPhoneNumberSearch.Text);


            if (searchCustomer != null)
            {
                //Put the data in the rent boxes
                txtCustomerFirstName.Text = searchCustomer.firstName;
                txtCustomerLastName.Text = searchCustomer.lastName;
                txtCustomerPhoneNumber.Text = searchCustomer.phoneNumber;
                txtCustomerEmail.Text = searchCustomer.email;

                //Set the current customer to the found searched customer for later use in the rental process 
                //(eg. Creating the rental record)
                currentCustomer = searchCustomer;
                lateFeeTotal = DB.Customers.GetLateFee(currentCustomer);
                updateTotals();

                //enable update button
                btnUpdateCustomerInfo.IsEnabled = true;
                btnStartRental.IsEnabled = true;
            }
            else
            {
                //Phone number not found in database
                MessageBox.Show("Phone number not found");

                // disbale update infor button
                btnUpdateCustomerInfo.IsEnabled = false;


                //Clear out the customer
                currentCustomer = null;
                //txtCustomerPhoneNumberSearch.Text = "";
                txtCustomerFirstName.Text = "";
                txtCustomerLastName.Text = "";
                txtCustomerPhoneNumber.Text = "";
                txtCustomerEmail.Text = "";

                //set currentCustomer to null
                currentCustomer = null;
            }


        }
       
        private void btnDeleteRentalEntry_Click(object sender, RoutedEventArgs e)
        {
            if(lstRent.SelectedItems.Count == 0)
            {
                MessageBox.Show("Must select movie to delete from rental list.");
                return;
            }


            List<Movie> delete = new List<Movie>();
            foreach (Movie soonToBeDeletedMovie in lstRent.SelectedItems)
            {
                    //Reduce the total by the movie price
                    rentalTotal -= Convert.ToDouble(soonToBeDeletedMovie.price);

                //Update txtTotal with new price
                updateTotals();

                //Add item to Delete List
                delete.Add(soonToBeDeletedMovie);
            }

            foreach (Movie i in delete) 
            {
                //Delete from rent list
                lstRent.Items.Remove(i);
            }
            //Give focuse back to the text entry box
            txtRentalEntry.Focus();
        }

        private void btnUpdateCustomerInfo_Click(object sender, RoutedEventArgs e)
        {

            //Brings to customer tab to update inforamtion
            if (currentCustomer == null || currentCustomer.phoneNumber == null)
            { return; }
            txtCustomerSearch.Text = currentCustomer.phoneNumber;
            chkCustDeactiveSearch.IsChecked = true;
            btnCustomerSearch_Click(sender, e);

            custToUpdate = currentCustomer;
            btnCustEdit_Click(sender, e);
            tabControl.SelectedIndex = 3;

            return;
            //Make sure there is a currentCustomer set
            if (currentCustomer != null)
            {
                if (currentCustomer.phoneNumber != txtCustPhone.Text && DB.Customers.GetByNumber(txtCustPhone.Text) != null)
                {
                    MessageBox.Show("Phone number is used by another customer, cannot update.", "Update Error");
                    return;
                }

                //Update currentCustomer with the info on the text boxex
                currentCustomer.phoneNumber = txtCustomerPhoneNumber.Text;
                currentCustomer.firstName = txtCustomerFirstName.Text;
                currentCustomer.lastName = txtCustomerLastName.Text;
                currentCustomer.email = txtCustomerEmail.Text;

                //Send the updated customer to the database
                if (DB.Customers.Update(currentCustomer))
                {
                    MessageBox.Show($"{currentCustomer.firstName} {currentCustomer.lastName}'s information was updated");
                }
                else
                {
                    MessageBox.Show($"Could not update {currentCustomer.firstName} {currentCustomer.lastName}'s information");
                }

            }else
            {
                MessageBox.Show("Must lookup customer first.");
            }
        }


        private void btnNoCustomerFoundAdd_Click(object sender, RoutedEventArgs e)
        {
            //Open the add customer window
            AddCustomer addCustomer = new AddCustomer(this);
            addCustomer.ShowDialog();
        }

        private void btnCancelRental_Click(object sender, RoutedEventArgs e)
        {
            btnStartRental.Content = "Start Rental";

            btnCompleteRental.IsEnabled = false;
            btnStartRental.IsEnabled = false;

            //Hide the txtRentalEntry Box and btnDelete
            txtRentalEntry.Visibility = Visibility.Hidden;
            btnDeleteRentalEntry.Visibility = Visibility.Hidden;
            btnCancelRental.Visibility = Visibility.Hidden;

            //Check if there are movies in the list
            if (lstRent.HasItems)
            {
                double rentTotal = 0.00;
                int totalMoviesRented = 0;
                string notRented = "";
                List<Movie> notRentedLst = new List<Movie>();

                //Clear the lists
                lstRent.Items.Clear();
                notRentedLst.Clear();


                //Reset txtTotal
                clearTotals();

                //Reset total
                total = 0.00;
            }

            //UnHide and unlock fields 
            btnCustomerLookup.IsEnabled = true;
            btnNoCustomerFoundAdd.IsEnabled = true;
            btnUpdateCustomerInfo.IsEnabled = true;
            txtCustomerPhoneNumberSearch.Focusable = true;
            txtCustomerFirstName.Focusable = true;
            txtCustomerLastName.Focusable = true;
            txtCustomerPhoneNumber.Focusable = true;
            txtCustomerEmail.Focusable = true;
            btnCustomerLookup.Focusable = true;
            btnCustomerAdd.Focusable = true;
            btnUpdateCustomerInfo.Focusable = true;
            tbReturn.Focusable = true;
            tbMovies.Focusable = true;
            tbCustomer.Focusable = true;
            tbReports.Focusable = true;
            tbEmployee.Focusable = true;

            //Clear out the customer
            currentCustomer = null;
            txtCustomerPhoneNumberSearch.Text = "";
            txtCustomerFirstName.Text = "";
            txtCustomerLastName.Text = "";
            txtCustomerPhoneNumber.Text = "";
            txtCustomerEmail.Text = "";

            //Reenable customer lookup 
            btnCustomerLookup.IsEnabled = true;
            btnNoCustomerFoundAdd.IsEnabled = true;
            btnUpdateCustomerInfo.IsEnabled = false;
        }

        #endregion
        #region Return Tab
        private void btnStartReturn_Click(object sender, RoutedEventArgs e)
        {
            //Show the txtReturnEntry Box and btnDeleteReturnEntry
            txtReturnEntry.Visibility = Visibility.Visible;
            btnDeleteReturnEntry.Visibility = Visibility.Visible;
            btnCancelReturn.Visibility = Visibility.Visible;
            btnCompleteReturn.IsEnabled = true;

            //Lock tabs
            tbRent.Focusable = false;
            tbMovies.Focusable = false;
            tbCustomer.Focusable = false;
            tbReports.Focusable = false;
            tbEmployee.Focusable = false;

            //Set focus to the txtReturnEntry Box
            txtReturnEntry.Focus();
        }

        private void txtReturnEntry_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Look for the Return key press
            if (e.Key == Key.Return)
            {
                //Variable to hold whether movie is already in the list
                bool found = false;

                //Search each list entry
                foreach (Movie i in lstReturn.Items)
                {
                    //Determine if the upc entered is already in the list
                    if (i.upc.ToString().Equals(txtReturnEntry.Text))
                    {
                        //Set found to true since the movie is already in the list
                        found = true;
                    }
                }

                //Check if the movie is already in the list
                if (found)
                {
                    MessageBox.Show("Movie already added");
                }
                else
                {

                    //Create a movie object by passing the scanned text
                    Movie scannedEntry = new Movie();
                    scannedEntry = DB.Movies.Get(txtReturnEntry.Text);

                    //Check if the movie is actually a movie in our database & is rented out
                    if (scannedEntry != null && scannedEntry.isRented)
                    {
                        //Add the movie object to the list
                        lstReturn.Items.Add(scannedEntry);

                    }
                    else
                    {
                        MessageBox.Show("This movie is not rented out");
                    }

                }

                //Reset the txtRentalEntry box
                txtReturnEntry.Text = "";
            }
        }

        private void btnCompleteReturn_Click(object sender, RoutedEventArgs e)
        {
            //Hide the txtReturnEntry Box and btnDeleteReturnEntry
            txtReturnEntry.Visibility = Visibility.Hidden;
            btnDeleteReturnEntry.Visibility = Visibility.Hidden;
            btnCancelReturn.Visibility = Visibility.Hidden;
            btnCompleteReturn.IsEnabled = false;

            //Check if there are movies in the list
            if (lstReturn.HasItems){

                int moviesReturned = 0;
                List<Movie> notReturnedList = new List<Movie>();
                foreach (Movie i in lstReturn.Items)
                {
                    // check if return movie was sucessful
                    if (DB.Rentals.Return(currentUser, i))
                    {
                        moviesReturned += 1;
                    }
                    else
                    {
                        //if did not return sucessfully add to error list.
                        notReturnedList.Add(i);
                    }
                }

                //default display text
                string display = $"You returned { lstReturn.Items.Count} movie(s) sucessfully.";


                //if movie had error returning
                if (notReturnedList.Count > 0)
                {
                    display += $"/n/nThere is {notReturnedList.Count} error(s)";
                }

                //Show message showing how many movies were rented and the total
                MessageBox.Show(display);

                //display return errors
                if(notReturnedList.Count > 0)
                {
                    string errors = "The following movies could not be returned:/n/n";
                    foreach (Movie i in notReturnedList)
                    {
                        errors += i.title + " " + i.upc +"\n";
                    }
                    MessageBox.Show(errors);
                }

                //Clear the list
                lstReturn.Items.Clear();
            }

            //Unlock tabs
            tbRent.Focusable = true;
            tbMovies.Focusable = true;
            tbCustomer.Focusable = true;
            tbReports.Focusable = true;
            tbEmployee.Focusable = true;
        }

        private void btnDeleteReturnEntry_Click(object sender, RoutedEventArgs e)
        {
            if (lstReturn.SelectedItems.Count == 0)
            {
                MessageBox.Show("Must select movie to delete from returns list.");
                return;
            }

            List<Movie> delete = new List<Movie>();

            foreach (Movie i in lstReturn.SelectedItems)
            {
                //Add to delete list
                delete.Add(i);
            }

            foreach (Movie i in delete)
            {
                //Delete the item
                lstReturn.Items.Remove(i);
            }
            //Give focuse back to the text entry box
            txtReturnEntry.Focus();
        }

        private void btnCancelReturn_Click(object sender, RoutedEventArgs e)
        {
            //Hide the txtReturnEntry Box and btnDeleteReturnEntry
            txtReturnEntry.Visibility = Visibility.Hidden;
            btnDeleteReturnEntry.Visibility = Visibility.Hidden;
            btnCancelReturn.Visibility = Visibility.Hidden;
            btnCompleteReturn.IsEnabled = false;



            //Clear the list
            lstReturn.Items.Clear();

            //Unlock tabs
            tbRent.Focusable = true;
            tbMovies.Focusable = true;
            tbCustomer.Focusable = true;
            tbReports.Focusable = true;
            tbEmployee.Focusable = true;
        }

        #endregion
        #region Movie Tab
        private void btnMovieSearch_Click(object sender, RoutedEventArgs e)
        {
            lstMovies.Items.Clear();
            List<Movie> searchMovie;

            //Pass input from textbox to the query which returns a list of movie objects
            searchMovie = DB.Movies.GetByTitleOrUPC(txtMovieSearch.Text);

            if (searchMovie != null)
            {
                //Add movie list details to listview
                foreach (Movie i in searchMovie)
                {
                    lstMovies.Items.Add(i);
                }

                lstMovies.SelectedIndex = 0;
            }
            else
            {
                //If movie title not found in database
                MessageBox.Show("Movies not found");
            }

            
        }

        
        /*private void lstMovies_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            selectedMovieInList = lstMovies.SelectedItem as Movie;
            if (selectedMovieInList != null)
            {
                btnEditMovie.IsEnabled = true;
                
                //Clear upc drop down
                cboMovieUPC.Items.Clear();

                //Add Master Title to the top of the combo box to represent MOTHA
                selectedMovieInList.upc = "Master Title";
                cboMovieUPC.Items.Add(selectedMovieInList);
                cboMovieUPC.SelectedIndex = 0;

                //SetBinding fields = total mother information
                txtMovieTitle.Text = selectedMovieInList.title;
                txtMovieYear.Text = selectedMovieInList.releaseYear;
                txtMovieGenre.Text = selectedMovieInList.genre;
                txtMovieRating.Text = selectedMovieInList.rating;
                txtMoviePrice.Text = selectedMovieInList.price;
                chkMovieActive.IsChecked = selectedMovieInList.isActive;
            }
        }*/

        //private void btnEditCopy_Click(object sender, RoutedEventArgs e)
        //{
        //    if(selectedMovieInList != null)
        //    {
        //        tbRent.Focusable = false;
        //        tbReturn.Focusable = false;
        //        tbCustomer.Focusable = false;
        //        tbReports.Focusable = false;
        //        tbEmployee.Focusable = false;
        //        btnMovieSearch.Focusable = false;
        //        btnMovieAdd.Focusable = false;
        //        txtMovieSearch.Focusable = false;
        //        lstMovies.IsEnabled = false;

        //        //Enable the update info and cancel buttons
        //        btnMovieUpdate.IsEnabled = true;
        //        btnMovieUpdateCancel.IsEnabled = true;

        //        //Hide the movie title fields are not enabled
        //        txtMovieTitle.IsEnabled = false;
        //        txtMovieYear.IsEnabled = false;
        //        txtMovieRating.IsEnabled = false;
        //        txtMovieGenre.IsEnabled = false;
        //        txtMoviePrice.IsEnabled = false;

        //        //Make sure the movie isn't rented out
        //        if (chkMovieRented.IsChecked == true)
        //        {
        //            //Reset buttons
        //            btnMovieUpdate.IsEnabled = false;
        //            btnMovieUpdateCancel.IsEnabled = false;

        //            MessageBox.Show($"Cannot alter {selectedMovieInList.title}({selectedMovieInList.upc}) while rented.");

        //        }
        //        else
        //        {
        //            //Enable the movie copy field
        //            chkMovieActive.IsEnabled = true;
        //        }
        //    } 
        //    else
        //    {
        //        MessageBox.Show("Select a movie in the list.");
        //    }

        //}

        private void btnEditMovie_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMovieInList != null)
            {
                //Disable the edit movie button
                btnEditMovie.IsEnabled = false;

                //Activate combobox upc
                cboMovieUPC.IsEnabled = true;
                
                //Get a list of Movie objects that are all the copies of the master title
                List < Movie > kids = DB.Movies.GetChildMovies(selectedMovieInList);

                if (kids != null)
                {
                    //Fill the combobox with the list of individual copies
                    foreach (Movie i in kids)
                    {
                        cboMovieUPC.Items.Add(i);

                    }
                }
                
                //Disable the tabs and movie buttons
                tbRent.Focusable = false;
                tbReturn.Focusable = false;
                tbCustomer.Focusable = false;
                tbReports.Focusable = false;
                tbEmployee.Focusable = false;

                btnMovieSearch.IsEnabled = false;
                btnMovieAdd.IsEnabled = false;
                txtMovieSearch.IsEnabled = false;
                lstMovies.IsEnabled = false;

                //Enable the update info and cancel buttons
                btnMovieUpdate.IsEnabled = true;
                btnMovieUpdateCancel.IsEnabled = true;

                //Enable the movie title fields
                txtMovieTitle.IsEnabled = true;
                txtMovieYear.IsEnabled = true;
                txtMovieRating.IsEnabled = true;
                txtMovieGenre.IsEnabled = true;
                //txtMoviePrice.IsEnabled = true;

                //Disable the individual copy fields
                chkMovieActive.IsEnabled = false;


                //Show the movie title fields
                //txtMovieTitle.IsEnabled = true;
                //txtMovieYear.IsEnabled = true;
                //txtMovieRating.IsEnabled = true;
                //txtMovieGenre.IsEnabled = true;

                //txtMoviePrice.IsEnabled = true;

                //Disable the movie copy field
                //chkMovieActive.IsEnabled = false;
                //movieGrid.DataContext = _movie;
                BindingExpression be = txtMovieTitle.GetBindingExpression(TextBox.TextProperty);
                BindingExpression be2 = txtMovieRating.GetBindingExpression(TextBox.TextProperty);
                BindingExpression be3 = txtMovieGenre.GetBindingExpression(TextBox.TextProperty);
                BindingExpression be4 = txtMovieYear.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
                be2.UpdateSource();
                be3.UpdateSource();
                be4.UpdateSource();
            }
            else
            {
                MessageBox.Show("Select a movie in the list.");
            }

        }

        private int _noOfErrorsOnMovieTab = 0;
        private void MovieTabValidation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                _noOfErrorsOnMovieTab++;
            else
                _noOfErrorsOnMovieTab--;
        }

        private void MovieTabEditMovie_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _noOfErrorsOnMovieTab == 0;
            e.Handled = true;
        }

        private void btnMovieUpdateCancel_Click(object sender, RoutedEventArgs e)
        {
            //Clear cboMoviesUPC //adds mother as main copy
            Movie mother = cboMovieUPC.Items.GetItemAt(0) as Movie;
            cboMovieUPC.Items.Clear();
            cboMovieUPC.Items.Add(mother);
            cboMovieUPC.SelectedIndex = 0;

            //Disable all the movie update fields
            cboMovieUPC.IsEnabled = false;
            txtMovieTitle.IsEnabled = false;
            txtMovieYear.IsEnabled = false;
            txtMovieRating.IsEnabled = false;
            txtMovieGenre.IsEnabled = false;
            txtMoviePrice.IsEnabled = false;
            chkMovieActive.IsEnabled = false;

            //Disable the movie update and cancel buttons
            btnMovieUpdateCancel.IsEnabled = false;
            btnMovieUpdate.IsEnabled = false;

            btnEditMovie.IsEnabled = true;

            //Reset fields
            //txtMovieUPC.Text = selectedMovieInList.upc;
            txtMovieTitle.Text = selectedMovieInList.title;
            txtMovieYear.Text = selectedMovieInList.releaseYear;
            txtMovieGenre.Text = selectedMovieInList.genre;
            txtMovieRating.Text = selectedMovieInList.rating;
            txtMoviePrice.Text = selectedMovieInList.price;
            chkMovieRented.IsChecked = selectedMovieInList.isRented;
            chkMovieActive.IsChecked = selectedMovieInList.isActive;

            ///hide is rented checkbox
            chkMovieRented.Visibility = Visibility.Hidden;
            chkMovieActive.Visibility = Visibility.Hidden;

            ////Clear selected movie variable
            //selectedMovieInList = null;
            ////txtMovieUPC.Text = "";
            //cboMovieUPC.Items.Clear();
            //txtMovieTitle.Text = "";
            //txtMovieGenre.Text = "";
            //txtMovieRating.Text = "";
            //txtMovieYear.Text = "";
            //txtMoviePrice.Text = "";
            //chkMovieActive.IsChecked = false;
            //chkMovieRented.IsChecked = false;


            //Enable the tabs and movie buttons
            tbRent.Focusable = true;
            tbReturn.Focusable = true;
            tbCustomer.Focusable = true;
            tbReports.Focusable = true;
            tbEmployee.Focusable = true;

            txtMovieSearch.IsEnabled = true;
            lstMovies.IsEnabled = true;

            btnMovieSearch.IsEnabled = true;
            btnMovieAdd.IsEnabled = true;
            btnEditMovie.IsEnabled = true;

            BindingExpression be = txtMovieTitle.GetBindingExpression(TextBox.TextProperty);
            BindingExpression be2 = txtMovieRating.GetBindingExpression(TextBox.TextProperty);
            BindingExpression be3 = txtMovieGenre.GetBindingExpression(TextBox.TextProperty);
            BindingExpression be4 = txtMovieYear.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
            be2.UpdateSource();
            be3.UpdateSource();
            be4.UpdateSource();

            lstMovies.Items.Refresh();
        }

        private void btnMovieUpdate_Click(object sender, RoutedEventArgs e)
        {
            Movie selectedComboBoxMovie = cboMovieUPC.SelectedItem as Movie;

            if (selectedComboBoxMovie.upc == null)
            {
                MessageBox.Show("Please select the master title or an individual copy from the dropdown list.");
            }
            else if (selectedComboBoxMovie.upc == "Master Title")
            {
                //Assign the edited text fields back to the global movie variable
                selectedMovieInList.title = txtMovieTitle.Text;
                selectedMovieInList.rating = txtMovieRating.Text;
                selectedMovieInList.genre = txtMovieGenre.Text;
                selectedMovieInList.releaseYear = txtMovieYear.Text;
                //selectedMovieInList.price = txtMoviePrice.Text;
                
                //Pass the updated movie and return a boolean
                bool updatedMovie = DB.Movies.Update(selectedMovieInList);

                if (updatedMovie)
                {
                    MessageBox.Show($"{selectedMovieInList.title} was updated.");
                }
                else
                {
                    MessageBox.Show($"{selectedMovieInList.title} could not be updated.");
                }
            }
            else
            {
                //Get the selected individual copy movie object out of the combobox list
                Movie individualCopyUpdate = cboMovieUPC.SelectedItem as Movie;

                bool priarIsActive = individualCopyUpdate.isActive;

                individualCopyUpdate.isActive = chkMovieActive.IsChecked.Value;

                //Pass the updated movie and return a boolean
                bool activeStatusChange = DB.Movies.Delete(individualCopyUpdate);

                //Check if query was successful
                if (activeStatusChange)
                {
                    if (priarIsActive && !individualCopyUpdate.isActive)
                    {
                        //changed from active to deactive
                        MessageBox.Show($"{individualCopyUpdate.title}({individualCopyUpdate.upc}) has been deactivated.");
                    }
                    else if (!priarIsActive && individualCopyUpdate.isActive)
                    {
                        //was inactive but now active
                        MessageBox.Show($"{individualCopyUpdate.title}({individualCopyUpdate.upc}) has been activated.");
                    }
                    else
                    {
                        //no changes made
                        MessageBox.Show($"{individualCopyUpdate.title}({individualCopyUpdate.upc}) no changes.");
                    }

                }
                else
                {
                    MessageBox.Show($"{individualCopyUpdate.title}({individualCopyUpdate.upc}) no changes.");
                }
            }



            //Clear cboMoviesUPC //adds mother as main copy
            Movie mother = cboMovieUPC.Items.GetItemAt(0) as Movie;
            cboMovieUPC.Items.Clear();
            cboMovieUPC.Items.Add(mother);
            cboMovieUPC.SelectedIndex = 0;

            //Disable all the movie update fields
            cboMovieUPC.IsEnabled = false;
            txtMovieTitle.IsEnabled = false;
            txtMovieYear.IsEnabled = false;
            txtMovieRating.IsEnabled = false;
            txtMovieGenre.IsEnabled = false;
            txtMoviePrice.IsEnabled = false;
            chkMovieActive.IsEnabled = false;

            //Clear the checkboxes
            chkMovieRented.IsChecked = false;
            chkMovieActive.IsChecked = false;

            chkMovieRented.Visibility = Visibility.Hidden;
            chkMovieActive.Visibility = Visibility.Hidden;

            //Disable the movie update and cancel buttons
            btnMovieUpdateCancel.IsEnabled = false;
            btnMovieUpdate.IsEnabled = false;

            lstMovies.Items.Refresh();

            //Enable the tabs and movie buttons
            tbRent.Focusable = true;
            tbReturn.Focusable = true;
            tbCustomer.Focusable = true;
            tbReports.Focusable = true;
            tbEmployee.Focusable = true;
           
            txtMovieSearch.IsEnabled = true;
            lstMovies.IsEnabled = true;

            btnMovieSearch.IsEnabled = true;
            btnMovieAdd.IsEnabled = true;
            btnEditMovie.IsEnabled = true;
        }

        private void btnMovieAdd_Click(object sender, RoutedEventArgs e)
        {
            //Create instance of addMovie
            AddMovie addMovie = new AddMovie();

            //Open addMovie window
            addMovie.ShowDialog();

        }

        private void btnMovieAddCancel_Click(object sender, RoutedEventArgs e)
        {
            // //gridAddMovies.Name = "gridAddMovies";
            // //this.RegisterName(gridAddMovies.Name, gridAddMovies);
            // //myStoryboard = new Storyboard();
            DoubleAnimation removeMovieAnimation = new DoubleAnimation();
            removeMovieAnimation.From = 1.0;
            removeMovieAnimation.To = 0.0;
            removeMovieAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
            // //myStoryboard.Children.Add(removeMovieAnimation);
            // //Storyboard.SetTargetName(removeMovieAnimation,gridAddMovies.Name);
            // //Storyboard.SetTargetProperty(removeMovieAnimation, new PropertyPath(Grid.OpacityProperty));

            //removeGridAnimation(gridAddMovies).Wait();

            gridAddMovies.BeginAnimation(Grid.OpacityProperty, removeMovieAnimation);
            // //TimeSpan.FromSeconds(1);
            // //gridAddMovies.BeginAnimation(Grid.VisibilityProperty, removeMovieAnimation);
            // ////VisibilityAnim

            gridAddMovies.Visibility = Visibility.Hidden;

            // //ObjectAnimationUsingKeyFrames animate = new ObjectAnimationUsingKeyFrames();

            // //animate.Duration = new TimeSpan(0, 0, 1);
            // ////animate.RepeatBehavior = RepeatBehavior.Forever;

            // //DiscreteObjectKeyFrame kf1 = new DiscreteObjectKeyFrame(
            // //    Visibility.Hidden,
            // //    new TimeSpan(0, 0, 0, 0, 100));
            // //animate.KeyFrames.Add(kf1);

            // //gridAddMovies.BeginAnimation(Grid.VisibilityProperty, animate);
        }

        private async void removeGridAnimation()
        {
            ////gridAddMovies.Name = "gridAddMovies";
            ////this.RegisterName(gridAddMovies.Name, gridAddMovies);
            ////myStoryboard = new Storyboard();
            ////DoubleAnimation removeMovieAnimation = new DoubleAnimation();
            ////removeMovieAnimation.From = 1.0;
            ////removeMovieAnimation.To = 0.0;
            ////removeMovieAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
            ////myStoryboard.Children.Add(removeMovieAnimation);
            ////Storyboard.SetTargetName(removeMovieAnimation, gridAddMovies.Name);
            ////Storyboard.SetTargetProperty(removeMovieAnimation, new PropertyPath(Grid.OpacityProperty));
            ////myStoryboard.Children.Add()
            ////myStoryboard.Begin(this);
            ////myStoryboard.Completed();
        }

        private void lstMovies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedMovieInList = lstMovies.SelectedItem as Movie;
            if (selectedMovieInList != null)
            {
                btnEditMovie.IsEnabled = true;

                //Clear upc drop down
                cboMovieUPC.Items.Clear();

                //Add Master Title to the top of the combo box to represent MOTHA
                selectedMovieInList.upc = "Master Title";
                cboMovieUPC.Items.Add(selectedMovieInList);
                cboMovieUPC.SelectedIndex = 0;

                //SetBinding fields = total mother information
                txtMovieTitle.Text = selectedMovieInList.title;
                txtMovieYear.Text = selectedMovieInList.releaseYear;
                txtMovieGenre.Text = selectedMovieInList.genre;
                txtMovieRating.Text = selectedMovieInList.rating;
                txtMoviePrice.Text = selectedMovieInList.price;
                chkMovieActive.IsChecked = selectedMovieInList.isActive;
            }
        }

        private void cboMovieUPC_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if edit button is enabled, means we are currently not editing a movie
            if (btnEditMovie.IsEnabled)
            {
                return;
            }

            if(cboMovieUPC.SelectedItem != null)
            {

                //Create an instance of the movie object in the combo box
                Movie comboSelectedMovie = cboMovieUPC.SelectedItem as Movie;

                //Mother movie
                if (cboMovieUPC.SelectedIndex == 0)
                {
                    //Enable the movie title fields
                    txtMovieTitle.IsEnabled = true;
                    txtMovieYear.IsEnabled = true;
                    txtMovieRating.IsEnabled = true;
                    txtMovieGenre.IsEnabled = true;
                    //txtMoviePrice.IsEnabled = true;

                    //Disable the individual copy fields
                    chkMovieActive.IsEnabled = false;
                    chkMovieActive.IsChecked = comboSelectedMovie.isActive;
                    chkMovieRented.Visibility = Visibility.Hidden;
                    chkMovieActive.Visibility = Visibility.Hidden;

                    btnMovieUpdateCancel.IsEnabled = true;
                }
                else
                {
                    chkMovieRented.Visibility = Visibility.Visible;
                    chkMovieActive.Visibility = Visibility.Visible;


                    //Set individual copy fields
                    chkMovieActive.IsChecked = comboSelectedMovie.isActive;
                    chkMovieRented.IsChecked = comboSelectedMovie.isRented;

                    //Check if the individual copy is rented out
                    if (comboSelectedMovie.isRented)
                    {
                        MessageBox.Show($"You cannot edit the copy {comboSelectedMovie.title}({comboSelectedMovie.upc}) while it is rented.");

                        chkMovieRented.Visibility = Visibility.Hidden;
                        cboMovieUPC.SelectedIndex = 0;
                        return;
                    }
                    else
                    {
                        //Enable the individual copy fields
                        chkMovieActive.IsEnabled = true;
                        btnMovieUpdateCancel.IsEnabled = true;
                        btnMovieUpdate.IsEnabled = true;

                        //Hide the movie title fields
                        txtMovieTitle.IsEnabled = false;
                        txtMovieYear.IsEnabled = false;
                        txtMovieRating.IsEnabled = false;
                        txtMovieGenre.IsEnabled = false;
                        //txtMoviePrice.IsEnabled = true;
                    }
                }
            }
        }

        private void txtMovieTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            txtMovieTitle.Text = txtMovieTitle.Text.Trim();
        }

        private void txtMovieRating_LostFocus(object sender, RoutedEventArgs e)
        {
            txtMovieRating.Text = txtMovieRating.Text.Trim();
        }

        private void txtMovieGenre_LostFocus(object sender, RoutedEventArgs e)
        {
            txtMovieGenre.Text = txtMovieGenre.Text.Trim();

        }

        private void txtMovieYear_LostFocus(object sender, RoutedEventArgs e)
        {
            txtMovieYear.Text = txtMovieYear.Text.Trim();

        }
        #endregion
        #region Customer Tab

        public void btnCustomerSearch_Click(object sender, RoutedEventArgs e)
        {
            lstCustomers.Items.Clear();

            List<Customer> customerList = DB.Customers.GetCustomersByName(txtCustomerSearch.Text);

            if(customerList == null)
            {
                MessageBox.Show("Could not find customer.");
                return;
            }

            foreach(Customer i in customerList)
            {
                if ((!chkCustDeactiveSearch.IsChecked.Value && i.isActive) || (bool)chkCustDeactiveSearch.IsChecked)
                {
                    lstCustomers.Items.Add(i);
                }                
            }
            
            if(lstCustomers.Items.Count > 0)
            {
                lstCustomers.SelectedIndex = 0;
            }
        }

        private void lstCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lstCustomers.Items.Count == 0)
            {
                return;
            }

            lstActiveRentals.Items.Clear();
            btnCustEdit.IsEnabled = true;
            btnCustUse.IsEnabled = true;

            custToUpdate = (Customer)lstCustomers.SelectedItem;
            if (custToUpdate != null)
            {

                txtCustFirstName.Text = custToUpdate.firstName;
                txtCustLastName.Text = custToUpdate.lastName;
                txtCustPhone.Text = custToUpdate.phoneNumber;
                txtCustEmail.Text = custToUpdate.email;
                txtPoints.Text = custToUpdate.points.ToString();
                custActiveCheck.IsChecked = custToUpdate.isActive;
                txtCustLateFee.Text = String.Format("{0:C}", DB.Customers.GetLateFee(custToUpdate));

                List<String[]> custRent = DB.Customers.GetActiveRentalsByCust(custToUpdate.id.ToString());

                if (custRent != null)
                {

                    foreach (String[] cr in custRent)
                    {
                        lstActiveRentals.Items.Add(new custRentals(cr[0], cr[1], cr[2]));
                    }
                }
            }

        }

        private int _noOfErrorsOnCustomerTab = 0;
        private void CustTabValidation_Error(object sender, ValidationErrorEventArgs e)
        {
                if (e.Action == ValidationErrorEventAction.Added)
                    _noOfErrorsOnCustomerTab++;
                else
                    _noOfErrorsOnCustomerTab--;
        }

        private void CustTabAddCustomer_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _noOfErrorsOnCustomerTab == 0;
            e.Handled = true;
        }

        /*private void lstCustomers_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            lstActiveRentals.Items.Clear();
            btnCustEdit.IsEnabled = true;
            btnCustUse.IsEnabled = true;

            custToUpdate = (Customer)lstCustomers.SelectedItem;
            if (custToUpdate != null)
            {

                txtCustFirstName.Text = custToUpdate.firstName;
                txtCustLastName.Text = custToUpdate.lastName;
                txtCustPhone.Text = custToUpdate.phoneNumber;
                txtCustEmail.Text = custToUpdate.email;
                txtPoints.Text = custToUpdate.points.ToString();
                custActiveCheck.IsChecked = custToUpdate.isActive;

                List<String[]> custRent = DB.Customers.GetActiveRentalsByCust(custToUpdate.id.ToString());

                foreach (String[] cr in custRent)
                {
                    lstActiveRentals.Items.Add(new custRentals(cr[0], cr[1], cr[2]));
                }
            }
        }
        */
        private void btnCustEdit_Click(object sender, RoutedEventArgs e)
        {
            if(custToUpdate == null)
            {
                return;
            }
            //Disables customer list, search add
            tbRent.Focusable = false;
            tbReturn.Focusable = false;
            tbMovies.Focusable = false;
            tbReports.Focusable = false;
            tbEmployee.Focusable = false;

            //Search  and list disbaled
            txtCustomerSearch.IsEnabled = false;
            btnCustomerSearch.IsEnabled = false;
            chkCustDeactiveSearch.IsEnabled = false;
            lstCustomers.IsEnabled = false;
            btnCustomerAdd.IsEnabled = false;


            //Sets text boxes editable
            lstActiveRentals.IsEnabled = true;
            txtCustFirstName.IsEnabled = true;
            txtCustLastName.IsEnabled = true;
            txtCustPhone.IsEnabled = true;
            txtCustEmail.IsEnabled = true;
            custActiveCheck.IsEnabled = true;

            //only managers can change customer points
            if (currentUser.isManager)
            {
                txtPoints.IsEnabled = true;
            }
            
            //Enable update button 
            btnCustUpdate.IsEnabled = true;

            //enable cancel button
            btnCustCancel.IsEnabled = true;
            
            //disable edit button
            btnCustEdit.IsEnabled = false;

            //disbale use button
            btnCustUse.IsEnabled = false;

            //custGrid.DataContext = _customer;
            BindingExpression be = txtCustFirstName.GetBindingExpression(TextBox.TextProperty);
            BindingExpression be2 = txtCustLastName.GetBindingExpression(TextBox.TextProperty);
            BindingExpression be3 = txtCustPhone.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
            be2.UpdateSource();
            be3.UpdateSource();

        }

        private void btnCustUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (custToUpdate == null)
            {
                return;
            }

            //Verify change in phone number is not already a phone number in system
            //If in db state cannot change phone number to one in use
            if (custToUpdate.phoneNumber != txtCustPhone.Text && DB.Customers.GetByNumber(txtCustPhone.Text) != null)
            {
                MessageBox.Show("Phone number is used by another customer, cannot update.", "Update Error");
                return;
            }

            custToUpdate.firstName = txtCustFirstName.Text;
            custToUpdate.lastName = txtCustLastName.Text;
            custToUpdate.phoneNumber = txtCustPhone.Text;
            custToUpdate.email = txtCustEmail.Text;
            custToUpdate.points = Int32.Parse(txtPoints.Text);
            custToUpdate.isActive = custActiveCheck.IsChecked.Value;

            //Send Update to db
            if (DB.Customers.Update(custToUpdate))
            {
                MessageBox.Show("Customer information updated.");
            }
            else
            {
                //Error updating db
                MessageBox.Show("Error updating customer information.");
            };

            //if phone number is same as current customer selected on rent tab, update that information
            if (currentCustomer != null && currentCustomer.phoneNumber == custToUpdate.phoneNumber)
            {
                currentCustomer = custToUpdate;
                txtCustomerFirstName.Text = currentCustomer.firstName;
                txtCustomerLastName.Text = currentCustomer.lastName;
                txtCustomerPhoneNumber.Text = currentCustomer.phoneNumber;
                txtCustomerEmail.Text = currentCustomer.email;
            }

            //Update inforamtion in customer list to left
            lstCustomers.Items.Refresh();



            //Disable text fields
            lstActiveRentals.IsEnabled = false;
            txtCustFirstName.IsEnabled = false;
            txtCustLastName.IsEnabled = false;
            txtCustPhone.IsEnabled = false;
            txtCustEmail.IsEnabled = false;
            txtPoints.IsEnabled = false;
            custActiveCheck.IsEnabled = false;

            //disbale update button and cancel button
            btnCustUpdate.IsEnabled = false;
            btnCustCancel.IsEnabled = false;

            //Endable edit and use buttons
            btnCustEdit.IsEnabled = true;
            btnCustUse.IsEnabled = true;

            //enable all tabs and buttons
            tbRent.Focusable = true;
            tbReturn.Focusable = true;
            tbCustomer.Focusable = true;
            tbMovies.Focusable = true;
            tbReports.Focusable = true;
            tbEmployee.Focusable = true;


            txtCustomerSearch.IsEnabled = true;
            btnCustomerSearch.IsEnabled = true;
            chkCustDeactiveSearch.IsEnabled = true;
            btnCustomerAdd.IsEnabled = true;
            lstCustomers.IsEnabled = true;


        }

        private void btnCustCancel_Click(object sender, RoutedEventArgs e)
        {
            lstCustomers.Items.Refresh();

            //Disable text fields
            lstActiveRentals.IsEnabled = false;
            txtCustFirstName.IsEnabled = false;
            txtCustLastName.IsEnabled = false;
            txtCustPhone.IsEnabled = false;
            txtCustEmail.IsEnabled = false;
            txtPoints.IsEnabled = false;
            custActiveCheck.IsEnabled = false;

            //disbale update button and cancel button
            btnCustUpdate.IsEnabled = false;
            btnCustCancel.IsEnabled = false;
           

            //Endable edit and use buttons
            btnCustEdit.IsEnabled = true;
            btnCustUse.IsEnabled = true;

            //enable all tabs and buttons
            tbRent.Focusable = true;
            tbReturn.Focusable = true;
            tbCustomer.Focusable = true;
            tbMovies.Focusable = true;
            tbReports.Focusable = true;
            tbEmployee.Focusable = true;


            txtCustomerSearch.IsEnabled = true;
            btnCustomerSearch.IsEnabled = true;
            chkCustDeactiveSearch.IsEnabled = true;
            btnCustomerAdd.IsEnabled = true;
            lstCustomers.IsEnabled = true;

            txtCustFirstName.Text = custToUpdate.firstName;
            txtCustLastName.Text = custToUpdate.lastName;
            txtCustPhone.Text = custToUpdate.phoneNumber;
            txtCustEmail.Text = custToUpdate.email;

            BindingExpression be = txtCustFirstName.GetBindingExpression(TextBox.TextProperty);
            BindingExpression be2 = txtCustLastName.GetBindingExpression(TextBox.TextProperty);
            BindingExpression be3 = txtCustPhone.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
            be2.UpdateSource();
            be3.UpdateSource();
        }

        private void btnCustUse_Click(object sender, RoutedEventArgs e)
        {
            if (custToUpdate == null)
            {
                return;
            }

            txtCustomerPhoneNumberSearch.Text = custToUpdate.phoneNumber;
            btnCustomerLookup_Click(sender, e);

            tabControl.SelectedIndex = 0;            
        }

        private class custRentals
        {
            public String movieTitle { get; }
            public String rentDate { get; }
            public String dueDate { get; }




            public custRentals(String movieTitle, String rentDate, String dueDate)
            {
                this.movieTitle = movieTitle;
                this.rentDate = rentDate;
                this.dueDate = dueDate;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void txtCustFirstName_LostFocus(object sender, RoutedEventArgs e)
        {
            txtCustFirstName.Text = txtCustFirstName.Text.Trim();
        }

        private void txtCustLastName_LostFocus(object sender, RoutedEventArgs e)
        {
            txtCustLastName.Text = txtCustLastName.Text.Trim();
        }

        private void txtCustPhone_LostFocus(object sender, RoutedEventArgs e)
        {
            txtCustPhone.Text = txtCustPhone.Text.Trim();
        }

        private void txtCustEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            txtCustEmail.Text = txtCustEmail.Text.Trim();
        }

        private void txtPoints_LostFocus(object sender, RoutedEventArgs e)
        {
            txtPoints.Text = txtPoints.Text.Trim();
        }
        #endregion
        #region Employee Tab
        private void btnEmployeeSearch_Click(object sender, RoutedEventArgs e)
        {
            lstEmployees.Items.Clear();

            List<Employee> employeeList = DB.Employees.GetEmployeesByName(txtEmployeeSearch.Text);
            if(employeeList == null)
            {
                MessageBox.Show("Could not find employee.");
            }

            foreach (Employee i in employeeList)
            {
                if ((!chkDeactiveEmployeeSearch.IsChecked.Value && i.isActive) || (bool)chkDeactiveEmployeeSearch.IsChecked)
                {
                    lstEmployees.Items.Add(i);
                }
            }
        }

        private void btnEmployeeAdd_Click(object sender, RoutedEventArgs e)
        {
            Window1 newUser = new Window1();

            newUser.ShowDialog();
            newUser.Close();
        }

        private void lstEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstEmployees.Items.Count == 0)
            {
                return;
            }

            btnEmpEdit.IsEnabled = true;

            empToUpdate = (Employee)lstEmployees.SelectedItem;
            if (empToUpdate != null)
            {

                txtEmpUserName.Text = empToUpdate.username;
                txtEmpFName.Text = empToUpdate.firstName;
                txtEmpLName.Text = empToUpdate.lastName;
                txtEmpEmail.Text = empToUpdate.email;
                chkEmpIsManager.IsChecked = empToUpdate.isManager;
                chkEmpIsActive.IsChecked = empToUpdate.isActive;

            }

        }

        private void btnEmpEdit_Click(object sender, RoutedEventArgs e)
        {
            if (empToUpdate == null)
            {
                return;
            }
            //Disables customer list, search add
            tbRent.Focusable = false;
            tbCustomer.Focusable = false;
            tbReturn.Focusable = false;
            tbMovies.Focusable = false;
            tbReports.Focusable = false;
            tbEmployee.Focusable = false;

            //Search  and list disbaled
            txtEmployeeSearch.IsEnabled = false;
            btnEmployeeSearch.IsEnabled = false;
            chkDeactiveEmployeeSearch.IsEnabled = false;
            lstEmployees.IsEnabled = false;
            btnEmployeeAdd.IsEnabled = false;


            //Sets text boxes editable
            //txtEmpUserName.IsEnabled = true;
            txtEmpFName.IsEnabled = true;
            txtEmpLName.IsEnabled = true;
            txtEmpEmail.IsEnabled = true;

            if (currentUser.username != empToUpdate.username)
            {
                chkEmpIsManager.IsEnabled = true;
                chkEmpIsActive.IsEnabled = true;
            }


            //Enable update button 
            btnEmpUpdate.IsEnabled = true;

            //enable cancel button
            btnEmpCancel.IsEnabled = true;

            //disable edit button
            btnEmpEdit.IsEnabled = false;

            BindingExpression be = txtEmpUserName.GetBindingExpression(TextBox.TextProperty);
            BindingExpression be2 = txtEmpFName.GetBindingExpression(TextBox.TextProperty);
            BindingExpression be3 = txtEmpLName.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
            be2.UpdateSource();
            be3.UpdateSource();

        }
        private int _noOfErrorsOnEmployeeTab = 0;
        private void EmployeeTabValidation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                _noOfErrorsOnEmployeeTab++;
            else
                _noOfErrorsOnEmployeeTab--;
        }

        private void EmployeeTabEditEmployee_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _noOfErrorsOnEmployeeTab == 0;
            e.Handled = true;
        }

        private void btnEmpUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (empToUpdate == null)
            {
                return;
            }

            //check with Curtis 
            //Verify change in phone number is not already a phone number in system
            //If in db state cannot change phone number to one in use
            //if (custToUpdate.phoneNumber != txtCustPhone.Text && DB.Customers.GetByNumber(txtCustPhone.Text) != null)
                //{
                //    MessageBox.Show("Phone number is used by another customer, cannot update.", "Update Error");
                //    return;
                //}

            empToUpdate.username = txtEmpUserName.Text;
            empToUpdate.firstName = txtEmpFName.Text;
            empToUpdate.lastName = txtEmpLName.Text;
            empToUpdate.email = txtEmpEmail.Text;
            empToUpdate.isActive = chkEmpIsActive.IsChecked.Value;
            empToUpdate.isManager = chkEmpIsManager.IsChecked.Value;

            //Send Update to db
            if (DB.Employees.Update(empToUpdate))
            {
                MessageBox.Show("Employee information updated.");
            }
            else
            {
                //Error updating db
                MessageBox.Show("Error updating employee information.");
            };


            //Update inforamtion in customer list to left
            lstEmployees.Items.Refresh();

            //Disable text fields
            txtEmpUserName.IsEnabled = false;
            txtEmpFName.IsEnabled = false;
            txtEmpLName.IsEnabled = false;
            txtEmpEmail.IsEnabled = false;
            chkEmpIsActive.IsEnabled = false;
            chkEmpIsManager.IsEnabled = false;

            //disbale update button and cancel button
            btnEmpUpdate.IsEnabled = false;
            btnEmpCancel.IsEnabled = false;

            //Endable edit and use buttons
            btnEmpEdit.IsEnabled = true;

            //enable all tabs and buttons
            tbRent.Focusable = true;
            tbReturn.Focusable = true;
            tbCustomer.Focusable = true;
            tbMovies.Focusable = true;
            tbReports.Focusable = true;
            tbEmployee.Focusable = true;


            txtEmployeeSearch.IsEnabled = true;
            btnEmployeeSearch.IsEnabled = true;
            chkDeactiveEmployeeSearch.IsEnabled = true;
            btnEmployeeAdd.IsEnabled = true;
            lstEmployees.IsEnabled = true;


        }

        private void btnEmpCancel_Click(object sender, RoutedEventArgs e)
        {
            lstEmployees.Items.Refresh();

            //Disable text fields
            txtEmpUserName.IsEnabled = false;
            txtEmpFName.IsEnabled = false;
            txtEmpLName.IsEnabled = false;
            txtEmpEmail.IsEnabled = false;
            chkEmpIsActive.IsEnabled = false;
            chkEmpIsManager.IsEnabled = false;

            //disbale update button and cancel button
            btnEmpUpdate.IsEnabled = false;
            btnEmpCancel.IsEnabled = false;


            //Endable edit and use buttons
            btnEmpEdit.IsEnabled = true;


            //enable all tabs and buttons
            tbRent.Focusable = true;
            tbReturn.Focusable = true;
            tbCustomer.Focusable = true;
            tbMovies.Focusable = true;
            tbReports.Focusable = true;
            tbEmployee.Focusable = true;


            txtEmployeeSearch.IsEnabled = true;
            btnEmployeeSearch.IsEnabled = true;
            chkDeactiveEmployeeSearch.IsEnabled = true;
            btnEmployeeAdd.IsEnabled = true;
            lstEmployees.IsEnabled = true;

            txtEmpUserName.Text = empToUpdate.username;
            txtEmpFName.Text = empToUpdate.firstName;
            txtEmpLName.Text = empToUpdate.lastName;
            txtEmpEmail.Text = empToUpdate.email;

            BindingExpression be = txtEmpUserName.GetBindingExpression(TextBox.TextProperty);
            BindingExpression be2 = txtEmpFName.GetBindingExpression(TextBox.TextProperty);
            BindingExpression be3 = txtEmpLName.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
            be2.UpdateSource();
            be3.UpdateSource();
        }

        private void txtEmpUserName_LostFocus(object sender, RoutedEventArgs e)
        {
            txtEmpUserName.Text = txtEmpUserName.Text.Trim();
        }

        private void txtEmpFName_LostFocus(object sender, RoutedEventArgs e)
        {
            txtEmpFName.Text = txtEmpFName.Text.Trim();
        }

        private void txtEmpLName_LostFocus(object sender, RoutedEventArgs e)
        {
            txtEmpLName.Text = txtEmpLName.Text.Trim();
        }

        private void txtEmpEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            txtEmpEmail.Text = txtEmpEmail.Text.Trim();
        }


        #endregion
        #region Reports Tab
        private void btnRunReport_Click(object sender, RoutedEventArgs e)
        {
            //getting selected report
            if (cboReports.SelectedItem == null)
            {
                return;
            }
            string chosenReport = cboReports.SelectedValue.ToString();

            switch (chosenReport)
            {
                //Late Rentals report
                case "Late Rentals":

                    lstReportReturn.Items.Clear();

                    //creating column

                    GridView lrGridView = new GridView();
                    lrGridView.AllowsColumnReorder = true;

                    //column 1

                    GridViewColumn gvc1 = new GridViewColumn();
                    gvc1.DisplayMemberBinding = new Binding("customerName");
                    gvc1.Header = "Customer";
                    gvc1.Width = lstReportReturn.Width / 5;
                    lrGridView.Columns.Add(gvc1);

                    //column 2

                    GridViewColumn gvc2 = new GridViewColumn();
                    gvc2.DisplayMemberBinding = new Binding("customerPhone");
                    gvc2.Header = "Phone";
                    gvc2.Width = lstReportReturn.Width / 5;
                    lrGridView.Columns.Add(gvc2);

                    //column 3

                    GridViewColumn gvc3 = new GridViewColumn();
                    gvc3.DisplayMemberBinding = new Binding("movieTitle");
                    gvc3.Header = "Movie";
                    gvc3.Width = lstReportReturn.Width / 5;
                    lrGridView.Columns.Add(gvc3);

                    //column 4

                    GridViewColumn gvc4 = new GridViewColumn();
                    gvc4.DisplayMemberBinding = new Binding("dueDate");
                    gvc4.Header = "Due Date";
                    gvc4.Width = lstReportReturn.Width / 5;
                    lrGridView.Columns.Add(gvc4);

                    //column 5

                    GridViewColumn gvc5 = new GridViewColumn();
                    gvc5.DisplayMemberBinding = new Binding("daysLate");
                    gvc5.Header = "Days Late";
                    gvc5.Width = lstReportReturn.Width / 5;
                    lrGridView.Columns.Add(gvc5);

                    lstReportReturn.View = lrGridView;

                    //getting list of late rentals from db
                    List<String[]> lateList = DB.Rentals.Late();


                    //adding list of late rentals to listview, creating LateReturn object to align with column headers
                    if (lateList != null)
                    {
                        foreach (String[] lateItem in lateList)
                        {
                            lstReportReturn.Items.Add(new LateReturn(lateItem[0], lateItem[1], lateItem[2], lateItem[3], lateItem[4]));
                        }
                    }
                    break;

                //Best Customers report
                case "Best Customers":

                    //Getting List of best customers from db

                    List<String[]> bestCust = DB.Customers.bestCust();



                    lstReportReturn.Items.Clear();

                    //Creating gridview with appropriate columns

                    GridView bcGridView = new GridView();
                    bcGridView.AllowsColumnReorder = true;

                    //Column 1
                    GridViewColumn bcgvc1 = new GridViewColumn();
                    bcgvc1.DisplayMemberBinding = new Binding("customerName");
                    bcgvc1.Header = "Customer";
                    bcgvc1.Width = lstReportReturn.Width / 3;
                    bcGridView.Columns.Add(bcgvc1);

                    //Column 2
                    GridViewColumn bcgvc2 = new GridViewColumn();
                    bcgvc2.DisplayMemberBinding = new Binding("customerPhone");
                    bcgvc2.Header = "Phone";
                    bcgvc2.Width = lstReportReturn.Width / 3;
                    bcGridView.Columns.Add(bcgvc2);

                    //Column 3
                    GridViewColumn bcgvc3 = new GridViewColumn();
                    bcgvc3.DisplayMemberBinding = new Binding("customerPoints");
                    bcgvc3.Header = "Points";
                    bcgvc3.Width = lstReportReturn.Width / 3;
                    bcGridView.Columns.Add(bcgvc3);


                    lstReportReturn.View = bcGridView;

                    //adding list items to listview, creating bestCustomer object to align with column binding

                    if (bestCust != null)
                    {

                        foreach (String[] bc in bestCust)
                        {
                            lstReportReturn.Items.Add(new bestCustomer(bc[0], bc[1], bc[2]));
                        }
                    }
                    break;

                case "Best Movies":

                    //Getting List of best movies from db

                    List<String[]> bestMovies = DB.Movies.bestMovie();

                    lstReportReturn.Items.Clear();

                    //Creating gridview with appropriate columns

                    GridView bmGridView = new GridView();
                    bmGridView.AllowsColumnReorder = true;

                    //Column 1
                    GridViewColumn bmgvc1 = new GridViewColumn();
                    bmgvc1.DisplayMemberBinding = new Binding("movieName");
                    bmgvc1.Header = "Movie Title";
                    bmgvc1.Width = lstReportReturn.Width / 2;
                    bmGridView.Columns.Add(bmgvc1);

                    //Column 2
                    GridViewColumn bmgvc2 = new GridViewColumn();
                    bmgvc2.DisplayMemberBinding = new Binding("timesRented");
                    bmgvc2.Header = "Times Rented";
                    bmgvc2.Width = lstReportReturn.Width / 2;
                    bmGridView.Columns.Add(bmgvc2);


                    lstReportReturn.View = bmGridView;


                    //adding list items to listview, creating bestCustomer object to align with column binding
                    if (bestMovies != null)
                    {
                        foreach (String[] bm in bestMovies)
                        {
                            lstReportReturn.Items.Add(new bestMovie(bm[0], bm[1]));
                        }
                    }
                    break;

                case "Active Rentals":

                    //Getting List of active rentals from db

                    List<String[]> activeRentalsList = DB.Rentals.Active();

                    lstReportReturn.Items.Clear();

                    //Creating gridview with appropriate columns

                    GridView arGridView = new GridView();
                    arGridView.AllowsColumnReorder = true;

                    //Column 1
                    GridViewColumn argvc1 = new GridViewColumn();
                    argvc1.DisplayMemberBinding = new Binding("title");
                    argvc1.Header = "Movie Title";
                    argvc1.Width = lstReportReturn.Width / 5;
                    arGridView.Columns.Add(argvc1);

                    //Column 2
                    GridViewColumn argvc2 = new GridViewColumn();
                    argvc2.DisplayMemberBinding = new Binding("upc");
                    argvc2.Header = "UPC";
                    argvc2.Width = lstReportReturn.Width / 5;
                    arGridView.Columns.Add(argvc2);

                    //Column 3
                    GridViewColumn argvc3 = new GridViewColumn();
                    argvc3.DisplayMemberBinding = new Binding("dueDate");
                    argvc3.Header = "Due Date";
                    argvc3.Width = lstReportReturn.Width / 5;
                    arGridView.Columns.Add(argvc3);

                    //Column 4
                    GridViewColumn argvc4 = new GridViewColumn();
                    argvc4.DisplayMemberBinding = new Binding("customerName");
                    argvc4.Header = "Customer Name";
                    argvc4.Width = lstReportReturn.Width / 5;
                    arGridView.Columns.Add(argvc4);
                    
                    lstReportReturn.View = arGridView;

                    //Column 4
                    GridViewColumn argvc5 = new GridViewColumn();
                    argvc5.DisplayMemberBinding = new Binding("number");
                    argvc5.Header = "Phone Number";
                    argvc5.Width = lstReportReturn.Width / 5;
                    arGridView.Columns.Add(argvc5);

                    lstReportReturn.View = arGridView;


                    //adding list items to listview, creating bestCustomer object to align with column binding
                    if (activeRentalsList != null)
                    {
                        foreach (String[] ar in activeRentalsList)
                        {
                            lstReportReturn.Items.Add(new activeRental(ar[0], ar[1], ar[2], ar[3], ar[4]));
                        }
                    }
                    break;

            }

        }

        //Class for late returns report
        private class LateReturn
        {
            public String customerName { get; }
            public String customerPhone { get; }
            public String movieTitle { get; }
            public String dueDate { get; }
            public String daysLate { get; }


            public LateReturn(String customerName, String customerPhone, String movieTitle, String dueDate, String daysLate)
            {
                this.customerName = customerName;
                this.customerPhone = customerPhone;
                this.movieTitle = movieTitle;
                this.dueDate = dueDate;
                this.daysLate = daysLate;
            }
        }

        //Class for best customer reports
        private class bestCustomer
        {
            public String customerName { get; }
            public String customerPhone { get; }
            public String customerPoints { get; }


            public bestCustomer(String customerName, String customerPhone, String customerPoints)
            {
                this.customerName = customerName;
                this.customerPhone = customerPhone;
                this.customerPoints = customerPoints;
            }
        }

        //Class for best movie report
        private class bestMovie
        {
            public String movieName { get; }
            public String timesRented { get; }

            public bestMovie(String movieName, String timesRented)
            {
                this.movieName = movieName;
                this.timesRented = timesRented;
            }
        }

        //Class for Active Rentals Report

        private class activeRental
        {
            public String title { get; }
            public String upc { get; }
            public String dueDate { get; }
            public String customerName { get; }
            public String number { get; }

            public activeRental(String title, String upc, String dueDate, String customerName, String number)
            {
                this.title = title;
                this.upc = upc;
                this.dueDate = dueDate;
                this.customerName = customerName;
                this.number = number;
            }
        }


        #endregion


        private void updateTotals()
        {
            total = rentalTotal + lateFeeTotal;
            txtRentalTotal.Text = String.Format("{0:C}", rentalTotal);
            txtLateFeeTotal.Text = String.Format("{0:C}", lateFeeTotal);
            txtTotal.Text = String.Format("{0:C}", total);
        }

        private void clearTotals()
        {
            total = 0.00;
            lateFeeTotal = 0.00;
            rentalTotal = 0.00;

            txtRentalTotal.Text = String.Format("{0:C}", rentalTotal);
            txtLateFeeTotal.Text = String.Format("{0:C}", lateFeeTotal);
            txtTotal.Text = String.Format("{0:C}", total);
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            Help help = new Help();
            help.ShowDialog();
        }

        private void btnExportReportsToPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int numOfColumns;
                string folderPath;
                if (lstReportReturn.HasItems)
                {
                    //Exporting to PDF
                    folderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\";
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    if (cboReports.SelectedIndex == 0)
                    {
                        numOfColumns = 5;

                        using (FileStream stream = new FileStream($"{folderPath}LateRentals{DateTime.Now.ToString("yyyyMMddHHmmss")}.pdf", FileMode.Create))
                        {
                            Document pdfDoc = new Document(PageSize.A2, 10f, 10f, 10f, 0f);
                            var pdfWriter = PdfWriter.GetInstance(pdfDoc, stream);

                            pdfDoc.Open();

                            PdfContentByte cb = new PdfContentByte(pdfWriter);

                            PdfPTable pdfTable = new PdfPTable(numOfColumns);
                            pdfTable.DefaultCell.Padding = 20;
                            pdfTable.WidthPercentage = 50;
                            pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
                            pdfTable.DefaultCell.BorderWidth = 1;

                            PdfPCell cell = new PdfPCell(new Phrase("Customer"));
                            pdfTable.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Phone"));
                            pdfTable.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Movie"));
                            pdfTable.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Due Date"));
                            pdfTable.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Days Late"));
                            pdfTable.AddCell(cell);

                            foreach (LateReturn item in lstReportReturn.Items)
                            {
                                pdfTable.AddCell(item.customerName);
                                pdfTable.AddCell(item.customerPhone);
                                pdfTable.AddCell(item.movieTitle);
                                pdfTable.AddCell(item.dueDate);
                                pdfTable.AddCell(item.daysLate);
                            }

                            pdfDoc.Add(pdfTable);
                            pdfDoc.Close();
                            stream.Close();

                        }
                    }
                    else if (cboReports.SelectedIndex == 1)
                    {
                        numOfColumns = 3;

                        using (FileStream stream = new FileStream($"{folderPath}BestCustomer{DateTime.Now.ToString("yyyyMMddHHmmss")}.pdf", FileMode.Create))
                        {
                            Document pdfDoc = new Document(PageSize.A2, 10f, 10f, 10f, 0f);
                            var pdfWriter = PdfWriter.GetInstance(pdfDoc, stream);

                            pdfDoc.Open();

                            PdfContentByte cb = new PdfContentByte(pdfWriter);

                            PdfPTable pdfTable = new PdfPTable(numOfColumns);
                            pdfTable.DefaultCell.Padding = 20;
                            pdfTable.WidthPercentage = 50;
                            pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
                            pdfTable.DefaultCell.BorderWidth = 1;

                            PdfPCell cell = new PdfPCell(new Phrase("Customer"));
                            pdfTable.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Phone"));
                            pdfTable.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Points"));
                            pdfTable.AddCell(cell);

                            foreach (bestCustomer item in lstReportReturn.Items)
                            {
                                pdfTable.AddCell(item.customerName);
                                pdfTable.AddCell(item.customerPhone);
                                pdfTable.AddCell(item.customerPoints);
                            }

                            pdfDoc.Add(pdfTable);
                            pdfDoc.Close();
                            stream.Close();

                        }
                    }
                    else if (cboReports.SelectedIndex == 2)
                    {
                        numOfColumns = 2;


                        using (FileStream stream = new FileStream($"{folderPath}BestMovies{DateTime.Now.ToString("yyyyMMddHHmmss")}.pdf", FileMode.Create))
                        {
                            Document pdfDoc = new Document(PageSize.A2, 10f, 10f, 10f, 0f);
                            var pdfWriter = PdfWriter.GetInstance(pdfDoc, stream);

                            pdfDoc.Open();

                            PdfContentByte cb = new PdfContentByte(pdfWriter);

                            PdfPTable pdfTable = new PdfPTable(numOfColumns);
                            pdfTable.DefaultCell.Padding = 20;
                            pdfTable.WidthPercentage = 50;
                            pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
                            pdfTable.DefaultCell.BorderWidth = 1;

                            PdfPCell cell = new PdfPCell(new Phrase("Movie"));
                            pdfTable.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Times Rented"));
                            pdfTable.AddCell(cell);

                            foreach (bestMovie item in lstReportReturn.Items)
                            {
                                pdfTable.AddCell(item.movieName);
                                pdfTable.AddCell(item.timesRented);
                            }

                            pdfDoc.Add(pdfTable);
                            pdfDoc.Close();
                            stream.Close();

                        }
                    }
                    else if (cboReports.SelectedIndex == 3)
                    {
                        numOfColumns = 5;

                        using (FileStream stream = new FileStream($"{folderPath}ActiveRentals{DateTime.Now.ToString("yyyyMMddHHmmss")}.pdf", FileMode.Create))
                        {
                            Document pdfDoc = new Document(PageSize.A2, 10f, 10f, 10f, 0f);
                            var pdfWriter = PdfWriter.GetInstance(pdfDoc, stream);

                            pdfDoc.Open();

                            PdfContentByte cb = new PdfContentByte(pdfWriter);

                            PdfPTable pdfTable = new PdfPTable(numOfColumns);
                            pdfTable.DefaultCell.Padding = 20;
                            pdfTable.WidthPercentage = 50;
                            pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
                            pdfTable.DefaultCell.BorderWidth = 1;

                            PdfPCell cell = new PdfPCell(new Phrase("Movie"));
                            pdfTable.AddCell(cell);
                            cell = new PdfPCell(new Phrase("UPC"));
                            pdfTable.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Due Date"));
                            pdfTable.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Customer Name"));
                            pdfTable.AddCell(cell);
                            cell = new PdfPCell(new Phrase("Phone Number"));
                            pdfTable.AddCell(cell);

                            foreach (activeRental item in lstReportReturn.Items)
                            {
                                pdfTable.AddCell(item.title);
                                //pdfTable.AddCell(item.upc);

                                Barcode39 barcode39 = new Barcode39();
                                barcode39.StartStopText = true;
                                barcode39.Code = item.upc.ToString();
                                iTextSharp.text.Image image39 = barcode39.CreateImageWithBarcode(cb, null, null);
                                pdfTable.AddCell(image39);


                                pdfTable.AddCell(item.dueDate);
                                pdfTable.AddCell(item.customerName);
                                pdfTable.AddCell(item.number);



                            }


                            pdfDoc.Add(pdfTable);
                            pdfDoc.Close();
                            stream.Close();

                        }
                    }
                }

                else
                {
                    MessageBox.Show("You must run a report first.");
                    return;
                }

                MessageBox.Show($"Report Created Successfully", "Report Created");
            }catch(Exception exp)
            {
                MessageBox.Show("Error Exporting Report!");
            }
        }
    }

}

