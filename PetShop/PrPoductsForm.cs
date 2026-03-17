using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PetShop
{
    public partial class PrPoductsForm : Form
    {
        public PrPoductsForm()
        {
            InitializeComponent();

            // Загружаем данные после полной отрисовки формы
            this.Shown += PrPoductsForm_Shown;
        }

        private void PrPoductsForm_Shown(object sender, EventArgs e)
        {
            LoadProducts();
        }

        // ===============================
        // Загрузка товаров с фото
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
                    p.Article AS 'Артикул',
                    p.Name AS 'Название',
                    p.Price AS 'Цена',
                    IFNULL(w.Quantity,0) AS 'Количество',
                    p.Discount AS 'Скидка (%)',
                    c.Name AS 'Категория',
                    p.ImagePath AS 'Фото'
                FROM Products p
                JOIN Categories c ON p.CategoryId = c.Id
                LEFT JOIN Warehouse w ON p.Id = w.ProductId
                ORDER BY p.Name
            ", con);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgv.DataSource = dt;

                    dgv.Columns["Id"].Visible = false;
                    dgv.Columns["Фото"].Visible = false;

                    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgv.RowTemplate.Height = 60;

                    if (!dgv.Columns.Contains("ФотоКолонка"))
                    {
                        DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
                        imgCol.Name = "ФотоКолонка";
                        imgCol.HeaderText = "Фото";
                        imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                        dgv.Columns.Insert(0, imgCol);
                    }

                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (row.IsNewRow) continue;

                        string path = row.Cells["Фото"].Value?.ToString();
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        {
                            row.Cells["ФотоКолонка"].Value = Image.FromFile(path);
                        }

                        // Подсветка если 0
                        int qty = Convert.ToInt32(row.Cells["Количество"].Value);
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
