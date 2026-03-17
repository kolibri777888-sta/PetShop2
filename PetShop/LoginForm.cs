using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace PetShop
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, System.EventArgs e)
        {
            using (var con = DB.Get())
            {
                var cmd = new MySqlCommand(@"
                SELECT r.Name
                FROM Employees e
                JOIN Roles r ON e.RoleId=r.Id
                WHERE Login=@l AND Password=@p", con);

                cmd.Parameters.AddWithValue("@l", txtLogin.Text);
                cmd.Parameters.AddWithValue("@p", txtPassword.Text);

                con.Open();

                var r = cmd.ExecuteReader();

                if (r.Read())
                {
                    string role = r.GetString(0);

                    MessageBox.Show("Вход выполнен");

                    // Открываем форму по роли
                    if (role == "Администратор")
                    {
                        new MainForm(role).Show(); // Передаем роль в конструктор MainForm
                    }
                    else if (role == "Кладовщик")
                    {
                        new StorekeeperForm().Show();
                    }
                    else if (role == "Менеджер")
                    {
                        new ManagerForm().Show();
                    }

                    Hide(); // Скрываем форму входа
                }
                else
                {
                    MessageBox.Show("Ошибка входа");
                }
            }
        }
    }
}