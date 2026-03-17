using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Word = Microsoft.Office.Interop.Word;

namespace PetShop
{
    public partial class CheckForm : Form
    {
        DataTable ordersTable = new DataTable();

        public CheckForm()
        {
            InitializeComponent();

            LoadOrders();

            txtSearch.TextChanged += FilterOrders;
        }

        // ===============================
        // ЗАГРУЗКА ЗАКАЗОВ
        // ===============================
        void LoadOrders()
        {
            using (var c = DB.Get())
            {
                c.Open();

                var da = new MySqlDataAdapter(@"
SELECT
    o.Id,
    o.OrderCode,
    o.OrderDate,
    o.Status,
    e.FullName AS Employee
FROM Orders o
JOIN Employees e ON e.Id = o.EmployeeId
ORDER BY o.OrderDate DESC", c);

                ordersTable.Clear();
                da.Fill(ordersTable);

                dgvOrders.DataSource = ordersTable;
            }

            dgvOrders.Columns["Id"].Visible = false;
        }

        // ===============================
        // ПОИСК ЗАКАЗОВ
        // ===============================
        void FilterOrders(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Replace("'", "''");

            string filter = "";

            if (!string.IsNullOrWhiteSpace(search))
            {
                filter =
                $"OrderCode LIKE '%{search}%' OR " +
                $"Status LIKE '%{search}%' OR " +
                $"Employee LIKE '%{search}%'";
            }

            (dgvOrders.DataSource as DataTable).DefaultView.RowFilter = filter;
        }

        // ===============================
        // СОЗДАНИЕ ЧЕКА
        // ===============================
        private void btnCreateCheck_Click(object sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow == null)
            {
                MessageBox.Show("Выберите заказ");
                return;
            }

            int orderId = Convert.ToInt32(
                dgvOrders.CurrentRow.Cells["Id"].Value);

            Word.Application app = null;
            Word.Document doc = null;

            try
            {
                app = new Word.Application();
                doc = app.Documents.Add();

                // ===============================
                // ШАПКА ЧЕКА
                // ===============================

                var shop = doc.Paragraphs.Add();
                shop.Range.Text = "";
                shop.Range.Font.Size = 18;
                shop.Range.Font.Bold = 1;
                shop.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                shop.Range.InsertParagraphAfter();

                var addr = doc.Paragraphs.Add();
                addr.Range.Text = "Зоомагазин";
                addr.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                addr.Range.InsertParagraphAfter();

                var date = doc.Paragraphs.Add();
                date.Range.Text = "Дата: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                date.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                date.Range.InsertParagraphAfter();

                var order = doc.Paragraphs.Add();
                order.Range.Text = "ЧЕК № " + orderId;
                order.Range.Font.Bold = 1;
                order.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                order.Range.InsertParagraphAfter();
                order.Range.InsertParagraphAfter();

                // ===============================
                // ПОЛУЧАЕМ ТОВАРЫ
                // ===============================

                DataTable items = new DataTable();

                using (var c = DB.Get())
                {
                    c.Open();

                    var da = new MySqlDataAdapter(@"
SELECT
    p.Name,
    oi.Price,
    oi.Quantity,
    oi.Discount
FROM OrderItems oi
JOIN Products p ON oi.ProductId = p.Id
WHERE oi.OrderId = @id", c);

                    da.SelectCommand.Parameters.AddWithValue("@id", orderId);

                    da.Fill(items);
                }

                if (items.Rows.Count == 0)
                {
                    MessageBox.Show("В заказе нет товаров!");
                    return;
                }

                // ===============================
                // ТАБЛИЦА ТОВАРОВ
                // ===============================

                int rows = items.Rows.Count + 1;
                int cols = 4;

                Word.Table table = doc.Tables.Add(
                    doc.Bookmarks.get_Item("\\endofdoc").Range,
                    rows,
                    cols);

                table.Borders.Enable = 1;

                table.Cell(1, 1).Range.Text = "Товар";
                table.Cell(1, 2).Range.Text = "Цена";
                table.Cell(1, 3).Range.Text = "Кол-во";
                table.Cell(1, 4).Range.Text = "Сумма";

                decimal total = 0;

                for (int i = 0; i < items.Rows.Count; i++)
                {
                    string name = items.Rows[i]["Name"].ToString();
                    decimal price = Convert.ToDecimal(items.Rows[i]["Price"]);
                    int qty = Convert.ToInt32(items.Rows[i]["Quantity"]);
                    int discount = Convert.ToInt32(items.Rows[i]["Discount"]);

                    decimal sum = price * qty;
                    decimal disc = sum * discount / 100m;
                    decimal final = sum - disc;

                    total += final;

                    table.Cell(i + 2, 1).Range.Text = name;
                    table.Cell(i + 2, 2).Range.Text = price.ToString("0.00");
                    table.Cell(i + 2, 3).Range.Text = qty.ToString();
                    table.Cell(i + 2, 4).Range.Text = final.ToString("0.00");
                }

                doc.Paragraphs.Add().Range.InsertParagraphAfter();

                // ===============================
                // ИТОГ
                // ===============================

                var totalPar = doc.Paragraphs.Add();
                totalPar.Range.Text = "ИТОГО: " + total.ToString("0.00") + " руб.";
                totalPar.Range.Font.Size = 14;
                totalPar.Range.Font.Bold = 1;
                totalPar.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;

                doc.Paragraphs.Add().Range.InsertParagraphAfter();

                var thanks = doc.Paragraphs.Add();
                thanks.Range.Text = "Спасибо за покупку!";
                thanks.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                // ===============================
                // СОХРАНЕНИЕ
                // ===============================

                string path =
                    Environment.GetFolderPath(
                    Environment.SpecialFolder.Desktop)
                    + $"\\Check_{orderId}.docx";

                doc.SaveAs(path);

                MessageBox.Show("Чек создан!\nФайл: " + path);

                app.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка:\n" + ex.Message);
            }
            finally
            {
                if (doc != null)
                    doc.Close();

                if (app != null)
                    app.Quit();
            }
        }
    }
}
