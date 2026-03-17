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
                    string sql = "SELECT COUNT(*) FROM Products"; // Исправлено: Products, а не product
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
        void LoadProducts()
        {
            try
            {
                // Получаем общее количество записей
                totalRecords = GetTotalRecords();

                // Вычисляем общее количество страниц
                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                if (totalPages == 0) totalPages = 1;

                // Проверяем, что текущая страница не выходит за пределы
                if (currentPage < 1) currentPage = 1;
                if (currentPage > totalPages) currentPage = totalPages;

                // Вычисляем смещение для SQL запроса
                int offset = (currentPage - 1) * pageSize;

                using (var con = DB.Get())
                {
                    // Измененный запрос с LIMIT и OFFSET для пагинации
                    var da = new MySqlDataAdapter(@"
                        SELECT
                            p.Id,
                            p.Article,
                            p.Name,
                            p.Price,
                            IFNULL(w.Quantity,0) AS Quantity,
                            p.Discount,
                            c.Name AS Category,
                            p.ImagePath
                        FROM Products p
                        JOIN Categories c ON p.CategoryId = c.Id
                        LEFT JOIN Warehouse w ON p.Id = w.ProductId
                        ORDER BY p.Name
                        LIMIT @offset, @pageSize
                    ", con);

                    da.SelectCommand.Parameters.AddWithValue("@offset", offset);
                    da.SelectCommand.Parameters.AddWithValue("@pageSize", pageSize);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgv.DataSource = dt;

                    // скрываем системные колонки
                    if (dgv.Columns["Id"] != null)
                        dgv.Columns["Id"].Visible = false;
                    if (dgv.Columns["ImagePath"] != null)
                        dgv.Columns["ImagePath"].Visible = false;

                    // названия колонок
                    if (dgv.Columns["Article"] != null)
                        dgv.Columns["Article"].HeaderText = "Артикул";
                    if (dgv.Columns["Name"] != null)
                        dgv.Columns["Name"].HeaderText = "Название";
                    if (dgv.Columns["Price"] != null)
                        dgv.Columns["Price"].HeaderText = "Цена";
                    if (dgv.Columns["Quantity"] != null)
                        dgv.Columns["Quantity"].HeaderText = "Количество";
                    if (dgv.Columns["Discount"] != null)
                        dgv.Columns["Discount"].HeaderText = "Скидка (%)";
                    if (dgv.Columns["Category"] != null)
                        dgv.Columns["Category"].HeaderText = "Категория";

                    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgv.RowTemplate.Height = 60;

                    // создаём колонку фото если нет
                    if (!dgv.Columns.Contains("PhotoColumn"))
                    {
                        DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
                        imgCol.Name = "PhotoColumn";
                        imgCol.HeaderText = "Фото";
                        imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;

                        dgv.Columns.Insert(0, imgCol);
                    }

                    // загружаем изображения
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (row.IsNewRow) continue;

                        string path = row.Cells["ImagePath"].Value?.ToString();

                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            try
                            {
                                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                                {
                                    row.Cells["PhotoColumn"].Value = Image.FromStream(fs);
                                }
                            }
                            catch
                            {
                                row.Cells["PhotoColumn"].Value = null;
                            }
                        }

                        // подсветка если товара нет
                        int qty = 0;
                        int.TryParse(row.Cells["Quantity"].Value?.ToString(), out qty);

                        if (qty == 0)
                        {
                            row.DefaultCellStyle.BackColor = Color.MistyRose;
                            row.DefaultCellStyle.ForeColor = Color.DarkRed;
                        }
                    }

                    // Обновляем информацию о страницах
                    UpdatePaginationInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        // ===== НОВЫЙ МЕТОД: Обновление информации о пагинации =====
        private void UpdatePaginationInfo()
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

            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);

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

            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);

            try
            {
                using (var con = DB.Get())
                {
                    var cmd = new MySqlCommand("DELETE FROM Products WHERE Id=@id", con);
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