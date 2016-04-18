using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using Microsoft.Win32;
using System.Threading;

namespace Infecon.Common.COM
{
    public class SerialPortHelper
    {
        public SerialPort SerialPortA;

        
        public delegate void BarcodeDecodeDelegate(string barcode);        // 创建委托和委托对象
        public event BarcodeDecodeDelegate BarcodeDecodeEvent;

        public SerialPortHelper(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, bool isBarcodeInput)
        {
            SerialPortA = new SerialPort();
            SerialPortA.PortName = portName; 
            SerialPortA.BaudRate = baudRate;
            SerialPortA.Parity = parity;
            SerialPortA.DataBits = dataBits;
            SerialPortA.StopBits = stopBits;
            SerialPortA.DtrEnable = true;
            SerialPortA.RtsEnable = true;

            if (isBarcodeInput == true)
            {
                SerialPortA.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(SerialPort_DataReceived);
            }
              
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //int bytes = SerialPortA.BytesToRead;
            //if (bytes == 0)
            //{ return; }
            //byte[] buffer = new byte[bytes];
            //SerialPortA.Read(buffer, 0, SerialPortA.BytesToRead);
            //string barcode = System.Text.Encoding.Default.GetString(buffer);

            Thread.Sleep(50);
            string barcode = SerialPortA.ReadExisting();
            
            barcode = barcode.TrimEnd(System.Environment.NewLine.ToCharArray());
            if (barcode.Length == 17 && barcode.Substring(0, 4) == "0001")
            {
                barcode = barcode.TrimStart("0001".ToCharArray());
            }
            else if (barcode.Substring(0, 1) == "`")
            {
                barcode = barcode.TrimStart("`".ToCharArray());
                barcode = barcode.TrimEnd("\t".ToCharArray());
            }

            if (BarcodeDecodeEvent != null) BarcodeDecodeEvent(barcode);
           
        }
        public void Open()
        {
            //if (SerialPortA.IsOpen == true)
            try
            {
                SerialPortA.Close();
            }
            catch (Exception ex)
            { }
            try
            {
                SerialPortA.Open();
            }
            catch (Exception ex)
            { }
            

        }

        public bool HasOpen()
        {
            if (SerialPortA == null) return false;
            return SerialPortA.IsOpen;
        }

        public void Close()
        {
            //if (SerialPortA.IsOpen == true)
            //{
                try
                {
                    //SerialPortA.ReadExisting();
                    SerialPortA.Close();                    
                }
                catch (Exception ex) { }
            //}
        }

        public static string  GetComName(string keyword)
        {
            string temp = string.Empty;
            switch (keyword)
            {
                case "自动DataLogic":
                    temp = "DLSUSB";
                    break;
                case "自动CipherLab":
                    temp = "Silabser";
                    break;
                case "自动BlueTooth":
                    temp = "BthModem";
                    break;
            }
            RegistryKey keyCom = Registry.LocalMachine.OpenSubKey("Hardware\\DeviceMap\\SerialComm");
            if (keyCom != null)
            {
                string[] sSubKeys = keyCom.GetValueNames();
                foreach (string sName in sSubKeys)
                {
                    if (sName.Contains(temp) == true) return (string)keyCom.GetValue(sName);
                   
                }
                
            }
            return string.Empty;
        }
    }
}
