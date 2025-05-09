using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace YCHEBKA2
{

    public partial class Form1: Form
    {
        private NpgsqlConnection connection;
        private string connectionString = "Host=localhost;Username=postgres;Password=18273645;Database=BIBLIOTEKA";
        public Form1()
        {
            InitializeComponent();
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text;
            string password = textBox2.Text;

            if (AuthenticateUser(username, password))
            {
                MessageBox.Show("Успешная аутентификация! Добро пожаловать, " + username + "!");
                Form f2 = new Form2();
                this.Hide();
                f2.Show();
            }
            else
            {
                MessageBox.Show("Ошибка аутентификации. Пользователь с такими данными не найден.");
            }
        }

        private bool AuthenticateUser(string username, string password)
        {
            string connectionString = "Host=localhost;Username=postgres;Password=18273645;Database=BIBLIOTEKA";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM db_users WHERE user_login = @Username AND user_password = @Password";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    return count > 0;
                }
            }
        }
    }
}
