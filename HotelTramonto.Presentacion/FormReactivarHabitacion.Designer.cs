namespace HotelTramonto.Presentacion
{
    partial class FormReactivarHabitacion
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
            lblTitulo = new Label();
            dgvHabitacionesInactivas = new DataGridView();
            btnReactivar = new Button();
            btnCancelar = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvHabitacionesInactivas).BeginInit();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(33, 23);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(125, 15);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Habitaciones inactivas";
            // 
            // dgvHabitacionesInactivas
            // 
            dgvHabitacionesInactivas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvHabitacionesInactivas.Location = new Point(33, 50);
            dgvHabitacionesInactivas.Name = "dgvHabitacionesInactivas";
            dgvHabitacionesInactivas.Size = new Size(733, 150);
            dgvHabitacionesInactivas.TabIndex = 1;
            // 
            // btnReactivar
            // 
            btnReactivar.Location = new Point(33, 229);
            btnReactivar.Name = "btnReactivar";
            btnReactivar.RightToLeft = RightToLeft.No;
            btnReactivar.Size = new Size(128, 23);
            btnReactivar.TabIndex = 2;
            btnReactivar.Text = "Reactivar habitación";
            btnReactivar.UseVisualStyleBackColor = true;
            btnReactivar.Click += btnReactivar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(167, 229);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 23);
            btnCancelar.TabIndex = 3;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // FormReactivarHabitacion
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnCancelar);
            Controls.Add(btnReactivar);
            Controls.Add(dgvHabitacionesInactivas);
            Controls.Add(lblTitulo);
            Name = "FormReactivarHabitacion";
            Text = "FormReactivarHabitacion";
            ((System.ComponentModel.ISupportInitialize)dgvHabitacionesInactivas).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitulo;
        private DataGridView dgvHabitacionesInactivas;
        private Button btnReactivar;
        private Button btnCancelar;
    }
}