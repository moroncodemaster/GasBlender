using GasMixLogic.Models;

namespace GasMixerTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //textBox5.Text = "32";
            //textBox1.Text = "34.5";
            //textBox6.Text = "32";
            //textBox2.Text = "207";
            //textBox3.Text = "40";
            textBox4.Text = "21";
            radioButton2.Checked = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var startingMix = new GasMix();
            var tartgetMix = new GasMix();
            var firstTopMix = new GasMix();
            var finalTopMix = new GasMix();

            startingMix.Oxygen = double.Parse(textBox5.Text);
            startingMix.Pressure = double.Parse(textBox1.Text);

            tartgetMix.Oxygen = double.Parse(textBox6.Text);
            tartgetMix.Pressure = double.Parse(textBox2.Text);

            firstTopMix.Oxygen = double.Parse(textBox3.Text);
            //startingMix.Pressure = double.Parse(textBox1.Text);

            finalTopMix.Oxygen = double.Parse(textBox4.Text);
            //startingMix.Pressure = double.Parse(textBox1.Text);

            var isPsi = true;
            isPsi = radioButton1.Checked;

            var gasCalculator =
                new GasMixLogic.GasCalculator(startingMix, tartgetMix, firstTopMix, null, finalTopMix, isPsi);

            var dirs = gasCalculator.CalculateNitroxGasMix();


            textBox7.Text = dirs;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "21";
            textBox5.Text = "";
            textBox6.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var startingMix = new GasMix();
            var tartgetMix = new GasMix();
            var firstTopMix = new GasMix();
            var secondTopMix = new GasMix();
            var finalTopMix = new GasMix();

            startingMix.Oxygen = double.Parse(textBox10.Text);
            startingMix.Pressure = double.Parse(textBox14.Text);
            startingMix.Helium = double.Parse(textBox18.Text);

            tartgetMix.Oxygen = double.Parse(textBox9.Text);
            tartgetMix.Helium = double.Parse(textBox17.Text);
            tartgetMix.Pressure = double.Parse(textBox13.Text);

            firstTopMix.Oxygen = double.Parse(textBox12.Text);
            firstTopMix.Helium = double.Parse(textBox16.Text);
            
            secondTopMix.Oxygen = double.Parse(textBox20.Text);
            secondTopMix.Helium = double.Parse(textBox19.Text);

            finalTopMix.Oxygen = double.Parse(textBox11.Text);
            finalTopMix.Helium = double.Parse(textBox15.Text);

            var isPsi = true;
            isPsi = radioButton1.Checked;

            var gasCalculator =
                new GasMixLogic.GasCalculator(startingMix, tartgetMix, firstTopMix, null, finalTopMix, isPsi);

            var dirs = gasCalculator.CalculateNitroxGasMix();


            textBox8.Text = dirs;

        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox9.Text = "";
            textBox10.Text = "";
            textBox11.Text = "";
            textBox12.Text = "";
            textBox13.Text = "";
            textBox14.Text = "";
            textBox15.Text = "";
            textBox16.Text = "";
            textBox17.Text = "";
            textBox18.Text = "";
            textBox19.Text = "";
            textBox20.Text = "";
        }
    }
}