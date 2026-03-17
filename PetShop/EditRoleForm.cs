using MySql.Data.MySqlClient;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PetShop
{
    public partial class EditRoleForm : Form
    {
        int id;

        public EditRoleForm(int id)
        {
            InitializeComponent();
            this.id = id;

            // Ограничение длины имени
            txtName.MaxLength = 15;

            // Разрешаем ввод только русских букв
            txtName.KeyPress += TxtName_KeyPress;

            if (id > 0)
                LoadRole();
        }

        // Метод для проверки ввода в поле "Имя"
        private void TxtName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только русские буквы и клавиши управления (Backspace, Delete и т.д.)
            if (!char.IsControl(e.KeyChar) && !Regex.IsMatch(e.KeyChar.ToString(), @"^[а-яА-Я]$"))
            {
                e.Handled = true;
            }
        }

        // Загрузка роли
        void LoadRole()
        {
            using (var con = DB.Get())
            {
                var cmd = new MySqlCommand(
                    "SELECT * FROM Roles WHERE Id=@id", con);

                cmd.Parameters.AddWithValue("@id", id);

                con.Open();

                var r = cmd.ExecuteReader();

                if (r.Read())
                {
                    txtName.Text = r["Name"].ToString();
                }
            }
        }

        // Проверка дубликатов
        bool IsDuplicate()
        {
            using (var con = DB.Get())
            {
                var cmd = new MySqlCommand(@"
                SELECT COUNT(*) FROM Roles
                WHERE Name=@n AND Id<>@id", con);

                cmd.Parameters.AddWithValue("@n", txtName.Text);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // Сохранение
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название роли!");
                return;
            }

            if (IsDuplicate())
            {
                MessageBox.Show("Такая роль уже существует!");
                return;
            }

            try
            {
                using (var con = DB.Get())
                {
                    MySqlCommand cmd;

                    // Добавление
                    if (id == 0)
                    {
                        cmd = new MySqlCommand(
                            "INSERT INTO Roles(Name) VALUES(@n)", con);
                    }
                    // Редактирование
                    else
                    {
                        cmd = new MySqlCommand(
                            "UPDATE Roles SET Name=@n WHERE Id=@id", con);

                        cmd.Parameters.AddWithValue("@id", id);
                    }

                    cmd.Parameters.AddWithValue("@n", txtName.Text);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Сохранено!");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}