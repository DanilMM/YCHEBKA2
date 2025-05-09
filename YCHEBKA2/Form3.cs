using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

namespace YCHEBKA2
{
    public partial class Form3 : Form
    {
        private string connectionString = "Host=localhost;Username=postgres;Password=18273645;Database=BIBLIOTEKA";

        public Form3()
        {
            InitializeComponent();
            LoadDebtors();  // Загружаем данные сразу при старте формы
            this.FormClosing += new FormClosingEventHandler(FormClosingevent);
        }

        private void LoadDebtors()
        {
            flowLayoutPanel1.Controls.Clear();

            var debtorList = new Dictionary<string, (string phone, List<(string author, string title, int count, DateTime date, int overdueDays)>)>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
            SELECT 
                ч.""ФИО"",
                ч.""Телефон"",
                и.""Автор"",
                и.""Название"",
                в.""Количество"",
                в.""Дата_выдачи"",
                EXTRACT(DAY FROM CURRENT_DATE - (в.""Дата_выдачи"" + INTERVAL '14 days')) AS days_overdue
            FROM ""Выдачи"" в
            JOIN ""Читатели"" ч ON в.""Код_читателя"" = ч.""Код_читателя""
            JOIN ""Издания"" и ON в.""Инвентарный_номер"" = и.""Инвентарный_номер""
            WHERE в.""Дата_возврата"" IS NULL
              AND в.""Дата_выдачи"" + INTERVAL '14 days' < CURRENT_DATE
            ORDER BY ч.""ФИО"";
        ";

                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string fio = reader.GetString(0);
                        string phone = reader.IsDBNull(1) ? "—" : reader.GetString(1);
                        string author = reader.GetString(2);
                        string title = reader.GetString(3);
                        int count = reader.GetInt32(4);
                        DateTime borrowDate = reader.GetDateTime(5);
                        int overdueDays = Convert.ToInt32(reader["days_overdue"]);

                        if (!debtorList.ContainsKey(fio))
                        {
                            debtorList[fio] = (phone, new List<(string, string, int, DateTime, int)>());
                        }

                        debtorList[fio].Item2.Add((author, title, count, borrowDate, overdueDays));
                    }
                }
            }

            // Отображение на форме
            foreach (var debtor in debtorList)
            {
                Panel panel = new Panel
                {
                    BackColor = Color.White,
                    Width = flowLayoutPanel1.Width - 40,
                    AutoSize = true,
                    Padding = new Padding(10),
                    Margin = new Padding(5),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // ФИО и Телефон на первой строке
                Label lblHeader = new Label
                {
                    Text = $"{debtor.Key} / {debtor.Value.phone}",
                    Font = new Font("Times New Roman", 11, FontStyle.Bold),
                    AutoSize = true
                };
                panel.Controls.Add(lblHeader);

                // Отступ перед данными о книгах
                Label lblSpacer = new Label
                {
                    Text = "\n", // Пустой Label для создания отступа
                    AutoSize = true,
                    Margin = new Padding(0, 5, 0, 5) // Отступ сверху и снизу
                };
                panel.Controls.Add(lblSpacer);

                // Данные о книгах
                foreach (var book in debtor.Value.Item2)
                {
                    // Информация о книге на новой строке
                    Label lblBook = new Label
                    {
                        Text = $"\n{book.author} / {book.title} / {book.count} / {book.date.ToShortDateString()} / {book.overdueDays} дней задолженности",
                        Font = new Font("Times New Roman", 10),
                        AutoSize = true,
                        Margin = new Padding(0, 5, 0, 0),
                        ForeColor = book.overdueDays > 0 ? Color.Red : Color.Black
                    };
                    panel.Controls.Add(lblBook);

                    // Отступ между книгами
                    Label lblBookSpacer = new Label
                    {
                        Text = "", // Пустой Label для создания отступа
                        AutoSize = true,
                        Margin = new Padding(0, 5, 0, 10) // Отступ после каждого элемента книги
                    };
                    panel.Controls.Add(lblBookSpacer);
                }

                flowLayoutPanel1.Controls.Add(panel);
            }
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form2 form2 = new Form2();
            form2.Show();
        }
        private void FormClosingevent(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
