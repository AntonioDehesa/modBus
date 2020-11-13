using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace modBus1
{
    public partial class Form1 : Form
    {
        static byte SLAVE01 = 1;
        static byte WRITE_COIL = 5;
        static byte READ_REG = 4;
        static byte COIL_REGISTER = 0;
        static byte COIL_ON = 0xFF;
        static byte COIL_OFF = 0x00;
        bool slaveOneStatus = false;
        public Form1()
        {
            InitializeComponent();
            if (!serialPort1.IsOpen)
            {
                serialPort1.Open();
                label1.Text = "Port opened";
            }
            else
            {
                label1.Text = "Port busy";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void toggleButton1_ButtonStateChanged(object sender, ToggleButton_Demo.ToggleButton.ToggleButtonStateEventArgs e)
        {
            byte[] arr1 = { SLAVE01, WRITE_COIL, COIL_REGISTER, 0x00, 0x00, 0x00, 0x00, 0x00 };
            arr1[4] = !slaveOneStatus ? COIL_ON : COIL_OFF;
            slaveOneStatus = !slaveOneStatus;
            /*if (slaveOneStatus == false)
            {
                slaveOneStatus = true;
                arr1[4] = COIL_ON;
            }
            else
            {
                slaveOneStatus = false;
                arr1[4] = COIL_OFF;
            }*/
            SendString(arr1);
        }

        public void SendString(byte[] arr)
        {
            uint ModbusCRC = CalcCRC(arr);
            arr[7] = Convert.ToByte(ModbusCRC & 0xFF);
            arr[6] = Convert.ToByte((ModbusCRC >> 8) & 0xFF);
            for (int n = 0; n <= 7; n++)
            {
                byte[] ch = new byte[1];
                ch[0] = arr[n];
                serialPort1.Write(ch, 0, 1);
            }
        }

        public uint CalcCRC(byte[] ch)
        {
            uint crc16 = 0xFFFF;
            uint temp;
            uint flag;
            for (int i = 0; i < 6; i++)
            {
                temp = (uint)ch[i]; // temp has the first byte 
                temp &= 0x00ff; // mask the MSB 
                crc16 = crc16 ^ temp; //crc16 XOR with temp 
                for (uint c = 0; c < 8; c++)
                {
                    flag = crc16 & 0x01; // LSBit di crc16 is mantained
                    crc16 = crc16 >> 1; // Lsbit di crc16 is lost 
                    if (flag != 0)
                        crc16 = crc16 ^ 0x0a001; // crc16 XOR with 0x0a001 
                }
            }
            crc16 = (crc16 >> 8) | (crc16 << 8); // LSB is exchanged with MSB
            crc16 = crc16 & 0xFFFF;
            return (crc16);
        }

        public uint CalcCRC(byte[] ch, int bytesToRead)
        {
            uint crc16 = 0xFFFF;
            uint temp;
            uint flag;
            for (int i = 0; i < bytesToRead; i++)
            {
                temp = (uint)ch[i]; // temp has the first byte 
                temp &= 0x00ff; // mask the MSB 
                crc16 = crc16 ^ temp; //crc16 XOR with temp 
                for (uint c = 0; c < 8; c++)
                {
                    flag = crc16 & 0x01; // LSBit di crc16 is mantained
                    crc16 = crc16 >> 1; // Lsbit di crc16 is lost 
                    if (flag != 0)
                        crc16 = crc16 ^ 0x0a001; // crc16 XOR with 0x0a001 
                }
            }
            crc16 = (crc16 >> 8) | (crc16 << 8); // LSB is exchanged with MSB
            crc16 = crc16 & 0xFFFF;
            return (crc16);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            serialPort1.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //byte[] arr1 = { 0x01, 0x04, 0x00, 0x00, 0x00, 0x01, 0x31, 0xCA };
            byte[] arr1 = { SLAVE01, READ_REG, COIL_REGISTER, 0x00, 0x00, 0x01, 0x00, 0x00 };
            SendString(arr1);
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte[] buffer = { 0x00, 0x00 , 0x00 , 0x00 , 0x00 , 0x00 , 0x00, 0x00 };
            int bytesToRead = serialPort1.BytesToRead;
            serialPort1.Read(buffer, 0, bytesToRead);
            if (buffer[1] == READ_REG && buffer[2] == 2)//Read coil and two arguments
            {
                uint ModbusCRC = CalcCRC(buffer, bytesToRead-2);
                byte FirstCRC = Convert.ToByte(ModbusCRC & 0xFF);
                byte SecondCRC = Convert.ToByte((ModbusCRC >> 8) & 0xFF);
                if (FirstCRC == buffer[6] && SecondCRC == buffer[5])
                {
                    int valorUpper = Convert.ToInt32(buffer[3]) << 8;
                    int valorLower = Convert.ToInt32(buffer[4]);
                    int valor = (valorUpper | valorLower);
                    if(progressBar1.InvokeRequired)
                    {
                        _ = progressBar1.Invoke(new MethodInvoker(delegate
                          {
                              progressBar1.Value = valor;
                          }));
                    }
                }
            }
            /*Console.WriteLine("Valor del buffer = {0},{1},{2},{3},{4},{5},{6},{7}", buffer[0]
                    , buffer[1], buffer[2], buffer[3], buffer[4], buffer[5], buffer[6], buffer[7]);
                Console.WriteLine("Upper = {0}", valorUpper);
                Console.WriteLine("Lower = {0}", valorLower);
                Console.WriteLine("Valor leido = {0}",valor);*/
            /*
                 uint ModbusCRC = CalcCRC(arr);
            arr[7] = Convert.ToByte(ModbusCRC & 0xFF);
            arr[6] = Convert.ToByte((ModbusCRC >> 8) & 0xFF);*/

            //progressBar1.Value = valor;
        }
    }
}
