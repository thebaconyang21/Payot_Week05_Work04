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
