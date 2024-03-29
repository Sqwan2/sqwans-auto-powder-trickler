﻿using System;
using System.Windows;
using System.Windows.Input;
using System.IO.Ports;
using System.Threading;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;

using CsvHelper;
using System.Collections.Generic;
using CsvHelper.Configuration;
using System.Text;
using Microsoft.Win32;

namespace AutoTricklerGui
{
    public partial class MainWindow : Window
    {
        private Semaphore startButtonSemaphore = new Semaphore(1);
        private Thread measureThread;
        private SerialPortWrapper serialPort;
        private ScaleData _scaleData;
        private ScaleController scaleController;
        private bool _isMessureThreadRunning = true;
        private delegate void SetTextDeleg(string text);
        public bool isEnabled = false;

        decimal powderQtyD = 0;

        public MainWindow()
        {
            InitializeComponent();

            serialPort = new SerialPortWrapper("COM1", 9600);
            _scaleData = new ScaleData();
            scaleController = new ScaleController(serialPort, _scaleData);

            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comPorts.Items.Add(port);
                comPortsTrickler.Items.Add(port);
            }
            serialPort.SerialConnection.DataReceived += new SerialDataReceivedEventHandler(scaleController.ScaleDataReceivedHandler);

            this.DataContext = _scaleData;
            new Thread(scaleController.requestScaleValue).Start();
        }

        private void messure(string comPortTricklerValue, string baudRatesTricklerValue) {
            //SerialPort sp = new SerialPort("COM7", 115200, Parity.None, 8, StopBits.One); // Hier weiter
            SerialPort sp = new SerialPort(comPortTricklerValue, int.Parse(baudRatesTricklerValue), Parity.None, 8, StopBits.One);

            sp.Handshake = Handshake.None;
            sp.ReadTimeout = 500;
            sp.WriteTimeout = 500;
            sp.Open();
            
            while (_scaleData.CurrentScaleValue < powderQtyD && _isMessureThreadRunning)
            {
                if (_scaleData.CurrentScaleValue < (powderQtyD - 5))
                {
                    byte[] bytesToSend = { 0x07 }; // <<--- 04 mit Netzteil?
                    sp.Write(bytesToSend, 0, bytesToSend.Length);
                } else {
                    var remainindPowder = powderQtyD - _scaleData.CurrentScaleValue; //Allways less than 5 due to if-statement
                    var portionOf255 = (remainindPowder / 5) * 255; // Speed ist defined from 0 to 255 (1 Byte). The speed is adapted to the portion of the remaining 5 grain. The closer it get's to 0 remaing grain, the slower the speed gets
                    int speed = Decimal.ToInt16(portionOf255); //Speed has to be an int. Also transaction is limited to one byte. A decimal is always at least 2 bytes. By converting it the floating point number becomes an int, which can be converted to 1 byte if smaller than 255 (in case of uInt). 
                    byte[] bytesToSend = { Convert.ToByte(speed) };
                    sp.Write(bytesToSend, 0, bytesToSend.Length);
                }
                Thread.Sleep(100);
            }

            byte[] bytesToSend2 = { 0x0 };
            sp.Write(bytesToSend2, 0, bytesToSend2.Length);
            Thread.Sleep(100);

            sp.Close();

            startButtonSemaphore.decrease();
            _scaleData.StartButtonText = "Start";
            _scaleData.IsScaleGuiActive = true;

            _scaleData.addScaleValue(_scaleData.CurrentScaleValue);
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e) {

            if (_scaleData.StartButtonText.Equals("Start"))
            {
                if (!startButtonSemaphore.isThreadAvailable()) {
                    MessageBox.Show("Es läuft bereits ein Trickel-Vorgang!");
                    return;
                }

                _isMessureThreadRunning = true;
                startButtonSemaphore.increase();
                powderQtyD = Convert.ToDecimal(powderQty.Text);

                //string comPortTricklerValue = ((ComboBoxItem)comPortsTrickler.SelectedItem).Content.ToString();
                //string baudRatesTricklerValue = ((ComboBoxItem)baudRatesTrickler.SelectedItem).Content.ToString();

                string comPortTricklerValue = comPortsTrickler.Text;
                string baudRatesTricklerValue = baudRatesTrickler.Text;
                measureThread = new Thread(() => messure(comPortTricklerValue, baudRatesTricklerValue));
                measureThread.Start();
                _scaleData.IsScaleGuiActive = false;
                _scaleData.StartButtonText = "Stop";
            } else if (_scaleData.StartButtonText.Equals("Stop")) {
                _isMessureThreadRunning = false;
            }
        }

        private void Reset_Button_Click(object sender, RoutedEventArgs e) {
            _scaleData.ResetScaleValueList();
        }

        private void powderQty_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            if (e.Text.Equals(",")) {
                if(powderQty.Text.Contains(',')) {
                    e.Handled = true;
                    return;
                }
            }

            foreach (var ch in e.Text) {
                if (!(Char.IsDigit(ch) || ch.Equals(','))) {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void taraBtn_Click(object sender, RoutedEventArgs e) {
            scaleController.tara();
        }

        private void comPorts_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            serialPort.changeComPort(comPorts.SelectedItem.ToString());
            serialPort.SerialConnection.DataReceived += new SerialDataReceivedEventHandler(scaleController.ScaleDataReceivedHandler);
            scaleController.resetSemaphore();
        }

        private void export_button_click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";", Encoding = Encoding.UTF8 };

                //using (var writer = new StreamWriter("filePersons.csv"))
                using (var writer = new StreamWriter(saveFileDialog.FileName))
                using (var csv = new CsvWriter(writer, config))
                {
                    List<ExportData> strList = new List<ExportData>();
                    foreach (decimal d in _scaleData.getScaleValues())
                    {
                        ExportData exp = new ExportData();
                        exp.ScaleValue = d;
                        if (strList.Count <= 0)
                        {
                            exp.MinWeight = _scaleData.MinWeight.ToString();
                            exp.MaxWeight = _scaleData.MaxWeight.ToString();
                            exp.ExtremeSpread = _scaleData.ExtremeSpread.ToString();
                            exp.AverageWeight = _scaleData.AverageWeight.ToString();
                            exp.StandardDeviation = _scaleData.StandardDeviation.ToString();
                        }

                        strList.Add(exp);
                    }

                    csv.WriteRecords(strList);
                }
            }
        }
    }
}
