using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.ComponentModel;

namespace Lackluster
{
    public class Movie : IDataErrorInfo
    {
        //Variables for the movie object
        public int id { get; set; }
        public string title { get; set; }
        public string rating { get; set; }
        public string releaseYear { get; set; }
        public string genre { get; set; }
        public string upc { get; set; }
        public string price { get; set; }
        public bool isRented { get; set; }
        public bool isActive { get; set; }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                string result = null;
                if (columnName == "title")
                {
                    if (string.IsNullOrEmpty(title))
                    {
                        result = "Please enter the Movie title";
                    }

                }
                if (columnName == "rating")
                {
                    if (string.IsNullOrEmpty(rating))
                    {
                        result = "Please enter the Movie rating (G, PG, PG-13, R, NC-17)";
                    }

                }
                if (columnName == "releaseYear")
                {
                    if (string.IsNullOrEmpty(releaseYear))
                    {
                        result = "Please enter the Movie release year";
                        return result;
                    }

                    if (releaseYear.Length < 4)
                    {
                        result = "The Movie release year must be 4 digits long (eg. 1999)";
                    }

                    foreach (char i in releaseYear)
                    {
                        if (i < '0' || i > '9')
                        {
                            result = "The Movie release year can only contain digits";
                            return result;
                        }
                    }
                    if(Convert.ToInt32(releaseYear) < 1888 || Convert.ToInt32(releaseYear) > DateTime.Now.Year+1)
                    {
                        result = $"The Movie release year must be between 1899 and {DateTime.Now.Year + 1}";
                    }
                }
                if (columnName == "genre")
                {
                    if (string.IsNullOrEmpty(genre))
                    {
                        result = "Please enter a Movie genre (eg. Action)";
                    }
                }
                return result;
            }
        }

        public Movie(string title, string rating, string year, string genre, string upc, string price, bool isActive)
        {
            this.title = title;
            this.rating = rating;
            this.releaseYear = year;
            this.genre = genre;
            this.upc = upc;
            this.price = price;
            this.isActive = isActive;
        }

        public Movie()
        {

        }

        public void Save() {
            DB.Movies.Update(this);
        }

        
        public int TotalNumCopies()
        {
            return 0;
        }

        //reaches db to see number of movies available
        public int NumAvailable()
        {
            return 0;
        }
    }
}
