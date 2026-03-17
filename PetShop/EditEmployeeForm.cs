using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PetShop
{
    public partial class EditEmployeeForm : Form
    {
        int id;

        public EditEmployeeForm(int id)
        {
            InitializeComponent();
            this.id = id;

            cbRole.DropDownStyle = ComboBoxStyle.DropDownList;

            LoadRoles();
            LoadEmployee();

            SetLimits();
            SetValidation();
            SetPhoneMask();
        }

        // Ограничения длины
        void SetLimits()
        {
            txtName.MaxLength = 30;
            txtLogin.MaxLength = 15;
        }

        // Маска телефона
        void SetPhoneMask()
        {
            // txtPhone должен быть MaskedTextBox
            txtPhone.Mask = "+7 (000) 000-00-00";
        }

        // Проверки ввода
        void SetValidation()
        {
            txtName.KeyPress += OnlyRussianLetters;
            txtLogin.KeyPress += OnlyEnglishAndDigits;
        }

        // Только русские буквы
        private void OnlyRussianLetters(object sender, KeyPressEventArgs e)
        {
            if (!Regex.IsMatch(e.KeyChar.ToString(), @"[а-яА-ЯёЁ\s]") &&
                e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        // Только английские буквы и цифры
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

        // Загрузка ролей
        void LoadRoles()
        {
            using (var con = DB.Get())
            {
                var da = new MySqlDataAdapter(
                    "SELECT Id, Name FROM Roles", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                cbRole.DataSource = dt;
                cbRole.DisplayMember = "Name";
                cbRole.ValueMember = "Id";
            }
        }

        // Загрузка сотрудника
        void LoadEmployee()
        {
            using (var con = DB.Get())
            {
                var cmd = new MySqlCommand(
                    "SELECT * FROM Employees WHERE Id=@id", con);

                cmd.Parameters.AddWithValue("@id", id);

                con.Open();

                var r = cmd.ExecuteReader();

                if (r.Read())
                {
                    txtName.Text = r["FullName"].ToString();
                    txtPhone.Text = r["Phone"].ToString();
                    txtLogin.Text = r["Login"].ToString();
                    txtPassword.Text = r["Password"].ToString();
                    cbRole.SelectedValue = r["RoleId"];
                }
            }
        }

        // Проверка дубликата логина
        bool IsDuplicate()
        {
            using (var con = DB.Get())
            {
                var cmd = new MySqlCommand(@"
                SELECT COUNT(*) FROM Employees
                WHERE Login=@l AND Id<>@id", con);

                cmd.Parameters.AddWithValue("@l", txtLogin.Text.Trim());
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();

                return Convert.ToInt32(
                    cmd.ExecuteScalar()) > 0;
            }
        }

        // Сохранение
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Проверка заполнения
            if (txtName.Text.Trim() == "" ||
                txtLogin.Text.Trim() == "" ||
                txtPassword.Text.Trim() == "" ||
                !txtPhone.MaskFull)
            {
                MessageBox.Show("Заполните все поля корректно!");
                return;
            }

            // Проверка дубликата
            if (IsDuplicate())
            {
                MessageBox.Show("Логин уже существует!");
                return;
            }

            try
            {
                using (var con = DB.Get())
                {
                    var cmd = new MySqlCommand(@"
                    UPDATE Employees SET
                        FullName=@n,
                        Phone=@p,
                        Login=@l,
                        Password=@pw,
                        RoleId=@r
                    WHERE Id=@id", con);

                    cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
                    cmd.Parameters.AddWithValue("@p", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@l", txtLogin.Text.Trim());
                    cmd.Parameters.AddWithValue("@pw", txtPassword.Text);
                    cmd.Parameters.AddWithValue("@r", cbRole.SelectedValue);
                    cmd.Parameters.AddWithValue("@id", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Сотрудник обновлён!");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}
