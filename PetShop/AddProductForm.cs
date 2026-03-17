using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PetShop
{
    public partial class AddProductForm : Form
    {
        string imagePath = ""; // путь к выбранной картинке

        public AddProductForm()
        {
            InitializeComponent();
            LoadCategories();

            SetLimits();
            SetValidation();
        }

        // Ограничение длины
        void SetLimits()
        {
            txtArticle.MaxLength = 15;
            txtName.MaxLength = 50;
            txtPrice.MaxLength = 15;
            txtQuantity.MaxLength = 15;
            txtDiscount.MaxLength = 15;
        }

        // Подключение проверок
        void SetValidation()
        {
            txtPrice.KeyPress += OnlyDigits;
            txtQuantity.KeyPress += OnlyDigits;
            txtDiscount.KeyPress += OnlyDigits;

            txtName.KeyPress += OnlyLetters;

            txtArticle.KeyPress += LettersAndDigits;
        }

        // Загрузка категорий
        void LoadCategories()
        {
            using (var con = DB.Get())
            {
                var da = new MySqlDataAdapter(
                    "SELECT Id, Name FROM Categories", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                cbCategory.DataSource = dt;
                cbCategory.DisplayMember = "Name";
                cbCategory.ValueMember = "Id";
            }
        }

        // Только цифры
        private void OnlyDigits(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
                e.Handled = true;
        }

        // Только буквы
        private void OnlyLetters(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != ' ')
                e.Handled = true;
        }

        // Буквы + цифры
        private void LettersAndDigits(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != '\b')
                e.Handled = true;
        }

        // ===============================
        // Кнопка выбрать фото
        // ===============================
        private void btnChooseImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Выберите изображение";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    imagePath = ofd.FileName;
                    pictureBox1.Image = Image.FromFile(imagePath);
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        // ===============================
        // Сохранение товара
        // ===============================
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtName.Text == "" || txtPrice.Text == "" || txtQuantity.Text == "")
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) ||
                !int.TryParse(txtQuantity.Text, out int quantity) ||
                !int.TryParse(txtDiscount.Text == "" ? "0" : txtDiscount.Text, out int discount))
            {
                MessageBox.Show("Некорректные данные!");
                return;
            }

            try
            {
                string dbImagePath = "";

                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    string folder = Path.Combine(Application.StartupPath, "Images", "Products");
                    Directory.CreateDirectory(folder);

                    string fileName = Path.GetFileName(imagePath);
                    string destPath = Path.Combine(folder, fileName);

                    File.Copy(imagePath, destPath, true);
                    dbImagePath = Path.Combine("Images", "Products", fileName);
                }

                using (var con = DB.Get())
                {
                    con.Open();

                    // 1️⃣ Добавляем товар
                    var cmd = new MySqlCommand(@"
                INSERT INTO Products
                (Article, Name, Price, Discount, CategoryId, ImagePath)
                VALUES
                (@a,@n,@p,@d,@c,@img);
                SELECT LAST_INSERT_ID();", con);

                    cmd.Parameters.AddWithValue("@a", txtArticle.Text);
                    cmd.Parameters.AddWithValue("@n", txtName.Text);
                    cmd.Parameters.AddWithValue("@p", price);
                    cmd.Parameters.AddWithValue("@d", discount);
                    cmd.Parameters.AddWithValue("@c", cbCategory.SelectedValue);
                    cmd.Parameters.AddWithValue("@img", dbImagePath);

                    int productId = Convert.ToInt32(cmd.ExecuteScalar());

                    // 2️⃣ Создаём запись на складе
                    var stockCmd = new MySqlCommand(@"
                INSERT INTO Warehouse (ProductId, Location, Quantity)
                VALUES (@id, 'Основной склад', @q)", con);

                    stockCmd.Parameters.AddWithValue("@id", productId);
                    stockCmd.Parameters.AddWithValue("@q", quantity);
                    stockCmd.ExecuteNonQuery();
                }

                MessageBox.Show("Товар успешно добавлен!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}