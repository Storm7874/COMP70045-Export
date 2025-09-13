using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPF04.Domain.Interfaces;
using WPF04.Infrastructure.Radio.Serial.SerialJsonInterface;
using System.IO.Ports;

namespace WPF04.Infrastructure.Radio.Serial
{
    /// <summary>
    /// Handles serial port communication with the LoRa transceiver using the Serialised Json Statement (SJS) protocol.
    /// </summary>
    public class SerialHandler : ISerialHandler
    {
        //Serial port instance and configuration
        private SerialPort _serialPort;
        private string _portName;
        private int _baud;

        //Connection status flag
        public bool isConnected;

        /// <summary>
        /// Constructor to initialize the SerialHandler with specified port name and baud rate.
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudrate"></param>
        public SerialHandler(string portName, int baudrate = 115200)
        {
            //Populate Fields
            this._portName = portName;
            this._baud = baudrate;

            //Create new port instance
            _serialPort = new SerialPort(portName, baudrate);

            //Pause & Open port
            Thread.Sleep(1000);
            this.OpenPort();
        }


        /// <summary>
        /// Attempts to open the serial port and updates the connection status.
        /// </summary>
        /// <returns></returns>
        public bool OpenPort()
        {
            try
            {
                //Initial open port attempt
                _serialPort.Open();

                //Flush any data
                this.FlushSerialBuffers();

                //Update flags
                isConnected = true;
                return true;
            }
            
            //Something went wrong
            catch (Exception e)
            {
                //Indicate to user
                MessageBox.Show($"Could not open: {this._portName}");

                //Update flags
                isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Checks if the serial port is currently open.
        /// </summary>
        /// <returns></returns>
        public bool GetPortStatus()
        {
            return _serialPort.IsOpen;
        }

        /// <summary>
        /// Method to flush the IO buffers for the port
        /// </summary>
        public void FlushSerialBuffers()
        {
            //Input
            _serialPort.DiscardInBuffer();

            //Output
            _serialPort.DiscardOutBuffer();
        }

        /// <summary>
        /// Executes a Serialised Json Statement (SJS) by sending it to the transceiver and returns the response.
        /// </summary>
        /// <param name="statementType"></param>
        /// <param name="statementPayload"></param>
        /// <returns></returns>
        public string ExecuteSJS(string statementType, object statementPayload)
        {
            //Verify port state
            if (!_serialPort.IsOpen)
            {
                //Try to open port
                if (!this.OpenPort())
                {
                    return null;
                }
            }

            //Build final command string using SJSBuilder
            string commandToExecute = SJSBuilder.BuildSJSStatement(statementType, statementPayload);

            //Clear buffers and write command to output
            this.FlushSerialBuffers();
            _serialPort.WriteLine(commandToExecute);

            //TODO: Do this better
            //Wait for Transceiver processing
            Thread.Sleep(2000);

            //Read response from transceiver
            string response = _serialPort.ReadExisting();

            //Format response
            response = response.TrimEnd('\r', '\n');

            //Get the index of the closing JSON brace
            int lastBrace = response.LastIndexOf('}');

            //Verify correct brace
            if (lastBrace != -1 && lastBrace < response.Length - 1)
            {
                //Trim any additional data
                response = response.Substring(0, lastBrace + 1);
            }

            //Clear buffers again
            this.FlushSerialBuffers();

            //Return response, should be a raw SJS string
            return response;
        }
    }
}
