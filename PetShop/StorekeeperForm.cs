using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace PetShop
{
    public partial class StorekeeperForm : Form
    {
        DataTable warehouseTable = new DataTable();

        public StorekeeperForm()
        {
            InitializeComponent();

            LoadWarehouse();
            LoadCategories();

            dgvWarehouse.CellFormatting += DgvWarehouse_CellFormatting;

            txtSearch.TextChanged += FilterData;
            cmbCategory.SelectedIndexChanged += FilterData;
        }

        // ===============================
        // ЗАГРУЗКА СКЛАДА
        // ===============================
        void LoadWarehouse()
        {
            using (var c = DB.Get())
            {
                c.Open();

                var da = new MySqlDataAdapter(@"
SELECT
    w.Id,
    p.Id as ProductId,
    p.Article,
    p.Name,
    c.Name as Category,
    p.Price,
    w.Location,
    w.Quantity
FROM Warehouse w
JOIN Products p ON p.Id = w.ProductId
JOIN Categories c ON c.Id = p.CategoryId", c);

                warehouseTable.Clear();
                da.Fill(warehouseTable);

                dgvWarehouse.DataSource = warehouseTable;
            }

            dgvWarehouse.Columns["Id"].Visible = false;
            dgvWarehouse.Columns["ProductId"].Visible = false;
        }

        // ===============================
        // ЗАГРУЗКА КАТЕГОРИЙ
        // ===============================
        void LoadCategories()
        {
            using (var c = DB.Get())
            {
                c.Open();

                var da = new MySqlDataAdapter(
                    "SELECT Id, Name FROM Categories", c);

                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbCategory.DataSource = dt;
                cmbCategory.DisplayMember = "Name";
                cmbCategory.ValueMember = "Id";

                cmbCategory.SelectedIndex = -1;
            }
        }

        // ===============================
        // ПОДСВЕТКА ОСТАТКОВ
        // ===============================
        private void DgvWarehouse_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvWarehouse.Columns[e.ColumnIndex].Name == "Quantity")
            {
                int qty = Convert.ToInt32(e.Value);

                if (qty == 0)
                    e.CellStyle.BackColor = Color.Red;
                else if (qty < 5)
                    e.CellStyle.BackColor = Color.Yellow;
                else
                    e.CellStyle.BackColor = Color.LightGreen;
            }
        }

        // ===============================
        // ПОИСК И ФИЛЬТР
        // ===============================
        void FilterData(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Replace("'", "''");

            string filter = "";

            if (!string.IsNullOrEmpty(search))
            {
                filter += $"Article LIKE '%{search}%' OR Name LIKE '%{search}%'";
            }

            if (cmbCategory.SelectedIndex != -1)
            {
                if (filter != "")
                    filter += " AND ";

                filter += $"Category = '{cmbCategory.Text}'";
            }

            (dgvWarehouse.DataSource as DataTable).DefaultView.RowFilter = filter;
        }

        // ===============================
        // ЖУРНАЛ ОПЕРАЦИЙ
        // ===============================
        void AddOperation(int productId, string type, int qty, string comment)
        {
            using (var c = DB.Get())
            {
                c.Open();

                var cmd = new MySqlCommand(@"
INSERT INTO WarehouseOperations
(ProductId, OperationType, Quantity, OperationDate, Comment)
VALUES(@p,@t,@q,NOW(),@c)", c);

                cmd.Parameters.AddWithValue("@p", productId);
                cmd.Parameters.AddWithValue("@t", type);
                cmd.Parameters.AddWithValue("@q", qty);
                cmd.Parameters.AddWithValue("@c", comment);

                cmd.ExecuteNonQuery();
            }
        }

        // ===============================
        // ПРИХОД ТОВАРА
        // ===============================
        private void btnIncoming_Click(object sender, EventArgs e)
        {
            if (dgvWarehouse.CurrentRow == null)
                return;

            int id = Convert.ToInt32(dgvWarehouse.CurrentRow.Cells["Id"].Value);
            int productId = Convert.ToInt32(dgvWarehouse.CurrentRow.Cells["ProductId"].Value);
            int current = Convert.ToInt32(dgvWarehouse.CurrentRow.Cells["Quantity"].Value);
            int incoming = (int)numIncoming.Value;

            if (incoming <= 0)
            {
                MessageBox.Show("Введите количество больше 0");
                return;
            }

            using (var c = DB.Get())
            {
                c.Open();

                var cmd = new MySqlCommand(
                    "UPDATE Warehouse SET Quantity=@q WHERE Id=@id", c);

                cmd.Parameters.AddWithValue("@q", current + incoming);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }

            AddOperation(productId, "Приход", incoming, "Поступление товара");

            LoadWarehouse();
        }

        // ===============================
        // РАЗМЕЩЕНИЕ НА СКЛАДЕ
        // ===============================
        private void btnMove_Click(object sender, EventArgs e)
        {
            if (dgvWarehouse.CurrentRow == null)
                return;

            int id = Convert.ToInt32(dgvWarehouse.CurrentRow.Cells["Id"].Value);
            int productId = Convert.ToInt32(dgvWarehouse.CurrentRow.Cells["ProductId"].Value);

            string newLocation = txtLocation.Text.Trim();

            if (newLocation == "")
            {
                MessageBox.Show("Введите ячейку склада");
                return;
            }

            using (var c = DB.Get())
            {
                c.Open();

                var cmd = new MySqlCommand(
                    "UPDATE Warehouse SET Location=@loc WHERE Id=@id", c);

                cmd.Parameters.AddWithValue("@loc", newLocation);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }

            AddOperation(productId, "Размещение", 0, "Ячейка: " + newLocation);

            LoadWarehouse();
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            cmbCategory.SelectedIndex = -1;

            (dgvWarehouse.DataSource as DataTable).DefaultView.RowFilter = "";
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            // Показать окно с подтверждением
            var result = MessageBox.Show(
                "Точно хотите выйти?", // текст
                "Подтверждение", // заголовок окна
                MessageBoxButtons.YesNo, // кнопки Да/Нет
                MessageBoxIcon.Question // иконка вопроса
            );

            if (result == DialogResult.Yes)
            {
                // Создаем и показываем другую форму
                var mainForm = new LoginForm();
                mainForm.Show();

                // Закрываем текущую форму
                this.Close();
            }
        }
    }
}
