using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
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
            txtCost.TextChanged += txtCost_TextChanged;   
            txtMarkup.TextChanged += txtMarkup_TextChanged;
            txtPrice.TextChanged += txtPrice_TextChanged;

            btnSave.Click += btnSave_Click; // tempo

            // remove a field's red icon as soon as the user edits it
            txtBarcode.TextChanged += ClearError_Changed;
            txtName.TextChanged += ClearError_Changed;
            cmbCategory.SelectedIndexChanged += ClearError_Changed;
            txtCost.TextChanged += ClearError_Changed;
            txtPrice.TextChanged += ClearError_Changed;
            txtMarkup.TextChanged += ClearError_Changed;

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

        // converts text to a number; returns false if it isn't a valid number
        private static bool TryParse(string text, out decimal value)
        {
            return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        // puts a number into a textbox with 2 decimals, without triggering the other handlers
        private void SetText(TextBox target, decimal value)
        {
            isCalculating = true; // switch ON so we don't loop
            try
            {
                target.Text = value.ToString("0.00", CultureInfo.InvariantCulture);
            }
            finally
            {
                isCalculating = false; // always switch OFF again
            }
        }

        private void txtCost_TextChanged(object sender, EventArgs e)
        {
            if (isCalculating) return; // ignore changes made by our own code

            decimal cost, markup, price;
            if (!TryParse(txtCost.Text, out cost)) return; // no valid cost yet

            if (TryParse(txtMarkup.Text, out markup))
                SetText(txtPrice, cost * (1 + markup / 100m));            // cost + markup -> price
            else if (cost > 0 && TryParse(txtPrice.Text, out price))
                SetText(txtMarkup, (price - cost) / cost * 100m);         // cost + price -> markup
        }

        private void txtMarkup_TextChanged(object sender, EventArgs e)
        {
            if (isCalculating) return;

            decimal cost, markup;
            if (TryParse(txtCost.Text, out cost) && TryParse(txtMarkup.Text, out markup))
                SetText(txtPrice, cost * (1 + markup / 100m)); // new markup -> new price
        }

        private void txtPrice_TextChanged(object sender, EventArgs e)
        {
            if (isCalculating) return;

            decimal cost, price;
            if (TryParse(txtCost.Text, out cost) && cost > 0 && TryParse(txtPrice.Text, out price))
                SetText(txtMarkup, (price - cost) / cost * 100m); // new price -> new markup
        }

        private void ClearError_Changed(object sender, EventArgs e)
        {
            errorProvider.SetError((Control)sender, ""); // empty message = no icon
        }

        private void SetError(Control control, string message)
        {
            errorProvider.SetError(control, message);      // show red icon + message
            if (firstInvalid == null) firstInvalid = control; // remember the first bad field
        }

        private bool ValidateForm()
        {
            errorProvider.Clear(); // remove old icons
            firstInvalid = null;

            // Barcode: required and unique
            string barcode = txtBarcode.Text.Trim();
            if (barcode.Length == 0)
                SetError(txtBarcode, "Barcode is required.");
            else if (products.Exists(p => p.Barcode == barcode))
                SetError(txtBarcode, "This barcode is already in use.");

            // Name: required
            if (txtName.Text.Trim().Length == 0)
                SetError(txtName, "Name is required.");

            // Category: something must be selected (-1 means nothing)
            if (cmbCategory.SelectedIndex < 0)
                SetError(cmbCategory, "Please select a category.");

            decimal cost, price, markup;

            // Cost: valid number above 0
            if (!TryParse(txtCost.Text, out cost) || cost <= 0)
                SetError(txtCost, "Enter a cost greater than 0.");

            // Price: valid number above 0 and not lower than cost
            if (!TryParse(txtPrice.Text, out price) || price <= 0)
                SetError(txtPrice, "Enter a price greater than 0.");
            else if (cost > 0 && price < cost)
                SetError(txtPrice, "Price cannot be lower than cost.");

            // Markup: valid number, 0 or more
            if (!TryParse(txtMarkup.Text, out markup) || markup < 0)
                SetError(txtMarkup, "Enter a markup of 0 or more.");

            // If anything failed, put the cursor on the first bad field
            if (firstInvalid != null)
            {
                firstInvalid.Focus();
                return false; // not valid
            }
            return true; // all good
        }

        // TEMPORARY: Step 6 will replace this with the real Save
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
                MessageBox.Show("All fields are valid!");
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
