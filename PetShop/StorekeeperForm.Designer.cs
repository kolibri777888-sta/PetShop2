
namespace PetShop
{
    partial class StorekeeperForm
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
            this.dgvWarehouse = new System.Windows.Forms.DataGridView();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.txtLocation = new System.Windows.Forms.TextBox();
            this.numIncoming = new System.Windows.Forms.NumericUpDown();
            this.btnMove = new System.Windows.Forms.Button();
            this.btnIncoming = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnClearFilters = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWarehouse)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numIncoming)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvWarehouse
            // 
            this.dgvWarehouse.AllowUserToAddRows = false;
            this.dgvWarehouse.AllowUserToDeleteRows = false;
            this.dgvWarehouse.BackgroundColor = System.Drawing.Color.Gold;
            this.dgvWarehouse.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWarehouse.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvWarehouse.Location = new System.Drawing.Point(0, 157);
            this.dgvWarehouse.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvWarehouse.Name = "dgvWarehouse";
            this.dgvWarehouse.ReadOnly = true;
            this.dgvWarehouse.RowHeadersWidth = 51;
            this.dgvWarehouse.RowTemplate.Height = 24;
            this.dgvWarehouse.Size = new System.Drawing.Size(722, 306);
            this.dgvWarehouse.TabIndex = 0;
            // 
            // txtSearch
            // 
            this.txtSearch.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.txtSearch.Location = new System.Drawing.Point(9, 25);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(158, 20);
            this.txtSearch.TabIndex = 1;
            // 
            // txtLocation
            // 
            this.txtLocation.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.txtLocation.Location = new System.Drawing.Point(195, 102);
            this.txtLocation.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtLocation.Name = "txtLocation";
            this.txtLocation.Size = new System.Drawing.Size(160, 20);
            this.txtLocation.TabIndex = 2;
            // 
            // numIncoming
            // 
            this.numIncoming.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.numIncoming.Location = new System.Drawing.Point(195, 25);
            this.numIncoming.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.numIncoming.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numIncoming.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numIncoming.Name = "numIncoming";
            this.numIncoming.Size = new System.Drawing.Size(159, 20);
            this.numIncoming.TabIndex = 4;
            this.numIncoming.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnMove
            // 
            this.btnMove.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnMove.Location = new System.Drawing.Point(195, 124);
            this.btnMove.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnMove.Name = "btnMove";
            this.btnMove.Size = new System.Drawing.Size(159, 29);
            this.btnMove.TabIndex = 6;
            this.btnMove.Text = "Разместить";
            this.btnMove.UseVisualStyleBackColor = false;
            this.btnMove.Click += new System.EventHandler(this.btnMove_Click);
            // 
            // btnIncoming
            // 
            this.btnIncoming.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnIncoming.Location = new System.Drawing.Point(195, 46);
            this.btnIncoming.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnIncoming.Name = "btnIncoming";
            this.btnIncoming.Size = new System.Drawing.Size(159, 53);
            this.btnIncoming.TabIndex = 10;
            this.btnIncoming.Text = "Приход";
            this.btnIncoming.UseVisualStyleBackColor = false;
            this.btnIncoming.Click += new System.EventHandler(this.btnIncoming_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Поиск";
            // 
            // cmbCategory
            // 
            this.cmbCategory.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategory.FormattingEnabled = true;
            this.cmbCategory.Location = new System.Drawing.Point(10, 133);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(158, 21);
            this.cmbCategory.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Категория";
            // 
            // btnClearFilters
            // 
            this.btnClearFilters.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnClearFilters.Location = new System.Drawing.Point(8, 46);
            this.btnClearFilters.Name = "btnClearFilters";
            this.btnClearFilters.Size = new System.Drawing.Size(158, 67);
            this.btnClearFilters.TabIndex = 14;
            this.btnClearFilters.Text = "Сбросить фильтры";
            this.btnClearFilters.UseVisualStyleBackColor = false;
            this.btnClearFilters.Click += new System.EventHandler(this.btnClearFilters_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnExit.Location = new System.Drawing.Point(592, 102);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(120, 51);
            this.btnExit.TabIndex = 15;
            this.btnExit.Text = "Выйти";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // StorekeeperForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gold;
            this.ClientSize = new System.Drawing.Size(722, 463);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnClearFilters);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbCategory);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnIncoming);
            this.Controls.Add(this.btnMove);
            this.Controls.Add(this.numIncoming);
            this.Controls.Add(this.txtLocation);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.dgvWarehouse);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.Name = "StorekeeperForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Кладовщик";
            ((System.ComponentModel.ISupportInitialize)(this.dgvWarehouse)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numIncoming)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvWarehouse;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.TextBox txtLocation;
        private System.Windows.Forms.NumericUpDown numIncoming;
        private System.Windows.Forms.Button btnMove;
        private System.Windows.Forms.Button btnIncoming;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnClearFilters;
        private System.Windows.Forms.Button btnExit;
    }
}