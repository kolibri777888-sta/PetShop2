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

        // ===============================
        // Загрузка товаров
        // ===============================
        void LoadProducts()
        {
            try
            {
                using (var con = DB.Get())
                {
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
                    ", con);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgv.DataSource = dt;

                    // скрываем системные колонки
                    dgv.Columns["Id"].Visible = false;
                    dgv.Columns["ImagePath"].Visible = false;

                    // названия колонок
                    dgv.Columns["Article"].HeaderText = "Артикул";
                    dgv.Columns["Name"].HeaderText = "Название";
                    dgv.Columns["Price"].HeaderText = "Цена";
                    dgv.Columns["Quantity"].HeaderText = "Количество";
                    dgv.Columns["Discount"].HeaderText = "Скидка (%)";
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
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