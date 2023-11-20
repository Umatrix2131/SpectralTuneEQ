using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpectralTuneEQ
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            EQ.EQInit(pictureBox1); 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Open the audio file and feed the bytes in
            byte[] AudioData = new byte[20];
            double BitPerSample = 32;
            double SampleRate = 44100;
            byte[] EQProcessedData = EQ.EqualizeAudioData(AudioData, BitPerSample, SampleRate);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}
