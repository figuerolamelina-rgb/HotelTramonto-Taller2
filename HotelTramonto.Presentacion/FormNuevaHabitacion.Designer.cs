namespace HotelTramonto.Presentacion
{
    partial class FormNuevaHabitacion
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
            lblTipo = new Label();
            cmbTipoHabitacion = new ComboBox();
            lblPiso = new Label();
            lblNumero = new Label();
            txtPiso = new TextBox();
            txtNumero = new TextBox();
            lblEstado = new Label();
            cmbEstado = new ComboBox();
            btnGuardar = new Button();
            btnCancelar = new Button();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(278, 9);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.RightToLeft = RightToLeft.No;
            lblTitulo.Size = new Size(100, 15);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Nueva habitación";
            // 
            // lblTipo
            // 
            lblTipo.AutoSize = true;
            lblTipo.Location = new Point(74, 86);
            lblTipo.Name = "lblTipo";
            lblTipo.Size = new Size(109, 15);
            lblTipo.TabIndex = 1;
            lblTipo.Text = "Tipo de habitación:";
            // 
            // cmbTipoHabitacion
            // 
            cmbTipoHabitacion.FormattingEnabled = true;
            cmbTipoHabitacion.Location = new Point(189, 83);
            cmbTipoHabitacion.Name = "cmbTipoHabitacion";
            cmbTipoHabitacion.Size = new Size(121, 23);
            cmbTipoHabitacion.TabIndex = 2;
            // 
            // lblPiso
            // 
            lblPiso.AutoSize = true;
            lblPiso.Location = new Point(96, 125);
            lblPiso.Name = "lblPiso";
            lblPiso.Size = new Size(32, 15);
            lblPiso.TabIndex = 3;
            lblPiso.Text = "Piso:";
            // 
            // lblNumero
            // 
            lblNumero.AutoSize = true;
            lblNumero.Location = new Point(96, 164);
            lblNumero.Name = "lblNumero";
            lblNumero.Size = new Size(54, 15);
            lblNumero.TabIndex = 5;
            lblNumero.Text = "Número:";
            // 
            // txtPiso
            // 
            txtPiso.Location = new Point(189, 122);
            txtPiso.Name = "txtPiso";
            txtPiso.Size = new Size(100, 23);
            txtPiso.TabIndex = 6;
            // 
            // txtNumero
            // 
            txtNumero.Location = new Point(189, 164);
            txtNumero.Name = "txtNumero";
            txtNumero.Size = new Size(100, 23);
            txtNumero.TabIndex = 7;
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = true;
            lblEstado.Location = new Point(96, 203);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(45, 15);
            lblEstado.TabIndex = 8;
            lblEstado.Text = "Estado:";
            // 
            // cmbEstado
            // 
            cmbEstado.FormattingEnabled = true;
            cmbEstado.Location = new Point(189, 200);
            cmbEstado.Name = "cmbEstado";
            cmbEstado.Size = new Size(121, 23);
            cmbEstado.TabIndex = 9;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(214, 246);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(75, 23);
            btnGuardar.TabIndex = 10;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(303, 246);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 23);
            btnCancelar.TabIndex = 11;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            // 
            // FormNuevaHabitacion
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(cmbEstado);
            Controls.Add(lblEstado);
            Controls.Add(txtNumero);
            Controls.Add(txtPiso);
            Controls.Add(lblNumero);
            Controls.Add(lblPiso);
            Controls.Add(cmbTipoHabitacion);
            Controls.Add(lblTipo);
            Controls.Add(lblTitulo);
            Name = "FormNuevaHabitacion";
            Text = "FormNuevaHabitacion";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitulo;
        private Label lblTipo;
        private ComboBox cmbTipoHabitacion;
        private Label lblPiso;
        private Label lblNumero;
        private TextBox txtPiso;
        private TextBox txtNumero;
        private Label lblEstado;
        private ComboBox cmbEstado;
        private Button btnGuardar;
        private Button btnCancelar;
    }
}