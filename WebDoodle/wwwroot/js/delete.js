
var passwordField = null;

function SendDeleteRequest() {
    var request = new XMLHttpRequest();
    request.open("GET", "/delete_account", false);
    request.setRequestHeader("password", passwordField.value);
    request.onload = () => {
        if (request.status != 200) {
            ShowInfoPopup("Operation failed.", ipWhite, ipRed, 3500);
            return;
        }
        window.location.assign("/Home/Index");
    };
    ShowInfoPopup("Waiting for server...", ipWhite, ipLightBlue);
    request.send();
}

function GoBack() {
    window.location.assign("/Login/Manage");
}

window.onload = () => {
    passwordField = document.getElementById("passwordField");
};
