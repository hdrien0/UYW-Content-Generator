print("\nStarting...\n")

import http.server  
import socketserver
import os
import json
import argparse
import re
from socket import gethostbyname, gethostname
from cv2 import VideoCapture, imwrite
from requests import get
from subprocess import check_output, STDOUT
from glob2 import glob
from urllib.parse import parse_qs
from shutil import copyfile
from requests_toolbelt.multipart import decoder
from PIL import Image

port = 8080 # the port used by the webserver
id = 1000 # the id number of the generated content. All new content has an id above 1000.
jsonData = None #The path of the json file used to save the new content
jsonPath = None #The json data
dataFolder = os.path.join(os.environ['USERPROFILE'],"AppData","LocalLow","Smiling Buddha Games","Use Your Words") #The "persistent data folder" of the game, used to save new content.
removeOriginalContent = False

gameModes = { 
    "BOM" : "blank-o-matic",
    "EE" : "extraExtra",
    "SS" : "surveySays",
    "STT" : "subTheTitle"
} # the name of the game modes in english.

class ThreadingSimpleServer(socketserver.ThreadingMixIn, http.server.HTTPServer):
    pass #using this so the webserver can handle mutliple requests at a time

class myWebServer(http.server.SimpleHTTPRequestHandler):

    def do_POST(self): #the content is submitted via a POST request

        #does_stuff
        self.send_response(200) #sending http response code 
        self.send_header('Content-type','text/html') #headers of the response
        self.end_headers()

        content_length = int(self.headers['Content-Length'])
        content_type = self.headers['Content-Type']
        post_data = self.rfile.read(content_length)
        
        response = handleWebInput(post_data,content_type) #the "handleWebInput" function will take care of the submitted content and will return a string with the error if there is one. If it's all good, no return value.

        if response == None: #writing a response according to the return value of "handleWebInput"
            self.wfile.write('Content sumbitted ! <br>'.encode(encoding='utf_8'))
        else:
            self.wfile.write(('Content not submitted : ' + response + '<br>').encode(encoding='utf_8'))

        self.wfile.write('<a href="/">Back to the main page</a>'.encode(encoding='utf_8'))     

def initJson(jsonPath): #loads or creates the json save file.
    global id 
    global jsonData
    if os.path.exists(jsonPath):
        print("Preexisting JSON file found.")

    else:
        copyfile(os.path.join(dataFolder,"ModData","base.json"),jsonPath) # if no json file is found, starts a new one with "base.json" as template
        print("No JSON file found. Creating a new one.")

    with open(jsonPath,'r') as json_file:
            jsonData = json.load(json_file)
            json_file.close()

    id = int(jsonData["idmax"])+1 #gets the id of the last generated content and saves it
    print("ID set to "+ str(id))

def saveJson(gameMode): #saves generated content to the json file 

    global jsonData
    global jsonPath
    global id
    global removeOriginalContent

    if removeOriginalContent and not gameMode in jsonData["remove"]: #check if the user wants to disable the original content and if yes checks if the game mode of the content being is already disabled.
        jsonData["remove"].append(gameMode) #the "remove" key holds a list with the game modes to disable.

    jsonData["idmax"] = id #updates the "idmax" number of the json file.
    id+=1
    with open(jsonPath, 'w') as outfile: #saves json
        json.dump(jsonData, outfile, indent=4)
        outfile.close() 

