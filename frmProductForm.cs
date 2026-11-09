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
            btnClear.Click += btnClear_Click;   
            btnCancel.Click += btnCancel_Click; 

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

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return; // stop if any field is invalid

            // build a Product from what the user typed
            Product product = new Product
            {
                Barcode = txtBarcode.Text.Trim(),
                Name = txtName.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Category = cmbCategory.SelectedItem.ToString(),
                Origin = rdoLocal.Checked ? "Local" : "Abroad", // which radio is selected
                IsActive = chkActive.Checked,
                Cost = decimal.Parse(txtCost.Text, CultureInfo.InvariantCulture),
                Price = decimal.Parse(txtPrice.Text, CultureInfo.InvariantCulture),
                Markup = decimal.Parse(txtMarkup.Text, CultureInfo.InvariantCulture)
            };

            products.Add(product); // store it in the list

            // show a summary ("\n" = new line)
            MessageBox.Show(
                "Product saved!\n\n" +
                "Barcode: " + product.Barcode + "\n" +
                "Name: " + product.Name + "\n" +
                "Category: " + product.Category + "\n" +
                "Origin: " + product.Origin + "\n" +
                "Status: " + (product.IsActive ? "Active" : "Inactive") + "\n" +
                "Cost: ₱" + product.Cost.ToString("N2") + "\n" +
                "Price: ₱" + product.Price.ToString("N2") + "\n" +
                "Markup: " + product.Markup.ToString("N2") + "%\n" +
                "Profit per item: ₱" + product.Profit.ToString("N2") + "\n\n" +
                "Total products saved: " + products.Count,
                "Product Form", MessageBoxButtons.OK, MessageBoxIcon.Information);

            ClearForm(); // get ready for the next product
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (HasData()) // only ask if something was typed
            {
                DialogResult result = MessageBox.Show(
                    "Discard the information you entered and close the form?",
                    "Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No) return; // stay on the form
            }
            Close(); // close the form
        }

        // true if any field has something in it
        private bool HasData()
        {
            return txtBarcode.Text.Length > 0 || txtName.Text.Length > 0 ||
                   txtDescription.Text.Length > 0 || cmbCategory.SelectedIndex >= 0 ||
                   txtCost.Text.Length > 0 || txtPrice.Text.Length > 0 ||
                   txtMarkup.Text.Length > 0;
        }

        private void ClearForm()
        {
            isCalculating = true; // so clearing doesn't trigger the calculations
            try
            {
                txtBarcode.Clear();
                txtName.Clear();
                txtDescription.Clear();
                cmbCategory.SelectedIndex = -1; // nothing selected
                rdoLocal.Checked = true;        // back to default
                chkActive.Checked = false;
                txtCost.Clear();
                txtPrice.Clear();
                txtMarkup.Clear();
                lblBarcodeStatus.Text = "";
            }
            finally
            {
                isCalculating = false;
            }

            errorProvider.Clear(); // remove red icons
            txtBarcode.Focus();    // cursor back to the first field
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
}
