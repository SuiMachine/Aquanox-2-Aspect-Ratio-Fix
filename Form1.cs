using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Aquanox2Widescreen
{
    public partial class Form1 : Form
    {
        static string _GamesExecutable = "AN2.dat";
        static string _GamesExecutableBackup = "AN2.dat.bak";
        static string PCGW_URL = "http://pcgamingwiki.com/";
        static string donationURL = "https://www.twitchalerts.com/donate/suicidemachine";

        int width = 1280;
        int height = 720;
        float aspectRatio = 1.7777777f;

        byte[] sequence = { 0x68, 0x39, 0x8E, 0xE3, 0x3F, 0x51, 0x8B, 0xD1, 0xD9, 0x1C, 0x24, 0x52, 0x8D, 0x9C, 0x24, 0x90 };
        int sequenceoffset = 1;
        int adress = 0;
        byte[] AspectAsArray = new byte[4];
        byte[] data;
        byte[] nav_sequence1 = { 0xC0, 0x8B, 0x7C, 0x24, 0x3C, 0x68, 0xAB, 0xAA, 0xAA, 0x3F, 0x68, 0x00, 0x00, 0xF0, 0x43, 0x68 };      //Nav stretching
        int nav_sequence1offset = 6;
        int nav_sequence1adress = 0;
        byte[] nav_sequence2 = { 0xC7, 0x40, 0x04, 0xAB, 0xAA, 0xAA, 0x3F, 0x8B, 0x4C, 0x24, 0x64, 0x8B, 0x91, 0x84, 0x83, 0x01 };      //Nav zoom-in distance
        int nav_sequence2offset = 3;
        int nav_sequence2adress = 0;

        bool autoCalculate = true;

        public Form1()
        {
            InitializeComponent();
            if(!File.Exists(@_GamesExecutable))
            {
                MessageBox.Show("No executable found. Please place the file in a folder with a game.");
                Close();
            }
            else
            {
                data = GetBytesFromAFile(@_GamesExecutable);

                string s = BitConverter.ToString(data);

                adress = findSequence(data, sequence, sequenceoffset);
                nav_sequence1adress = findSequence(data, nav_sequence1, nav_sequence1offset);
                nav_sequence2adress = findSequence(data, nav_sequence2, nav_sequence2offset);

                if (adress == -1 && nav_sequence1adress != -1 && nav_sequence2adress != -1)
                {
                    MessageBox.Show("Nothing found in the file. Sorry.");
                    Close();
                }
                else
                {
                    Trace.WriteLine("Found address: 0x" + adress.ToString("X4"));
                    Trace.WriteLine("Found address: 0x" + nav_sequence1adress.ToString("X4"));
                    Trace.WriteLine("Found address: 0x" + nav_sequence2adress.ToString("X4"));
                    aspectRatio = BitConverter.ToSingle(data, adress);
                    TB_AspectRatio.Text = aspectRatio.ToString();
                }
            }
        }

        #region FindAdress
        private int findSequence(byte[] source, byte[] _sequence, int offset)
        {
            for (int adress = 0; adress < source.Length; adress++)
            {
                if (compareByteArrays(_sequence, source, adress, offset))
                {
                    return adress + offset;
                }
            }
            return -1;
        }

        private bool compareByteArrays(byte[] sequenceArray, byte[] dataArray, int lookFrom, int dataOffset)
        {
            if(dataArray.Length-lookFrom > sequenceArray.Length)
            {
                for (int i = 0; i<sequenceArray.Length; i++)
                {
                    if (i == dataOffset)
                        i = i + 4;
                    else if (sequenceArray[i] != dataArray[lookFrom + i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
#endregion

        #region Buttons_Lables_and_Stuff
        private void TB_ResX_TextChanged(object sender, EventArgs e)
        {
            var temp = 1280;
            if (int.TryParse(TB_ResX.Text, out temp))
            {
                width = temp;
            }
            else
                width = 1280;

            if (autoCalculate)
            {
                aspectRatio = CalculateAspect(width, height);
                TB_AspectRatio.Text = aspectRatio.ToString();
            }
        }

        private void TB_ResY_TextChanged(object sender, EventArgs e)
        {
            var temp = 720;
            if (int.TryParse(TB_ResY.Text, out temp))
            {
                height = temp;
            }
            else
                height = 720;

            if (autoCalculate)
            {
                aspectRatio = CalculateAspect(width, height);
                TB_AspectRatio.Text = aspectRatio.ToString();
            }
        }

        float CalculateAspect(int X, int Y)
        {
            float outputValue = X*1.0f / Y*1.0f;
            return outputValue;
        }

        private void C_AutomaticAspect_CheckedChanged(object sender, EventArgs e)
        {
            if (C_AutomaticAspect.Checked)
            {
                autoCalculate = true;
                TB_AspectRatio.Enabled = false;
                aspectRatio = CalculateAspect(width, height);
                TB_AspectRatio.Text = aspectRatio.ToString();
            }
            else
            {
                autoCalculate = false;
                TB_AspectRatio.Enabled = true;
            }
        }


        private void getArray()
        {
            AspectAsArray = BitConverter.GetBytes(aspectRatio);
            L_Byte0.Text = AspectAsArray[0].ToString("X2");
            L_Byte1.Text = AspectAsArray[1].ToString("X2");
            L_Byte2.Text = AspectAsArray[2].ToString("X2");
            L_Byte3.Text = AspectAsArray[3].ToString("X2");
        }

        private void TB_AspectRatio_TextChanged(object sender, EventArgs e)
        {
            if(!autoCalculate)
            {
                var temp = 1.333f;
                if(float.TryParse(TB_AspectRatio.Text, out temp))
                {
                    aspectRatio = temp;
                }
            }
            getArray();
        }
        #endregion

        #region Load_Save
        private byte[] GetBytesFromAFile(string filename)
        {
            FileStream fs = null;
            try
            {
                fs = File.OpenRead(@filename);
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                return bytes;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

        private bool WriteBytesToAFile(string filename, byte[] usedData)
        {
            try
            {
                File.WriteAllBytes(@filename, usedData);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                return false;
            }
        }

        private void B_WriteToAFile_Click(object sender, EventArgs e)
        {
            //Replacing bytes
            data[adress + 0] = AspectAsArray[0];
            data[adress + 1] = AspectAsArray[1];
            data[adress + 2] = AspectAsArray[2];
            data[adress + 3] = AspectAsArray[3];
            data[nav_sequence1adress + 0] = AspectAsArray[0];
            data[nav_sequence1adress + 1] = AspectAsArray[1];
            data[nav_sequence1adress + 2] = AspectAsArray[2];
            data[nav_sequence1adress + 3] = AspectAsArray[3];
            data[nav_sequence2adress + 0] = AspectAsArray[0];
            data[nav_sequence2adress + 1] = AspectAsArray[1];
            data[nav_sequence2adress + 2] = AspectAsArray[2];
            data[nav_sequence2adress + 3] = AspectAsArray[3];
            //Could have written some function for it

            //Backup
            if (!File.Exists(@_GamesExecutableBackup))
            {
                File.Copy(@_GamesExecutable, @_GamesExecutableBackup);
            }

            bool success = true;
            success = WriteBytesToAFile(_GamesExecutable, data);

            if (!success)
                MessageBox.Show("There was an error writting to a file!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                MessageBox.Show("Successfully made the changes!", "Note", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        private void LL_PCGW_link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(PCGW_URL);
        }

        private void P_Donate_Click(object sender, EventArgs e)
        {
            Process.Start(donationURL);
        }
    }
}
