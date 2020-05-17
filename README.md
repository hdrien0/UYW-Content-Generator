# Use Your Words content generator

**This is still a beta. There might be bugs, it can even not work at all.**

This is a mod of the game [Use Your Words](https://store.steampowered.com/app/521350/Use_Your_Words/) which allows you to add your own questions, images and videos to the four game modes : Sub the Title, Extra Extra, Blank-O-Matic and Survey Says. You can also provide a house answer.

The mod goes with a CLI (Command line interface) application which can be used to easily generate new content. It consists in a webserver which serves a simple webpage that can be used to submit new content.
It can be accessed from the local network and by anyone connected to the Internet if you do the proper port configuration : you will just have to give your IP to your friends and they'll will be able to submit their own ideas which will automatically be added to your game.

Also, I'm French so there will surely be tons of mistakes in this README and in the mod in the general. Don't hesitate to contribute to correct me and improve the readabilty of all this!

![SC1](https://imgur.com/YSLoAS7.png)
![SC2](https://imgur.com/yzCwwxC.png)
![SC3](https://imgur.com/MH5MzqX.png)


![SCG1](https://imgur.com/AFqwfaP.png)
![SCG2](https://imgur.com/dNCtcIf.png)
![SCG3](https://imgur.com/GvPKnwF.png)
![SCG4](https://imgur.com/Vc2x66l.png)

## Features

* Add new content (house answers included) to all the game modes 

    * **Sub the Title** : add your own videos. All common video formats are supported. Animated GIFs are also supported.
    * **Extra! Extra!** : add your own photos. Only JPG and PNG images are supported.
    * **Blank-O-Matic** : add your own prompts.
    * **Survey Says** : add your own questions. 

* The game’s assets are not modified. All the new content can be easily removed

* You can choose to enable or disable the original content for the game modes with new content.

* The mod goes with a CLI application which allows you to add new content easily. It hosts a simple webserver and provides a web interface that can be used by you and your friends to submit new content very simply. It will automatically be added to your game. 

* The new content is saved on your computer, and is only deleted if you want to.

* It supports the English and French versions of the game.

## Compatibility

This mod only works with Windows. MacOS support might come in the future.

## Installing

There is no installer yet : you have to do the installation manually.

### Step 1 : Copying the mod files

You need to copy the files needed by the mod to the right location on your computer.

* First, you'll need to grab the latest release by going to the "Release" tab. 

* Decompress the zip file

* Copy the directory `ModData`.

* Hit `[WindowsKey] + R`, type `%appdata%` and press enter.

* Go back to the `AppData` directory

* Go to `LocalLow`, `Smiling Buddha Games` and finally `Use Your Words`.

* Paste the `ModData` directory you copied earlier here (at the end, it should look like `C:\Users\[user]\AppData\LocalLow\Smiling Buddha Games\Use Your Words\ModData`)

* Open the `ModData` directory, right-click on `UYWGenerator.exe` and create a shortcut.

* Move the shortcut to the location of your choice, you will use it to start `UYWGenerator.exe`

**Do not** move the file `UYWGenerator.exe` itself somewhere else or use a copy of it : the webserver won't work.

### Step 2 : Replacing the game's DLL

#### Finding the game's executable path

There are multiple ways to do it. If you purchased the game on Steam you can do that :

* Open steam and go to your Library

* Right-click on `Use Your Words` > `Properties` > `Local Files` > `Browse Local Files`

#### Replacing the DLL

* Now that you are in the game's directory, open the directory `uyw_Data` and then `Managed`

* Finally copy the file `Assembly-CSharp.dll` somewhere else (as a backup) and paste here the file `Assembly-CSharp.dll` that is in the zip file you downloaded.

**The installation is now finished!**

## Using `UYWGenerator.exe`

### Configuration 

You have two methods to configure `UYWGenerator.exe` :

#### Manual mode (the easiest one)

Just start `UYWGenerator.exe` or its shortcut, and you'll be prompted to configure it manually.

#### Arguments (the fastest one)

You can also supply arguments to the executable :

* `-fr, --french` : Use this flag if your game language is set to French. 

* `-d, --disable-original-content` : Use this flag to disable the original content of the game, only for the game modes with new content.

* `-e, --enable-original-content` : Use this flag to re-enable the original game content if you disabled it previously by using the `-d` flag.

* `-c, --clear` : Use this flag to remove all the content previously generated. It will be permanently deleted.

* `-p, --port [port]` : Use a cutom port for the webserver. Default is 8080. *Advanced users only*

* `-j, --json [path]` : Use a custom JSON file to save the new content. *Advanced users only*

### Connecting

**IMPORTANT** : The first time you execute the application, a Windows Firewall message like [this one](https://winaero.com/blog/wp-content/uploads/2017/03/Windows-10-Firewall-Notifications.png) should show up. 

**You need to allow access to the app, else you won't be able to access the webserver** 

Ok, now the webserver should be up and running. The output should be something like :

```
Started webserver on port [port] on the computer "[computer]".

To access the web interface from "[Computer]", use "http://localhost:[port]" in a browser.
To access the web interface from the local network, use "http://[localIp]:[port]" in a browser.
To access the web interface from any device connected to the internet, use "http://[publicIp]" in a browser. (you will need to open this specific port)

No JSON file found. Creating a new one.
ID set to 1000
```
The words in brackets will be replaced by real values.

* `[port]` will be the port of your computer the webserver is using. The default one is 8080.

* `[computer]` will be the name of your computer.

* `[localIp]` and `[publicIp]` will be the your local and public IP

Connecting from your computer or the local network won't require additional configuration.
However, if you want people that are not on your local network to access to the webserver (i.e your friends at their house), there is one last step.

### Opening your ports (optional)

You'll need to tell your router to open a specific port, and forward the traffic to your computer.
The processs depends on your ISP/router, so I can't help you there. You should be able to find how to do it online.
However, it should look like this :

* Go to your router's web interface, go to the options and then `Port forwarding`

* Click `Add new`

* Use the following options :

| Option        | Value                                                                      | 
| ------------- |:--------------------------------------------------------------------------:|
| Name          | The name of your choice                                                    | 
| IP address    | The local IP of your computer (the value of [localIp] above)               | 
| Protocol      | Choose `TCP`                                                               |
| External port | `80`                                                                       |
| Internal port | The port the webserver is using (the value of [port] above, default `8080`)|

#### Case 1 : Your router allows you to open the port `80`

This is the best scenario : you will be able to access the web interface from any device connected to the Internet just with `http://[publicIp]` (as mentioned in the output of the program)

#### Case 2 : You can't open the port `80`

If your router won't allow you to open the port `80`, instead of the port `80` for the option `External port` just choose any available port, it doesn't matter which one.

The only thing it will change is that from outside your local network (the Internet) you'll have to use http://[publicIP]:[externalPort] to connect to the web server.

### Adding new content

Now that you are connected to the webserver, the interface should be very simple and straightforward. Once you're done, just close `UYWGenerator.exe` and play the game. You should see the content you added!

**Note** : Submitting high quality images and videos can take some time, after hitting `Submit` on the web interface just wait until you see a confirmation.

## Tools used for this mod

* [DNSpy](https://github.com/0xd4d/dnSpy) : It allowed me to decompile the game's DLL, view and modify some C# scripts and then recompile it.

* [Unity Assets Bundle Extractor](https://github.com/DerPopo/UABE) : Useful for extracting compiled assets of a Unity game. It helped extract `Manifest.json` and `Manifest - Addon.json`

* [PyInstaller](https://www.pyinstaller.org/) : Very useful to convert python scripts to standalone executables (.exe). 

## Want to know more about how the mod works?

The mod is open source. Here on the GitHub I included the python script of `UYWGenerator.exe`, and all the game's C# scripts I modified. 

`UYWGenerator.exe` creates a JSON file with the new content. This file is very similar to the file `Manifest.json`, the file from which the game loads the original content. Every piece of content in Use Your Words has a unique ID number which is used to identify it and locate the assets needed (the video if it's Sub the Title, or the photo if it's Extra Extra).

All the generated content have an ID above 1000 : that's how the game knows if the assets needed are not part of the original assets. All the new assets, alongside with the JSON file containing the new content package (`nc.json`) are located in a directory called `NewContent` in the game's persistent data directory (`AppData/LocalLow/Smiling Buddha Games/Use Your Words.`)

* `Sub the Title` : there are two resources needed : 
    * The video of course, which is converted to `.mp4` and saved as `stt[ID].mp4`
    * A frame of the video, which will be used during the reveal of the votes. It is saved as `sttImg[ID]` (without extension, but it's a JPG file.)

* `Extra! Extra!` : the newspaper's image is the only resource needed. It is saved as `ee[ID]` (without extension, but it's either a JPG or a PNG file.)

Now the mod makes the game load the new JSON file (`nc.json`) as a normal content package. Also, for `Extra! Extra!` and `Sub the Title` if the game sees that the content's ID is above 1000, it will fetch the assets it needs from the correct location (the directory `NewContent`) and convert them to usable in-game objects. 

If you want to see how all of this looks like, you can take a look at the directory `NewContentDemo` which corresponds to the screenshots of the game you saw earlier. 

## Contributing 

As I said, don't hesitate to contribute even if it's just to correct typos and english mistakes!
Also, the mod in general can be improved on many aspects : the ugly web interface and the interface of `UYWGenerator.exe` are some examples.


This mod uses [FFmpeg](http://ffmpeg.org) licensed under the [LGPLv2.1](http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html).

