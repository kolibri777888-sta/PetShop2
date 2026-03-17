using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace PetShop
{
    public partial class EmployeesForm : Form
    {
        public EmployeesForm()
        {
            InitializeComponent();
            this.Shown += EmployeesForm_Shown;
        }

        private void EmployeesForm_Shown(object sender, EventArgs e)
        {
            LoadData();
        }

        // ===============================
        // Загрузка сотрудников
        // ===============================
        void LoadData()
        {
            try
            {
                using (var con = DB.Get())
                {
                    var da = new MySqlDataAdapter(@"
                        SELECT
                            e.Id,
                            e.FullName,
                            e.Phone,
                            e.Login,
                            r.Name AS Role
                        FROM Employees e
                        JOIN Roles r ON e.RoleId = r.Id
                        ORDER BY e.FullName
                    ", con);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgv.DataSource = dt;

                    // скрываем Id
                    if (dgv.Columns.Contains("Id"))
                        dgv.Columns["Id"].Visible = false;

                    // заголовки
                    if (dgv.Columns.Contains("FullName"))
                        dgv.Columns["FullName"].HeaderText = "ФИО";

                    if (dgv.Columns.Contains("Phone"))
                        dgv.Columns["Phone"].HeaderText = "Телефон";

                    if (dgv.Columns.Contains("Login"))
                        dgv.Columns["Login"].HeaderText = "Логин";

                    if (dgv.Columns.Contains("Role"))
                        dgv.Columns["Role"].HeaderText = "Роль";

                    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dgv.MultiSelect = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        // ===============================
        // Добавление сотрудника
        // ===============================
        private void btnAdd_Click(object sender, EventArgs e)
        {
            new AddEmployeeForm().ShowDialog();
            LoadData();
        }

        // ===============================
        // Редактирование сотрудника
        // ===============================
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сотрудника!");
                return;
            }

            int id = Convert.ToInt32(
                dgv.SelectedRows[0].Cells["Id"].Value);

            new EditEmployeeForm(id).ShowDialog();

            LoadData();
        }

        // ===============================
        // Удаление сотрудника
        // ===============================
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сотрудника!");
                return;
            }

            int id = Convert.ToInt32(
                dgv.SelectedRows[0].Cells["Id"].Value);

            // защита администратора
            if (id == 1)
            {
                MessageBox.Show("Администратора удалять нельзя!");
                return;
            }

            if (MessageBox.Show(
                "Удалить сотрудника?",
                "Подтверждение",
                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            try
            {
                using (var con = DB.Get())
                {
                    var cmd = new MySqlCommand(
                        "DELETE FROM Employees WHERE Id=@id", con);

                    cmd.Parameters.AddWithValue("@id", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Сотрудник удалён!");
                LoadData();
            }
            catch
            {
                MessageBox.Show(
                    "Нельзя удалить: есть связанные данные!");
            }
        }
    }
}