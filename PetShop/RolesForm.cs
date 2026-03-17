using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace PetShop
{
    public partial class RolesForm : Form
    {
        public RolesForm()
        {
            InitializeComponent();
            LoadData();
        }

        void LoadData()
        {
            try
            {
                using (var con = DB.Get())
                {
                    var da = new MySqlDataAdapter(
                        "SELECT Id, Name AS 'Роль' FROM Roles", con);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgv.DataSource = dt;

                    dgv.Columns["Id"].Visible = false;

                    dgv.AutoSizeColumnsMode =
                        DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка: " + ex.Message);
            }
        }

        // Добавить
        private void btnAdd_Click(object sender, EventArgs e)
        {
            new EditRoleForm(0).ShowDialog();
            LoadData();
        }

        // Редактировать
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите роль!");
                return;
            }

            int id = Convert.ToInt32(
                dgv.SelectedRows[0].Cells["Id"].Value);

            new EditRoleForm(id).ShowDialog();
            LoadData();
        }

        // Удалить
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите роль!");
                return;
            }

            int id = Convert.ToInt32(
                dgv.SelectedRows[0].Cells["Id"].Value);

            if (id == 1)
            {
                MessageBox.Show(
                    "Роль администратора удалять нельзя!");
                return;
            }

            if (MessageBox.Show(
                "Удалить роль?",
                "Подтверждение",
                MessageBoxButtons.YesNo)
                != DialogResult.Yes)
                return;

            try
            {
                using (var con = DB.Get())
                {
                    var cmd = new MySqlCommand(
                        "DELETE FROM Roles WHERE Id=@id", con);

                    cmd.Parameters.AddWithValue("@id", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Роль удалена!");
                LoadData();
            }
            catch
            {
                MessageBox.Show(
                    "Роль используется сотрудниками!");
            }
        }
    }
}