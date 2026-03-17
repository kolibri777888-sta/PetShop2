using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PetShop
{
    public partial class AddEmployeeForm : Form
    {
        public AddEmployeeForm()
        {
            InitializeComponent();

            this.Shown += AddEmployeeForm_Shown;

            SetLimits();
            SetValidation();
            SetPhoneMask();
        }

        private void AddEmployeeForm_Shown(object sender, EventArgs e)
        {
            LoadRoles();
        }

        // ===============================
        // Ограничения длины
        // ===============================
        void SetLimits()
        {
            txtFullName.MaxLength = 30;
            txtLogin.MaxLength = 15;
            txtPassword.MaxLength = 30;
        }

        // ===============================
        // Маска телефона
        // ===============================
        void SetPhoneMask()
        {
            // txtPhone должен быть MaskedTextBox
            txtPhone.Mask = "+7 (000) 000-00-00";
        }

        // ===============================
        // Подключение проверок
        // ===============================
        void SetValidation()
        {
            txtFullName.KeyPress += OnlyRussianLetters;
            txtLogin.KeyPress += OnlyEnglishAndDigits;
        }

        // ===============================
        // Только русские буквы
        // ===============================
        private void OnlyRussianLetters(object sender, KeyPressEventArgs e)
        {
            if (!Regex.IsMatch(e.KeyChar.ToString(), @"[а-яА-ЯёЁ\s]") &&
                e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        // ===============================
        // Только английские буквы и цифры
        // ===============================
        private void OnlyEnglishAndDigits(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetterOrDigit(e.KeyChar) ||
                (char.IsLetter(e.KeyChar) &&
                !((e.KeyChar >= 'a' && e.KeyChar <= 'z') ||
                  (e.KeyChar >= 'A' && e.KeyChar <= 'Z'))))
            {
                if (e.KeyChar != '\b')
                    e.Handled = true;
            }
        }

        // ===============================
        // Загрузка ролей
        // ===============================
        void LoadRoles()
        {
            try
            {
                using (var con = DB.Get())
                {
                    var da = new MySqlDataAdapter(
                        "SELECT Id, Name FROM Roles ORDER BY Name", con);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cbRole.DataSource = dt;
                    cbRole.DisplayMember = "Name";
                    cbRole.ValueMember = "Id";

                    cbRole.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки ролей: " + ex.Message);
            }
        }

        // ===============================
        // Сохранение
        // ===============================
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Проверка заполнения
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtLogin.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text) ||
                !txtPhone.MaskFull ||
                cbRole.SelectedIndex == -1)
            {
                MessageBox.Show("Заполните все поля корректно!");
                return;
            }

            try
            {
                using (var con = DB.Get())
                {
                    var cmd = new MySqlCommand(@"
                        INSERT INTO Employees
                        (FullName, Phone, Login, Password, RoleId)
                        VALUES
                        (@f,@p,@l,@pass,@r)", con);

                    cmd.Parameters.AddWithValue("@f", txtFullName.Text.Trim());
                    cmd.Parameters.AddWithValue("@p", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@l", txtLogin.Text.Trim());
                    cmd.Parameters.AddWithValue("@pass", txtPassword.Text); // позже можно добавить хэш
                    cmd.Parameters.AddWithValue("@r", cbRole.SelectedValue);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Сотрудник успешно добавлен!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }
    }
}