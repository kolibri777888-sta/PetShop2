using MySql.Data.MySqlClient;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PetShop
{
    public partial class AddSupplierForm : Form
    {
        public AddSupplierForm()
        {
            InitializeComponent();

            // Ограничение длины имени
            txtName.MaxLength = 15;

            // Обработчик ввода только букв
            txtName.KeyPress += TxtName_KeyPress;

            // Настройка маски для телефона
            // Пример для формата +7 (999) 999-99-99
            txtPhone.Mask = "+7 (999) 000-00-00";
        }

        // Метод для проверки ввода в поле "Имя"
        private void TxtName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только буквы русского и английского алфавита, а также клавиши управления (Backspace)
            if (!char.IsControl(e.KeyChar) && !Regex.IsMatch(e.KeyChar.ToString(), @"[a-zA-Zа-яА-Я]"))
            {
                e.Handled = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtName.Text == "")
            {
                MessageBox.Show("Введите название!");
                return;
            }

            try
            {
                using (var con = DB.Get())
                {
                    var cmd = new MySqlCommand(@"
                    INSERT INTO Suppliers
                    (Name, Phone)
                    VALUES
                    (@n,@p)", con);

                    cmd.Parameters.AddWithValue("@n", txtName.Text);
                    cmd.Parameters.AddWithValue("@p", txtPhone.Text);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Поставщик добавлен!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}