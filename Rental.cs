using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lackluster
{
    public class Rental
    {
        Employee emp;
        Customer cst;
        Movie movie;
        string upc;
        DateTime checkoutDate;
        DateTime dueDate;

        public Rental()
        {

        }

        public Rental(Employee emp, Customer cst, Movie mov)
        {
            this.emp = emp;
            this.cst = cst;
            this.movie = mov;

            this.checkoutDate = DateTime.Now;
            this.dueDate = DateTime.Now;
        }
    }

}
