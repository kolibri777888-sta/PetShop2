using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace PetShop
{
    public partial class ZakazForm : Form
    {
        private int CurrentEmployeeId = 2; // текущий сотрудник
        private System.Windows.Forms.Timer searchTimer;

        public ZakazForm(int currentEmployeeId = 2)
        {
            InitializeComponent();
            CurrentEmployeeId = currentEmployeeId;

            // Настройка таймера для автопоиска
            searchTimer = new System.Windows.Forms.Timer();
            searchTimer.Interval = 300; // миллисекунды задержки
            searchTimer.Tick += SearchTimer_Tick;

            LoadData();
        }

        // ================================
        // ЗАГРУЗКА ДАННЫХ С ГРУППИРОВКОЙ
        // ================================
        void LoadData(string search = "")
        {
            using (var con = DB.Get())
            {
                string sql = @"
SELECT 
    o.Id,
    o.OrderCode AS 'Код заказа',
    o.OrderDate AS 'Дата',
    o.Status AS 'Статус',
    e.FullName AS 'Сотрудник',
    p.Article AS 'Артикул',
    p.Name AS 'Товар',
    oi.Price AS 'Цена',
    oi.Quantity AS 'Количество',
    oi.Discount AS 'Скидка %',
    (oi.Price * oi.Quantity - oi.Price * oi.Quantity * oi.Discount / 100) AS 'Сумма',
    (
        SELECT SUM(oi2.Price * oi2.Quantity - oi2.Price * oi2.Quantity * oi2.Discount / 100)
        FROM OrderItems oi2
        WHERE oi2.OrderId = o.Id
    ) AS 'Итого по заказу'
FROM Orders o
JOIN Employees e ON o.EmployeeId = e.Id
JOIN OrderItems oi ON o.Id = oi.OrderId
JOIN Products p ON oi.ProductId = p.Id
WHERE (@search = ''
    OR o.OrderCode LIKE @like
    OR o.Status LIKE @like
    OR e.FullName LIKE @like
    OR p.Article LIKE @like
    OR p.Name LIKE @like
    OR oi.Price LIKE @like
    OR oi.Quantity LIKE @like
    OR oi.Discount LIKE @like
    OR (oi.Price * oi.Quantity - oi.Price * oi.Quantity * oi.Discount / 100) LIKE @like
)
ORDER BY o.Id, oi.Id;
";

                var da = new MySqlDataAdapter(sql, con);
                da.SelectCommand.Parameters.AddWithValue("@search", search);
                da.SelectCommand.Parameters.AddWithValue("@like", "%" + search + "%");

                DataTable dt = new DataTable();
                da.Fill(dt);

                dgv.DataSource = dt;

                if (dgv.Columns.Contains("Id"))
                    dgv.Columns["Id"].Visible = false;

                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Форматирование валюты
                string[] moneyCols = { "Цена", "Сумма", "Итого по заказу" };
                foreach (var col in moneyCols)
                {
                    if (dgv.Columns.Contains(col))
                        dgv.Columns[col].DefaultCellStyle.Format = "C2";
                }

                // Выравнивание чисел
                string[] numCols = { "Количество", "Скидка %" };
                foreach (var col in numCols)
                {
                    if (dgv.Columns.Contains(col))
                        dgv.Columns[col].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        // ================================
        // ГЕНЕРАЦИЯ КОДА ЗАКАЗА
        // ================================
        string GenerateOrderCode()
        {
            using (var con = DB.Get())
            {
                con.Open();
                var cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM Orders WHERE OrderDate = CURDATE()", con);

                int count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                return $"ORD-{DateTime.Now:yyyyMMdd}-{count:D3}";
            }
        }

        // ================================
        // ДОБАВИТЬ ЗАКАЗ
        // ================================
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string code = GenerateOrderCode();

            using (var con = DB.Get())
            {
                con.Open();
                var cmd = new MySqlCommand(
@"INSERT INTO Orders (OrderCode, OrderDate, Status, EmployeeId)
  VALUES (@code, @date, @status, @emp)", con);

                cmd.Parameters.AddWithValue("@code", code);
                cmd.Parameters.AddWithValue("@date", DateTime.Now);
                cmd.Parameters.AddWithValue("@status", "Новый");
                cmd.Parameters.AddWithValue("@emp", CurrentEmployeeId);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Заказ создан!");
            LoadData();
        }

        // ================================
        // УДАЛИТЬ ЗАКАЗ
        // ================================
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите заказ!");
                return;
            }

            if (MessageBox.Show(
                "Удалить заказ?",
                "Подтверждение",
                MessageBoxButtons.YesNo)
                != DialogResult.Yes)
                return;

            int id = Convert.ToInt32(
                dgv.SelectedRows[0].Cells["Id"].Value);

            using (var con = DB.Get())
            {
                con.Open();
                var cmd = new MySqlCommand(
                    "DELETE FROM Orders WHERE Id=@id", con);

                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Заказ удалён!");
            LoadData();
        }

        // ================================
        // АВТОПОИСК ПРИ ВВОДЕ ТЕКСТА
        // ================================
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            searchTimer.Stop();
            searchTimer.Start();
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            searchTimer.Stop();
            LoadData(txtSearch.Text.Trim());
        }
    }
}
