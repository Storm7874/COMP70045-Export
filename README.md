<h1>COMP70045-Export</h1>
Main repository for the MSc Dissertation module, COM70045
Contains the desktop handler application, transciever firmware, and support tools

<h2>Desktop Application</h2>
<p>To run the application, some configuration is required. This is covered in the sections below</p>

<h3>Dictionaries</h3>
<p>Dictionaries are already configured and included in the required application directory. No further modification should be required.</p>

<h3>OTP File</h3>
<p>The binary pad file is not included, but can be found at the following location:</p>
<p></p>https://drive.google.com/file/d/1evELK3DBIOzZ1nKkQmNDcd1zdEK_Bb0O/view?usp=sharing</p>
<p>This is due to the file size limits on Github.</p>

<h3>Config File</h3>
<p>An application config file is included, but the directories for the dictionary and pad files should be updated to represent their locations within the local solution.</p>
<p>In addition to this, the COM port of the radio should be updated</p>

<h2>Transciever Firmware</h2>
<p>No configuration is required for this component, if it is to be used to flash a new Heltec board, the required libraries must be configured as per the setup guide for the radio.</p>
