using MySql.Data.MySqlClient;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PetShop
{
    public partial class EditSupplierForm : Form
    {
        int id;

        public EditSupplierForm(int id)
        {
            InitializeComponent();
            this.id = id;

            // Ограничение длины имени
            txtName.MaxLength = 15;

            // Обработчик ввода только букв
            txtName.KeyPress += TxtName_KeyPress;

            // Маска для телефона
            txtPhone.Mask = "+7 (999) 000-00-00";

            LoadData();
        }

        void LoadData()
        {
            using (var con = DB.Get())
            {
                var cmd = new MySqlCommand(
                    "SELECT * FROM Suppliers WHERE Id=@id", con);

                cmd.Parameters.AddWithValue("@id", id);

                con.Open();

                var r = cmd.ExecuteReader();

                if (r.Read())
                {
                    txtName.Text = r["Name"].ToString();
                    txtPhone.Text = r["Phone"].ToString();
                }
            }
        }

        // Метод для проверки ввода в поле "Имя"
        private void TxtName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только буквы русского и английского алфавита и клавиши управления
            if (!char.IsControl(e.KeyChar) && !Regex.IsMatch(e.KeyChar.ToString(), @"[a-zA-Zа-яА-Я]"))
            {
                e.Handled = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Проверка, что имя введено
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название!");
                return;
            }

            // Проверка, что телефон полностью введен
            if (!txtPhone.MaskFull)
            {
                MessageBox.Show("Введите корректный телефон!");
                return;
            }

            try
            {
                using (var con = DB.Get())
                {
                    var cmd = new MySqlCommand(@"
                    UPDATE Suppliers SET
                    Name=@n, Phone=@p
                    WHERE Id=@id", con);

                    cmd.Parameters.AddWithValue("@n", txtName.Text);
                    cmd.Parameters.AddWithValue("@p", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@id", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Поставщик обновлён");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}