using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF04.Domain.Entities;
using WPF04.Infrastructure.Radio.Serial;
using WPF04.Domain.Interfaces;
using System.Text.Json;
using WPF04.Infrastructure.Radio.Serial.SerialJsonInterface;
using System.Windows;
using WPF04.Domain.Entities.Message;

namespace WPF04.Infrastructure.Radio
{
    /// <summary>
    /// Handles the communication with a LoRa Transceiver Device, using a serial interface.
    /// </summary>
    public class Transceiver : ITransceiver
    {
        //Serial interface associated with radio
        private SerialHandler _TransceiverInterface;

        //Copy of current transceiver configuration
        public TransceiverConfig? CurrentTransceiverConfig;

        //Connection status flag
        public bool IsConnected;

        /// <summary>
        /// Default constructur to initialize the transceiver with a specified serial interface name and baud rate.
        /// </summary>
        /// <param name="interfaceName"></param>
        /// <param name="baud"></param>
        public Transceiver(string interfaceName, int baud)
        {
            //Create new serial interface instance
            _TransceiverInterface = new SerialHandler(interfaceName, baud);

            //Update connection status
            this.IsConnected = _TransceiverInterface.isConnected;
        }

        /// <summary>
        /// Method to retrieve the current transceiver configuration from the device and update the local copy.
        /// </summary>
        /// <returns></returns>
        public TransceiverConfig? UpdateLocalConfig()
        {
            //Build query command
            string transceiverResponse = _TransceiverInterface.ExecuteSJS("GetGlobalConfig", "");

            //Return null response if no response is received
            if (transceiverResponse == null)
            {
                return null;
            }

            //Parse returned statement into an SJS object
            var wrapper = JsonSerializer.Deserialize<SJSWrapper>(transceiverResponse);

            //Validate response
            if(wrapper != null)
            {
                //Deserialize the payload into a TransceiverConfig object
                this.CurrentTransceiverConfig = JsonSerializer.Deserialize<TransceiverConfig>(wrapper.statementPayload);

                //If valid
                if (CurrentTransceiverConfig != null)
                {
                    return CurrentTransceiverConfig;
                }

                //If not  
                else
                {
                    MessageBox.Show("Error: Could not retrieve transceiver configuration.", "Configuration Retrieval Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }

            //Return config
            return CurrentTransceiverConfig;
        }

        /// <summary>
        /// Method to update the remote transceiver config with the supplied object
        /// </summary>
        /// <param name="newConfig"></param>
        /// <returns></returns>
        public string UpdateRemoteConfig(TransceiverConfig newConfig)
        {
            //Supply new config and return the remote validation check string from the MCU
            return _TransceiverInterface.ExecuteSJS("SetGlobalConfig", newConfig);
            
        }

        /// <summary>
        /// Method to command the transceiver to process the TX queue
        /// </summary>
        public void ProcessTxQueue()
        {
            //Execute the command to process the TX queue
            _TransceiverInterface.ExecuteSJS("ProcessTxQueue", "");
        }


        //public string WriteMessageToTx(TxMessage message)
        //{
        //    //Execute the command to enqueue the message
        //    string transceiverResult = _TransceiverInterface.ExecuteSJS("TxNewMessage", message.GenerateMessageJson());

        //    //Return the result string from the transceiver
        //    return transceiverResult;
        //}

        /// <summary>
        /// Method to return the top of the FIFO RxMessage queue from the transceiver device.
        /// </summary>
        /// <returns></returns>
        public RxMessage? ReadRxMessages()
        {
            //Execute the retrieve command on the transceiver interface
            string transceiverResult = _TransceiverInterface.ExecuteSJS("RetrieveRxMessages", "");

            //Deserialize into SJS object
            var wrapper = JsonSerializer.Deserialize<SJSWrapper>(transceiverResult);

            //Catch error
            if (wrapper.statementType == "Error")
            {
                MessageBox.Show(wrapper.statementPayload.ToString());
            }

            //Instantiate and return a new RxMessage object
            else
            {
                RxMessage? newRxMessage = JsonSerializer.Deserialize<RxMessage>(wrapper.statementPayload);
                return newRxMessage;
            }

            return null;
        }

        /// <summary>
        /// Method to open the radio connection if not already open.
        /// </summary>
        /// <returns></returns>
        public bool OpenRadioConnection()
        {
            //If port is not open, open port.
            if (!_TransceiverInterface.GetPortStatus())
            {
                _TransceiverInterface.OpenPort();
            }

            //Return status
            return _TransceiverInterface.GetPortStatus();

        }

        /// <summary>
        /// Method to enqueue a transmissible message on the transceiver device's internal TX queue.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public void QueueNewMessage(string rawMessageString)
        {
            //Execute the command to queue a new message
            _TransceiverInterface.ExecuteSJS("TxNewMessage", rawMessageString);
        }

    }
}
