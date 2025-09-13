using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WPF04.Domain.Entities
{
    //TODO: Centralise the configuration settings for the application

    // This class represents the application configuration settings and define the general behaviour
    // This class can be updated at run time depending on the specific settings selected by the user in the GUI.
    class ApplicationConfiguration
    {
        //Object representation of the configuration file
        public ConfigurationFile ConfigParams { get; set; }

        //Radio Connection State Flag
        public bool IsRadioConnected { get; set; }

        //Message settings
        public bool MessageEncryptionEnabled { get; set; }
        public bool MessageEncodingEnabled { get; set; }

        //Flag response settings
        public bool MessageAcknowledgementEnabled { get; set; }
        public bool MessageRebroadcastEnabled { get; set; }

        //Constructor
        public ApplicationConfiguration(ConfigurationFile ConfigFile)
        {
            ConfigParams = ConfigFile;
            IsRadioConnected = false;
            MessageEncryptionEnabled = false;
            MessageEncodingEnabled = true;
            MessageAcknowledgementEnabled = false;
            MessageRebroadcastEnabled = false;
        }

    }
}
