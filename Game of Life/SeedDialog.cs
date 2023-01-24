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
    public partial class Seed_Dialog : Form
    {
        public Seed_Dialog()
        {
            InitializeComponent();
        }
        public int Seed
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

        private void button3_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            Seed = r.Next();
        }
    }
}