def handleSTTData(data): #handles Sub the Title content
    
    global jsonData
    global id
    global gameModes    

    if data[0].content != b'' : #checks if a video has been submitted
        
        with open(os.path.join(dataFolder,"NewContent","sttTemp"+str(id)), 'wb+') as f: #temporarily saves the submitted video
            f.write(data[0].content)
            f.close()
        
        ffmpegArgs = [ 
            os.path.join(dataFolder,"ModData","ffmpeg.exe"),
            "-i",
            os.path.join(dataFolder,"NewContent","sttTemp"+str(id)),
            "-movflags",
            "faststart",
            "-pix_fmt",
            "yuv420p",
            "-vf",
            "scale=trunc(iw/2)*2:trunc(ih/2)*2",
            "-y",
            os.path.join(dataFolder,"NewContent","stt"+str(id)+".mp4")
        ] #arguments passed to FFMPEG to convert the video to MP4

        try : #converts the video to MP4, and extracts its duration and framerate from FFMPEG output.
            ffmpegOutput = check_output(ffmpegArgs,stderr=STDOUT).decode('utf-8')
            duration = re.search('Duration: ([0-9:\.]+)',ffmpegOutput,re.MULTILINE |re.IGNORECASE).group(1).split(".")[0]
            fps = float(re.search('([0-9\.]+) fps',ffmpegOutput,re.MULTILINE |re.IGNORECASE)[1])
            durationInSeconds = sum(x * int(t) for x, t in zip([3600, 60, 1], duration.split(":")))
            os.remove(os.path.join(dataFolder,"NewContent","sttTemp"+str(id))) 

        except Exception as e:
            print("Dropped request : conversion error. ({})".format(e))
            return "error during conversion. Maybe try another file type."  

        if data[1].text == '' or data[2].text == '': #if the time frame of the part to subtitle is not specified, it's set to the duration of the video.
            start = 0.05 
            end = durationInSeconds - 0.05        
        else : #if the time frame of the part to subtitle is specified, converts the beinning and end value to seconds and checks if they are correct.
            start = sum(x * int(t) for x, t in zip([60, 1], data[1].text.split(":")))
            end = sum(x * int(t) for x, t in zip([60, 1], data[2].text.split(":")))
            if start > end or end > durationInSeconds:
                print("Dropped request : time values are incorrect.")
                return "time values incorrect. Check if they do not exceed the duration of the video."
            if start == 0:
                start = 0.05
            if end == durationInSeconds:
                end = durationInSeconds - 0.05            

        try: #extracts a screenshot of the video that will be used for the answers reveal. It gets the image in the middle of the time frame of the part to subtitle.
            screenShotTime = (start + end)/2
            screenShotFrame = screenShotTime * fps
            cap = VideoCapture(os.path.join(dataFolder,"NewContent","stt"+str(id)+".mp4"))
            cap.set(1,screenShotFrame)
            ret, frame = cap.read() 
            imwrite(os.path.join(dataFolder,"NewContent","sttImg"+str(id)+".jpg"), frame)
            copyfile(os.path.join(dataFolder,"NewContent","sttImg"+str(id)+".jpg"),os.path.join(dataFolder,"NewContent","sttImg"+str(id)))
            os.remove(os.path.join(dataFolder,"NewContent","sttImg"+str(id)+".jpg")) #saves the image without extension, and gets rid of the original image.
        except Exception as e:
            print("Dropped request : error while reading video. ({})".format(e))
            return "error while reading video." 

        
        if data[3].text == '': #check if there is an house answer 
            houseAnswer = "No House Answer available."
            print("No house answer provided.")
        else: 
            houseAnswer = data[3].text
            print("A house answer was provided for the question.")
        
        questionData ={ 
                    "familyMode": "true",   
                    "asset": id,
                    "id": id,
                    "houseAnswers": [houseAnswer],
                    "position": "0,-192.0,837,129",
                    "start": start,
                    "end": end
                } #the dictionnary which contains the data and will be dumped to the json file

        jsonData["packages"][0][gameModes["STT"]].append(questionData)
        saveJson(gameModes["STT"])
        print("\"Sub the Title\" content successfully added !")

    else : 
        print("Dropped request : no video submitted.")
        return "no image submitted."   

def handleEEData(data):
    
    global jsonData
    global id
    global gameModes

    if data[0].content != b'' : #checks if an image has been submitted

        with open(os.path.join(dataFolder,"NewContent","ee"+str(id)), 'wb+') as f: #saves the image
            f.write(data[0].content)
            f.close()

        if data[1].text == '': #checks if there is an house answer
            houseAnswer = "No House Answer available."
            print("No house answer provided.")
        else: 
            houseAnswer = data[1].text
            print("A house answer was provided for the question.")

        imgsize = Image.open(os.path.join(dataFolder,"NewContent","ee"+str(id))).size #opens the image with PIL to get its dimensions
        if imgsize[0] > imgsize[1]: #gets the orientation, it will be used in the json file
            orientation = "landscape"
        else:
            orientation = "portrait"
        
        questionData = {  
                "asset": id,
                "id": id,
                "houseAnswers": [houseAnswer],
                "orientation": orientation,
                "familyMode": "true"
            } #the dictionnary which contains the data and will be dumped to the json file

        jsonData["packages"][0][gameModes["EE"]].append(questionData)
        saveJson(gameModes["EE"])
        print("\"Extra Extra\" content successfully added !")

    else : 
        print("Dropped request : no image submitted.")
        return "no image submitted."    

def handleBOMData(data):

    global jsonData
    global id
    global gameModes

    if data["BoMPrompt"][0] != None:
        if not "houseAnswer" in data :
            houseAnswer = "No House Answer available."
            print("No house answer provided.")
        else: 
            houseAnswer = data["houseAnswer"][0]
            print("A house answer was provided for the question.")

        questionData = {
            "id": id,
            "prompt": data["BoMPrompt"][0],
            "houseAnswers": [houseAnswer],
            "familyMode": "true"      
        } 

        jsonData["packages"][0][gameModes["BOM"]].append(questionData) 
        saveJson(gameModes["BOM"])
        print("\"Blank-o-Matic\" content successfully added !")

    else : 
        print("Aborted submission : content empty.")
        return "content empty" 

