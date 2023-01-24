using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game_of_Life
{
    public partial class OptionsDialog : Form
    {
        public OptionsDialog()
        {
            InitializeComponent();
        }

        public int Interval
        {
            get
            {
                return (int)numericUpDown1.Value;
            }
            set
            {
                numericUpDown1.Value = value;
            }
        }

        public int Width
        {
            get
            {
                return (int)numericUpDown2.Value;
            }
            set
            {
                numericUpDown2.Value = value;
            }
        }

        public int Height
        {
            get
            {
                return (int)numericUpDown3.Value;
            }
            set
            {
                numericUpDown3.Value = value;
            }
        }
    }
}
