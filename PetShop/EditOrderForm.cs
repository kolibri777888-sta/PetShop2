using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace PetShop
{
    public partial class EditOrderForm : Form
    {
        int id;

        public EditOrderForm(int id)
        {
            InitializeComponent();
            this.id = id;

            // Добавляем варианты статуса
            cbStatus.Items.Add("Новый");
            cbStatus.Items.Add("В работе");
            cbStatus.Items.Add("Завершён");

            // Статус сразу редактируемый
            cbStatus.DropDownStyle = ComboBoxStyle.DropDownList;

            LoadData();
        }

        void LoadData()
        {
            using (var con = DB.Get())
            {
                var cmd = new MySqlCommand(
                    "SELECT * FROM Orders WHERE Id=@id", con);

                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                var r = cmd.ExecuteReader();

                if (r.Read())
                {
                    cbStatus.Text = r["Status"].ToString();
                }
            }
        }

        // Кнопка "Сохранить"
        private void btnSave_Click(object sender, EventArgs e)
        {
            using (var con = DB.Get())
            {
                var cmd = new MySqlCommand(
                    "UPDATE Orders SET Status=@s WHERE Id=@id", con);

                cmd.Parameters.AddWithValue("@s", cbStatus.Text);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Статус изменён");
            Close();
        }
    }
}
