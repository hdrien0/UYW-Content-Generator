
function ChangeController(controller){

    var EEController = document.getElementsByClassName("EEController")[0]
    var BoMController = document.getElementsByClassName("BoMController")[0]
    var SSController = document.getElementsByClassName("SSController")[0]
    var STTController = document.getElementsByClassName("STTController")[0]

    if (controller == "ExtraExtra" ){
        EEController.style.display = "block";
        BoMController.style.display = "none";
        SSController.style.display = "none";
        STTController.style.display = "none";
    } else if (controller == "BlankoMatic"){
        BoMController.style.display = "block";
        EEController.style.display = "none";
        SSController.style.display = "none";
        STTController.style.display = "none";
    } else if (controller == "SurveySays"){
        BoMController.style.display = "none";
        EEController.style.display = "none";
        SSController.style.display = "block";
        STTController.style.display = "none";
    } else if (controller == "SubTheTitle"){
        BoMController.style.display = "none";
        EEController.style.display = "none";
        SSController.style.display = "none";
        STTController.style.display = "block";
    }
}