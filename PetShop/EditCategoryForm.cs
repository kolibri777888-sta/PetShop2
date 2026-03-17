using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace PetShop
{
    public partial class EditCategoryForm : Form
    {
        int id;

        public EditCategoryForm(int id)
        {
            InitializeComponent();
            this.id = id;

            LoadCategory();

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

        // Загрузка категории
        void LoadCategory()
        {
            using (var con = DB.Get())
            {
                var cmd = new MySqlCommand(
                    "SELECT Name FROM Categories WHERE Id=@id", con);

                cmd.Parameters.AddWithValue("@id", id);

                con.Open();

                var r = cmd.ExecuteReader();

                if (r.Read())
                {
                    txtName.Text = r["Name"].ToString();
                }
            }
        }

        // Проверка дубликата
        bool IsDuplicate()
        {
            using (var con = DB.Get())
            {
                var cmd = new MySqlCommand(@"
                SELECT COUNT(*) FROM Categories
                WHERE Name=@n AND Id<>@id", con);

                cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();

                return Convert.ToInt32(
                    cmd.ExecuteScalar()) > 0;
            }
        }

        // Сохранение
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Проверка пустого поля
            if (txtName.Text.Trim() == "")
            {
                MessageBox.Show("Введите название!");
                return;
            }

            // Проверка дубликата
            if (IsDuplicate())
            {
                MessageBox.Show("Такая категория уже есть!");
                return;
            }

            // Подтверждение
            if (MessageBox.Show(
                "Сохранить изменения?",
                "Подтверждение",
                MessageBoxButtons.YesNo)
                == DialogResult.No)
                return;

            try
            {
                using (var con = DB.Get())
                {
                    var cmd = new MySqlCommand(
                        "UPDATE Categories SET Name=@n WHERE Id=@id", con);

                    cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Категория обновлена!");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}