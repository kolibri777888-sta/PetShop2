using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PetShop
{
    public partial class ProductsForm : Form
    {
        private int currentPage = 1;
        private int totalPages = 1;
        private int totalRecords = 0;
        private int pageSize = 20;
        private DataTable fullDataTable; // для хранения всех данных при фильтрации
        private DataTable productsTable; // добавлено для хранения текущей таблицы

        // Строка подключения - замените на вашу!
        private string connStr = "Server=localhost;Database=PetShop;Uid=root;Pwd=;";

        public ProductsForm()
        {
            InitializeComponent();

            // Загружаем данные после полной загрузки формы
            this.Shown += ProductsForm_Shown;
        }

        private void ProductsForm_Shown(object sender, EventArgs e)
        {
            LoadProducts();
        }

        // ===== НОВЫЙ МЕТОД: Подсчет общего количества записей =====
        private int GetTotalRecords()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM product"; // Исправлено на product (в соответствии с первой версией)
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подсчёта записей: " + ex.Message);
                return 0;
            }
        }

        // ===============================
        // Загрузка товаров с пагинацией
        // ===============================
        private void LoadProducts()
        {
            try
            {
                // Получаем общее количество записей
                totalRecords = GetTotalRecords();
                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                // Корректируем текущую страницу
                if (currentPage < 1) currentPage = 1;
                if (currentPage > totalPages) currentPage = totalPages;

                int offset = (currentPage - 1) * pageSize;

                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT
                            p.product_id AS ID,
                            p.title AS Название,
                            p.author AS Автор,
                            p.price AS Цена,
                            c.name AS Категория,
                            p.stock_quantity AS Остаток,
                            p.image_path
                          FROM product p
                          JOIN category c ON p.category_id = c.category_id
                          ORDER BY p.title
                          LIMIT @pageSize OFFSET @offset";

                    MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                    da.SelectCommand.Parameters.AddWithValue("@pageSize", pageSize);
                    da.SelectCommand.Parameters.AddWithValue("@offset", offset);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // СОЗДАЁМ КОЛОНКУ ДЛЯ КАРТИНОК
                    DataColumn imageColumn = new DataColumn("Обложка", typeof(Image));
                    dt.Columns.Add(imageColumn);
                    imageColumn.SetOrdinal(0);

                    // ЗАГРУЖАЕМ КАРТИНКИ
                    DatabaseHelper db = new DatabaseHelper();
                    foreach (DataRow row in dt.Rows)
                    {
                        string imagePath = row["image_path"]?.ToString();
                        row["Обложка"] = db.GetProductImage(imagePath);
                    }

                    // СКРЫВАЕМ СЛУЖЕБНЫЕ ПОЛЯ
                    dt.Columns["ID"].ColumnMapping = MappingType.Hidden;
                    dt.Columns["image_path"].ColumnMapping = MappingType.Hidden;

                    // ПРИВЯЗЫВАЕМ ДАННЫЕ
                    dgv.DataSource = dt;
                    productsTable = dt;

                    // Обновляем информацию о страницах
                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке товаров: " + ex.Message);
            }
        }

        // ===== НОВЫЙ МЕТОД: Обновление информации о пагинации =====
        private void UpdatePageInfo()
        {
            // Предполагается, что у вас есть Label для отображения информации
            // Если нет - создайте через дизайнер или раскомментируйте создание

            // Ищем Label на форме
            Label lblPageInfo = this.Controls["lblPageInfo"] as Label;
            if (lblPageInfo == null)
            {
                // Если нет - создаем программно
                lblPageInfo = new Label();
                lblPageInfo.Name = "lblPageInfo";
                lblPageInfo.Location = new Point(10, 400); // Настройте позицию
                lblPageInfo.Size = new Size(200, 20);
                this.Controls.Add(lblPageInfo);
            }

            int startRecord = (currentPage - 1) * pageSize + 1;
            int endRecord = Math.Min(currentPage * pageSize, totalRecords);

            lblPageInfo.Text = $"Записи {startRecord}-{endRecord} из {totalRecords} | Страница {currentPage} из {totalPages}";

            // Обновляем состояние кнопок навигации
            Button btnPrev = this.Controls["btnPrev"] as Button;
            Button btnNext = this.Controls["btnNext"] as Button;
            Button btnFirst = this.Controls["btnFirst"] as Button;
            Button btnLast = this.Controls["btnLast"] as Button;

            if (btnPrev != null)
                btnPrev.Enabled = (currentPage > 1);
            if (btnNext != null)
                btnNext.Enabled = (currentPage < totalPages);
            if (btnFirst != null)
                btnFirst.Enabled = (currentPage > 1);
            if (btnLast != null)
                btnLast.Enabled = (currentPage < totalPages);
        }

        // ===== НОВЫЕ МЕТОДЫ: Навигация по страницам =====
        private void btnFirst_Click(object sender, EventArgs e)
        {
            currentPage = 1;
            LoadProducts();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadProducts();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadProducts();
            }
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            currentPage = totalPages;
            LoadProducts();
        }

        // ===== НОВЫЙ МЕТОД: Переход на конкретную страницу =====
        private void btnGoToPage_Click(object sender, EventArgs e)
        {
            TextBox txtPage = this.Controls["txtPage"] as TextBox;
            if (txtPage != null && int.TryParse(txtPage.Text, out int page))
            {
                if (page >= 1 && page <= totalPages)
                {
                    currentPage = page;
                    LoadProducts();
                }
                else
                {
                    MessageBox.Show($"Введите страницу от 1 до {totalPages}");
                }
            }
        }

        // ===============================
        // Добавление товара
        // ===============================
        private void btnAdd_Click(object sender, EventArgs e)
        {
            new AddProductForm().ShowDialog();
            LoadProducts();
        }

        // ===============================
        // Редактирование товара
        // ===============================
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите товар!");
                return;
            }

            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);

            new EditProductForm(id).ShowDialog();

            LoadProducts();
        }

        // ===============================
        // Удаление товара
        // ===============================
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите товар!");
                return;
            }

            if (MessageBox.Show("Удалить выбранный товар?", "Подтверждение", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["ID"].Value);

            try
            {
                using (var con = new MySqlConnection(connStr))
                {
                    var cmd = new MySqlCommand("DELETE FROM product WHERE product_id=@id", con);
                    cmd.Parameters.AddWithValue("@id", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Товар удалён!");
                LoadProducts();
            }
            catch
            {
                MessageBox.Show("Нельзя удалить: есть связанные данные!");
            }
        }
    }
}