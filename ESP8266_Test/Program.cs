using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace ESP8266_Test
{
    public class Program
    {
        private static SerialPort _serial;
        private static OutputPort _reset = new OutputPort(Pins.GPIO_PIN_D7, true);
        private static AutoResetEvent _sync = new AutoResetEvent(false);
        public static void Main()
        {
            
            
            // perform a hard reset on ESP to make sure it's in a known state
            reset();

            _serial = new SerialPort(SerialPorts.COM2, 115200, Parity.None, 8, StopBits.One);
            _serial.DataReceived += dataReceive;
            _serial.Open();
            var buffer = Encoding.UTF8.GetBytes("AT+RST");
            _serial.Write(buffer, 0, buffer.Length);
            Thread.Sleep(1000);
            //receiveData();
           
            //Turn off Echo

            buffer = Encoding.UTF8.GetBytes("ATE0");
            //_serial.Write(buffer, 0, buffer.Length);
            //receiveData();
            send(buffer);

            
            buffer = Encoding.UTF8.GetBytes("AT+CWMODE=1");
            //_serial.Write(buffer, 0, buffer.Length);
            //receiveData();
            send(buffer);


            buffer = Encoding.UTF8.GetBytes("AT+CWJAP=\"rphome\",\"homesweethome\"");
            //_serial.Write(buffer, 0, buffer.Length);
            //receiveData();
            send(buffer);


            buffer = Encoding.UTF8.GetBytes("AT+CIFSR");
            //_serial.Write(buffer, 0, buffer.Length);
            //receiveData();
            send(buffer);



            //construct packet to send
            var payload = "{'DeviceId':'Netduino','SensorId':'Netduino01','SensorType':'Test','SensorValue':'1'  }\r\n";
            var header = "POST http://rrpiot.azurewebsites.net/SensorData HTTP/1.0\r\n";
            header += "Content-Type: application/json; charset=utf-8\r\n";
            header += "Content-Length: "+payload.Length.ToString() + "\r\n";
            header += "Host: rrpiot.azurewebsites.net\r\n";

            var packet = header + "\r\n" + payload + "\r\n";

            while (true)
            {
                //Connect to endpoint
                buffer = Encoding.UTF8.GetBytes("AT+CIPSTART=\"TCP\",\"rrpiot.azurewebsites.net\",80\r\n");
                //_serial.Write(buffer, 0, buffer.Length);
                //receiveData();
                send(buffer);


                //prepare to send Data
                buffer = Encoding.UTF8.GetBytes("AT+CIPSEND=" + packet.Length + "\r\n");
                //_serial.Write(buffer, 0, buffer.Length);
                //receiveData();
                send(buffer);

                //send data
                buffer = Encoding.UTF8.GetBytes(packet + "\r\n");
                //_serial.Write(buffer, 0, buffer.Length);
                //receiveData();
                send(buffer);


                Thread.Sleep(3000);
            }
        }

        private static string send(byte[] buffer)
        {
            _sync.Reset();
            _serial.Write(buffer, 0, buffer.Length);
            _sync.WaitOne(3000, false);
            return ""; //receiveData();
        }

        private static void dataReceive(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(100);
            String response = "";
            int len;
            byte[] buf;
            while (_serial.BytesToRead > 0)
            {
                len = _serial.BytesToRead;
                buf = new byte[len];
                _serial.Read(buf, 0, len);
                foreach (byte b in buf)
                    response += (char)b;
            }
            Debug.Print(response);
            _sync.Set();
        }

        private static void reset()
        {
            _reset.Write(false);
            Thread.Sleep(20);
            _reset.Write(true);
            Thread.Sleep(200);
        }
        private static string receiveData()
        {
            Thread.Sleep(100);
            String response = "";
            int len;
            byte[] buf;
            while (_serial.BytesToRead > 0)
            {
                len = _serial.BytesToRead;
                buf = new byte[len];
                _serial.Read(buf, 0, len);
                foreach (byte b in buf)
                    response += (char) b;
            }
            Debug.Print(response);
            return response;
        }



    }


}
