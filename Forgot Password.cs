using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lackluster
{
    public partial class Forgot_Password: Form
    {
        private Boolean go = false;
        public Employee emp;
        public Forgot_Password()
        {
            InitializeComponent();
            textBox2.ReadOnly = true;
            textBox3.ReadOnly = true;
            textBox4.ReadOnly = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!go)
            {
                MessageBox.Show("Please search for username first.");
                lockForm();
                return;
            }
            

            string tempPass = DB.Employees.Public_VerifyAnswer(textBox5.Text.ToLower(), textBox2.Text);

            if (tempPass == null)
            {
                MessageBox.Show("Answer not correct.");
                return;
            }
            if (textBox3.Text != textBox4.Text)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }
            if (String.IsNullOrWhiteSpace(textBox3.Text) || String.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Password or Answer cannot be blank.");
                return;
            }

            if(!DB.Employees.Public_ChangePswd(textBox5.Text.ToLower(),tempPass, textBox3.Text))
            {
                MessageBox.Show("Error Changing password.");
                return;
            }
           
            string text = "Password has been changed.";
            MessageBox.Show(text);
            go = false;
            this.Close();
          
        }

        private void button2_Click(object sender, EventArgs e)
        {
            go = false;
            this.Close();
        }

        //Go Button
        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox5.Text == null)
            {
                string text = "Please enter a Valid Username.";
                MessageBox.Show(text);
                lockForm();
                return;

            }

            string question = DB.Employees.Public_GetQuestion(textBox5.Text.ToLower());

            if (question == null)
            {
                string text = "Please enter a Valid Username.";
                MessageBox.Show(text);
                lockForm();
                return;
            }

            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();


            textBox2.ReadOnly = false;
                textBox3.ReadOnly = false;
                textBox4.ReadOnly = false;
                textBox1.Text = question;
                go = true;
         
               
        }

        private void lockForm()
        {
            textBox2.ReadOnly = true;
            textBox3.ReadOnly = true;
            textBox4.ReadOnly = true;
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
