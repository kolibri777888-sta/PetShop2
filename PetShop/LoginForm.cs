using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace PetShop
{
    public partial class LoginForm : Form
    {
        // ===== ПОЛЯ ДЛЯ CAPTCHA =====
        private CaptchaGenerator captchaGenerator;
        private string currentCaptchaText;
        private int failedAttempts = 0;
        private bool isBlocked = false;
        private int blockTimeRemaining = 10;
        private Timer blockTimer;

        public LoginForm()
        {
            InitializeComponent();
            InitializeCaptcha();

            // ===== ПРИВЯЗКА ОБРАБОТЧИКА КНОПКИ ОБНОВЛЕНИЯ =====
            // Added: эта строка привязывает событие Click к методу btnRefreshCaptcha_Click
            this.btnRefreshCaptcha.Click += new System.EventHandler(this.btnRefreshCaptcha_Click);
        }

        // ===== ИНИЦИАЛИЗАЦИЯ CAPTCHA =====
        private void InitializeCaptcha()
        {
            captchaGenerator = new CaptchaGenerator();

            blockTimer = new Timer();
            blockTimer.Interval = 1000;
            blockTimer.Tick += BlockTimer_Tick;

            ShowCaptchaControls(false);
        }

        // ===== ПОКАЗАТЬ/СКРЫТЬ ЭЛЕМЕНТЫ CAPTCHA =====
        private void ShowCaptchaControls(bool show)
        {
            if (pbCaptcha != null)
                pbCaptcha.Visible = show;

            if (txtCaptcha != null)
                txtCaptcha.Visible = show;

            if (btnRefreshCaptcha != null)
                btnRefreshCaptcha.Visible = show;

            if (lblCaptcha != null)
                lblCaptcha.Visible = show;

            if (show)
            {
                GenerateNewCaptcha();
            }
        }

        // ===== ГЕНЕРАЦИЯ НОВОЙ CAPTCHA =====
        private void GenerateNewCaptcha()
        {
            currentCaptchaText = captchaGenerator.GenerateCaptchaText(4);
            pbCaptcha.Image = captchaGenerator.CreateCaptchaImage(currentCaptchaText);
        }

        // ===== ОБНОВЛЕНИЕ CAPTCHA =====
        private void btnRefreshCaptcha_Click(object sender, EventArgs e)
        {
            GenerateNewCaptcha();
            txtCaptcha.Clear();
        }

        // ===== ЗАПУСК БЛОКИРОВКИ =====
        private void StartBlocking()
        {
            isBlocked = true;
            blockTimeRemaining = 10;

            txtLogin.Enabled = false;
            txtPassword.Enabled = false;
            txtCaptcha.Enabled = false;
            btnLogin.Enabled = false;
            btnRefreshCaptcha.Enabled = false;

            MessageBox.Show($"Система заблокирована на {blockTimeRemaining} секунд",
                           "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            blockTimer.Start();
        }

        // ===== СНЯТИЕ БЛОКИРОВКИ =====
        private void StopBlocking()
        {
            isBlocked = false;
            blockTimer.Stop();

            txtLogin.Enabled = true;
            txtPassword.Enabled = true;
            txtCaptcha.Enabled = true;
            btnLogin.Enabled = true;
            btnRefreshCaptcha.Enabled = true;

            GenerateNewCaptcha();
            txtCaptcha.Clear();
        }

        // ===== ТАЙМЕР БЛОКИРОВКИ =====
        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            blockTimeRemaining--;

            if (blockTimeRemaining <= 0)
            {
                StopBlocking();
            }
        }

        // ===== ОСНОВНОЙ МЕТОД ВХОДА =====
        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Проверка блокировки
            if (isBlocked)
            {
                MessageBox.Show($"Система заблокирована. Подождите {blockTimeRemaining} секунд.");
                return;
            }

            // Проверка CAPTCHA (если нужно)
            if (failedAttempts >= 1)
            {
                if (string.IsNullOrWhiteSpace(txtCaptcha.Text))
                {
                    MessageBox.Show("Введите код с картинки!");
                    return;
                }

                if (txtCaptcha.Text.Trim().ToUpper() != currentCaptchaText.ToUpper())
                {
                    MessageBox.Show("Неверный код CAPTCHA!");

                    GenerateNewCaptcha();
                    txtCaptcha.Clear();

                    if (failedAttempts >= 2)
                    {
                        StartBlocking();
                    }

                    return;
                }
            }

            // Проверка логина и пароля
            try
            {
                using (var con = DB.Get())
                {
                    // Проверка admin/admin (из задания)
                    if (txtLogin.Text == "admin" && txtPassword.Text == "admin")
                    {
                        // Успешный вход для admin
                        failedAttempts = 0;
                        ShowCaptchaControls(false);

                        MessageBox.Show("Вход выполнен как администратор");

                        // Открываем форму для админа (например, ImportForm)
                        // new ImportForm().Show();
                        // this.Hide();
                        return;
                    }

                    // Проверка в БД
                    var cmd = new MySqlCommand(@"
                        SELECT r.Name
                        FROM Employees e
                        JOIN Roles r ON e.RoleId = r.Id
                        WHERE e.Login = @l AND e.Password = @p", con);

                    cmd.Parameters.AddWithValue("@l", txtLogin.Text);
                    cmd.Parameters.AddWithValue("@p", txtPassword.Text);

                    con.Open();
                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string role = reader.GetString(0);

                        // Успешный вход
                        failedAttempts = 0;
                        ShowCaptchaControls(false);

                        MessageBox.Show($"Вход выполнен. Роль: {role}");

                        // Открываем форму по роли
                        switch (role)
                        {
                            case "Администратор":
                                new MainForm(role).Show();
                                break;
                            case "Кладовщик":
                                new StorekeeperForm().Show();
                                break;
                            case "Менеджер":
                                new ManagerForm().Show();
                                break;
                            default:
                                MessageBox.Show("Неизвестная роль!");
                                return;
                        }

                        this.Hide();
                    }
                    else
                    {
                        // Неудачная попытка
                        failedAttempts++;

                        MessageBox.Show("Неверный логин или пароль!");

                        if (failedAttempts >= 1)
                        {
                            ShowCaptchaControls(true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}");
            }
        }
    }
}