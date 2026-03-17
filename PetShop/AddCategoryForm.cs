using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace PetShop
{
    public partial class AddCategoryForm : Form
    {
        public AddCategoryForm()
        {
            InitializeComponent();

            SetLimits();
            SetValidation();
        }

        // Ограничение длины
        void SetLimits()
        {
            txtName.MaxLength = 15;
        }

        // Подключение проверок
        void SetValidation()
        {
            txtName.KeyPress += OnlyLetters;
        }

        // Только буквы
        private void OnlyLetters(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) &&
                e.KeyChar != '\b' &&
                e.KeyChar != ' ')
            {
                e.Handled = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Проверка пустого поля
            if (txtName.Text.Trim() == "")
            {
                MessageBox.Show("Введите название категории!");
                return;
            }

            try
            {
                using (var con = DB.Get())
                {
                    var cmd = new MySqlCommand(
                        "INSERT INTO Categories (Name) VALUES (@n)", con);

                    cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Категория добавлена!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}