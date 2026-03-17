using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PetShop
{
    public partial class ManagerForm : Form
    {
        public ManagerForm()
        {
            InitializeComponent();
        }

        private void btnOrders_Click(object sender, EventArgs e)
        {
            new ORrdersForm().ShowDialog();
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            new PrPoductsForm().ShowDialog();
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            new ReportsForm().ShowDialog();
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            new CheckForm().ShowDialog();
        }

        private void btnZakaz_Click(object sender, EventArgs e)
        {
            new ZakazForm().ShowDialog();
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
