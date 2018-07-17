using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Lackluster
{
     public class Employee : IDataErrorInfo
    {
        public int id { get; set; }
        public string username { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public bool isActive { get; set; }
        public bool isManager { get; set; }



        public Employee(string username, string firstName, string lastName, string email, bool isActive, bool isManager)
        {
            this.username = username;
            this.firstName = firstName;
            this.lastName = lastName;
            this.email = email;
            this.isActive = isActive;
            this.isManager = isManager;
        }

        public Employee()
        {

        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                string result = null;
                if (columnName == "username")
                {
                    if (string.IsNullOrEmpty(username))
                    {
                        result = "Please enter a username";
                    }

                }
                if (columnName == "firstName")
                {
                    if (string.IsNullOrEmpty(firstName))
                    {
                        result = "Please enter a First Name";
                    }

                }
                if (columnName == "lastName")
                {
                    if (string.IsNullOrEmpty(lastName))
                    {
                        result = "Please enter a Last Name";
                        return result;
                    }
                }
                return result;
            }
        }

        public void Save()
        {
            DB.Employees.Update(this);
        }

        public string GetSecurityQuestion()
        {
            return DB.Employees.GetQA(this)[0];
        }

        public bool VerifyQA(string answer)
        {
            try
            {
                //gets hashed password from db
                //Stored in this.password field
                string dbAnswer = DB.Employees.GetQA(this)[1];
                byte[] dbHashedBytes = Convert.FromBase64String(dbAnswer);

                //Hashing text password
                byte[] salt = new byte[16];
                Array.Copy(dbHashedBytes, 0, salt, 0, 16);
                var pbkdf2 = new Rfc2898DeriveBytes(answer, salt, 10000);
                byte[] textHash = pbkdf2.GetBytes(20);

                for (int i = 0; i < 20; i++)
                {
                    if (dbHashedBytes[i + 16] != textHash[i])
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public void SetQA(string question, string answer)
        {
            DB.Employees.UpdateQA(this, question, HashPassword(answer));
            this.Save();
        }

        //method to change employees password
        public void SetPassword(string password)
        {
            DB.Employees.UpdatePassword(this, HashPassword(password));
            this.Save();
        }

        //Takes employee hashed password from DB, then hashes this password paramater and compares
        public bool VerifyPassword(string password)
        {
            try
            {
                //gets hashed password from db
                //Stored in this.password field
                string dbPassword = DB.Employees.GetPassword(this);
                byte[] dbHashedBytes = Convert.FromBase64String(dbPassword);

                //Hashing text password
                byte[] salt = new byte[16];
                Array.Copy(dbHashedBytes, 0, salt, 0, 16);
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
                byte[] textHash = pbkdf2.GetBytes(20);

                for (int i = 0; i < 20; i++)
                {
                    if (dbHashedBytes[i + 16] != textHash[i])
                    {
                        return false;
                    }
                }

                return true;
            }catch(Exception e)
            {
                return false;
            }
        }

        private static string HashPassword(string password)
        {
            byte[] salt;

            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);

            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];

            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            string PasswordHash = Convert.ToBase64String(hashBytes);

            return PasswordHash;
        }
    }
}
