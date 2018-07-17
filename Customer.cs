using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lackluster
{
    public class Customer : IDataErrorInfo
    {
        public int id { get; set; }
        public string phoneNumber { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public int points { get; set; }
        public bool isActive { get; set; }

        public Customer()
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
                    }
                        
                }
                if (columnName == "phoneNumber")
                {
                    if (string.IsNullOrEmpty(phoneNumber))
                    {
                        result = "Please enter a Phone Number";
                        return result;
                    }

                    if(phoneNumber.Length < 10)
                    {
                        result = "Phone Number must be 10 digits long";
                    }
                        
                    foreach(char i in phoneNumber)
                    {
                        if(i < '0' || i > '9')
                        {
                            result = "Phone Number can only contain digits";
                        }
                    }
                }
                if(columnName == "points")
                {
                    if(points < 0)
                    {
                        result = "Points cannot be negative";
                    }
                }
                return result;
            }
        }

        public Customer(string phoneNumber, string firstName, string lastName, string email, int points=0, bool isActive=true)
        {
            this.id = id;
            this.phoneNumber = phoneNumber;
            this.firstName = firstName;
            this.lastName = lastName;
            this.email = email;
            this.points = points;
            this.isActive = isActive;
        }

        public void Save()
        {
            DB.Customers.Update(this);
        }
    }
}
