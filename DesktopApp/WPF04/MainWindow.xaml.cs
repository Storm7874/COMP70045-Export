using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF04.Core;
using WPF04.Domain.Entities;
using WPF04.Domain.Entities.Message;
using System.Collections.ObjectModel;


namespace WPF04
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Main entry point for application
        private LBMS _LBMS; // LBMS: LoRa Based Messaging System

        //Display messages in the UI
        public ObservableCollection<DisplayMessage> DisplayMessages { get; set; }

        //Fallback transceiverConfig, for use when the radio is not connected
        private TransceiverConfig _fallbackConfig = new TransceiverConfig(0, 0, 0, 0, 0, 0, 0, 0, false, false, 0, 0, 0, 0, 0, 0, 0, false, 0);

        //Message global configuration parameters
        public bool EncryptionRequired { get; set; } = false;
        public bool EncodingRequired { get; set; } = false;
        public bool AckRequired { get; set; } = false;
        public bool RebroadcastRequired { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
            //Initialize the LBMS instance and perform initial setup
            _LBMS = new LBMS();
            _LBMS.PerformInitialSetup();
            _UpdateRadioStatusIndicators();
            UpdateCurrentConfigDisplay();

            //Instantiate displayed message list
            DisplayMessages = new ObservableCollection<DisplayMessage>();
            DataContext = this;

        }

        /// <summary>
        /// Update the radio connection status indicators and Connect button visibility
        /// </summary>
        private void _UpdateRadioStatusIndicators()
        {
            //Update the connection status label and button visibility based on the radio connection status
            //Disconnected
            if (!_LBMS.AppConfig.IsRadioConnected)
            {
                ConnectionStatusLabel.Foreground = Brushes.Red;
                ConnectionStatusLabel.Content = "Disconnected";
                ConnectToRadioButton.Visibility = Visibility.Visible;

            }
            //Connected
            else
            {
                ConnectionStatusLabel.Foreground = Brushes.Green;
                ConnectionStatusLabel.Content = "Connected";
                ConnectToRadioButton.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Refresh the current transceiver configuration from the connected Transceiver Device
        /// </summary>
        private void _RefreshCurrentTransceiverConfig()
        {
            _LBMS.GetCurrentConfig();
        }

        /// <summary>
        /// Update the current transceiver configuration display in the UI
        /// </summary>
        private void UpdateCurrentConfigDisplay()
        {
            //Get fresh copy of the current transceiver configuration
            _RefreshCurrentTransceiverConfig();

            //Update the display fields
            //If the returned configuration is null, assume radio disconnect - Populate values with fallback config
            if (_LBMS.ActiveTransceiverConfig == null)
            {
                VFODisplay.Foreground = Brushes.DarkGray;
                VFODisplay.Text = _fallbackConfig.RF_FREQUENCY.ToString();

                SFDisplay.Foreground = Brushes.DarkGray;
                SFDisplay.Text = _fallbackConfig.LORA_SPREADING_FACTOR.ToString();

                PowerDisplay.Foreground = Brushes.DarkGray;
                PowerDisplay.Text = _fallbackConfig.TX_OUTPUT_POWER.ToString();

            }
            //Fresh copy of transceiver config obtained - Populate values with live config
            else
            {
                VFODisplay.Foreground = Brushes.Cyan;
                VFODisplay.Text = _LBMS.ActiveTransceiverConfig.RF_FREQUENCY.ToString();

                SFDisplay.Foreground = Brushes.Cyan;
                SFDisplay.Text = _LBMS.ActiveTransceiverConfig.LORA_SPREADING_FACTOR.ToString();

                PowerDisplay.Foreground = Brushes.Cyan;
                PowerDisplay.Text = _LBMS.ActiveTransceiverConfig.TX_OUTPUT_POWER.ToString();
            }

            //Update the radio status indicators
            _UpdateRadioStatusIndicators();
        }

        /// <summary>
        /// Event handler for the Send button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            //Verify that the message entry box is not empty
            if (!string.IsNullOrEmpty(MessageEntryBox.Text))
            {
                //Queue the message with LBMS and add it to the display list - Pass the message config parameters
                DisplayMessage? returnedDisplay = _LBMS.SendNewMessage(MessageEntryBox.Text, EncodingRequired, EncryptionRequired, AckRequired, RebroadcastRequired);
                
                //If the returned response from LBMS is null, the message encoding process has failed.
                if (returnedDisplay == null)
                {
                    //Exit the message transmission process
                    return;
                } 

                //Create new display message to appear on screen
                DisplayMessage tempDisplayMessage = new DisplayMessage(messageType: "Sent", TxID: _LBMS.AppConfig.ConfigParams.TxID, messagePayload: MessageEntryBox.Text, messageSize: MessageEntryBox.Text.Length, messageRssi: 0, messageSnr: 0, DateTime.UtcNow);
                //Set the message source as TX.
                tempDisplayMessage.messageSource = "TX";

                //Set the message type display field based on the config parameters
                if (EncodingRequired)
                {
                    tempDisplayMessage.messageType = "Encoded";
                    if(EncryptionRequired)
                    {
                        tempDisplayMessage.messageType = "Encrypted";
                    }
                }
                else
                {
                    tempDisplayMessage.messageType = "Raw";
                }
                //Add the message to the display list
                DisplayMessages.Add(tempDisplayMessage);

                //Clear the message entry box
                MessageEntryBox.Clear();
            }
            else
            {
                MessageBox.Show("Please enter a message to send.");
                return;
            }


        }

        /// <summary>
        /// Event handler for the Clear button click event - Clears the displayed message list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayMessages.Clear();
        }

        /// <summary>
        /// Event handler for the Refresh button click event - Checks for new received messages from the radio
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            //Check for new received messages from the radio
            DisplayMessage? tempDisplayMessage = _LBMS.UpdateRxMessages();

            //If a new message has been received, add it to the display list
            if (tempDisplayMessage != null)
            {
                //Add the received message to the display list
                DisplayMessages.Add(tempDisplayMessage);
            }
            //If returned value is null, no new messages are present.
            else
            {
                //MessageBox.Show("No messages received.");
            }

        }

        /// <summary>
        /// Event handler for the "Update" button click event - Updates the transceiver configuration with the values in the display fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateBaseRFParamsButton_Click(object sender, RoutedEventArgs e)
        {
            //Load the current active config into temp value for modification
            TransceiverConfig tempConfig = _LBMS.ActiveTransceiverConfig;

            //Update the temp config values with the display field values
            tempConfig.RF_FREQUENCY = UInt32.Parse(VFODisplay.Text);
            tempConfig.LORA_SPREADING_FACTOR = int.Parse(SFDisplay.Text);
            tempConfig.TX_OUTPUT_POWER = int.Parse(PowerDisplay.Text);

            //Send the updated config to the transceiver
            _LBMS.UpdateTransceiverConfig(tempConfig);
        }
    }
}