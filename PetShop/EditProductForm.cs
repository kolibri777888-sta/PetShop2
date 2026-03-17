using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace PetShop
{
    public partial class EditProductForm : Form
    {
        int id;
        string imagePath = ""; // путь к картинке

        public EditProductForm(int id)
        {
            InitializeComponent();
            this.id = id;

            cbCategory.DropDownStyle = ComboBoxStyle.DropDownList;

            LoadCategories();
            LoadProduct();

            SetLimits();
            SetValidation();
        }

        void SetLimits()
        {
            txtArticle.MaxLength = 15;
            txtName.MaxLength = 50;
            txtPrice.MaxLength = 15;
            txtQuantity.MaxLength = 15;
            txtDiscount.MaxLength = 15;
        }

        void SetValidation()
        {
            txtPrice.KeyPress += OnlyDigitsAndComma;
            txtQuantity.KeyPress += OnlyDigits;
            txtDiscount.KeyPress += OnlyDigits;

            txtName.KeyPress += OnlyLetters;
            txtArticle.KeyPress += LettersAndDigits;
        }

        void LoadCategories()
        {
            using (var con = DB.Get())
            {
                var da = new MySqlDataAdapter("SELECT Id, Name FROM Categories", con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cbCategory.DataSource = dt;
                cbCategory.DisplayMember = "Name";
                cbCategory.ValueMember = "Id";
            }
        }

        void LoadProduct()
        {
            using (var con = DB.Get())
            {
                var cmd = new MySqlCommand(@"
SELECT p.*, IFNULL(w.Quantity,0) as Quantity
FROM Products p
LEFT JOIN Warehouse w ON p.Id = w.ProductId
WHERE p.Id=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();

                var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    txtArticle.Text = r["Article"].ToString();
                    txtName.Text = r["Name"].ToString();
                    txtPrice.Text = r["Price"].ToString();
                    txtQuantity.Text = r["Quantity"].ToString();
                    txtDiscount.Text = r["Discount"].ToString();

                    cbCategory.SelectedValue = r["CategoryId"];

                    imagePath = r["ImagePath"].ToString();
                    if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                    {
                        using (var imgTemp = Image.FromFile(imagePath))
                        {
                            pictureBox1.Image = new Bitmap(imgTemp);
                            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                        }
                    }
                }
            }
        }

        bool IsDuplicate()
        {
            using (var con = DB.Get())
            {
                var cmd = new MySqlCommand(@"
                    SELECT COUNT(*) FROM Products
                    WHERE (Article=@a OR Name=@n) AND Id<>@id", con);

                cmd.Parameters.AddWithValue("@a", txtArticle.Text);
                cmd.Parameters.AddWithValue("@n", txtName.Text);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private void OnlyDigits(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b') e.Handled = true;
        }

        private void OnlyDigitsAndComma(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != ',') e.Handled = true;
        }

        private void OnlyLetters(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != ' ') e.Handled = true;
        }

        private void LettersAndDigits(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != '\b') e.Handled = true;
        }

        // Кнопка выбрать картинку
        private void btnChooseImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Выберите изображение";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    imagePath = ofd.FileName;
                    using (var imgTemp = Image.FromFile(imagePath))
                    {
                        pictureBox1.Image = new Bitmap(imgTemp); // копия в памяти
                        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                }
            }
        }

        // Кнопка удалить картинку
        private void btnDeleteImage_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            imagePath = ""; // при сохранении в базу запишется NULL
        }

        // Кнопка сохранить изменения
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtArticle.Text == "" || txtName.Text == "" || txtPrice.Text == "" || txtQuantity.Text == "")
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            if (!decimal.TryParse(txtPrice.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
            {
                MessageBox.Show("Цена должна быть числом!");
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity))
            {
                MessageBox.Show("Количество должно быть числом!");
                return;
            }

            if (!int.TryParse(txtDiscount.Text == "" ? "0" : txtDiscount.Text, out int discount))
            {
                MessageBox.Show("Скидка должна быть числом!");
                return;
            }

            if (IsDuplicate())
            {
                MessageBox.Show("Такой товар уже существует!");
                return;
            }

            if (MessageBox.Show("Сохранить изменения?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            try
            {
                string dbImagePath = null;

                if (!string.IsNullOrEmpty(imagePath))
                {
                    string folder = Path.Combine(Application.StartupPath, "Images", "Products");
                    Directory.CreateDirectory(folder);

                    string fileName = Path.GetFileName(imagePath);
                    string destPath = Path.Combine(folder, fileName);

                    // Копируем только если файла ещё нет
                    if (!File.Exists(destPath))
                    {
                        File.Copy(imagePath, destPath, true);
                    }

                    dbImagePath = Path.Combine("Images", "Products", fileName);
                }

                using (var con = DB.Get())
                {
                    con.Open();

                    // Обновляем товар
                    var cmd = new MySqlCommand(@"
        UPDATE Products SET
            Article=@a,
            Name=@n,
            Price=@p,
            Discount=@d,
            CategoryId=@c,
            ImagePath=@img
        WHERE Id=@id", con);

                    cmd.Parameters.AddWithValue("@a", txtArticle.Text);
                    cmd.Parameters.AddWithValue("@n", txtName.Text);
                    cmd.Parameters.AddWithValue("@p", price);
                    cmd.Parameters.AddWithValue("@d", discount);
                    cmd.Parameters.AddWithValue("@c", cbCategory.SelectedValue);
                    cmd.Parameters.AddWithValue("@img", (object)dbImagePath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();

                    // Обновляем склад
                    var stockCmd = new MySqlCommand(@"
        UPDATE Warehouse
        SET Quantity=@q
        WHERE ProductId=@id", con);

                    stockCmd.Parameters.AddWithValue("@q", quantity);
                    stockCmd.Parameters.AddWithValue("@id", id);

                    stockCmd.ExecuteNonQuery();
                }

                MessageBox.Show("Товар обновлён!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}
