
var infoPopup = null;
var infoPopupShown = false;
var infoPopupTimer = null;

const ipGreen = "#12F212";
const ipRed = "#F21212";
const ipWhite = "#FFFFFF";
const ipLightBlue = "#73F1FF";
const ipGray = "#7F7F7F";

function ShowInfoPopup(text, col, bgcol, time_ms) {
    if (infoPopupShown) {
        infoPopup.remove();
        infoPopupShown = false;
    }
    infoPopupShown = true;
    if (text === undefined) {
        text = "";
    }
    if (col === undefined) {
        col = ipWhite;
    }
    if (bgcol === undefined) {
        bgcol = ipGray;
    }
    if (time_ms !== undefined) {
        infoPopupTimer = setTimeout(CloseInfoPopup, time_ms);
    }

    infoPopup = document.createElement("div");
    infoPopup.setAttribute("class", "infoPopup");

    var infoPopupText = document.createElement("h4");
    $(infoPopupText).css("text-align", "center");

    infoPopup.appendChild(infoPopupText);
    document.body.appendChild(infoPopup);

    SetInfoPopupColor(col);
    SetInfoPopupText(text);
    SetInfoPopupBackgroundColor(bgcol);
}

function SetInfoPopupText(text) {
    if (!infoPopupShown) {
        return;
    }
    infoPopup.childNodes.item(0).innerHTML = text;
}

function SetInfoPopupBackgroundColor(bgcol) {
    if (!infoPopupShown) {
        return;
    }
    $(infoPopup).css("background-color", bgcol);
}

function SetInfoPopupColor(col) {
    if (!infoPopupShown) {
        return;
    }
    $(infoPopup).css("color", col);
}

function CloseInfoPopup() {
    if (!infoPopupShown) {
        return;
    }
    infoPopup.remove();
    infoPopupShown = false;
    if (infoPopupTimer !== null) {
        clearTimeout(infoPopupTimer);
        infoPopupTimer = null;
    }
}

function SetInfoPopupTimer(time_ms) {
    if (!infoPopupShown) {
        return;
    }
    if (infoPopupTimer !== null) {
        clearTimeout(infoPopupTimer);
    }
    infoPopupTimer = setTimeout(CloseInfoPopup, time_ms);
}
