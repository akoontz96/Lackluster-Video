using System;
using System.Collections.Generic;
using System.IO;
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
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Lackluster
{
    /// <summary>
    /// Interaction logic for AddMovie.xaml
    /// </summary>
    public partial class AddMovie : Window
    {
        private Movie _movie = new Movie();
        public AddMovie()
        {
            InitializeComponent();
            movieGrid.DataContext = _movie;
            refreshList();
        }

        private int _noOfErrorsOnAddMovie = 0;
        private void AddMovieValidation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                _noOfErrorsOnAddMovie++;
            else
                _noOfErrorsOnAddMovie--;
        }

        private void AddMovie_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _noOfErrorsOnAddMovie == 0;
            e.Handled = true;
        }

        private void refreshList()
        {

            cboMovieTitleList.Items.Clear();
            
            //Create a new blank movie object for the user to select
            Movie createNewMovie = new Movie();

            //Set the title as Create New Movie to show in the list
            createNewMovie.title = "Create New Movie";

            //Add blank movie object to the combobox
            cboMovieTitleList.Items.Add(createNewMovie);

            //Get a list of all movies
            List<Movie> movieList = DB.Movies.GetByTitleOrUPC("");

            if (movieList != null)
            {

                //Put the movies in the combo Box
                foreach (Movie i in movieList)
                {
                    cboMovieTitleList.Items.Add(i);
                }
            }

            //setting "Create New Movie" as default selection
            cboMovieTitleList.SelectedIndex = 0;
               
        }

        //private Movie selectedMovie = new Movie();

        private void cboMovieTitleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            

            Movie selectedMovie = cboMovieTitleList.SelectedItem as Movie;

            if (selectedMovie != null)
            {
                if (selectedMovie.title == "Create New Movie")
                {
                    //Clear the textboxes
                    txtAddMovieTitle.Text = "";
                    txtAddMovieRating.Text = "";
                    txtAddMovieGenre.Text = "";
                    txtAddMovieYear.Text = "";
                    txtAddMoviePrice.Text = "";

                    //Unlock textboxes for user entry
                    txtAddMovieTitle.IsEnabled = true;
                    txtAddMovieRating.IsEnabled = true;
                    txtAddMovieGenre.IsEnabled = true;
                    txtAddMovieYear.IsEnabled = true;
                    txtAddMoviePrice.Text = "3.00";
                }
                else
                {
                    //Disable textboxes for user entry
                    txtAddMovieTitle.IsEnabled = false;
                    txtAddMovieRating.IsEnabled = false;
                    txtAddMovieGenre.IsEnabled = false;
                    txtAddMovieYear.IsEnabled = false;

                    //Fill the textboxes with the selected movie info
                    txtAddMovieTitle.Text = selectedMovie.title;
                    txtAddMovieRating.Text = selectedMovie.rating;
                    txtAddMovieGenre.Text = selectedMovie.genre;
                    txtAddMovieYear.Text = selectedMovie.releaseYear;
                    txtAddMoviePrice.Text = selectedMovie.price;
                }

                BindingExpression be = txtAddMovieTitle.GetBindingExpression(TextBox.TextProperty);
                BindingExpression be2 = txtAddMovieRating.GetBindingExpression(TextBox.TextProperty);
                BindingExpression be3 = txtAddMovieGenre.GetBindingExpression(TextBox.TextProperty);
                BindingExpression be4 = txtAddMovieYear.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
                be2.UpdateSource();
                be3.UpdateSource();
                be4.UpdateSource();
            }
        }

        private void btnCancelAddNewMovie_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAddNewMovie_Click(object sender, RoutedEventArgs e)
        {
            Movie createdMovie = cboMovieTitleList.SelectedItem as Movie;
            List<Movie> returnedMovies;
            int numOfCopies = cboNumberOfCopies.SelectedIndex + 1;

            if (createdMovie == null)
            {
                MessageBox.Show("You need to select or create a new movie.");
            }
            else if (txtAddMovieTitle.Text == "" || txtAddMovieRating.Text == "" || txtAddMovieGenre.Text == "" || txtAddMovieYear.Text == "")
            {
                MessageBox.Show("You cannot leave any fields empty.");
            }
            else if (cboMovieTitleList.SelectedIndex == 0)
            {

                Movie newm = new Movie();
                //Assign the textboxes to the createdMovie
                newm.title = txtAddMovieTitle.Text;
                newm.rating = txtAddMovieRating.Text;
                newm.genre = txtAddMovieGenre.Text;
                newm.releaseYear = txtAddMovieYear.Text;
                newm.isActive = true;


                //for (int i = 0; i < cboMovieTitleList.Items.Count; i++)
                //{
                //    Movie forLoopMovie = cboMovieTitleList.SelectedItem as Movie;
                //    if (createdMovie.title == forLoopMovie.title && createdMovie.releaseYear == SelectedItem.releaseYear)
                //    {

                //    }



                //}
                bool y = false;
                foreach (Movie m in cboMovieTitleList.Items)
                {
                    if (newm.title == m.title && newm.releaseYear == m.releaseYear)
                    {
                        y = true;
                    }
                }

                if (y)
                {
                    MessageBoxResult duplicateQ = MessageBox.Show($"{newm.title} {newm.releaseYear} already exists. \nAre you sure you want to create another Title with the same name and year?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (duplicateQ == MessageBoxResult.No)
                    {
                        return;
                    }

                }



                //if (createdMovie.title == i.title && createdMovie.releaseYear == i.releaseYear)
                //{
                //    MessageBoxResult duplicateQ = MessageBox.Show($"{createdMovie.title} {createdMovie.releaseYear} already exists. \nAre you sure you want to create another Title with the same name and year?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                //    switch (duplicateQ)
                //    {
                //        case MessageBoxResult.Yes:

                //            returnedMovies = DB.Movies.Create(createdMovie, numOfCopies, false);

                //            if (returnedMovies == null)
                //            {
                //                break;
                //            }

                //            populateListView(returnedMovies);
                //            break;
                //    }
                //}

                MessageBoxResult result = MessageBox.Show($"Are you sure you want to create\n{numOfCopies} of the movie {newm.title}?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Yes:

                        returnedMovies = DB.Movies.Create(newm, numOfCopies, false);

                        if (returnedMovies == null)
                        {
                            MessageBox.Show("Movie not created.");
                            break;
                        }
                        else
                        {

                            populateListView(returnedMovies);
                        }
                        break;
                }

            }
            else
            {
                //movie already in list
                createdMovie = cboMovieTitleList.SelectedItem as Movie;

                MessageBoxResult result = MessageBox.Show($"Are you sure you want to create\n{numOfCopies} of the movie {createdMovie.title}?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Yes:

                        returnedMovies = DB.Movies.Create(createdMovie, numOfCopies, true);

                        if (returnedMovies == null)
                        {
                            MessageBox.Show("Movie not created.");
                            break;
                        }else
                        {
                            populateListView(returnedMovies);
                        }

                        break;
                }

            }


        }


        private void populateListView(List<Movie> returnedMovies)
        {
            if(returnedMovies == null)
            {
                return;
            }

            foreach (Movie i in returnedMovies)
            {
                lstReturnedMovies.Items.Add(i);
            }

            refreshList();

/*
            txtAddMovieTitle.IsEnabled = false;
            txtAddMovieRating.IsEnabled = false;
            txtAddMovieGenre.IsEnabled = false;
            txtAddMovieYear.IsEnabled = false;
            
    */
      
        }
        #region TrimText
        private void txtAddMovieTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            txtAddMovieTitle.Text = txtAddMovieTitle.Text.Trim();

        }

        private void txtAddMovieRating_LostFocus(object sender, RoutedEventArgs e)
        {
            txtAddMovieRating.Text = txtAddMovieRating.Text.Trim();

        }

        private void txtAddMovieGenre_LostFocus(object sender, RoutedEventArgs e)
        {
            txtAddMovieGenre.Text = txtAddMovieGenre.Text.Trim();

        }

        private void txtAddMovieYear_LostFocus(object sender, RoutedEventArgs e)
        {
            txtAddMovieRating.Text = txtAddMovieRating.Text.Trim();

        }
        #endregion

        private void btnExportMoviesToPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstReturnedMovies.HasItems)
                {
                    //Exporting to PDF
                    string folderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\";
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    using (FileStream stream = new FileStream($"{folderPath}CreatedMovies{DateTime.Now.ToString("yyyyMMddHHmmss")}.pdf", FileMode.Create))
                    {
                        Document pdfDoc = new Document(PageSize.A2, 10f, 10f, 10f, 0f);
                        var pdfWriter = PdfWriter.GetInstance(pdfDoc, stream);

                        pdfDoc.Open();

                        PdfContentByte cb = new PdfContentByte(pdfWriter);

                        PdfPTable pdfTable = new PdfPTable(3);
                        pdfTable.DefaultCell.Padding = 20;
                        pdfTable.WidthPercentage = 50;
                        pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
                        pdfTable.DefaultCell.BorderWidth = 1;

                        PdfPCell cell = new PdfPCell(new Phrase("Title"));
                        pdfTable.AddCell(cell);
                        cell = new PdfPCell(new Phrase("ID"));
                        pdfTable.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Barcode"));
                        pdfTable.AddCell(cell);

                        foreach (Movie item in lstReturnedMovies.Items)
                        {
                            pdfTable.AddCell(item.title);
                            pdfTable.AddCell(item.upc.ToString());

                            Barcode39 barcode39 = new Barcode39();
                            barcode39.StartStopText = true;
                            barcode39.Code = item.upc.ToString();
                            iTextSharp.text.Image image39 = barcode39.CreateImageWithBarcode(cb, null, null);
                            pdfTable.AddCell(image39);

                        }


                        pdfDoc.Add(pdfTable);
                        pdfDoc.Close();
                        stream.Close();
                    }

                }
                else
                {
                    MessageBox.Show("You need to create a movie first.");
                    return;
                }
                MessageBox.Show($"Report Created Successfully", "Report Created");

            }
            catch(Exception exp)
            {
                MessageBox.Show("Error Exporting Report!");
            }
        }
    }
}

