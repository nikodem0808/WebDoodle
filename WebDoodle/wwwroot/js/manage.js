
var popupShown = false;
var popup = null;
var popupText = null;
var fieldInput = null;
var fieldLabel = null;
var passwordInput = null;
var action = null;

function Cancel() {
    if (!popupShown) {
        return;
    }

    popup.setAttribute("style", "display: none !important;");

    popupShown = false;
}

function EditUsername() {
    if (popupShown) {
        return;
    }
    popupShown = true;

    popupText.innerHTML = "Change Username";
    fieldInput.innerHTML = "New Username";
    fieldLabel.innerHTML = "New Username";

    action = "/edit_username";

    popup.setAttribute("style", "");
}

function EditPassword() {
    if (popupShown) {
        return;
    }
    popupShown = true;

    popupText.innerHTML = "Change Password";
    fieldInput.innerHTML = "New Password";
    fieldLabel.innerHTML = "New Password";

    action = "/edit_password";

    popup.setAttribute("style", "");
}

function ExecuteAction() {
    var request = new XMLHttpRequest();
    request.open("GET", action);
    request.setRequestHeader("field", fieldInput.value);
    request.setRequestHeader("password", passwordInput.value);
    request.onload = () => {
        if (request.status == 401) {
            ShowInfoPopup("Wrong password!", ipWhite, ipRed, 3500);
            return;
        }
        else {
            ShowInfoPopup("Operation sucessful!", ipWhite, ipGreen);
            setTimeout(() => { window.location.assign(window.location) }, 500);
        }
    };
    request.onerror = () => {
        if (request.status == 401) {
            ShowInfoPopup("Wrong password!", ipWhite, ipRed, 3500);
        }
        else {
            ShowInfoPopup("Operation failed.", ipWhite, ipRed, 3500);
        }
    };
    ShowInfoPopup("Waiting for server...", ipWhite, ipLightBlue);
    request.send();
}

window.onload = () => {
    popup = document.getElementById("popup");
    popupText = document.getElementById("popupText");
    fieldInput = document.getElementById("fieldInput");
    fieldLabel = document.getElementById("fieldLabel");
    passwordInput = document.getElementById("Password");
};
