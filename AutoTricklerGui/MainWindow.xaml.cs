using System;
using System.Windows;
using System.Windows.Input;
using System.IO.Ports;
using System.Threading;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace AutoTricklerGui
{
    public partial class MainWindow : Window
    {
        private SerialPortWrapper serialPort;
        private ScaleData _scaleData;
        private ScaleController scaleController;
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

        private void messure() {
            _scaleData.addScaleValue(_scaleData.CurrentScaleValue);

            SerialPort sp = new SerialPort("COM7", 115200, Parity.None, 8, StopBits.One);
            sp.Handshake = Handshake.None;
            sp.ReadTimeout = 500;
            sp.WriteTimeout = 500;
            sp.Open();
            
            while (_scaleData.CurrentScaleValue < powderQtyD)
            {
                if(_scaleData.CurrentScaleValue < (powderQtyD-5)) { 
                    byte[] bytesToSend = { 0xFF };
                    sp.Write(bytesToSend, 0, bytesToSend.Length);
                } else if(_scaleData.CurrentScaleValue < (powderQtyD - 1)) {
                    byte[] bytesToSend = { 0x7F };
                    sp.Write(bytesToSend, 0, bytesToSend.Length);
                } else  if(_scaleData.CurrentScaleValue < (powderQtyD - 0.3M)) {
                    byte[] bytesToSend = { 0x0A };
                    sp.Write(bytesToSend, 0, bytesToSend.Length);
                } else {
                    byte[] bytesToSend = { 0x03 };
                    sp.Write(bytesToSend, 0, bytesToSend.Length);
                }
                Thread.Sleep(100);
            }

            byte[] bytesToSend2 = { 0x0 };
            sp.Write(bytesToSend2, 0, bytesToSend2.Length);
            Thread.Sleep(100);

            sp.Close();
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e) {
            powderQtyD = Convert.ToDecimal(powderQty.Text);
            new Thread(messure).Start();
            /*_scaleData.addScaleValue(_scaleData.CurrentScaleValue);

            SerialPort sp = new SerialPort("COM7", 115200, Parity.None, 8, StopBits.One);
            sp.Handshake = Handshake.None;
            sp.ReadTimeout = 500;
            sp.WriteTimeout = 500;
            sp.Open();
            decimal d = Convert.ToDecimal(powderQty.Text);
            while (_scaleData.CurrentScaleValue < d)
            {
                sp.WriteLine("50000");
                Thread.Sleep(1000);
                sp.WriteLine("0");
                //MessageBox.Show("Test");

            }
            
            //string test = sp.ReadLine();
            //MessageBox.Show(test);
            sp.Close();
            /*
            decimal powderQtyDouble = 0.0M;
            
            try
            {
                powderQtyDouble = Convert.ToDecimal(powderQty.Text);
                _serialPort.WriteLine("TARA");
                
                new Thread(() => {
                    Thread.Sleep(3000);
                    while (Decimal.Compare(scaleValue, powderQtyDouble) < 0)
                    {
                        _serialPort.WriteLine("Add");
                        Thread.Sleep(50);
                    }
                }).Start();

                this.count++;
                Stk.Content = count + " Stk";
            }
            catch {
                MessageBox.Show("Ihre Eingabe ist keine Zahl", "Fehler im Zahlenformat");
                return;
            }
            */
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
    }
}
