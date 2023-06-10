<h1 align=center>
    <img align=center width="30%" src="https://github.com/mnikonov/gpt-labs/blob/main/src/Gpt.Labs/splash.png?raw=true" />
</h1>


# How to install application?

## Install app from microsoft store

## Install app from sideloading package

1. Download the app 

    - Navigate to [Github Releases](https://github.com/mnikonov/gpt-labs/releases).
    - Download the MSIXBUNDLE zip archive, of app version that you want to install, on your PC from Assets secion.
    - Extract the contents of the .zip file by right-clicking on the file and selecting “Extract All”.

2. Enable developer mode (optional)

    - On Windows 10 device:
        - Go to Settings > Update & security > For developers > Select Developer mode > Allow sideloading of apps.
    - On Windows 11 device:
        - Go to Settings > Privacy & security > For developers > Turn On Developer mode.

3. Install app certificate (optional)

    - Navigate to the extracted folder and double-click a file named “Gpt.Labs_1.0.0.0_x64_x86_arm64.cer”.
    - Click “Install Certificate”.
    - Select “Local Mashine” option and click “Next”.
    - Select “Place all certificates in the following store” > “Browse...” > Select "Trusted Root Certificate Authorities" >  “Ok”
    - “Finish” certeficate installation

3. Install dependant packages (optional)

    - Navigate to the extracted folder and open a folder named “Dependencies”. 
    - Open the folder that corresponds to your processor architecture (for example  “x64”).
    - Install all .msix packages located in this directory.
        - Double-click on the .msix file and select “Install”.
        - Follow the installation prompts to complete the installation of the developer package on your computer.

3. Install app

    - Navigate to the extracted folder and double-click a file named “Gpt.Labs_1.0.0.0_x64_x86_arm64.msixbundle”.
    - Follow the installation prompts to complete the installation
    - Once the app has installed successfully, you should be able to find it in the Start menu or the All apps list.