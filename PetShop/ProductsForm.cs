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
        private DataTable fullDataTable;

        public ProductsForm()
        {
            InitializeComponent();

            // Проверяем подключение при загрузке формы
            this.Load += ProductsForm_Load;
            this.Shown += ProductsForm_Shown;
        }

        private void ProductsForm_Load(object sender, EventArgs e)
        {
            // Проверяем подключение к БД
            if (!DB.TestConnection())
            {
                MessageBox.Show("Не удалось подключиться к базе данных. Проверьте настройки подключения.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProductsForm_Shown(object sender, EventArgs e)
        {
            LoadProducts();
            LoadCategories(); // Загружаем категории для фильтрации
        }

        // ===============================
        // Загрузка категорий для фильтрации
        // ===============================
        private void LoadCategories()
        {
            try
            {
                using (var con = DB.Get())
                {
                    string sql = "SELECT Name FROM Categories ORDER BY Name";
                    using (var da = new MySqlDataAdapter(sql, con))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Добавляем пустой элемент для сброса фильтра
                        DataRow emptyRow = dt.NewRow();
                        emptyRow["Name"] = "Все категории";
                        dt.Rows.InsertAt(emptyRow, 0);

                        comboCategory.DisplayMember = "Name";
                        comboCategory.ValueMember = "Name";
                        comboCategory.DataSource = dt;
                        comboCategory.SelectedIndex = 0; // Выбираем "Все категории"
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message);
            }
        }

        // ===============================
        // Подсчет общего количества записей
        // ===============================
        private int GetTotalRecords()
        {
            try
            {
                using (var con = DB.Get())
                {
                    string sql = "SELECT COUNT(*) FROM Products";
                    using (var cmd = new MySqlCommand(sql, con))
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
                totalRecords = GetTotalRecords();
                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                if (totalPages == 0) totalPages = 1;
                if (currentPage < 1) currentPage = 1;
                if (currentPage > totalPages) currentPage = totalPages;

                int offset = (currentPage - 1) * pageSize;

                using (var con = DB.Get())
                {
                    string sql = @"
                        SELECT
                            p.Id,
                            p.Article,
                            p.Name,
                            p.Price,
                            IFNULL(w.Quantity, 0) AS Quantity,
                            p.Discount,
                            c.Name AS Category,
                            p.ImagePath
                        FROM Products p
                        JOIN Categories c ON p.CategoryId = c.Id
                        LEFT JOIN Warehouse w ON p.Id = w.ProductId
                        ORDER BY p.Name
                        LIMIT @pageSize OFFSET @offset";

                    using (var cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@pageSize", pageSize);
                        cmd.Parameters.AddWithValue("@offset", offset);

                        using (var da = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            dgv.DataSource = dt;
                            fullDataTable = dt;
                        }
                    }

                    ConfigureDataGridView();
                    LoadImages();
                    UpdatePageInfo();
                    HighlightRows();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки товаров: " + ex.Message);
            }
        }

        // ===============================
        // Настройка DataGridView
        // ===============================
        private void ConfigureDataGridView()
        {
            // Скрываем системные колонки
            if (dgv.Columns["Id"] != null)
                dgv.Columns["Id"].Visible = false;

            if (dgv.Columns["ImagePath"] != null)
                dgv.Columns["ImagePath"].Visible = false;

            // Названия колонок
            if (dgv.Columns["Article"] != null)
                dgv.Columns["Article"].HeaderText = "Артикул";

            if (dgv.Columns["Name"] != null)
                dgv.Columns["Name"].HeaderText = "Название";

            if (dgv.Columns["Price"] != null)
            {
                dgv.Columns["Price"].HeaderText = "Цена";
                dgv.Columns["Price"].DefaultCellStyle.Format = "C2";
            }

            if (dgv.Columns["Quantity"] != null)
                dgv.Columns["Quantity"].HeaderText = "Количество";

            if (dgv.Columns["Discount"] != null)
            {
                dgv.Columns["Discount"].HeaderText = "Скидка (%)";
                dgv.Columns["Discount"].DefaultCellStyle.Format = "0'%'";
            }

            if (dgv.Columns["Category"] != null)
                dgv.Columns["Category"].HeaderText = "Категория";

            // Создаем колонку для фото если её нет
            if (!dgv.Columns.Contains("PhotoColumn"))
            {
                DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
                imgCol.Name = "PhotoColumn";
                imgCol.HeaderText = "Фото";
                imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                imgCol.Width = 80;
                dgv.Columns.Insert(0, imgCol);
            }

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowTemplate.Height = 60;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
        }

        // ===============================
        // Загрузка изображений
        // ===============================
        private void LoadImages()
        {
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
                        // Если не удалось загрузить изображение, ставим заглушку
                        row.Cells["PhotoColumn"].Value = null;
                    }
                }
                else
                {
                    // Если нет изображения, ставим null
                    row.Cells["PhotoColumn"].Value = null;
                }
            }
        }

        // ===============================
        // Подсветка строк
        // ===============================
        private void HighlightRows()
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;

                if (row.Cells["Quantity"].Value != null)
                {
                    int qty = Convert.ToInt32(row.Cells["Quantity"].Value);

                    if (qty == 0)
                    {
                        row.DefaultCellStyle.BackColor = Color.MistyRose;
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    }
                    else if (qty < 5)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightCoral;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                }
            }
        }

        // ===============================
        // Обновление информации о страницах
        // ===============================
        private void UpdatePageInfo()
        {
            if (lblPageInfo != null)
                lblPageInfo.Text = $"Страница {currentPage} из {totalPages}";

            if (lblRecordInfo != null && totalRecords > 0)
            {
                int startRecord = (currentPage - 1) * pageSize + 1;
                int endRecord = Math.Min(currentPage * pageSize, totalRecords);
                lblRecordInfo.Text = $"{startRecord}-{endRecord} из {totalRecords}";
            }
            else if (lblRecordInfo != null)
            {
                lblRecordInfo.Text = "Нет записей";
            }

            // Включаем/выключаем кнопки
            if (btnFirstPage != null)
            {
                btnFirstPage.Enabled = currentPage > 1 && totalPages > 0;
                btnPrevPage.Enabled = currentPage > 1 && totalPages > 0;
                btnNextPage.Enabled = currentPage < totalPages && totalPages > 0;
                btnLastPage.Enabled = currentPage < totalPages && totalPages > 0;
            }
        }

        // ===============================
        // Применение фильтров
        // ===============================
        private void ApplyFilters()
        {
            if (fullDataTable == null) return;

            string filter = "";

            // Фильтр по поиску
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                filter += $"Name LIKE '%{txtSearch.Text}%'";
            }

            // Фильтр по категории
            if (comboCategory.SelectedIndex > 0) // Не "Все категории"
            {
                if (!string.IsNullOrWhiteSpace(filter))
                    filter += " AND ";
                filter += $"Category = '{comboCategory.Text}'";
            }

            DataTable dt = (DataTable)dgv.DataSource;
            dt.DefaultView.RowFilter = filter;

            // Пересчитываем количество страниц для отфильтрованных данных
            int filteredCount = dt.DefaultView.Count;
            totalPages = (int)Math.Ceiling((double)filteredCount / pageSize);
            if (totalPages == 0) totalPages = 1;
            currentPage = 1;

            UpdatePageInfo();
            HighlightRows();
        }

        // ===============================
        // Обработчики фильтрации
        // ===============================
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void comboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        // ===============================
        // Сброс фильтров
        // ===============================
        private void btnClearFilter_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            comboCategory.SelectedIndex = 0;
            LoadProducts();
        }

        // ===============================
        // Навигация по страницам
        // ===============================
        private void btnFirstPage_Click(object sender, EventArgs e)
        {
            currentPage = 1;
            LoadProducts();
        }

        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadProducts();
            }
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadProducts();
            }
        }

        private void btnLastPage_Click(object sender, EventArgs e)
        {
            currentPage = totalPages;
            LoadProducts();
        }

        // ===============================
        // CRUD операции
        // ===============================
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Открыть форму добавления
                // AddProductForm addForm = new AddProductForm();
                // addForm.ShowDialog();
                MessageBox.Show("Форма добавления товара", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при открытии формы добавления: " + ex.Message);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите товар для редактирования!",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);
                // Открыть форму редактирования с id
                // EditProductForm editForm = new EditProductForm(id);
                // editForm.ShowDialog();
                MessageBox.Show($"Редактирование товара с ID: {id}", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при открытии формы редактирования: " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите товар для удаления!",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Вы действительно хотите удалить выбранный товар?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);

            try
            {
                using (var con = DB.Get())
                {
                    string sql = "DELETE FROM Products WHERE Id = @id";
                    using (var cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Товар успешно удалён!", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadProducts();
                        }
                        else
                        {
                            MessageBox.Show("Товар не найден!", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1451) // Foreign key constraint
                {
                    MessageBox.Show("Невозможно удалить товар, так как он используется в заказах!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Ошибка MySQL: " + ex.Message, "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===============================
        // Обновление данных
        // ===============================
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }
    }
}