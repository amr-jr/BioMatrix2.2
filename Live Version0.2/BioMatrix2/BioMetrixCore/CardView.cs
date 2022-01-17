using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BioMetrixCore
{
    public partial class CardView : UserControl
    {
        private Image _empIMG;
        private string _empName;
        private string _empID;
        private string _empTime;
        private string _empbb;


        [Category("CardView Property")]
        public Image empIMG
        {
            get { return _empIMG; }
            set { _empIMG = value; empIMG_picBox.Image = value; }
        }

        [Category("CardView Property")]
        public string empName
        {
            get { return _empName; }
            set { _empName = value; empName_labl.Text = value; }
        }

        [Category("CardView Property")]
        public string empID
        {
            get { return _empID; }
            set { _empID = value; empID_labl.Text = value; }
        }

        [Category("CardView Property")]
        public string empTime
        {
            get { return _empTime; }
            set { _empTime = value; empTime_labl.Text = value; }
        }

        [Category("CardView Property")]
        public string empbb
        {
            get { return _empbb; }
            set { _empbb = value; empbb_labl.Text = value; }
        }

        public CardView()
        {
            InitializeComponent();
        }
    }
}
