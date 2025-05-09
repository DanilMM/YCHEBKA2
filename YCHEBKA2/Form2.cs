using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YCHEBKA2
{
    public partial class Form2 : Form
    {
        NpgsqlConnection conn;
        NpgsqlDataAdapter adapter;
        DataTable dt;

        Dictionary<string, Dictionary<object, string>> lookupData = new Dictionary<string, Dictionary<object, string>>();

        public Form2()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(FormClosingevent);

            string connection = "Host=localhost;Username=postgres;Password=18273645;Database=BIBLIOTEKA";
            conn = new NpgsqlConnection(connection);

            comboBox1.Items.AddRange(new object[] {
                "Издания", "Читатели", "Выдачи", "Типы_изданий", "Жанры", "Издательства"
            });

            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string table = comboBox1.Text;

            LoadLookupTables();

            string query = GetSelectQuery(table);
            adapter = new NpgsqlDataAdapter(query, conn);
            NpgsqlCommandBuilder builder = new NpgsqlCommandBuilder(adapter);

            dt = new DataTable();
            adapter.Fill(dt);

            dataGridView1.Columns.Clear();
            dataGridView1.DataSource = dt;

            HidePrimaryKey();
            ReplaceForeignKeyColumns(table);

            dataGridView1.DefaultValuesNeeded -= dataGridView1_DefaultValuesNeeded;
            dataGridView1.DefaultValuesNeeded += dataGridView1_DefaultValuesNeeded;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                NpgsqlCommandBuilder builder = new NpgsqlCommandBuilder(adapter);
                adapter.UpdateCommand = builder.GetUpdateCommand();
                adapter.InsertCommand = builder.GetInsertCommand();
                adapter.DeleteCommand = builder.GetDeleteCommand();

                adapter.Update(dt);
                MessageBox.Show("Изменения сохранены.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                try
                {
                    dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);
                    NpgsqlCommandBuilder builder = new NpgsqlCommandBuilder(adapter);
                    adapter.Update(dt);
                    MessageBox.Show("Запись удалена.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при удалении: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Выберите строку для удаления.");
            }
        }

        private string GetSelectQuery(string table)
        {
            return $"SELECT * FROM \"{table}\"";
        }

        private void HidePrimaryKey()
        {
            var hiddenKeys = new[] {
                "Код_выдачи", "Код_читателя", "Код_жанра", "Код_издательства", "Код_типа"
            };

            foreach (var key in hiddenKeys)
            {
                if (dataGridView1.Columns.Contains(key))
                    dataGridView1.Columns[key].Visible = false;
            }
        }

        private void LoadLookupTables()
        {
            lookupData["Типы_изданий"] = LoadLookup("Типы_изданий", "Код_типа", "Тип");
            lookupData["Жанры"] = LoadLookup("Жанры", "Код_жанра", "Жанр");
            lookupData["Издательства"] = LoadLookup("Издательства", "Код_издательства", "Издательство");
            lookupData["Читатели"] = LoadLookup("Читатели", "Код_читателя", "ФИО");
            lookupData["Издания"] = LoadLookup("Издания", "Инвентарный_номер", "Название");
        }

        private Dictionary<object, string> LoadLookup(string table, string keyCol, string valCol)
        {
            Dictionary<object, string> dict = new Dictionary<object, string>();
            string query = $"SELECT \"{keyCol}\", \"{valCol}\" FROM \"{table}\"";

            using (var cmd = new NpgsqlCommand(query, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dict.Add(reader[0], reader[1].ToString());
                    }
                }
                conn.Close();
            }

            return dict;
        }

        private void ReplaceForeignKeyColumns(string table)
        {
            Dictionary<string, string> fkMap = new Dictionary<string, string>();

            if (table == "Издания")
            {
                fkMap.Add("Код_типа", "Типы_изданий");
                fkMap.Add("Код_жанра", "Жанры");
                fkMap.Add("Код_издательства", "Издательства");
            }
            else if (table == "Выдачи")
            {
                fkMap.Add("Код_читателя", "Читатели");
                fkMap.Add("Инвентарный_номер", "Издания");
            }

            foreach (var kvp in fkMap)
            {
                string column = kvp.Key;
                string lookupTable = kvp.Value;

                if (!dt.Columns.Contains(column)) continue;

                DataGridViewComboBoxColumn combo = new DataGridViewComboBoxColumn
                {
                    Name = column,
                    DataPropertyName = column,
                    HeaderText = column,
                    DataSource = new BindingSource(lookupData[lookupTable], null),
                    DisplayMember = "Value",
                    ValueMember = "Key",
                    FlatStyle = FlatStyle.Flat
                };

                int index = dataGridView1.Columns[column].Index;
                dataGridView1.Columns.Remove(column);
                dataGridView1.Columns.Insert(index, combo);
            }
        }

        private void dataGridView1_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            string table = comboBox1.Text;

            if (table == "Выдачи")
            {
                if (lookupData["Читатели"].Count > 0)
                    e.Row.Cells["Код_читателя"].Value = lookupData["Читатели"].First().Key;
                if (lookupData["Издания"].Count > 0)
                    e.Row.Cells["Инвентарный_номер"].Value = lookupData["Издания"].First().Key;

                e.Row.Cells["Дата_выдачи"].Value = DateTime.Today;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Hide(); 
            Form3 form3 = new Form3();
            form3.Show();
        }

        private void FormClosingevent(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
