using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace PetShop
{
    public partial class CategoriesForm : Form
    {
        public CategoriesForm()
        {
            InitializeComponent();
            this.Shown += CategoriesForm_Shown;
        }

        private void CategoriesForm_Shown(object sender, EventArgs e)
        {
            LoadData();
        }

        // ===============================
        // Загрузка категорий
        // ===============================
        void LoadData()
        {
            try
            {
                using (var con = DB.Get())
                {
                    var da = new MySqlDataAdapter(
                        "SELECT Id, Name FROM Categories ORDER BY Name", con);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgv.DataSource = dt;

                    if (dgv.Columns.Contains("Id"))
                        dgv.Columns["Id"].Visible = false;

                    if (dgv.Columns.Contains("Name"))
                        dgv.Columns["Name"].HeaderText = "Название категории";

                    dgv.AutoSizeColumnsMode =
                        DataGridViewAutoSizeColumnsMode.Fill;

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
        // Добавление категории
        // ===============================
        private void btnAdd_Click(object sender, EventArgs e)
        {
            new AddCategoryForm().ShowDialog();
            LoadData();
        }

        // ===============================
        // Редактирование категории
        // ===============================
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите категорию!");
                return;
            }

            int id = Convert.ToInt32(
                dgv.SelectedRows[0].Cells["Id"].Value);

            new EditCategoryForm(id).ShowDialog();

            LoadData();
        }

        // ===============================
        // Удаление категории
        // ===============================
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите категорию!");
                return;
            }

            if (MessageBox.Show(
                "Удалить категорию?",
                "Подтверждение",
                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            int id = Convert.ToInt32(
                dgv.SelectedRows[0].Cells["Id"].Value);

            try
            {
                using (var con = DB.Get())
                {
                    var cmd = new MySqlCommand(
                        "DELETE FROM Categories WHERE Id=@id", con);

                    cmd.Parameters.AddWithValue("@id", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Категория удалена!");
                LoadData();
            }
            catch
            {
                MessageBox.Show(
                    "Нельзя удалить: в категории есть товары!");
            }
        }
    }
}