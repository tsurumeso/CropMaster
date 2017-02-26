using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace CropMaster
{
    internal class NumericUpDownEx : NumericUpDown
    {
        public NumericUpDownEx()
        {
            this.Enter += new EventHandler(ToolstripNumericUpDown_Enter);
        }

        void ToolstripNumericUpDown_Enter(object sender, EventArgs e)
        {
            this.Select(0, this.Text.Length);
        }
    }

    [ToolStripItemDesignerAvailability(
        ToolStripItemDesignerAvailability.ToolStrip |
        ToolStripItemDesignerAvailability.StatusStrip)]
    internal class ToolStripNumericUpDown : ToolStripControlHost
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ToolStripNumericUpDown() : base(new NumericUpDownEx())
        {
        }

        public NumericUpDownEx NumericUpDown
        {
            get { return Control as NumericUpDownEx; }
        }

        public decimal Value
        {
            get { return NumericUpDown.Value; }
            set { NumericUpDown.Value = value; }
        }
        public decimal Minimum
        {
            get { return NumericUpDown.Minimum; }
            set { NumericUpDown.Minimum = value; }
        }

        public decimal Maximum
        {
            get { return NumericUpDown.Maximum; }
            set { NumericUpDown.Maximum = value; }
        }

        public decimal Increment
        {
            get { return NumericUpDown.Increment; }
            set { NumericUpDown.Increment = value; }
        }

        public int DecimalPlaces
        {
            get { return NumericUpDown.DecimalPlaces; }
            set { NumericUpDown.DecimalPlaces = value; }
        }

        protected override void OnSubscribeControlEvents(Control control)
        {
            base.OnSubscribeControlEvents(control);
            NumericUpDown.ValueChanged += _valueChanged;
        }

        protected override void OnUnsubscribeControlEvents(Control control)
        {
            base.OnUnsubscribeControlEvents(control);
            NumericUpDown.ValueChanged -= _valueChanged;
        }

        private void _valueChanged(object sender, EventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Value"));
        }
    }
}