
namespace HotelTramonto.Presentacion
{
    partial class FormModificarHuesped
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.Label lblApellido;
        private System.Windows.Forms.Label lblDNI;
        private System.Windows.Forms.Label lblTelefono;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.Label lblNacionalidad;

        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.TextBox txtApellido;
        private System.Windows.Forms.TextBox txtDNI;
        private System.Windows.Forms.TextBox txtTelefono;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.TextBox txtNacionalidad;

        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblNombre = new System.Windows.Forms.Label();
            lblApellido = new System.Windows.Forms.Label();
            lblDNI = new System.Windows.Forms.Label();
            lblTelefono = new System.Windows.Forms.Label();
            lblEmail = new System.Windows.Forms.Label();
            lblNacionalidad = new System.Windows.Forms.Label();

            txtNombre = new System.Windows.Forms.TextBox();
            txtApellido = new System.Windows.Forms.TextBox();
            txtDNI = new System.Windows.Forms.TextBox();
            txtTelefono = new System.Windows.Forms.TextBox();
            txtEmail = new System.Windows.Forms.TextBox();
            txtNacionalidad = new System.Windows.Forms.TextBox();

            btnGuardar = new System.Windows.Forms.Button();
            btnCancelar = new System.Windows.Forms.Button();

            SuspendLayout();

            lblNombre.Text = "Nombre";
            lblNombre.Location = new System.Drawing.Point(30, 30);
            lblNombre.AutoSize = true;

            txtNombre.Location = new System.Drawing.Point(140, 27);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new System.Drawing.Size(200, 23);

            lblApellido.Text = "Apellido";
            lblApellido.Location = new System.Drawing.Point(30, 70);
            lblApellido.AutoSize = true;

            txtApellido.Location = new System.Drawing.Point(140, 67);
            txtApellido.Name = "txtApellido";
            txtApellido.Size = new System.Drawing.Size(200, 23);

            lblDNI.Text = "DNI";
            lblDNI.Location = new System.Drawing.Point(30, 110);
            lblDNI.AutoSize = true;

            txtDNI.Location = new System.Drawing.Point(140, 107);
            txtDNI.Name = "txtDNI";
            txtDNI.Size = new System.Drawing.Size(200, 23);

            lblTelefono.Text = "Teléfono";
            lblTelefono.Location = new System.Drawing.Point(30, 150);
            lblTelefono.AutoSize = true;

            txtTelefono.Location = new System.Drawing.Point(140, 147);
            txtTelefono.Name = "txtTelefono";
            txtTelefono.Size = new System.Drawing.Size(200, 23);

            lblEmail.Text = "Email";
            lblEmail.Location = new System.Drawing.Point(30, 190);
            lblEmail.AutoSize = true;

            txtEmail.Location = new System.Drawing.Point(140, 187);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new System.Drawing.Size(200, 23);

            lblNacionalidad.Text = "Nacionalidad";
            lblNacionalidad.Location = new System.Drawing.Point(30, 230);
            lblNacionalidad.AutoSize = true;

            txtNacionalidad.Location = new System.Drawing.Point(140, 227);
            txtNacionalidad.Name = "txtNacionalidad";
            txtNacionalidad.Size = new System.Drawing.Size(200, 23);

            btnGuardar.Text = "Guardar cambios";
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Location = new System.Drawing.Point(100, 285);
            btnGuardar.Size = new System.Drawing.Size(120, 35);
            btnGuardar.Click += btnGuardar_Click;

            btnCancelar.Text = "Cancelar";
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Location = new System.Drawing.Point(240, 285);
            btnCancelar.Size = new System.Drawing.Size(100, 35);
            btnCancelar.Click += btnCancelar_Click;

            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(400, 350);
            Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblNombre, txtNombre,
                lblApellido, txtApellido,
                lblDNI, txtDNI,
                lblTelefono, txtTelefono,
                lblEmail, txtEmail,
                lblNacionalidad, txtNacionalidad,
                btnGuardar, btnCancelar
            });

            Name = "FormModificarHuesped";
            Text = "Modificar huésped";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

            ResumeLayout(false);
            PerformLayout();
        }
    }
}
