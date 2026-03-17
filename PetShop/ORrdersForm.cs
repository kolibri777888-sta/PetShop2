using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace PetShop
{
    public partial class ORrdersForm : Form
    {
        DataTable cart = new DataTable();

        public ORrdersForm()
        {
            InitializeComponent();
            InitCart();
            LoadProducts();
            LoadEmployees();
        }

        void InitCart()
        {
            cart.Columns.Add("ProductId", typeof(int));
            cart.Columns.Add("Article", typeof(string));
            cart.Columns.Add("Name", typeof(string));
            cart.Columns.Add("Price", typeof(decimal));
            cart.Columns.Add("Qty", typeof(int));
            cart.Columns.Add("Discount", typeof(double));
            cart.Columns.Add("Sum", typeof(decimal));

            dgvCart.DataSource = cart;
            dgvCart.Columns["ProductId"].Visible = false;

            dgvCart.Columns["Article"].HeaderText = "Артикул";
            dgvCart.Columns["Name"].HeaderText = "Наименование";
            dgvCart.Columns["Price"].HeaderText = "Цена";
            dgvCart.Columns["Qty"].HeaderText = "Кол-во";
            dgvCart.Columns["Discount"].HeaderText = "Скидка (%)";
            dgvCart.Columns["Sum"].HeaderText = "Сумма";

            DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
            btn.Name = "Delete";
            btn.Text = "Удалить";
            btn.UseColumnTextForButtonValue = true;
            dgvCart.Columns.Add(btn);

            dgvCart.CellClick += DgvCart_CellClick;
        }

        private void DgvCart_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvCart.Columns[e.ColumnIndex].Name == "Delete")
            {
                cart.Rows[e.RowIndex].Delete();
                CalcTotal();
            }
        }

        void LoadProducts()
        {
            using (var c = DB.Get())
            {
                c.Open();

                var da = new MySqlDataAdapter(@"
SELECT 
    p.Id,
    p.Article,
    p.Name,
    p.Price,
    p.Discount,
    IFNULL(w.Quantity, 0) AS Quantity
FROM Products p
LEFT JOIN Warehouse w ON p.Id = w.ProductId", c);

                DataTable t = new DataTable();
                da.Fill(t);
                dgvProducts.DataSource = t;
            }

            dgvProducts.Columns["Id"].Visible = false;
            dgvProducts.Columns["Article"].HeaderText = "Артикул";
            dgvProducts.Columns["Name"].HeaderText = "Наименование";
            dgvProducts.Columns["Price"].HeaderText = "Цена";
            dgvProducts.Columns["Discount"].HeaderText = "Скидка (%)";
            dgvProducts.Columns["Quantity"].HeaderText = "Остаток";
        }

        void LoadEmployees()
        {
            using (var c = DB.Get())
            {
                c.Open();
                var da = new MySqlDataAdapter(
                    "SELECT Id, FullName FROM Employees WHERE RoleId=2", c);

                DataTable t = new DataTable();
                da.Fill(t);

                cmbEmployee.DataSource = t;
                cmbEmployee.DisplayMember = "FullName";
                cmbEmployee.ValueMember = "Id";
            }

            cmbStatus.Items.AddRange(new string[]
            {
                "Новый",
                "В работе",
                "Завершён"
            });
            cmbStatus.SelectedIndex = 0;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;

            var r = dgvProducts.CurrentRow;

            int productId = Convert.ToInt32(r.Cells["Id"].Value);
            string art = r.Cells["Article"].Value.ToString();
            string name = r.Cells["Name"].Value.ToString();
            decimal price = Convert.ToDecimal(r.Cells["Price"].Value);
            int stock = Convert.ToInt32(r.Cells["Quantity"].Value);
            int dbDiscount = Convert.ToInt32(r.Cells["Discount"].Value);

            if (stock <= 0)
            {
                MessageBox.Show("Нет товара на складе!");
                return;
            }

            double totalDiscount = dbDiscount / 100.0;

            // Если товар уже есть в корзине, увеличиваем количество
            DataRow existing = null;
            foreach (DataRow row in cart.Rows)
            {
                if (Convert.ToInt32(row["ProductId"]) == productId)
                {
                    existing = row;
                    break;
                }
            }

            if (existing != null)
            {
                int newQty = Convert.ToInt32(existing["Qty"]) + 1;
                if (newQty > stock)
                {
                    MessageBox.Show("Недостаточно товара на складе!");
                    return;
                }
                existing["Qty"] = newQty;
                existing["Sum"] = price * newQty * (decimal)(1 - totalDiscount);
            }
            else
            {
                cart.Rows.Add(productId, art, name, price, 1, dbDiscount, price * (decimal)(1 - totalDiscount));
            }

            CalcTotal();
        }

        void CalcTotal()
        {
            decimal total = 0;
            foreach (DataRow r in cart.Rows)
                total += Convert.ToDecimal(r["Sum"]);

            lblTotal.Text = "Итого: " + total + " ₽";
        }

        string GenerateOrderCode(MySqlConnection c)
        {
            var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM Orders WHERE OrderDate = CURDATE()", c);

            int count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;

            return $"ORD-{DateTime.Now:yyyyMMdd}-{count:D3}";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cart.Rows.Count == 0)
            {
                MessageBox.Show("Корзина пуста!");
                return;
            }

            using (var c = DB.Get())
            {
                c.Open();

                try
                {
                    // =========================
                    // 1️ ПОЛНАЯ ПРОВЕРКА СКЛАДА
                    // =========================
                    foreach (DataRow r in cart.Rows)
                    {
                        int productId = Convert.ToInt32(r["ProductId"]);
                        int qty = Convert.ToInt32(r["Qty"]);

                        var checkCmd = new MySqlCommand(
                            "SELECT Quantity FROM Warehouse WHERE ProductId=@id", c);

                        checkCmd.Parameters.AddWithValue("@id", productId);

                        object result = checkCmd.ExecuteScalar();

                        if (result == null)
                        {
                            MessageBox.Show($"Товар {r["Name"]} отсутствует на складе!");
                            return;
                        }

                        int stock = Convert.ToInt32(result);

                        if (stock < qty)
                        {
                            MessageBox.Show($"Недостаточно товара на складе для {r["Name"]}!");
                            return;
                        }
                    }

                    // =========================
                    // 2️ СОЗДАЁМ ЗАКАЗ
                    // =========================
                    string orderCode = GenerateOrderCode(c);

                    var cmd = new MySqlCommand(@"
INSERT INTO Orders (OrderCode, OrderDate, Status, EmployeeId)
VALUES(@code, CURDATE(), @status, @emp);
SELECT LAST_INSERT_ID();", c);

                    cmd.Parameters.AddWithValue("@code", orderCode);
                    cmd.Parameters.AddWithValue("@status", cmbStatus.Text);
                    cmd.Parameters.AddWithValue("@emp", cmbEmployee.SelectedValue);

                    int orderId = Convert.ToInt32(cmd.ExecuteScalar());

                    // =========================
                    // 3️ СОХРАНЯЕМ ПОЗИЦИИ + СПИСЫВАЕМ
                    // =========================
                    foreach (DataRow r in cart.Rows)
                    {
                        int productId = Convert.ToInt32(r["ProductId"]);
                        int qty = Convert.ToInt32(r["Qty"]);

                        // Добавляем позицию
                        var cmd2 = new MySqlCommand(@"
INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price, Discount)
VALUES(@orderId, @productId, @qty, @price, @discount)", c);

                        cmd2.Parameters.AddWithValue("@orderId", orderId);
                        cmd2.Parameters.AddWithValue("@productId", productId);
                        cmd2.Parameters.AddWithValue("@qty", qty);
                        cmd2.Parameters.AddWithValue("@price", r["Price"]);
                        cmd2.Parameters.AddWithValue("@discount", r["Discount"]);
                        cmd2.ExecuteNonQuery();

                        // Списание со склада (безопасное)
                        var cmdStock = new MySqlCommand(@"
UPDATE Warehouse 
SET Quantity = Quantity - @qty
WHERE ProductId=@productId AND Quantity >= @qty", c);

                        cmdStock.Parameters.AddWithValue("@qty", qty);
                        cmdStock.Parameters.AddWithValue("@productId", productId);

                        int affected = cmdStock.ExecuteNonQuery();

                        if (affected == 0)
                        {
                            MessageBox.Show("Ошибка списания со склада!");
                            return;
                        }

                        // Журнал операций
                        var cmdLog = new MySqlCommand(@"
INSERT INTO WarehouseOperations 
(ProductId, OperationType, Quantity, OperationDate, Comment)
VALUES(@p,'Продажа',@q,NOW(),'Списание по заказу')", c);

                        cmdLog.Parameters.AddWithValue("@p", productId);
                        cmdLog.Parameters.AddWithValue("@q", qty);
                        cmdLog.ExecuteNonQuery();
                    }

                    MessageBox.Show("Заказ успешно оформлен!");
                    cart.Clear();
                    CalcTotal();
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }
    }
}
