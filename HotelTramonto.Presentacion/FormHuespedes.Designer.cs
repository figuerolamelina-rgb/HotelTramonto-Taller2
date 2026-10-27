namespace HotelTramonto.Presentacion
{
    partial class FormHuespedes
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
            dgvHuespedes = new DataGridView();
            btnNuevoHuesped = new Button();
            btnModificarHuesped = new Button();
            btnDesactivarHuesped = new Button();
            btnBuscarDNI = new Button();
            btnActualizar = new Button();
            btnReactivarHuesped = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvHuespedes).BeginInit();
            SuspendLayout();
            // 
            // dgvHuespedes
            // 
            dgvHuespedes.AllowUserToAddRows = false;
            dgvHuespedes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHuespedes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvHuespedes.Location = new Point(12, 86);
            dgvHuespedes.MultiSelect = false;
            dgvHuespedes.Name = "dgvHuespedes";
            dgvHuespedes.ReadOnly = true;
            dgvHuespedes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHuespedes.Size = new Size(743, 150);
            dgvHuespedes.TabIndex = 0;
            // 
            // btnNuevoHuesped
            // 
            btnNuevoHuesped.Location = new Point(28, 37);
            btnNuevoHuesped.Name = "btnNuevoHuesped";
            btnNuevoHuesped.Size = new Size(130, 23);
            btnNuevoHuesped.TabIndex = 1;
            btnNuevoHuesped.Text = "Nuevo huésped";
            btnNuevoHuesped.UseVisualStyleBackColor = true;
            btnNuevoHuesped.Click += btnNuevoHuesped_Click;
            // 
            // btnModificarHuesped
            // 
            btnModificarHuesped.Location = new Point(330, 37);
            btnModificarHuesped.Name = "btnModificarHuesped";
            btnModificarHuesped.Size = new Size(137, 23);
            btnModificarHuesped.TabIndex = 2;
            btnModificarHuesped.Text = "Modificar huésped";
            btnModificarHuesped.UseVisualStyleBackColor = true;
            btnModificarHuesped.Click += btnModificarHuesped_Click;
            // 
            // btnDesactivarHuesped
            // 
            btnDesactivarHuesped.Location = new Point(164, 37);
            btnDesactivarHuesped.Name = "btnDesactivarHuesped";
            btnDesactivarHuesped.Size = new Size(160, 23);
            btnDesactivarHuesped.TabIndex = 3;
            btnDesactivarHuesped.Text = "Desactivar huésped";
            btnDesactivarHuesped.UseVisualStyleBackColor = true;
            btnDesactivarHuesped.Click += btnDesactivarHuesped_Click;
            // 
            // btnBuscarDNI
            // 
            btnBuscarDNI.Location = new Point(473, 37);
            btnBuscarDNI.Name = "btnBuscarDNI";
            btnBuscarDNI.Size = new Size(99, 23);
            btnBuscarDNI.TabIndex = 4;
            btnBuscarDNI.Text = "Buscar por DNI";
            btnBuscarDNI.UseVisualStyleBackColor = true;
            btnBuscarDNI.Click += btnBuscarDNI_Click_1;
            // 
            // btnActualizar
            // 
            btnActualizar.Location = new Point(578, 37);
            btnActualizar.Name = "btnActualizar";
            btnActualizar.Size = new Size(109, 23);
            btnActualizar.TabIndex = 5;
            btnActualizar.Text = "Actualizar listado";
            btnActualizar.UseVisualStyleBackColor = true;
            btnActualizar.Click += btnActualizar_Click;
            // 
            // btnReactivarHuesped
            // 
            btnReactivarHuesped.Location = new Point(203, 8);
            btnReactivarHuesped.Name = "btnReactivarHuesped";
            btnReactivarHuesped.Size = new Size(121, 23);
            btnReactivarHuesped.TabIndex = 6;
            btnReactivarHuesped.Text = "Reactivar huésped";
            btnReactivarHuesped.UseVisualStyleBackColor = true;
            btnReactivarHuesped.Click += btnReactivarHuesped_Click;
            // 
            // FormHuespedes
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnReactivarHuesped);
            Controls.Add(btnActualizar);
            Controls.Add(btnBuscarDNI);
            Controls.Add(btnDesactivarHuesped);
            Controls.Add(btnModificarHuesped);
            Controls.Add(btnNuevoHuesped);
            Controls.Add(dgvHuespedes);
            Name = "FormHuespedes";
            Text = "FormHuespedes";
            ((System.ComponentModel.ISupportInitialize)dgvHuespedes).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvHuespedes;
        private Button btnNuevoHuesped;
        private Button btnModificarHuesped;
        private Button btnDesactivarHuesped;
        private Button btnBuscarDNI;
        private Button btnActualizar;
        private Button btnReactivarHuesped;
    }
}