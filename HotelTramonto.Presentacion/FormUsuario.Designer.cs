namespace HotelTramonto.Presentacion
{
    partial class FormUsuario
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
            label1 = new Label();
            txtNombre = new TextBox();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            txtApellido = new TextBox();
            txtDni = new TextBox();
            txtEmail = new TextBox();
            txtContrasena = new TextBox();
            txtTelefono = new TextBox();
            label7 = new Label();
            cmbRol = new ComboBox();
            btnGuardar = new Button();
            btnCancelar = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(47, 40);
            label1.Name = "label1";
            label1.Size = new Size(51, 15);
            label1.TabIndex = 0;
            label1.Text = "Nombre";
            label1.Click += label1_Click;
            // 
            // txtNombre
            // 
            txtNombre.Location = new Point(127, 39);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(100, 23);
            txtNombre.TabIndex = 1;
            txtNombre.TextChanged += textBox1_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(49, 71);
            label2.Name = "label2";
            label2.Size = new Size(51, 15);
            label2.TabIndex = 2;
            label2.Text = "Apellido";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(49, 100);
            label3.Name = "label3";
            label3.Size = new Size(27, 15);
            label3.TabIndex = 3;
            label3.Text = "DNI";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(49, 129);
            label4.Name = "label4";
            label4.Size = new Size(36, 15);
            label4.TabIndex = 4;
            label4.Text = "Email";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(49, 158);
            label5.Name = "label5";
            label5.Size = new Size(67, 15);
            label5.TabIndex = 5;
            label5.Text = "Contraseña";
            label5.Click += label5_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(49, 188);
            label6.Name = "label6";
            label6.Size = new Size(53, 15);
            label6.TabIndex = 6;
            label6.Text = "Teléfono";
            // 
            // txtApellido
            // 
            txtApellido.Location = new Point(127, 68);
            txtApellido.Name = "txtApellido";
            txtApellido.Size = new Size(100, 23);
            txtApellido.TabIndex = 7;
            // 
            // txtDni
            // 
            txtDni.Location = new Point(127, 97);
            txtDni.Name = "txtDni";
            txtDni.Size = new Size(98, 23);
            txtDni.TabIndex = 8;
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(127, 126);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(98, 23);
            txtEmail.TabIndex = 9;
            // 
            // txtContrasena
            // 
            txtContrasena.Location = new Point(127, 156);
            txtContrasena.Name = "txtContrasena";
            txtContrasena.Size = new Size(98, 23);
            txtContrasena.TabIndex = 10;
            // 
            // txtTelefono
            // 
            txtTelefono.Location = new Point(125, 185);
            txtTelefono.Name = "txtTelefono";
            txtTelefono.Size = new Size(100, 23);
            txtTelefono.TabIndex = 11;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(78, 225);
            label7.Name = "label7";
            label7.Size = new Size(24, 15);
            label7.TabIndex = 12;
            label7.Text = "Rol";
            // 
            // cmbRol
            // 
            cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRol.FormattingEnabled = true;
            cmbRol.Location = new Point(125, 222);
            cmbRol.Name = "cmbRol";
            cmbRol.Size = new Size(121, 23);
            cmbRol.TabIndex = 13;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(125, 275);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(75, 23);
            btnGuardar.TabIndex = 14;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(212, 275);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 23);
            btnCancelar.TabIndex = 15;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            // 
            // FormUsuario
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(cmbRol);
            Controls.Add(label7);
            Controls.Add(txtTelefono);
            Controls.Add(txtContrasena);
            Controls.Add(txtEmail);
            Controls.Add(txtDni);
            Controls.Add(txtApellido);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(txtNombre);
            Controls.Add(label1);
            MaximizeBox = false;
            Name = "FormUsuario";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Registrar usuario - Hotel Tramonto";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtNombre;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private TextBox txtApellido;
        private TextBox txtDni;
        private TextBox txtEmail;
        private TextBox txtContrasena;
        private TextBox txtTelefono;
        private Label label7;
        private ComboBox cmbRol;
        private Button btnGuardar;
        private Button btnCancelar;
    }
}