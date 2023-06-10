<h1 align=center>
    <img align=center width="40%" src="https://github.com/mnikonov/gpt-labs/blob/main/src/Gpt.Labs/splash.png?raw=true" />
</h1>

# What is “GPT Labs”?

**GPT Labs** is an **Windows 11** application that utilizes powerful content generative API from **OpenAI**. 

With **GTP Labs**, you can easily generate text and images based on a variety of prompts, giving you unparalleled control over your creative output.

App features the connection to GTP models from OpenAI, allowing you to:

- draft documents 
- write code 
- ask questions 
- create conversational chats 
- and more with ease. 

Plus, with Image Generation API (DALL·E 2), you can:

- create stunning and lifelike images from scratch
- make edits to existing images
- generate variations of those images

Whether you're a creative professional or simply curious about the possibilities of AI, **GTP Labs** has something for everyone. App offers flexible and intuitive features, allowing you to combine text and images to create stunning presentations, marketing campaigns, educational content, writing assistants, and more.

So whether you're a seasoned developer or just getting started with AI-powered applications feel free to contribute. 

![Chat screen](https://github.com/mnikonov/gpt-labs/blob/main/content/screens/chat.jpg?raw=true)

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

4. Install dependant packages (optional)

    - Navigate to the extracted folder and open a folder named “Dependencies”. 
    - Open the folder that corresponds to your processor architecture (for example  “x64”).
    - Install all .msix packages located in this directory.
        - Double-click on the .msix file and select “Install”.
        - Follow the installation prompts to complete the installation of the developer package on your computer.

5. Install .NET 7.0 Desktop Runtime (optional)
    
    - Go to the official Microsoft .NET [website](https://dotnet.microsoft.com/) to download the .NET 7.0 Desktop Runtime installer.
    - Click the Download .NET button on the home page.
    - Select the .NET desktop runtime installer based on your operating system. 
    - Click the Download button for the installer based on your preferred language.
    - Once the download is complete, locate the installer file and double-click it to launch the installation wizard.
    - Follow the prompts in the installation wizard to complete the installation.

6. Install app

    - Navigate to the extracted folder and double-click a file named “Gpt.Labs_1.0.0.0_x64_x86_arm64.msixbundle”.
    - Follow the installation prompts to complete the installation
    - Once the app has installed successfully, you should be able to find it in the Start menu or the All apps list.

# How can I get OpenAI Keys?

### Get API keys

- Go to OpenAI's Platform [website](https://platform.openai.com) 
- Sign in with an OpenAI account.
- Click your profile icon at the top-right corner of the page and select “[View API Keys](https://platform.openai.com/account/api-keys)“.
- Click "Create New Secret Key" to generate a new API key.

### Get Organization ID

- Go to OpenAI's Platform [website](https://platform.openai.com) 
- Sign in with an OpenAI account.
- Click your profile icon at the top-right corner of the page and select “[Manage Account](https://platform.openai.com/account/org-settings)“.
- Find value under “Organization ID“ label.