def handleSSData(data):

    global jsonData
    global id
    global gameModes

    houseAnswers = []
    questions = []
    
    if len(data["SSQuestions"]) < 3:
        print("Dropped request : content empty.")
        return "you must write all of the 3 Survey Says questions." 

    questions = data["SSQuestions"]

    if "SSHouseAnswers" in data:
        houseAnswers = data["SSHouseAnswers"]
    else :
        for i in range(3):
            if ("SSHouseAnswer" + str(i+1)) in data: 
                houseAnswers.append(data["SSHouseAnswer" + str(i+1)])
                print("A house answer was provided.")
            else:
                houseAnswers.append(["No House Answer available."])
                print("No house answer provided.")
    
    questionData = {
        "id": id,
        "prompt": questions[0],
        "prompt2": questions[1],
        "prompt3": questions[2],         
        "houseAnswers": houseAnswers[0],
        "houseAnswers2": houseAnswers[1],
        "houseAnswers3": houseAnswers[2],
        "familyMode": "true"
    }

    jsonData["packages"][0][gameModes["SS"]].append(questionData) 
    saveJson(gameModes["SS"])
    print("\"Survey Says\" content successfully added !") 

def handleWebInput(post_data,content_type):

    if 'multipart/form-data' in content_type: #The encoding for STT and EE content is "multipart/form-data" (there is a file uploaded)
        data = decoder.MultipartDecoder(post_data, content_type).parts #decodes the content
        if data[0].text == 'SubTheTitle': #a hidden input specifies the gamemode of the content. Checks its value.
            return handleSTTData(data[1:])
            
        elif data[0].text == 'ExtraExtra':
            return handleEEData(data[1:])
        

    else:
        data = parse_qs(post_data.decode('utf-8')) #else the post request content is just text. it decodes and checks the gamemode of the content.

        if "BoMPrompt" in data:
            return handleBOMData(data)
        
        elif "SSQuestions" in data:
            return handleSSData(data)
        
        else:
            return "content empty."

def askYesNo(prompt): #simple function to ask the user a yes/no question. returns the answer as a boolean

    answer = input(prompt).lower()
    while not (answer == "y" or answer == "n"):
        print("\nPlease write \"y\" (yes) or \"n\" (no).")
        answer = input(prompt).lower()
    if answer == "y":
        return True
    else:
        return False
            

