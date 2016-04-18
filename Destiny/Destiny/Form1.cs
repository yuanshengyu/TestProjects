using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Destiny
{
    public partial class Form1 : Form
    {
        private int Step=0;
        private string Sex;
        private string What;
        private int PickWhat;
        public Info infofrm;
        public BoyGirl sexfrm;
        public ForWhat whatfrm;
        public Pick pickfrm;
        public Show showfrm;
        public Form1()
        {
            InitializeComponent();

            infofrm = new Info();
            infofrm.TopLevel = false;
            this.Controls.Add(infofrm);
            infofrm.Location = new Point(10, 10);
            infofrm.Show();
            Next();
        }
        private void Next()
        {
            int y2 = 10 + infofrm.Size.Height + 5;
            if (Step == 0)
            {
                infofrm.SetInfo("告诉升哥你是boy 还是 girl");
                sexfrm = new BoyGirl();
                sexfrm.TopLevel = false;
                this.Controls.Add(sexfrm);
                sexfrm.Location = new Point(10, y2);
                sexfrm.Show();
            }
            else if (Step == 1)
            {
                int result = sexfrm.GetSex();
                if (result == 1)
                {
                    Sex = "小伙子";
                }
                else if (result == 2)
                {
                    Sex = "闺女";
                }
                else
                {
                    Sex = "东方姑娘";
                }
                infofrm.SetInfo(Sex+"，想算什么丫？");
                sexfrm.Close();
                this.Controls.Remove(sexfrm);

                whatfrm = new ForWhat();
                whatfrm.TopLevel = false;
                this.Controls.Add(whatfrm);
                whatfrm.Location = new Point(10,y2);
                whatfrm.Show();
            }
            else if (Step == 2)
            {
                What = whatfrm.GetForWhat();
                infofrm.SetInfo(string.Format("想算{0}呐，抽枝签吧 - -", What));

                whatfrm.Close();
                this.Controls.Remove(whatfrm);

                pickfrm = new Pick();
                pickfrm.OnPick +=new PickHandler(GetPick);
                pickfrm.TopLevel = false;
                this.Controls.Add(pickfrm);
                pickfrm.Location = new Point(10, y2);
                pickfrm.Show();
                button1.Enabled = false;
            }
            else if (Step == 3)
            {
                pickfrm.Close();
                this.Controls.Remove(pickfrm);

                Future future = AnalysePick.GetFuture(PickWhat);
                infofrm.SetInfo(string.Format("{0}:{1}", future.Name, future.ShortCharge));

                showfrm = new Show(future, What);
                showfrm.TopLevel = false;
                this.Controls.Add(showfrm);
                showfrm.Location = new Point(10, y2);
                showfrm.Show();
            }
            Step++;
        }
        private void GetPick(int pick)
        {
            button1.Enabled = true;
            PickWhat = pick;
            Next();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Step == 4)
            {
                this.Close();
            }
            Next();
        }
    }
}
