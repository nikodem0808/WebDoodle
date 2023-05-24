
var drawingContainer = document.getElementById("drawingContainer");
var pageNumber = document.getElementById("pageNumber");
var drawings = [];
var currentPage = 0;
const maxPagesCached = 5;
const pageSize = 6;
var pageCache = [];
var lock = false;
var popupShown = false;
var popupContainer = null;
var popupDrawing = null;
var popupDrawingId = null;

function UpdatePageNumber() {
    pageNumber.innerHTML = (currentPage + 1).toString();
}

async function partlyCopyArray(min, max, source, target) {
    for (var i = min; i < max; i++) {
        target[i] = source[i];
    }
}

function ClosePopup() {
    if (!popupShown) {
        return;
    }
    popupContainer.remove();
    popupContainer = null;
    popupDrawing = null;
    popupDrawingId = null;
    popupShown = false;
}

function EditSelectedDrawing() {
    var request = new XMLHttpRequest();
    request.open("GET", "/put_for_edit");
    request.setRequestHeader("ID", popupDrawingId.toString());
    request.onload = () => {
        if (request.status == 400 || request.status == 410) {
            ShowInfoPopup("Operation failed.", ipWhite, ipRed, 3500);
            return;
        }
        else {
            ShowInfoPopup("Operation sucessful!", ipWhite, ipGreen);
            setTimeout(() => { window.location.assign("/Home/Draw") }, 500);
        }
    };
    request.onerror = () => {
        ShowInfoPopup("Operation failed.", ipWhite, ipRed, 3500);
    };
    request.send();
}

function DeleteSelectedDrawing() {
    var request = new XMLHttpRequest();
    request.open("GET", "/delete_drawing");
    request.setRequestHeader("ID", popupDrawingId.toString());
    request.onload = () => {
        if (request.status == 400 || request.status == 410) {
            ShowInfoPopup("Operation failed.", ipWhite, ipRed, 3500);
            return;
        }
        else {
            ShowInfoPopup("Operation sucessful!", ipWhite, ipGreen);
            setTimeout(() => { window.location.assign(window.location) }, 500);
        }
    };
    request.onerror = () => {
        ShowInfoPopup("Operation failed.", ipWhite, ipRed, 3500);
    };
    ShowInfoPopup("Waiting for server...", ipWhite, ipLightBlue);
    request.send();
}

function ShowPopup(elem, id) {
    if (popupShown) {
        return;
    }

    popupShown = true;
    popupDrawingId = id;

    var idata = elem.getContext("2d").getImageData(0, 0, 400, 400);

    popupContainer = document.createElement("div");
    popupContainer.setAttribute("class", "popup");

    var popupExitButton = document.createElement("div");
    popupExitButton.setAttribute("id", "popupExitButton");
    popupExitButton.innerHTML = "&ndash;";
    popupExitButton.addEventListener('mousedown', (e) => {
        ClosePopup();
    });

    popupDrawing = MakeExtendedDrawingDOMElement();
    popupDrawing.setAttribute("id", "popupDrawing");
    popupDrawing.loadImageData(idata.data);

    var popupEditButton = document.createElement("div");
    popupEditButton.setAttribute("class", "btn-light rounded-pill m-1 ps-1 pe-1 popupButton");
    popupEditButton.innerHTML = "Edit";
    popupEditButton.addEventListener('mousedown', (e) => {
        EditSelectedDrawing();
    });

    var popupDeleteButton = document.createElement("div");
    popupDeleteButton.setAttribute("class", "btn-danger rounded-pill m-1 ps-1 pe-1 popupButton");
    popupDeleteButton.innerHTML = "Delete";
    popupDeleteButton.addEventListener('mousedown', (e) => {
        DeleteSelectedDrawing();
    });

    popupContainer.appendChild(popupExitButton);
    popupContainer.appendChild(popupDrawing);
    popupContainer.appendChild(popupEditButton);
    popupContainer.appendChild(popupDeleteButton);

    document.body.appendChild(popupContainer);
}

function MakeExtendedDrawingDOMElement() {
    var elem = document.createElement("canvas");
    elem.setAttribute("class", "m-1 b-1 border-dark drawing");
    elem.width = 400;
    elem.height = 400;
    elem.addEventListener('mousedown', (e) => {
        var elem = e.target;
        var id = e.target.getAttribute("id");
        ShowPopup(elem, id);
    });
    elem.loadImageData = async function (sdata) {
        var pen = this.getContext("2d");
        var idata = pen.getImageData(0, 0, 400, 400);
        var i;
        const partitionSize = 128;
        for (i = 0; i + partitionSize < sdata.length; i += partitionSize) {
            await partlyCopyArray(i, i + partitionSize, sdata, idata.data);
        }
        await partlyCopyArray(i, sdata.length, sdata, idata.data);
        pen.putImageData(idata, 0, 0);
    };
    return elem;
}

async function PutDrawings(pageData) {
    while (drawings.length < pageData.length) {
        var elem = MakeExtendedDrawingDOMElement();
        drawingContainer.appendChild(elem);
        drawings.push(elem);
    }
    while (drawings.length > pageData.length) {
        var elem = drawings.pop();
        elem.remove();
    }
    var i = 0;
    for (var entry of pageData) {
        drawings[i].loadImageData(entry.data);
        drawings[i].setAttribute("id", entry.id);
        i++;
    }
}

async function LoadDrawings() {
    await GetDrawingPage(currentPage, PutDrawings);
}

async function GetDrawingPage(page, callback) {
    var elem = undefined;
    if ((elem = pageCache.find((entry) => (entry.num == page))) != undefined) {
        return await callback(elem.data);
    }
    var request = new XMLHttpRequest();
    request.open("GET", "/get_drawing_page");
    request.setRequestHeader("page", page.toString());
    request.onload = async function () {
        CloseInfoPopup();
        var pageData = JSON.parse(request.response);
        if (pageData.length == 0) {
            await callback(pageData);
            return;
        }
        pageCache.push({ num: page, data: pageData });
        if (pageCache.length > maxPagesCached) {
            pageCache = pageCache.splice(1);
        }
        await callback(pageData);
    };
    request.onerror = () => {
        ShowInfoPopup("Operation failed.", ipWhite, ipRed, 3500);
    };
    ShowInfoPopup("Waiting for server...", ipWhite, ipLightBlue);
    request.send(); 
}

async function nextPage() {
    if (lock) {
        return;
    }
    lock = true;
    await GetDrawingPage(currentPage + 1, async (pageData) => {
        if (pageData.length > 0) {
            currentPage++;
            UpdatePageNumber();
            await PutDrawings(pageData);
        }
        lock = false;
    });
}

async function previousPage() {
    if (lock) {
        return;
    }
    lock = true;
    if (currentPage == 0) {
        lock = false;
        return;
    }
    currentPage--;
    UpdatePageNumber();
    await LoadDrawings();
    lock = false;
}

window.onload = LoadDrawings;
