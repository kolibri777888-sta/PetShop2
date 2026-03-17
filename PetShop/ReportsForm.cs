using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Excel = Microsoft.Office.Interop.Excel;

namespace PetShop
{
    public partial class ReportsForm : Form
    {
        DataTable table = new DataTable();

        public ReportsForm()
        {
            InitializeComponent();

            LoadData(); // загрузка данных сразу
        }

        // ==========================
        // ЗАГРУЗКА ДАННЫХ В ТАБЛИЦУ
        // ==========================
        void LoadData()
        {
            using (var c = DB.Get())
            {
                c.Open();

                var da = new MySqlDataAdapter(@"
SELECT
    p.Name AS ProductName,
    SUM(oi.Quantity) AS Qty,
    SUM(oi.Price * oi.Quantity * (1 - oi.Discount/100)) AS Total
FROM Orders o
JOIN OrderItems oi ON oi.OrderId = o.Id
JOIN Products p ON p.Id = oi.ProductId
GROUP BY p.Name
ORDER BY Qty DESC", c);

                table.Clear();
                da.Fill(table);

                dgv.DataSource = table;
            }

            // русские заголовки
            dgv.Columns["ProductName"].HeaderText = "Наименование товара";
            dgv.Columns["Qty"].HeaderText = "Количество продано";
            dgv.Columns["Total"].HeaderText = "Общая выручка";

            dgv.Columns["Total"].DefaultCellStyle.Format = "N2";
        }

        // ==========================
        // СОЗДАНИЕ EXCEL ОТЧЕТА
        // ==========================
        private void btnBuild_Click(object sender, EventArgs e)
        {
            if (table.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для отчета");
                return;
            }

            string bestProduct = table.Rows[0]["ProductName"].ToString();
            int bestQty = Convert.ToInt32(table.Rows[0]["Qty"]);
            decimal bestTotal = Convert.ToDecimal(table.Rows[0]["Total"]);

            Excel.Application excel = new Excel.Application();
            Excel.Workbook workbook = excel.Workbooks.Add();
            Excel.Worksheet sheet = workbook.ActiveSheet;

            sheet.Name = "Отчет продаж";

            sheet.Cells[1, 1] = "ОТЧЕТ ПРОДАЖ PETSHOP";
            sheet.Cells[1, 1].Font.Size = 16;
            sheet.Cells[1, 1].Font.Bold = true;

            sheet.Cells[2, 1] = "Дата:";
            sheet.Cells[2, 2] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            sheet.Cells[4, 1] = "Самый продаваемый товар";
            sheet.Cells[4, 1].Font.Bold = true;

            sheet.Cells[5, 1] = "Название";
            sheet.Cells[5, 2] = bestProduct;

            sheet.Cells[6, 1] = "Количество";
            sheet.Cells[6, 2] = bestQty;

            sheet.Cells[7, 1] = "Выручка";
            sheet.Cells[7, 2] = bestTotal;

            sheet.Cells[9, 1] = "Товар";
            sheet.Cells[9, 2] = "Количество";
            sheet.Cells[9, 3] = "Выручка";

            Excel.Range header = sheet.Range["A9", "C9"];
            header.Font.Bold = true;

            for (int i = 0; i < table.Rows.Count; i++)
            {
                sheet.Cells[i + 10, 1] = table.Rows[i]["ProductName"];
                sheet.Cells[i + 10, 2] = table.Rows[i]["Qty"];
                sheet.Cells[i + 10, 3] = table.Rows[i]["Total"];
            }

            sheet.Columns.AutoFit();

            string path =
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                + "\\SalesReport.xlsx";

            workbook.SaveAs(path);

            excel.Visible = true;

            MessageBox.Show("Отчет создан:\n" + path);
        }
    }
}
