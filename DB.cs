using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace Lackluster
{
    public static class DB
    {
        private static string API = "https://lackluster.ml/api";
        private static string ConnectionString;

        internal static bool CanContactServer()
        {
            try
            {
                using (var client = new WebClient())
                {
                    var response = client.DownloadString("https://lackluster.ml");
                    return true; ;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }


        internal static bool GetConnectionString(string username, string password)
        {
            try
            {
                using (var client = new WebClient())
                {
                    
                    var values = new NameValueCollection();
                    values["user"] = username;
                    values["password"] = password;

                    var response = client.UploadValues(API+"/login", values);

                    var responseString = Encoding.Default.GetString(response);
                    ConnectionString = responseString;

                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        try
                        {
                            con.Open();
                            con.Close();
                            return true;
                        }
                        catch (SqlException)
                        {
                            ClearConnectionString();
                            return false;
                        }
                    }
                }
            }catch(Exception e)
            {
                if (e.Message == "The remote server returned an error: (429) Too Many Requests.")
                {
                    MessageBox.Show("To many attempts at user login. Please try again in 15 mins.", "To Many Attempts");
                }
                ClearConnectionString();
                return false;
            }
        }

        internal static void ClearConnectionString() {
            ConnectionString = null;
        }

        public static class Movies
        {
            //Method to retreive ONE Movie
            //Returns Movie Object
            public static Movie Get(string upc)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            //Query to get movie
                            //Try converting string to in - if fails returns null
                            int upcInt;
                            try
                            {
                                upcInt = Int32.Parse(upc);
                            }
                            catch (Exception E)
                            {
                                return null;
                            }

                            //query to get Movie by UPC
                            string query = "Select upc.upc as upc, upc.active as upcActive, upc.Rented as rented, m.* From movies m Inner Join moviesupc upc on m.id = upc.movieId Where upc.active = True and upc.upc = @upc Limit 1";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@upc", upcInt);

                            MySqlDataReader reader = cmd.ExecuteReader();

                            //Check if anything was returned
                            if (reader.Read())
                            {
                                Movie movie = new Movie();
                                //Set data to public variables
                                movie.id = reader.GetInt32("id");
                                movie.title = reader.GetString("title");
                                movie.rating = reader.GetString("rating");
                                movie.releaseYear = reader.GetString("year");
                                movie.genre = reader.GetString("genre");
                                movie.upc = reader.GetString("upc");
                                movie.isRented = reader.GetBoolean("rented");
                                movie.isActive = reader.GetBoolean("upcActive");
                                movie.price = reader.GetString("price");

                                reader.Close();
                                return movie;
                            }

                            reader.Close();
                            return null;
                        }
                    }
                }catch(Exception e)
                {
                    return null;
                }
                 
            }

            //Gets all movie in db order by movie ID
            //1, Start id Number
            //Returns 50 Movies in List
            /*public static List<Movie> GetAll(int StartMovieId = 1)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            List<Movie> movies = new List<Movie>();
                            string query = "Select m.* From movies m Where m.id >= '" + StartMovieId + "' AND m.id <='" + (StartMovieId + 49) + "' Order By m.title";

                            MySqlDataReader reader = SelectQry(query);

                            //Check if anything was returned
                            while (reader.Read())
                            {
                                Movie movie = new Movie();
                                //Set data to public variables
                                movie.id = reader.GetInt32("id");
                                movie.title = reader.GetString("title");
                                movie.rating = reader.GetString("rating");
                                movie.releaseYear = reader.GetString("year");
                                movie.genre = reader.GetString("genre");
                                movie.isActive = reader.GetBoolean("active");
                                movie.price = "3.00";

                                movies.Add(movie);

                            }

                            reader.Close();

                            return movies;
                        }
                    }
                }catch(Exception e)
                {
                    return null;
                }
            }
            */

            //Need to check if we want just tile names
            public static List<Movie> GetByTitleOrUPC(string title)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            string query = "Select m.* From movies m Where m.title Like @search";

                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@search", string.Format("%{0}%", title));


                            MySqlDataReader reader = cmd.ExecuteReader();
                            List<Movie> movies = new List<Movie>();

                            //Check if anything was returned
                            while (reader.Read())
                            {
                                //Set data to public variables
                                Movie movie = new Movie();
                                movie.id = reader.GetInt32("id");
                                movie.title = reader.GetString("title");
                                movie.rating = reader.GetString("rating");
                                movie.releaseYear = reader.GetString("year");
                                movie.genre = reader.GetString("genre");
                                movie.upc = null;// reader.GetString("upc");
                                movie.isRented = false; //reader.GetBoolean("rented");
                                movie.isActive = reader.GetBoolean("active");
                                movie.price = reader.GetString("price");


                                movies.Add(movie);
                            }


                            reader.Close();
                            if (movies.Count > 0)
                            {
                                return movies;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            //public static List<Movie> GetAvailableByTitle

            //Get UPCs
            public static List<Movie> GetChildMovies(Movie m)
            {
                try
                {
                    if(m.id == null)
                    {
                        return null;
                    }

                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            List<Movie> children = new List<Movie>();
                            string query = "Select * from moviesupc where movieId = @mid";

                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@mid", m.id);

                            MySqlDataReader reader = cmd.ExecuteReader();

                            //Check if anything was returned
                            while (reader.Read())
                            {
                                //Set data to public variables
                                Movie movie = new Movie();
                                movie.id = m.id;
                                movie.title = m.title;
                                movie.rating = m.rating;
                                movie.releaseYear = m.releaseYear;
                                movie.genre = m.genre;
                                movie.upc = reader.GetString("upc");
                                movie.isRented = reader.GetBoolean("rented");
                                movie.isActive = reader.GetBoolean("active");
                                movie.price = m.price;

                                children.Add(movie);
                            }


                            reader.Close();

                            return children;

                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }



            //Saves Movie in DB
            //Param is completed Movie Object
            //Return nothing
            public static List<Movie> Create(Movie movie, int numAvailable, bool alreadyCreated)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            List<Movie> movies = new List<Movie>();
                            string query;
                            int last =-1;
                            int movieId;
                            int active = 0;

                            if (!alreadyCreated)
                            {

                                if (movie.isActive)
                                {
                                    active = 1;
                                }

                                query = "Insert into movies (title, rating, year, genre, numOfCopies, availableCopies, active, price) values (@title, @rating, @year, @genre, 0, 0 , @active, '3.00')";

                                cmd.CommandText = query;
                                cmd.Parameters.AddWithValue("@title", movie.title);
                                cmd.Parameters.AddWithValue("@rating", movie.rating);
                                cmd.Parameters.AddWithValue("@year", movie.releaseYear);
                                cmd.Parameters.AddWithValue("@genre", movie.genre);
                                cmd.Parameters.AddWithValue("@active", active);

                                cmd.ExecuteNonQuery();
                                last = Convert.ToInt32(cmd.LastInsertedId);
                                cmd.Parameters.Clear();
                            }
                            else
                            {
                                last = 0;
                            }

                            if (last > -1)
                            {
                                if (!alreadyCreated)
                                {
                                    movieId = last;
                                }
                                else
                                {
                                    movieId = movie.id;
                                }

                                active = 1;

                                for (int i = 0; i < numAvailable; i++)
                                {
                                    int upc;
                                    query = "Insert into moviesupc (movieId, active, rented) values (@movieid, @active, 0)";

                                    cmd.CommandText=query;
                                    cmd.Parameters.AddWithValue("@movieid", movieId);
                                    cmd.Parameters.AddWithValue("@active", active);

                                    cmd.ExecuteScalar();
                                    upc = Convert.ToInt32(cmd.LastInsertedId);
                                    cmd.Parameters.Clear();
                                    if (upc != 0)
                                    {
                                        query = "Update movies set numOfCopies = numOfCopies + 1, availableCopies = availableCopies + 1 where movies.id = @id";
                                        cmd.CommandText = query; ;
                                        cmd.Parameters.AddWithValue("@id", movieId);

                                        if (cmd.ExecuteNonQuery() != 0)
                                        {
                                            Movie newUPC = new Movie(movie.title, movie.rating, movie.releaseYear, movie.genre, "" + upc, movie.price, movie.isActive);
                                            newUPC.id = movieId;
                                            movies.Add(newUPC);
                                        }
                                        else
                                        {
                                            return null;
                                        }
                                       
                                    }
                                }
                            }
                            else
                            {
                                return null;
                            }
                            return movies;
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            //update movie record in DB
            public static bool Update(Movie movie)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {

                            string query = "update movies set " +
                                "movies.title= @title ," +
                                "movies.rating= @rating ," +
                                "movies.year= @year," +
                                "movies.genre= @genre," +
                                "movies.price= @price " +
                                "where movies.id = @id";

                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@title", movie.title);
                            cmd.Parameters.AddWithValue("@rating", movie.rating);
                            cmd.Parameters.AddWithValue("@year", movie.releaseYear);
                            cmd.Parameters.AddWithValue("@genre", movie.genre);
                            cmd.Parameters.AddWithValue("@price", "3.00");
                            cmd.Parameters.AddWithValue("@id", movie.id );

                            if(cmd.ExecuteNonQuery() != 0)
                            {
                                return true;
                            }
                            
                            return false;
                        }                    
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }


            //Sets UPC of movie in DB as deactive
            //2 Params
            //1 - Movie Object
            //2 - boolean for either inactivating entire movie or just UPC
            public static bool Delete(Movie movie)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            if (movie == null)
                            {
                                return false;
                            }
                            if (movie.upc == null)
                            {
                                //could set whole movie set inactive // feature later
                                return false;
                            }

                            //gets current values of movie from db
                            string query = "Select active from moviesupc where moviesupc.upc = @upc";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@upc", movie.upc);

                            int previousActive;

                            MySqlDataReader r = cmd.ExecuteReader();
                            if (r.Read())
                            {
                                previousActive = r.GetInt32("active");
                            }
                            else
                            {
                                return false;
                            }
                            r.Close();


                            //checks if changing from active to inactive or vise versa
                            if (previousActive == 1 && movie.isActive)
                            {
                                return true;
                            }
                            else if(previousActive == 1 && !movie.isActive)
                            {
                                query = "Update moviesupc inner join movies on movies.id = moviesupc.movieId set moviesupc.active = 0, movies.availableCopies = movies.availableCopies - 1 where moviesupc.upc = @upc and moviesupc.active=1";
                            }
                            else
                            {
                                query = "Update moviesupc inner join movies on movies.id = moviesupc.movieId set moviesupc.active = 1, movies.availableCopies = movies.availableCopies + 1 where moviesupc.upc = @upc and moviesupc.active=0";
                            }

                            cmd.CommandText = query;

                            if(cmd.ExecuteNonQuery() != 0)
                            {
                                return true;
                            }

                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }


            public static List<String[]> bestMovie()
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {

                            string query = "SELECT movies.title, count(movies.title) as timesRented" +
                                            " FROM moviesupc Inner Join movies on moviesupc.movieId = movies.id, rentals" +
                                            " WHERE moviesupc.upc = rentals.upc AND moviesupc.active = 1" +
                                            " GROUP BY movies.title" +
                                            " ORDER BY timesRented DESC";
                            cmd.CommandText = query;

                            MySqlDataReader reader = cmd.ExecuteReader();

                            List<String[]> bestMovies = new List<string[]>();

                            while (reader.Read())
                            {

                                string movieTitle = reader.GetString("title");
                                string timesRented = reader.GetString("timesRented");

                                string[] cust = { movieTitle, timesRented.ToString() };
                                bestMovies.Add(cust);

                            }

                            reader.Close();
                            return bestMovies;
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        public static class Employees
        {
            //Creates new employee in DB 
            //Params Employee objct
            //Returns employee if successfull null otherwise
            public static Employee Create(Employee emp)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            int manager = 0;
                            int active = 0;

                            if (emp.isActive)
                            {
                                active = 1;
                            }
                            if (emp.isManager)
                            {
                                manager = 1;
                            }



                            string query = "insert into employees (username, fname, lname, email, active, manager)" +
                                " Values (@username, @fn, @ln, @email, @active, @manager) ";

                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@username", emp.username);
                            cmd.Parameters.AddWithValue("@fn", emp.firstName);
                            cmd.Parameters.AddWithValue("@ln", emp.lastName);
                            cmd.Parameters.AddWithValue("@email", emp.email);
                            cmd.Parameters.AddWithValue("@active", active);
                            cmd.Parameters.AddWithValue("@manager", manager);

                            cmd.ExecuteScalar();
                            if (cmd.LastInsertedId != 0)
                            {
                                emp.id = Convert.ToInt32(cmd.LastInsertedId);
                                return emp;
                            }
                            return null;
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            //Marks employee as inactive in DB
            //Returns true is successful
            public static bool Delete(Employee emp)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            string query = "update employees set active = 0 where employees.username = @username";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@username", emp.username);
                            
                            if (cmd.ExecuteNonQuery() != 0)
                            {
                                return true;
                            }
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            //Gets employee by username
            //Paramater is string username
            //Returns employee object
            public static Employee GetByUsername(string username)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            string query = "Select * from employees where username= @username Limit 1";

                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@username", username);

                            MySqlDataReader reader = cmd.ExecuteReader();

                            if (reader.Read())
                            {
                                Employee emp = new Employee();
                                emp.id = reader.GetInt32("id");
                                emp.username = reader.GetString("username");
                                emp.firstName = reader.GetString("fname");
                                emp.lastName = reader.GetString("lname");
                                emp.email = reader.GetString("email");
                                emp.isActive = reader.GetBoolean("active");
                                emp.isManager = reader.GetBoolean("manager");

                                reader.Close();
                                return emp;
                            }

                            reader.Close();
                            return null;
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            public static List<Employee> GetEmployeesByName(string name)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            List<Employee> emps = new List<Employee>();


                            if (string.IsNullOrWhiteSpace(name))
                            {
                                name = "";
                            }

                            string query = "Select * From employees Where fname Like @name OR lname Like @name OR username Like @username";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@name", string.Format("{0}%", name));
                            cmd.Parameters.AddWithValue("@username", string.Format("%{0}%", name));

                            MySqlDataReader reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                Employee emp = new Employee();
                                emp.username = reader.GetString("username");
                                emp.firstName = reader.GetString("fname");
                                emp.lastName = reader.GetString("lname");
                                emp.email = reader.GetString("email");
                                emp.isActive = reader.GetBoolean("active");
                                emp.isManager = reader.GetBoolean("manager");

                                emps.Add(emp);
                            }
                            reader.Close();

                            if (emps.Count > 0)
                            {
                                return emps;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }


            //Update employee in DB
            //Param is employee  object to update
            //Returns employee object
            public static bool Update(Employee emp)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            int act = 0;
                            int man = 0;
                            if (emp.isActive)
                            {
                                act = 1;
                            }
                            if (emp.isManager)
                            {
                                man = 1;
                            }

                            string query = "update employees " +
                                "set username = @username" +
                                ", fname = @fn" +
                                ", lname = @ln" +
                                ", email = @email" +
                                ", active = @active" +
                                ", manager = @manager" +
                                "  where employees.username = @username";
                            cmd.CommandText = query;
                            
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@username", emp.username);
                            cmd.Parameters.AddWithValue("@fn", emp.firstName);
                            cmd.Parameters.AddWithValue("@ln", emp.lastName);
                            cmd.Parameters.AddWithValue("@email", emp.email);
                            cmd.Parameters.AddWithValue("@active", act);
                            cmd.Parameters.AddWithValue("@manager", man);

                            if (cmd.ExecuteNonQuery() != 0)
                            {
                                return true;
                            }
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            //method to return employees hashed password
            internal static string GetPassword(Employee emp)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            string query = "Select password from employees where id= @id";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@id", emp.id);

                            MySqlDataReader reader = cmd.ExecuteReader();

                            string password = null;

                            if (reader.Read())
                            {
                                password = reader.GetString("password");
                            }

                            reader.Close();
                            return password;
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            //Returns db QA in array
            //[0] = question
            //[1] = hashed answer
            internal static string[] GetQA(Employee emp)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {

                            string query = "Select question, answer from employees where id= @id";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@id", emp.id);

                            MySqlDataReader reader = cmd.ExecuteReader();

                            string question = null;
                            string answer = null;

                            if (reader.Read())
                            {
                                question = reader.GetString("question");
                                answer = reader.GetString("answer");
                            }

                            reader.Close();
                            string[] qa = { question, answer };
                            return qa;
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            //updates employees hashed password in DB
            internal static void UpdatePassword(Employee employee, string HashedPassword)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            string query = "Update employees set password = @hash where employees.id = @id";

                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@hash", HashedPassword);
                            cmd.Parameters.AddWithValue("@id", employee.id);

                            cmd.ExecuteNonQuery();

                        }
                    }
                }
                catch (Exception e)
                {
                }                
            }

            internal static void UpdateQA(Employee employee, string question, string HashedAnswer)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            string query = "Update employees set question = @question, answer= @hash where employees.id = @id";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@question", question);
                            cmd.Parameters.AddWithValue("@hash", HashedAnswer);
                            cmd.Parameters.AddWithValue("@id", employee.id);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e)
                {                 
                }
            }


            internal static bool Public_IsUsernameAvailable(string username)
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        string user = WebUtility.UrlEncode(username); ;

                        var response = client.DownloadString(API + "/open/users/" + user);

                        if (response == "True")
                        {
                            return true;
                        }

                        return false;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message == "The remote server returned an error: (429) Too Many Requests.")
                    {
                        MessageBox.Show("To many attempts looking up usernames. Please try again in 15 mins.", "To Many Attempts");
                        //throw new Exception("To Many Attempts");
                    }
                    return false;
                }
            }

            internal static string Public_VerifyAnswer(string username, string answer)
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        var values = new NameValueCollection();
                        values["answer"] = answer;
                        string url = $"/open/users/{username}/question";

                        var response = client.UploadValues(API + url, values);

                        var responseString = Encoding.Default.GetString(response);

                        return responseString;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message == "The remote server returned an error: (429) Too Many Requests.")
                    {
                        MessageBox.Show("To many attempts at answer. Please try again in 15 mins.", "To Many Attempts");
                    }
                    return null;
                }
            }

            internal static string Public_GetQuestion(string username)
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        string user = WebUtility.UrlEncode(username); ;
                        string url = $"/open/users/{user}/question";
                        var response = client.DownloadString(API + url);

                        return response;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message == "The remote server returned an error: (429) Too Many Requests.")
                    {
                        MessageBox.Show("To many attempts looking up user's question. Please try again in 15 mins.", "To Many Attempts");
                    }
                    return null;
                }
            }

            internal static bool Public_ChangePswd(string username, string tempPass, string newPass)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(tempPass) || string.IsNullOrWhiteSpace(newPass))
                    {
                        return false;
                    }


                    using (var client = new WebClient())
                    {
                        var values = new NameValueCollection();
                        values["tempPass"] = tempPass;
                        values["newPass"] = newPass;
                        string url = $"/open/users/{username}/reset";

                        var response = client.UploadValues(API + url, values);

                        var responseString = Encoding.Default.GetString(response);

                        if (responseString == "True")
                        {
                            return true;
                        }

                        return false;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message == "The remote server returned an error: (429) Too Many Requests.")
                    {
                        MessageBox.Show("To many attempts resetting password. Please try again in 15 mins.", "To Many Attempts");
                        //throw new Exception("To Many Attempts");
                    }
                    return false;
                }
            }

        }

        public static class Customers
        {
            //Gets customer from DB by Phone Number
            //Params phone number string
            //Returns customer object
            public static Customer GetByNumber(string number)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            //return new Customer();
                            string query = "Select * from customers where phoneNumber= @number ";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@number", number);

                            MySqlDataReader reader = cmd.ExecuteReader();

                            if (reader.Read())
                            {
                                Customer cust = new Customer();
                                cust.id = reader.GetInt32("id");
                                cust.phoneNumber = reader.GetString("phoneNumber");
                                cust.firstName = reader.GetString("fname");
                                cust.lastName = reader.GetString("lname");
                                cust.email = reader.GetString("email");
                                cust.isActive = reader.GetBoolean("active");
                                cust.points = reader.GetInt16("points");

                                reader.Close();
                                return cust;
                            }

                            reader.Close();
                            return null;
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            public static List<String[]> GetActiveRentalsByCust(string custID)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            string query = "SELECT movies.title, rentals.checkoutDate, rentals.dueDate" +
                                " FROM movies INNER JOIN moviesupc on movies.id = moviesupc.movieId INNER JOIN rentals on rentals.upc = moviesupc.upc" +
                                " WHERE rentals.customerID = @cid" +
                                " AND rentals.returnedDate IS NULL AND rentals.returned = 0";

                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@cid", custID);

                            MySqlDataReader reader = cmd.ExecuteReader();

                            List<String[]> custRentals = new List<String[]>();

                            while (reader.Read())
                            {

                                string movieTitle = reader.GetString("title");
                                string rentDate = reader.GetString("checkoutDate");
                                string custDate = reader.GetString("dueDate");

                                string[] custRental = { movieTitle, rentDate, custDate };
                                custRentals.Add(custRental);

                            }

                            reader.Close();
                            return custRentals;
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            public static List<String[]> bestCust()
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            string query = "Select fname, lname, phoneNumber, points" +
                                " From customers" +
                                " where active = 1 AND points > 0" +
                                " Order By points DESC";

                            cmd.CommandText = query;

                            MySqlDataReader reader = cmd.ExecuteReader();

                            //List<Rental> LateRentals = new List<Rental>();

                            List<String[]> bestCustomers = new List<string[]>();



                            while (reader.Read())
                            {

                                string customerName = reader.GetString("fname") + " " + reader.GetString("lname");
                                string number = reader.GetString("phoneNumber");
                                int custPoints = reader.GetInt16("points");

                                string[] cust = { customerName, number, custPoints.ToString() };
                                bestCustomers.Add(cust);

                            }

                            reader.Close();
                            return bestCustomers;
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            public static double GetLateFee(Customer cust)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            double latefee = 0.0;

                            string query = "Select latefee from customers where customers.id = @id";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@id", cust.id);

                            MySqlDataReader r = cmd.ExecuteReader();
                            if (r.Read())
                            {
                                latefee = r.GetDouble("latefee");
                            }

                            return latefee;
                        }
                    }
                }catch(Exception e)
                {
                    return 0.00;
                }
            }

            public static bool ClearLateFee(Customer cst)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {

                            string query = "update customers set latefee = 0 where customers.id = @id";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@id", cst.id);

                            
                            if (cmd.ExecuteNonQuery() > 0)
                            {
                                return true;
                            }

                            return false; ;
                        }
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            public static List<Customer> GetCustomersByName(string preSearch)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            List<Customer> customers = new List<Customer>();

                            string search = "";
                            if (!string.IsNullOrWhiteSpace(preSearch))
                            {
                                search = preSearch;
                            }

                            string query = "Select * From customers Where fname Like @name OR lname Like @name OR phoneNumber Like @num Limit 50";
                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@name", string.Format("{0}%",search));
                            cmd.Parameters.AddWithValue("@num", string.Format("%{0}%",search));

                            MySqlDataReader reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                Customer cust = new Customer();
                                cust.id = reader.GetInt32("id");
                                cust.phoneNumber = reader.GetString("phoneNumber");
                                cust.firstName = reader.GetString("fname");
                                cust.lastName = reader.GetString("lname");
                                cust.email = reader.GetString("email");
                                cust.isActive = reader.GetBoolean("active");
                                cust.points = reader.GetInt16("points");

                                customers.Add(cust);
                            }
                            reader.Close();
                            return customers;
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }


            
            //updates customer info in db
            public static bool Update(Customer cst)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {

                            int act = 0;
                            if (cst.isActive)
                            {
                                act = 1;
                            }


                            string query = "update customers " +
                                "set phoneNumber = @pn" +
                                ", fname = @fn" +
                                ", lname = @ln" +
                                ", email = @email" +
                                ", active = @act" +
                                ",points = @point" +
                                "  where customers.id = @id";

                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@pn", cst.phoneNumber);
                            cmd.Parameters.AddWithValue("@fn", cst.firstName);
                            cmd.Parameters.AddWithValue("@ln", cst.lastName);
                            cmd.Parameters.AddWithValue("@email", cst.email);
                            cmd.Parameters.AddWithValue("@act", act);
                            cmd.Parameters.AddWithValue("@point", cst.points);
                            cmd.Parameters.AddWithValue("@id", cst.id);

                            if (cmd.ExecuteNonQuery() != 0)
                            {
                                return true;
                            }
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            //creates new Customer in DB
            public static Customer Create(Customer cst)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            int act = 0;
                            if (cst.isActive)
                            {
                                act = 1;
                            }

                            string query = "Insert into customers (phoneNumber, fname, lname, email, active, points) Values (@pn, @fn, @ln, @email, @act, @points)";

                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@pn", cst.phoneNumber);
                            cmd.Parameters.AddWithValue("@fn", cst.firstName);
                            cmd.Parameters.AddWithValue("@ln", cst.lastName);
                            cmd.Parameters.AddWithValue("@email", cst.email);
                            cmd.Parameters.AddWithValue("@act", act);
                            cmd.Parameters.AddWithValue("@points", 0);

                            cmd.ExecuteScalar();
                            int insert = Convert.ToInt32(cmd.LastInsertedId);
                            if (insert != 0)
                            {
                                cst.id = insert;
                                return cst;
                            }
                            return null;
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            //sets customer as inactive
            public static bool Delete(Customer cst)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            string query = "update customers set active = 0 where customers.id = @id";
                            cmd.CommandText = query;

                            if (cmd.ExecuteNonQuery() != 0)
                            {
                                return true;
                            }
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            
        }

        public static class Rentals
        {
            //create new rental in db
            //Returns rental object
            public static bool Create(Employee emp, Customer cst, Movie movie)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            //query to insert record into the rentals table
                            string query = $"insert into rentals (upc, customerId, employeeId, checkoutDate, dueDate) values (@upc, @cst, @emp, Now() , concat((DATE(Now()) + INTERVAL 1 DAY), ' 23:59:59') );";

                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@upc", movie.upc);
                            cmd.Parameters.AddWithValue("@cst", cst.id);
                            cmd.Parameters.AddWithValue("@emp", emp.id);                      
                            

                            //Check that only one row was indeed inserted
                            if (cmd.ExecuteNonQuery() != 0)
                            {
                                //query to update rented bool in movies upc
                                query = $"update moviesupc set rented = 1 where upc = @upc;" +
                                    $" update movies inner join moviesupc on movies.id = moviesupc.movieId set timesRented = timesRented+1, availableCopies = availableCopies-1 where moviesupc.upc = @upc";

                                cmd.CommandText = query;

                                //Check that only one row was updated
                                if (cmd.ExecuteNonQuery() != 0)
                                {
                                    return true;
                                }
                            }
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            //Returns rental in DB
            //Returns bool
            //True return successfull 
            //False error
            public static bool Return(Employee emp, /*Customer cst,*/ Movie movie)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            string query = $@"
                                Update rentals 
                                inner join customers
                                  on rentals.customerId = customers.id
                                inner join moviesupc
                                  on rentals.upc = moviesupc.upc
                                inner join movies
                                  on moviesupc.movieId = movies.id

                                set customers.points = CASE
                                  WHEN rentals.dueDate > Curdate()

                                  Then points+1
                                  ELSE points
                                  END
                                ,
                                customers.latefee = CASE
	                                When rentals.dueDate < Curdate()
	                                Then latefee +  (datediff(rentals.returnedDate, rentals.dueDate) *3)
	                                ELSE latefee
	                                end
                                ,
                                movies.availableCopies = movies.availableCopies+1,
                                rentals.returned = 1,
                                rentals.returnedDate = Now(),
                                rentals.returnedById = @emp,

                                moviesupc.rented = 0

                                where rentals.upc = @upc and rentals.returned = 0 and rentals.returnedDate IS null                                


                            ";

                            cmd.CommandText = query;
                            cmd.Parameters.AddWithValue("@upc", movie.upc);
                            cmd.Parameters.AddWithValue("@emp", emp.id);

                            if (cmd.ExecuteNonQuery() != 0)
                            {
                                return true;
                            }
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    return false ;
                }
            }

            //Gets all late rentals
            //Returns a list of arrays. array contains data need to print out late movies
            public static List<String[]> Late()
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            string query = "Select customers.fname, customers.lname, customers.phoneNumber, movies.title, rentals.dueDate " +
                                "From rentals " +
                                "Inner Join customers as customers on customers.id = rentals.customerId " +
                                "Inner Join moviesupc as upc on upc.upc = rentals.upc " +
                                "inner join movies as movies on movies.id = upc.movieId " +
                                "where dueDate<Now() AND rentals.returnedById IS null " +
                                "Order by rentals.dueDate ASC";

                            cmd.CommandText = query;
                            MySqlDataReader reader = cmd.ExecuteReader();

                            //List<Rental> LateRentals = new List<Rental>();

                            List<String[]> LateRentals = new List<string[]>();


                            while (reader.Read())
                            {

                                string customerName = reader.GetString("fname") + " " + reader.GetString("lname");
                                string number = reader.GetString("phoneNumber");
                                string title = reader.GetString("title");
                                DateTime dueDate = reader.GetDateTime("dueDate");
                                string dueDateText = reader.GetDateTime("dueDate").ToString("yyyy-MM-dd");
                                int daysLate = (DateTime.Now.Day - dueDate.Day);

                                string[] late = { customerName, number, title, dueDateText, daysLate.ToString() };
                                LateRentals.Add(late);

                            }

                            reader.Close();
                            return LateRentals;
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            //returns all active rentals
            public static List<String[]> Active()
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(ConnectionString))
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            //Returns title, upc, duedate, custname, custnumber
                            List<String[]> rentals = new List<String[]>();

                            string query = "Select customers.fname, customers.lname, customers.phoneNumber, movies.title, rentals.dueDate, rentals.upc " +
                               "From rentals " +
                               "Inner Join customers as customers on customers.id = rentals.customerId " +
                               "Inner Join moviesupc as upc on upc.upc = rentals.upc " +
                               "inner join movies as movies on movies.id = upc.movieId " +
                               "where rentals.returned = 0 AND rentals.returnedById IS null " +
                               "Order by rentals.dueDate ASC";

                            cmd.CommandText = query;
                            MySqlDataReader reader = cmd.ExecuteReader();

                            //returns active rentals
                            while (reader.Read())
                            {
                                string customerName = reader.GetString("fname") + " " + reader.GetString("lname");
                                string upc = reader.GetString("upc");
                                string number = reader.GetString("phoneNumber");
                                string title = reader.GetString("title");
                                DateTime dueDate = reader.GetDateTime("dueDate");
                                string dueDateText = reader.GetDateTime("dueDate").ToString("yyyy-MM-dd");

                                string[] active = { title, upc, dueDateText, customerName, number };
                                rentals.Add(active);
                            }

                            return rentals;
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }

        }
    }
}
