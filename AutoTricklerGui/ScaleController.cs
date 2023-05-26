using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace AutoTricklerGui
{
    class ScaleController
    {
        private ScaleData _scaleData;
        private SerialPortWrapper _serialPort;
        string scaleValueStr = "";
        private bool semaphore = true;

        public ScaleController(SerialPortWrapper sericalPort, ScaleData scaleData) {
            _scaleData = scaleData;
            _serialPort = sericalPort;
        }

        public void ScaleDataReceivedHandler(object sender, SerialDataReceivedEventArgs e) {
            byte[] data = new byte[1024];
            int bytesRead = _serialPort.SerialConnection.Read(data, 0, data.Length);
            string scaleValuePart = Encoding.ASCII.GetString(data, 0, bytesRead);

            scaleValueStr += scaleValuePart;
            if (scaleValueStr.Contains("\n")) {
                string valueNumberPart = scaleValueStr.Substring(3, 7).Replace('.', ',');
                _scaleData.CurrentScaleValue = Convert.ToDecimal(valueNumberPart);
                if (scaleValueStr.StartsWith('-')) {
                    _scaleData.CurrentScaleValue = _scaleData.CurrentScaleValue * -1;
                }

                scaleValueStr = "";
                semaphore = true;
            }
        }

        public void tara() {
            bool tara = false;

            while(!tara) {
                if (semaphore) {
                    semaphore = false;
                    byte[] bytesToSend = { 0x1B, 0x74 };
                    _serialPort.SerialConnection.Write(bytesToSend, 0, bytesToSend.Length);
                    Thread.Sleep(1000);
                    tara = true;
                    semaphore = true;
                }
            }
        }

        public void requestScaleValue() {
            while (true) {
                if (semaphore) {
                    try {
                        semaphore = false;
                        byte[] bytesToSend = { 0x1B, 0x70 };
                        _serialPort.SerialConnection.Write(bytesToSend, 0, bytesToSend.Length);
                    }
                    catch { }
                }
                Thread.Sleep(5);
            }
        }

        public void resetSemaphore()
        {
            semaphore = true;
        }
    }
}