try: 
    #parsing the arguments of the application
    parser = argparse.ArgumentParser(description='Use your words content generator / webserver : add new content to the game !')
    parser.add_argument('-fr','--french', help='Use this flag if your game language is set to french. If this argument is not specified, the generated content will only show up in a game in english.',action='store_true',required=False)
    parser.add_argument('-d','--disable-original-content', help='Use this this flag if you don\'t want the game to load its original content (effective only on the modified game modes). The original content is just not loaded, not deleted.',action='store_true',required=False)
    parser.add_argument('-e','--enable-original-content',help='Use this flag to re-enable the original game content if you disabled it previously by using the \'-d\' flag.',action='store_true',required=False)
    parser.add_argument('-c','--clear', help='Remove all the content previously generated. It will be permanently deleted.', action='store_true',required=False)
    parser.add_argument('-j','--json', help='Use a custom json file to save the new content. Usage : \"--json [path]\"',required=False,default=None)
    parser.add_argument('-p','--port', help="Use a cutom port for the webserver. Default is 8080.", required=False,default=None)
    args = parser.parse_args()

    try :

        #asks if the user wants a manual configuration and if yes it overrides the arguments' values by the user's answers.
        print("To see all the different arguments available, use \"--help\"")
        manualConfig = askYesNo("Do you want to configure the generator manually ? (y/n) ")
        if manualConfig:
            if askYesNo("Is your game language set to french ? (y/n) "):
                args.french = True
            if askYesNo("Do you want to disable the original content (only effective on the gamemodes with new questions) ? (y/n) "):
                args.disable_original_content = True
            elif askYesNo("Do you want to re-enable the game's original content ? (only needed if you disabled it previously) (y/n) "):
                args.enable_original_content = True
            if askYesNo("Do you want to remove all the content previously generated ? (it will be permanently deleted) (y/n) "):
                args.clear = True
            if askYesNo("Do you want to use a custom JSON file for the generated content ? (only for advanced users) (y/n) "):
                args.json = input("Path to the JSON file : ")
            if askYesNo("Do you want to use a different port for the webserver ? The default one is 8080 (only for advanced users) (y/n) "):
                args.port = input("Enter the port number : ") 

        #if the game is in french, changes the game modes' names to their french version (for the json save file)
        if args.french:
            gameModes = {
                "BOM" : "blank-o-matic_FR",
                "EE" : "extraExtra_FR",
                "SS" : "surveySays_FR",
                "STT" : "subTheTitle_FR"
            }
            print("\nLanguage set to french.")

        elif not manualConfig: 
            print("\nLanguage set to english. If your game is in french, please add \"-fr\" or \"--french\"")
        
        if os.path.exists(dataFolder): #checks if the new content exists. If not it creates it
            if not os.path.exists(os.path.join(dataFolder,"NewContent")):
                os.mkdir(os.path.join(dataFolder,"NewContent"))
                 
        else: #raises an exception if the "persistent data folder" of the game isn't found.
            print("The persistent data folder of the game can't be found. Are you sure you started the game at least once ?\nIf yes, check if this path is valid : \n" + dataFolder + "\nIf the path seems not valid, please specify a valid path with the option -j")
            input("Press enter to quit.")
            quit()
            
        try : #modifies the port number to the value specified by the user.
            if args.port != None or args.port == '':
                port = int(args.port)
        except Exception as e:
            port = 8080
            print("The specified port number is incorrect. Port number reset to default value (8080).")

        if args.json != None and args.json != '': #replaces the default port number by the value specified by the user.
            if os.path.exists(args.json):
                jsonPath = args.json

            else :
                print("The path to the json file is invalid.")
                input("Press enter to quit.")
                quit()

        else: #replaces the default json path by the the string specified by the user
            jsonPath = os.path.join(dataFolder,"NewContent","nc.json") 
        
        if args.clear: #if the user chose to clear the new content, ask a confirmation. If the user confirms, it deletes all the new content and clears the "used content" file.

            confirmation = askYesNo("\nAre you sure you want to deleted all the content generated previously ? (y/n) ")           
            if confirmation:
                EEFiles = glob(os.path.join(dataFolder,"NewContent","ee*")) #uses glob to find all the images and videos
                STTFiles = glob(os.path.join(dataFolder,"NewContent","stt*"))
                for EEFile in EEFiles:
                    os.remove(EEFile)
                for STTFile in STTFiles:
                    os.remove(STTFile)
                if os.path.exists(jsonPath):
                    os.remove(jsonPath)

                usedContentFile = open(os.path.join(dataFolder,"UsedContent.json"),'r') 
                usedContentJson = json.load(usedContentFile)
                for mode in usedContentJson:
                    for usedId in usedContentJson[mode]:
                        if int(usedId) > 999:
                            usedContentJson[mode].remove(usedId)
                usedContentFile.close()

                usedContentFile = open(os.path.join(dataFolder,"UsedContent.json"),'w') 
                json.dump(usedContentJson, usedContentFile, indent=4)
                usedContentFile.close()
                

                print("The custom content has been deleted, and the \"Used content\" file cleaned.\n")

            else:
                print("No action has been performed.")
        
        if args.enable_original_content and args.disable_original_content :
            print("You can't enable the game content (-e) and disable it (-d) at the same time.") 
            input("Press enter to quit.")
            quit()
        

    
    except Exception as error:
        print("Error at initialisation : " + repr(error))
        quit()

    server = ThreadingSimpleServer(('', port), myWebServer) 
    hostname = gethostname()
    localIp = gethostbyname(hostname)

    try:
        publicIp = get('https://api.ipify.org').text
    except Exception:
        publicIp = ""

    print("Started webserver on port {} on the computer \"{}\".".format(str(port),hostname))
    print("\nTo access the web interface from \"{}\", use \"http://localhost:{}\" in a browser.".format(hostname,str(port)))
    print("To access the web interface from the local network, use \"http://{}:{}\" in a browser.".format(localIp,str(port)))
    if publicIp != '':
        print("To access the web interface from any device connected to the internet, use \"http://{}\" in a browser. (you will need to open this specific port)\n".format(publicIp))

    else:
        print("Device not connected to the internet.\n")
    
    initJson(jsonPath)
    
    if args.enable_original_content : #if the user chose the re-enable the orginal contents, it clears the "remove" list and saves the json file.
        jsonData["remove"].clear()
        with open(jsonPath, 'w') as outfile:
            json.dump(jsonData, outfile, indent=4)
            outfile.close()
        removeOriginalContent = False
        print("The original content has been enabled again.")

    if args.disable_original_content : #if the user chose to diable the original content, checks if new content has already been added. If yes and if the corresponding gamemode isn't disabled, it disables it.
        removeOriginalContent = True
        for mode in jsonData["packages"][0]:
            if jsonData["packages"][0][mode] != [] and isinstance(jsonData["packages"][0][mode],list):
                if not str(mode) in jsonData["remove"]:
                    jsonData["remove"].append(str(mode))
        with open(jsonPath, 'w') as outfile:
            json.dump(jsonData, outfile, indent=4)
            outfile.close()
        print("The original content has been disabled.")


    server.serve_forever()

except KeyboardInterrupt:
    print('^C received, shutting down the program.')
    try :
        server.socket.close()
    except Exception: 
        pass

