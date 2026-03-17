using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace PetShop
{
    public partial class SuppliersForm : Form
    {
        public SuppliersForm()
        {
            InitializeComponent();
            LoadData();
        }

        void LoadData()
        {
            using (var con = DB.Get())
            {
                var da = new MySqlDataAdapter(
                    "SELECT Id, Name, Phone FROM Suppliers", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                dgv.DataSource = dt;

                // Скрываем ID
                dgv.Columns["Id"].Visible = false;

                dgv.AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill;
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите поставщика!");
                return;
            }

            int id = (int)dgv.SelectedRows[0].Cells["Id"].Value;

            new EditSupplierForm(id).ShowDialog();

            LoadData();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите поставщика!");
                return;
            }

            if (MessageBox.Show(
                "Удалить поставщика?",
                "Подтверждение",
                MessageBoxButtons.YesNo)
                != DialogResult.Yes)
                return;

            int id = Convert.ToInt32(
                dgv.SelectedRows[0].Cells["Id"].Value);

            try
            {
                using (var con = DB.Get())
                {
                    var cmd = new MySqlCommand(
                        "DELETE FROM Suppliers WHERE Id=@id", con);

                    cmd.Parameters.AddWithValue("@id", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Поставщик удалён!");
                LoadData();
            }
            catch
            {
                MessageBox.Show(
                    "Есть связанные товары!");
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            new AddSupplierForm().ShowDialog();
            LoadData();
        }
    }
}