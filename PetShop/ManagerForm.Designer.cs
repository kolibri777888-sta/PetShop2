
namespace PetShop
{
    partial class ManagerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOrders = new System.Windows.Forms.Button();
            this.btnProducts = new System.Windows.Forms.Button();
            this.btnReports = new System.Windows.Forms.Button();
            this.btnCheck = new System.Windows.Forms.Button();
            this.btnZakaz = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnOrders
            // 
            this.btnOrders.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnOrders.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOrders.Font = new System.Drawing.Font("Segoe UI Emoji", 14.25F);
            this.btnOrders.Location = new System.Drawing.Point(87, 264);
            this.btnOrders.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnOrders.Name = "btnOrders";
            this.btnOrders.Size = new System.Drawing.Size(137, 61);
            this.btnOrders.TabIndex = 0;
            this.btnOrders.Text = "Оформить заказ";
            this.btnOrders.UseVisualStyleBackColor = false;
            this.btnOrders.Click += new System.EventHandler(this.btnOrders_Click);
            // 
            // btnProducts
            // 
            this.btnProducts.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnProducts.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnProducts.Font = new System.Drawing.Font("Segoe UI Emoji", 14.25F);
            this.btnProducts.Location = new System.Drawing.Point(87, 202);
            this.btnProducts.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnProducts.Name = "btnProducts";
            this.btnProducts.Size = new System.Drawing.Size(137, 49);
            this.btnProducts.TabIndex = 1;
            this.btnProducts.Text = "Товары";
            this.btnProducts.UseVisualStyleBackColor = false;
            this.btnProducts.Click += new System.EventHandler(this.btnProducts_Click);
            // 
            // btnReports
            // 
            this.btnReports.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnReports.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnReports.Font = new System.Drawing.Font("Segoe UI Emoji", 14.25F);
            this.btnReports.Location = new System.Drawing.Point(87, 142);
            this.btnReports.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnReports.Name = "btnReports";
            this.btnReports.Size = new System.Drawing.Size(137, 49);
            this.btnReports.TabIndex = 2;
            this.btnReports.Text = "Отчёт";
            this.btnReports.UseVisualStyleBackColor = false;
            this.btnReports.Click += new System.EventHandler(this.btnReports_Click);
            // 
            // btnCheck
            // 
            this.btnCheck.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnCheck.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCheck.Font = new System.Drawing.Font("Segoe UI Emoji", 14.25F);
            this.btnCheck.Location = new System.Drawing.Point(87, 82);
            this.btnCheck.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(137, 49);
            this.btnCheck.TabIndex = 3;
            this.btnCheck.Text = "Чек";
            this.btnCheck.UseVisualStyleBackColor = false;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // btnZakaz
            // 
            this.btnZakaz.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnZakaz.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnZakaz.Font = new System.Drawing.Font("Segoe UI Emoji", 14.25F);
            this.btnZakaz.Location = new System.Drawing.Point(87, 23);
            this.btnZakaz.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnZakaz.Name = "btnZakaz";
            this.btnZakaz.Size = new System.Drawing.Size(137, 49);
            this.btnZakaz.TabIndex = 4;
            this.btnZakaz.Text = "Заказы";
            this.btnZakaz.UseVisualStyleBackColor = false;
            this.btnZakaz.Click += new System.EventHandler(this.btnZakaz_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnExit.Location = new System.Drawing.Point(87, 346);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(137, 43);
            this.btnExit.TabIndex = 5;
            this.btnExit.Text = "Выйти";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // ManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gold;
            this.ClientSize = new System.Drawing.Size(302, 410);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnZakaz);
            this.Controls.Add(this.btnCheck);
            this.Controls.Add(this.btnReports);
            this.Controls.Add(this.btnProducts);
            this.Controls.Add(this.btnOrders);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ManagerForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Менеджер";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOrders;
        private System.Windows.Forms.Button btnProducts;
        private System.Windows.Forms.Button btnReports;
        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.Button btnZakaz;
        private System.Windows.Forms.Button btnExit;
    }
}