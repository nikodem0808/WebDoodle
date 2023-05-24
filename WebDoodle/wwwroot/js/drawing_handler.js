
var drawKit = {
    canvas: null,
    pen: null
};

var colorInput = null;
var widthInput = null;

var dragging = false;

var xAdjust = 200;
var yAdjust = 200;

drawKit.canvas = document.getElementById("surface");
drawKit.pen = drawKit.canvas.getContext("2d");

function getMPos(e) {
    var rect = drawKit.canvas.getBoundingClientRect();
    var scaleX = drawKit.canvas.width / rect.width;
    var scaleY = drawKit.canvas.height / rect.height;

    return {
        x: (e.clientX - rect.left) * scaleX,
        y: (e.clientY - rect.top) * scaleY
    }
}

function HookCanvasToEvents() {
    drawKit.canvas.addEventListener('mousedown', function (e) {
        var dkit = drawKit;
        var { x, y } = getMPos(e);
        dkit.pen.moveTo(x, y);
        dragging = true;
    });
    drawKit.canvas.addEventListener('mousemove', function (e) {
        var dkit = drawKit;
        if (!dragging) return;
        var { x, y } = getMPos(e);
        dkit.pen.lineTo(x, y);
        dkit.pen.stroke();
    });
    drawKit.canvas.addEventListener('mouseup', function (e) {
        var dkit = drawKit;
        var { x, y } = getMPos(e);
        dkit.pen.stroke();
        dragging = false;
        drawKit.pen.beginPath();
    });
    drawKit.canvas.addEventListener('mouseleave', function (e) {
        var dkit = drawKit;
        var { x, y } = getMPos(e);
        dkit.pen.stroke();
        dragging = false;
        drawKit.pen.beginPath();
    });
}

function ClearCanvas() {
    drawKit.pen.clearRect(0, 0, Number(drawKit.canvas.width), Number(drawKit.canvas.height));
    drawKit.pen.beginPath();
}

function Send() {
    var idata = "[" + drawKit.pen.getImageData(0, 0, Number(drawKit.canvas.width), Number(drawKit.canvas.height)).data.toString() + "]";
    var request = new XMLHttpRequest();
    request.open("POST", "/save_drawing");

    ShowInfoPopup("Waiting for server...", ipWhite, ipLightBlue);

    request.onload = () => {
        var text;
        var bgcol;
        if (request.status == 200) {
            text = "Saved successfully!";
            bgcol = ipGreen;
        }
        else {
            text = "Save failed!";
            bgcol = ipRed;
        }
        ShowInfoPopup(text, ipWhite, bgcol, 3500);
    };

    request.onerror = () => {
        ShowInfoPopup("Unknown error.", ipWhite, ipRed, 3500);
    };

    request.send(idata);
}

function Edit() {
    var idata = "[" + drawKit.pen.getImageData(0, 0, Number(drawKit.canvas.width), Number(drawKit.canvas.height)).data.toString() + "]";
    var request = new XMLHttpRequest();
    request.open("POST", "/save_drawing");
    request.setRequestHeader("ID", editDrawingId.toString());

    ShowInfoPopup("Waiting for server...", ipWhite, ipLightBlue);

    request.onload = () => {
        var text;
        var bgcol;
        if (request.status == 200) {
            text = "Updated successfully!";
            bgcol = ipGreen;
        }
        else {
            text = "Update failed!";
            bgcol = ipRed;
        }
        ShowInfoPopup(text, ipWhite, bgcol, 3500);
    };

    request.onerror = () => {
        ShowInfoPopup("Unknown error.", ipWhite, ipRed, 3500);
    };

    request.send(idata);
}

function GetDrawing(id) {
    var request = new XMLHttpRequest();
    request.open("GET", "/get_drawing");
    request.setRequestHeader("ID", id.toString());
    request.onload = () => {
        var sdata = JSON.parse(request.response);
        var idata = drawKit.pen.getImageData(0, 0, 400, 400);
        for (var i in sdata) {
            idata.data[i] = sdata[i];
        }
        drawKit.pen.putImageData(idata, 0, 0);
        CloseInfoPopup();
    };
    request.onerror = () => {
        ShowInfoPopup("Unknown error.", ipWhite, ipRed, 3500);
    };
    ShowInfoPopup("Waiting for server...", ipWhite, ipLightBlue);
    request.send();
}

window.onload = function() {
    if (typeof(editDrawingId) !== typeof(undefined)) {
        GetDrawing(editDrawingId);
    }
    colorInput = document.getElementById("colorInput");
    widthInput = document.getElementById("widthInput");

    HookCanvasToEvents();

    colorInput.addEventListener("change", () => {
        drawKit.pen.strokeStyle = colorInput.value;
    });

    widthInput.addEventListener("change", () => {
        drawKit.pen.lineWidth = widthInput.value;
    });
};

