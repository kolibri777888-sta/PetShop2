using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace PetShop  // Замените на имя вашего проекта
{
    public partial class LoginForm : Form
    {
        // ===== НОВЫЕ ПОЛЯ ДЛЯ CAPTCHA =====
        private CaptchaGenerator captchaGenerator;
        private string currentCaptchaText;
        private int failedAttempts = 0;
        private bool isBlocked = false;
        private int blockTimeRemaining = 10;
        private Timer blockTimer;

        public LoginForm()
        {
            InitializeComponent();

            // ===== ИНИЦИАЛИЗАЦИЯ CAPTCHA =====
            InitializeCaptcha();
        }

        // ===== НОВЫЙ МЕТОД: Инициализация CAPTCHA =====
        private void InitializeCaptcha()
        {
            // Создаем генератор капчи
            captchaGenerator = new CaptchaGenerator();

            // Создаем таймер для блокировки
            blockTimer = new Timer();
            blockTimer.Interval = 1000; // 1 секунда
            blockTimer.Tick += BlockTimer_Tick;

            // Изначально скрываем все элементы CAPTCHA
            ShowCaptchaControls(false);
        }

        // ===== НОВЫЙ МЕТОД: Показать/скрыть элементы CAPTCHA =====
        private void ShowCaptchaControls(bool show)
        {
            // Убедитесь, что эти контролы есть на форме!
            // Если их нет, добавьте их через дизайнер
            if (pbCaptcha != null)
                pbCaptcha.Visible = show;

            if (txtCaptcha != null)
                txtCaptcha.Visible = show;

            if (btnRefreshCaptcha != null)
                btnRefreshCaptcha.Visible = show;

            if (lblCaptcha != null)
                lblCaptcha.Visible = show;

            // Если капча показывается - генерируем новую
            if (show)
            {
                GenerateNewCaptcha();
            }
        }

        // ===== НОВЫЙ МЕТОД: Генерация новой CAPTCHA =====
        private void GenerateNewCaptcha()
        {
            currentCaptchaText = captchaGenerator.GenerateCaptchaText(4);
            pbCaptcha.Image = captchaGenerator.CreateCaptchaImage(currentCaptchaText);
        }

        // ===== НОВЫЙ МЕТОД: Обработчик кнопки обновления CAPTCHA =====
        private void btnRefreshCaptcha_Click(object sender, EventArgs e)
        {
            GenerateNewCaptcha();
            txtCaptcha.Clear();
        }

        // ===== НОВЫЙ МЕТОД: Запуск блокировки =====
        private void StartBlocking()
        {
            isBlocked = true;
            blockTimeRemaining = 10;

            // Блокируем все элементы управления
            txtLogin.Enabled = false;
            txtPassword.Enabled = false;
            txtCaptcha.Enabled = false;
            btnLogin.Enabled = false;
            btnRefreshCaptcha.Enabled = false;

            // Показываем сообщение о блокировке
            MessageBox.Show($"Система заблокирована на {blockTimeRemaining} секунд",
                           "Блокировка", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            // Запускаем таймер
            blockTimer.Start();
        }

        // ===== НОВЫЙ МЕТОД: Снятие блокировки =====
        private void StopBlocking()
        {
            isBlocked = false;
            blockTimer.Stop();

            // Разблокируем элементы управления
            txtLogin.Enabled = true;
            txtPassword.Enabled = true;
            txtCaptcha.Enabled = true;
            btnLogin.Enabled = true;
            btnRefreshCaptcha.Enabled = true;

            // Генерируем новую капчу для следующей попытки
            GenerateNewCaptcha();
            txtCaptcha.Clear();
        }

        // ===== НОВЫЙ МЕТОД: Тик таймера блокировки =====
        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            blockTimeRemaining--;

            if (blockTimeRemaining <= 0)
            {
                StopBlocking();
            }
        }

        // ===== ИЗМЕНЕННЫЙ МЕТОД: Обработчик кнопки входа =====
        private void btnLogin_Click(object sender, System.EventArgs e)
        {
            // ===== ПРОВЕРКА БЛОКИРОВКИ =====
            if (isBlocked)
            {
                MessageBox.Show($"Система заблокирована. Подождите {blockTimeRemaining} секунд.");
                return;
            }

            // ===== ПРОВЕРКА CAPTCHA =====
            if (failedAttempts >= 1) // Если капча должна быть видна
            {
                // Проверяем, ввел ли пользователь капчу
                if (string.IsNullOrWhiteSpace(txtCaptcha.Text))
                {
                    MessageBox.Show("Введите код с картинки!");
                    return;
                }

                // Сравниваем введенный текст с текущим значением (без учета регистра)
                if (txtCaptcha.Text.Trim().ToUpper() != currentCaptchaText.ToUpper())
                {
                    MessageBox.Show("Неверный код CAPTCHA!");

                    // Генерируем новую капчу
                    GenerateNewCaptcha();
                    txtCaptcha.Clear();

                    // Если это уже не первая неудачная попытка с капчей - блокируем
                    if (failedAttempts >= 2)
                    {
                        StartBlocking();
                    }

                    return; // Важно: не проверяем логин, если капча неверна
                }
            }

            // ===== ВАША СУЩЕСТВУЮЩАЯ ЛОГИКА ВХОДА (НЕ ИЗМЕНЕНА) =====
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

                    // УСПЕШНЫЙ ВХОД - сбрасываем счетчик и скрываем капчу
                    failedAttempts = 0;
                    ShowCaptchaControls(false);

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
                    // НЕУДАЧНАЯ ПОПЫТКА - увеличиваем счетчик
                    failedAttempts++;

                    MessageBox.Show("Ошибка входа");

                    // После первой неудачи показываем капчу
                    if (failedAttempts >= 1)
                    {
                        ShowCaptchaControls(true);
                    }
                }
            }
        }
    }
}