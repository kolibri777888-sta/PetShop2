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
    public partial class MainForm : Form
    {
        string role;

        public MainForm(string role)
        {
            InitializeComponent();
            this.role = role;
        }

        private void товарыToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            new ProductsForm().ShowDialog();
        }

        private void категорииToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            new CategoriesForm().ShowDialog();
        }

        private void сотрудникиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new EmployeesForm().ShowDialog();
        }

        private void поставщикиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new SuppliersForm().ShowDialog();
        }

        private void заказыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new OrdersForm().ShowDialog();
        }

        private void ролиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new RolesForm().ShowDialog();
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