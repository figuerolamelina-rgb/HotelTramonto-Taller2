namespace HotelTramonto.Presentacion
{
    partial class FormModificarHabitacion
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
            txtPiso = new TextBox();
            txtNumero = new TextBox();
            lblNumero = new Label();
            lblEstado = new Label();
            cmbEstado = new ComboBox();
            btnGuardarCambios = new Button();
            btnCancelar = new Button();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(32, 18);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(117, 15);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Modificar habitación";
            // 
            // lblTipo
            // 
            lblTipo.AutoSize = true;
            lblTipo.Location = new Point(40, 51);
            lblTipo.Name = "lblTipo";
            lblTipo.Size = new Size(109, 15);
            lblTipo.TabIndex = 1;
            lblTipo.Text = "Tipo de habitación:";
            // 
            // cmbTipoHabitacion
            // 
            cmbTipoHabitacion.FormattingEnabled = true;
            cmbTipoHabitacion.Location = new Point(155, 48);
            cmbTipoHabitacion.Name = "cmbTipoHabitacion";
            cmbTipoHabitacion.Size = new Size(121, 23);
            cmbTipoHabitacion.TabIndex = 2;
            // 
            // lblPiso
            // 
            lblPiso.AutoSize = true;
            lblPiso.Location = new Point(98, 86);
            lblPiso.Name = "lblPiso";
            lblPiso.Size = new Size(32, 15);
            lblPiso.TabIndex = 3;
            lblPiso.Text = "Piso:";
            // 
            // txtPiso
            // 
            txtPiso.Location = new Point(155, 78);
            txtPiso.Name = "txtPiso";
            txtPiso.Size = new Size(107, 23);
            txtPiso.TabIndex = 4;
            // 
            // txtNumero
            // 
            txtNumero.Location = new Point(155, 119);
            txtNumero.Name = "txtNumero";
            txtNumero.Size = new Size(72, 23);
            txtNumero.TabIndex = 5;
            // 
            // lblNumero
            // 
            lblNumero.AutoSize = true;
            lblNumero.Location = new Point(92, 122);
            lblNumero.Name = "lblNumero";
            lblNumero.Size = new Size(54, 15);
            lblNumero.TabIndex = 6;
            lblNumero.Text = "Número:";
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = true;
            lblEstado.Location = new Point(98, 157);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(45, 15);
            lblEstado.TabIndex = 7;
            lblEstado.Text = "Estado:";
            // 
            // cmbEstado
            // 
            cmbEstado.FormattingEnabled = true;
            cmbEstado.Location = new Point(155, 154);
            cmbEstado.Name = "cmbEstado";
            cmbEstado.Size = new Size(121, 23);
            cmbEstado.TabIndex = 8;
            // 
            // btnGuardarCambios
            // 
            btnGuardarCambios.Location = new Point(92, 213);
            btnGuardarCambios.Name = "btnGuardarCambios";
            btnGuardarCambios.Size = new Size(117, 23);
            btnGuardarCambios.TabIndex = 9;
            btnGuardarCambios.Text = "Guardar cambios";
            btnGuardarCambios.UseVisualStyleBackColor = true;
            btnGuardarCambios.Click += btnGuardarCambios_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(215, 213);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 23);
            btnCancelar.TabIndex = 10;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            // 
            // FormModificarHabitacion
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(551, 286);
            Controls.Add(btnCancelar);
            Controls.Add(btnGuardarCambios);
            Controls.Add(cmbEstado);
            Controls.Add(lblEstado);
            Controls.Add(lblNumero);
            Controls.Add(txtNumero);
            Controls.Add(txtPiso);
            Controls.Add(lblPiso);
            Controls.Add(cmbTipoHabitacion);
            Controls.Add(lblTipo);
            Controls.Add(lblTitulo);
            Name = "FormModificarHabitacion";
            Text = "FormModificarHabitacion";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitulo;
        private Label lblTipo;
        private ComboBox cmbTipoHabitacion;
        private Label lblPiso;
        private TextBox txtPiso;
        private TextBox txtNumero;
        private Label lblNumero;
        private Label lblEstado;
        private ComboBox cmbEstado;
        private Button btnGuardarCambios;
        private Button btnCancelar;
    }
}