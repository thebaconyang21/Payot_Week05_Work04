using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Payot_Week05_Work04
{
    public partial class frmProductForm : Form
    {

        private readonly List<Product> products = new List<Product>();
        private readonly ErrorProvider errorProvider = new ErrorProvider();
        private Control firstInvalid;
        private bool isCalculating = false;
        public frmProductForm()
        {
            InitializeComponent();
            SetupForm();
            WireEvents();
        }

        private void SetupForm()
        {
            AcceptButton = btnSave;
            CancelButton = btnCancel;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            txtDescription.Multiline = true;
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.AcceptsReturn = true;

            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategory.Items.AddRange(new object[] 
            { 
                "Beverages", "Canned Goods", "Snacks", "Personal Care", "Household", "Others" 
            });

            rdoLocal.Checked = true;
            chkActive.Checked = false;

            lblBarcodeStatus.Text = "";
            lblBarcodeStatus.Font = new Font(lblBarcodeStatus.Font, FontStyle.Italic);

            // Set the error provider to display errors next to the controls
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
        }

        private void WireEvents()
        {
            // KeyPress runs every time a key is pressed in the box
            txtBarcode.KeyPress += DigitsOnly_KeyPress; // barcode: digits only
            txtBarcode.TextChanged += txtBarcode_TextChanged;
            txtCost.KeyPress += DecimalOnly_KeyPress;   // numbers + one "."
            txtPrice.KeyPress += DecimalOnly_KeyPress;
            txtMarkup.KeyPress += DecimalOnly_KeyPress;

        }

        private void DigitsOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            // block anything that is not a digit (Backspace is a "control" key, so it still works)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true; // Handled = true cancels the key
        }

        private void DecimalOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox tb = (TextBox)sender; // which box was typed in

            if (char.IsControl(e.KeyChar) || char.IsDigit(e.KeyChar))
                return; // Backspace and digits are OK

            if (e.KeyChar == '.' && !tb.Text.Contains("."))
                return; // allow only one decimal point

            e.Handled = true; // block everything else
        }

        private void txtBarcode_TextChanged(object sender, EventArgs e)
        {
            string code = txtBarcode.Text.Trim(); // text without extra spaces

            if (code.Length == 0)
            {
                lblBarcodeStatus.Text = ""; // empty box = no message
                return;
            }

            // does any saved product already have this barcode?
            bool exists = products.Exists(p => p.Barcode == code);

            lblBarcodeStatus.Text = exists ? "Barcode is already in use." : "Barcode is available.";
            lblBarcodeStatus.ForeColor = exists ? Color.Firebrick : Color.SeaGreen; // red or green
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void frmProductForm_Load(object sender, EventArgs e)
        {

        }
    }

    public class Product
    {
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Origin { get; set; }
        public bool IsActive { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public decimal Markup { get; set; }
        public decimal Profit { get { return Price - Cost; } }
    }
}
