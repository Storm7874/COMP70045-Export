using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF04.Domain.Entities;
using WPF04.Infrastructure.Crypto;
using WPF04.Infrastructure.Radio;
using WPF04.Infrastructure.Encoding;
using WPF04.Core.Services;
using System.Windows;
using WPF04.Domain.Entities.Message;
using WPF04.Domain.Entities.Packet;
using WPF04.Domain.Entities.Dictionaries;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Text.Json;

namespace WPF04.Core
{
    //Main entry point for program logic, and a "Backend" for the user interface
    class LBMS
    {
        //Application Configuration Elements
        public ApplicationConfiguration? AppConfig;

        //Transceiver elements
        private Transceiver _Transceiver;
        private List<RxMessage> _ReceivedMessages = new List<RxMessage>();
        private List<TxMessage> _TransmittedMessages = new List<TxMessage>();

        //Messaging Elements
        private MessageController _MessageController;

        //Current transceiver configuration
        public TransceiverConfig ActiveTransceiverConfig;

        public LBMS()
        {
            //Load the application configuration
            // TODO: Read this path from a more flexible location
            AppConfig = this._LoadApplicationConfig("N:\\UserData\\Documents\\Development\\Programming\\COMP70045-Master\\Core\\WPF04\\Resources\\LBMS_Config.lbm");
            if(AppConfig == null)
            {
                //Cannot load application configuration, cannot start
                MessageBox.Show("Error: Could not load application configuration file.", "Configuration Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                //Exit if configuration is not loaded
                Environment.Exit(1);
            }

            //Initialize the transceiver handler and message controller
            
            //Initiate a new transceiver object with the port and baud from the config file
            _Transceiver = new Transceiver(AppConfig.ConfigParams.RadioPort, AppConfig.ConfigParams.RadioBaud);
            this.AppConfig.IsRadioConnected = _Transceiver.IsConnected;

            //Initialize the message controller with the dictionary and pad file locations from the config file
            this._MessageController = new MessageController(AppConfig.ConfigParams.DictionaryFileLocation, AppConfig.ConfigParams.PadFileLocation);
        }

        /// <summary>
        /// Method to perform initial setup of the message controller components
        /// </summary>
        public void PerformInitialSetup()
        {
            //Perform initial setup of the message controller components
            _MessageController.PerformInitialSetup();
        }

        /// <summary>
        /// Loads the application configuration from a specified file path - Returns null if the config load fails
        /// </summary>
        /// <param name="configFilePath"></param>
        /// <returns></returns>
        private ApplicationConfiguration? _LoadApplicationConfig(string configFilePath)
        {
            //Temporary configuration object
            ConfigurationFile? config;

            //Check to see if the configuration file exists
            if (File.Exists(configFilePath))
            {
                //Read the configuration file and deserialize it into a ConfigurationFile object
                string configJson = File.ReadAllText(configFilePath);

                //Deserialise into object
                config = JsonSerializer.Deserialize<ConfigurationFile>(configJson);

                //If the configuration is not null, verify it
                if (config != null)
                {
                    if (config.VerifyConfig())
                    {
                        //If the configuration is valid, create a new ApplicationConfiguration object and return it
                        return new ApplicationConfiguration(config);
                    }

                    //Could not verify the contents of the configuration file
                    else
                    {
                        //Show error, return null
                        MessageBox.Show("Error: Application configuration is invalid.", "Configuration Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                }

                //Could not load the configuration file
                else
                {
                    //show error
                    MessageBox.Show("Error: Could not load application configuration.", "Configuration Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            //Could not find the configuration file
            else
            {
                MessageBox.Show("Error: Configuration file not found.", "Configuration Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }


        /// <summary>
        /// Method to retrieve the current radio configuration from the transceiver and update the ActiveTransceiverConfig property
        /// </summary>
        public void GetCurrentConfig()
        {
            //Query the Transceiver handler for the current configuration
            TransceiverConfig? retrievedConfig = _Transceiver.UpdateLocalConfig();

            //Update field if response is valid
            if(retrievedConfig != null)
            {
                ActiveTransceiverConfig = retrievedConfig;
            }
        }

        /// <summary>
        /// Method to update the current radio configuration on the transceiver with a new configuration object
        /// </summary>
        /// <param name="newConfig"></param>
        public void UpdateTransceiverConfig(TransceiverConfig newConfig)
        {
            //Request an update to the radio configuration through the transceiver handler class
            string result = _Transceiver.UpdateRemoteConfig(newConfig);

            //Display the result of the UpdateGlobalConfig command on the transciever
            MessageBox.Show(("Configuration Update Result: " + "\n\n" + result), "Update Result", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        /// <summary>
        /// Method to attempt to open a radio connection (When program starts disconnected).
        /// </summary>
        public void OpenRadioConnection()
        {
            //Attempt to open the radio connection through the transceiver handler class
            _Transceiver.OpenRadioConnection();

            //Update the application configuration connection status
            this.AppConfig.IsRadioConnected = _Transceiver.IsConnected;
        }

        //RX Logic

        /// <summary>
        /// Method to check for new messages from the transceiver, decode them, and return a DisplayMessage object if a new message was received
        /// </summary>
        /// <returns></returns>
        public DisplayMessage? UpdateRxMessages()
        {
            /////////////////////////////////////////////////////////
            //Get messages from transceiver
            RxMessage? tempRxMsg = _Transceiver.ReadRxMessages();
            //Debug method to inject a message for testing purposes
            //RxMessage tempRxMsg = _InjectDebugRxMessage();
            /////////////////////////////////////////////////////////
            

            if (tempRxMsg != null)
            {
                //If message is not null, pass to MessageController to decode
                DisplayMessage newDisplayMessage = _MessageController.DecodeMessage(tempRxMsg);

                //Set the message source to RX
                newDisplayMessage.messageSource = "RX";

                //Action Rebroadcast or Ack flags
                this.ProcessReceivedMessage(newDisplayMessage, tempRxMsg);

                //Return the new display message to the caller
                return newDisplayMessage;
            }
            
            //No message received
            else
            {
                //Do nothing
                return null;
            }

        }

        //Debug method ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Debug method to inject a test RxMessage for testing purposes
        /// </summary>
        /// <returns></returns>
        private RxMessage _InjectDebugRxMessage()
        {
            //Inject a debug message for testing purposes
            //RxMessage DebugRawMessage = new RxMessage("030045706F7461746F", 0, 50, 50);
            //RxMessage DebugEncodedMessage = new RxMessage("011B3901F9700002000C8", 0, 50, 50);
            RxMessage DebugEncryptedMessage = new RxMessage("021B39000090B3C39190", 0, 50, 50);

            return DebugEncryptedMessage;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method to process additional actions based on the flags set in a received message
        /// </summary>
        /// <param name="receivedMessage"></param>
        /// <param name="originalMessage"></param>
        private void ProcessReceivedMessage(DisplayMessage receivedMessage, RxMessage originalMessage)
        {
            // Rebroadcast requests that the message be rebroadcast by the transmitter upon reception
            // Rebroadcast messages are stripped of the rebroadcast flag before being sent again - Prevents infinite loops
            // Ack requests that the transmitter send an ACK message that the message has been received
            // ACK messages are sent as a raw message with the payload "ACK:<length of original message>"
            // The ACK message is sent without the ACK flag, again to prevent loops

            //If rebroadcast flag set and application configuration permits response
            if (receivedMessage.rebroadcast && this.AppConfig.ConfigParams.RespondToRebroadcast)
            {
                //Strip the old header from the message
                string newMessage = originalMessage.messagePayload.Substring(2);

                //No need to assemble new header from bits, can map response header to original message type
                if (receivedMessage.messageType == "RAW")
                {
                    newMessage = "03" + newMessage;
                } 
                else if (receivedMessage.messageType == "DICT")
                {
                    newMessage = "01" + newMessage;
                }
                else if (receivedMessage.messageType == "EDICT")
                {
                    newMessage = "02" + newMessage;
                }

                //Queue the rebroadcast message
                _Transceiver.QueueNewMessage(newMessage);
            }

            //If ACK flag set and application configuration permits response
            // TODO: Implement a check to ensure the original message is not an ACK message itself
            // TODO: Implement a cooling off period for ACK messages to prevent flooding the network with ACKs
            if (receivedMessage.ack && this.AppConfig.ConfigParams.RespondToAck)
            {
                //Generate the ACK message
                string? rawAckMessage = _MessageController.GenerateRawMessage($"ACK:{originalMessage.messagePayload.Length}", false, false, false, false, AppConfig.ConfigParams.TxID);

                //Queue the ACK message if it was generated successfully
                if (!string.IsNullOrEmpty(rawAckMessage))
                {
                    _Transceiver.QueueNewMessage(rawAckMessage);
                }

            }

            //Process the TX queue to send any queued messages
            _Transceiver.ProcessTxQueue();
        }

        //TX Logic

        /// <summary>
        /// Method to initiate the sending of a new message, returns a DisplayMessage object if the message was sent successfully
        /// </summary>
        /// <param name="rawMessageText"></param>
        /// <param name="encode"></param>
        /// <param name="encrypt"></param>
        /// <param name="ack"></param>
        /// <param name="rebroadcast"></param>
        /// <returns></returns>
        public DisplayMessage? SendNewMessage(string rawMessageText, bool encode, bool encrypt, bool ack, bool rebroadcast)
        {
            //Verify the message text is initially valid, and meets the requirements of the protocol.
            if (this._VerifyMessage(rawMessageText, encode, encrypt, ack, rebroadcast))
            {
                //Raw, transmissable message string, including packet header + params
                string? rawMessageString = "";

                //Generate the raw message string from the message controller
                rawMessageString = _MessageController.GenerateRawMessage(rawMessageText, encode, encrypt, ack, rebroadcast, AppConfig.ConfigParams.TxID);

                //If the message was generated successfully, queue it for transmission
                if (!string.IsNullOrEmpty(rawMessageString))
                {
                    //For debugging purposes, show the raw message to be sent
                    MessageBox.Show(rawMessageText + "\n" + rawMessageString, "Message to be sent", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    //Queue the message on the transceiver.
                    _Transceiver.QueueNewMessage(rawMessageString);

                    //Execute the TX queue.
                    _Transceiver.ProcessTxQueue();

                    //Add the message to the main display as a transmitted message.
                    DisplayMessage tempDisplayMessage = new DisplayMessage("Sent", AppConfig.ConfigParams.TxID, rawMessageText, rawMessageString.Length, 0, 0, DateTime.UtcNow);

                    //Set the message source to TX
                    tempDisplayMessage.messageSource = "TX";

                    //Return the display message to the caller
                    return tempDisplayMessage;
                }

                //Could not generate the raw message string
                else
                {
                    MessageBox.Show("Error: Could not generate message for transmission.", "Message Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }


            }

            //Message verification failed
            else
            {
                MessageBox.Show("Message verification failed. Please check the message format and try again.", "Message Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Method to verify a message before sending
        /// </summary>
        /// <param name="rawMessageText"></param>
        /// <param name="encode"></param>
        /// <param name="encrypt"></param>
        /// <param name="ack"></param>
        /// <param name="rebroadcast"></param>
        /// <returns></returns>
        private bool _VerifyMessage(string rawMessageText, bool encode, bool encrypt, bool ack, bool rebroadcast)
        {
            //Check for empty string
            if (string.IsNullOrEmpty(rawMessageText))
            {
                MessageBox.Show("Message text cannot be empty.", "Message Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            //Ensure dictionaries are loaded
            if (encode && !this._MessageController.DictLoadStatus)
            {
                MessageBox.Show("Dictionary not loaded. Cannot encode message.", "Message Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            //Check for Encryption + Encoding
            if (encrypt && !encode)
            {
                MessageBox.Show("Encryption can only be applied to encoded messages.", "Message Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;



        }
    }
}
