var colors = ["#000", "#005", "#00A", "#00F", "#050", "#055", "#05A", "#05F",
    "#0A0", "#0A5", "#0AA", "#0AF", "#0F0", "#0F5", "#0FA", "#0FF", "#500",
    "#505", "#50A", "#50F", "#550", "#555", "#55A", "#55F", "#5A0", "#5A5",
    "#5AA", "#5AF", "#5F0", "#5F5", "#5FA", "#5FF", "#A00", "#A05", "#A0A",
    "#A0F", "#A50", "#A55", "#A5A", "#A5F", "#AA0", "#AA5", "#AAA", "#AAF",
    "#AF0", "#AF5", "#AFA", "#AFF", "#F00", "#F05", "#F0A", "#F0F", "#F50",
    "#F55", "#F5A", "#F5F", "#FA0", "#FA5", "#FAA", "#FAF", "#FF0", "#FF5",
    "#FFA", "#FFF"];

var _reverse = {}

/* Custom base-64 encoding/decoding */
var base64 = {
    /* 0 -> 9 A -> Z a -> z - _ */
    toInt:function (str)
    {
        if (typeof(str) != "string" || str.length != 1)
            throw "Invalid input.";
        if (str == "-")
            return 62;
        if (str == "_")
            return 63;
        var code = str.charCodeAt(0);
        if (code >= 48 && code <= 57)
            return code - 48;
        if (code >= 65 && code <= 90)
            return code - 55;
        if (code >= 97 && code <= 122)
            return code - 61;
        throw "Invalid input."
    },
    toStr:function (int)
    {
        if (typeof(int) != "number" || int < 0 || int >= 64)
            throw "Invalid input.";
        int = int >> 0;
        if (int < 10)
            return int.toString();
        if (int < 36)
            return String.fromCharCode(int + 55);
        if (int < 62)
            return String.fromCharCode(int + 61);
        if (int == 62)
            return "-";
        return "_";
    }
}

// Fill color table of the page

for (var i = 0; i < colors.length; i++)
{
    var node = document.createElement("td");
    node.setAttribute("style", "background:" + colors[i]);
    var inner = document.createElement("input");
    inner.setAttribute("type", "radio");
    inner.setAttribute("name", "color");
    inner.setAttribute("id", base64.toStr(i));
    inner.setAttribute("value", base64.toStr(i));
    node.appendChild(inner);
    document.getElementById("colorTable" + ((i / 16)>>0)).appendChild(node);
}

var resetButton = document.getElementById("reset");
resetButton.addEventListener("click", resetView);
var zoomLabel = document.getElementById("zoomLabel");
var zoomSlider = document.getElementById("zoomSlider");
zoomSlider.value = 800;
zoomSlider.addEventListener("change", updateZoom);
var inputX = document.getElementById("inputX");
var inputY = document.getElementById("inputY");
var xOffset = 0;
var yOffset = 0;
var xOrig = 0;
var yOrig = 0;
var source = document.getElementById("data");
source.addEventListener("load", imageLoaded);
var loaded = false;
var canvas = document.getElementById("mainCanvas");
canvas.width = canvas.clientWidth;
canvas.height = (canvas.clientWidth / 16 * 9) >> 0;
canvas.addEventListener("mousedown", mouseDown);
canvas.addEventListener("mouseout", mouseOut);
canvas.addEventListener("keydown", keyDown);
canvas.focus();
var clicking = false;
var moved = false;
var data = "";
var draw = canvas.getContext("2d");
draw.imageSmoothingEnabled = false;
document.getElementById("form").addEventListener("click", updateCookies);
loadCookies();
updateCoordinates();
var ws = new WebSocket("ws://" + window.location.hostname + ":6660/set");

var saveButton = document.getElementById("savetodisk");
saveButton.addEventListener("click", function(){
    var dataUrl = canvas.toDataURL('image/png');
    dataUrl = dataUrl.replace('/^data:image\/[^;]*/', 'data:application/octet-stream');
    dataUrl = dataUrl.replace('/^data:application\/octet-stream/', 'data:application/octet-stream;headers=Content-Disposition%3A%20attachment%3B%20filename=canvas.png');
    this.href = dataUrl;
}, false);

function resetView()
{
    xOffset = 0;
    yOffset = 0;
    inputX.value = 0;
    inputY.value = 0;
    fillCanvas();
    updateCoordinates();
}

function imageLoaded()
{
    _reverse = {}
    var cvs = document.createElement('canvas');
    cvs.width = source.width; cvs.height = source.height;
    var ctx = cvs.getContext("2d");
    ctx.drawImage(source, 0, 0, cvs.width, cvs.height);
    var idt = ctx.getImageData(0, 0, cvs.width, cvs.height).data;
    for (var i = 0, len = idt.length / 4; i < len; i++)
    {
        var r = ((idt[i * 4] / 16) >> 0).toString(16).toUpperCase();
        var g = ((idt[i * 4 + 1] / 16) >> 0).toString(16).toUpperCase();
        var b = ((idt[i * 4 + 2] / 16) >> 0).toString(16).toUpperCase();
        if (_reverse[r + g + b] == undefined)
            _reverse[r + g + b] = base64.toStr(colors.indexOf("#" + r + g + b));
        data = data + _reverse[r + g + b];
    }
    loaded = true;
    fillCanvas();
}

function mouseDown(e)
{
    clicking = true;
    canvas.addEventListener("mousemove", mouseMove);
    canvas.addEventListener("mouseup", mouseUp);
    canImageData = canvas.toDataURL("image/png");
    xOrig = e.clientX;
    yOrig = e.clientY;
    xOffSinceStart = 0;
    yOffSinceStart = 0;
}

function mouseMove(e)
{
    xOffset -= xOrig - e.clientX;
    yOffset -= yOrig - e.clientY;
    xOrig = e.clientX;
    yOrig = e.clientY;
    moved = true;
    fillCanvas();
}

function mouseUp(e)
{
    clicking = false;
    /* We didn't move, change the coordinates of the pixel to set*/
    if (!moved)
    {
        var newX = (((e.clientX - canvas.getBoundingClientRect().left - xOffset) / (zoomSlider.value / 100)) >> 0);
        var newY = (((e.clientY - canvas.getBoundingClientRect().top - yOffset) / (zoomSlider.value / 100)) >> 0);
        if (newX >= 0 && newX < 640 && newY >= 0 & newY < 480)
        {
            inputX.value = newX;
            inputY.value = newY;
            updateCoordinates();
        }
        fillCanvas();
    }
    moved = false;
    canvas.removeEventListener("mousemove", mouseMove);
    canvas.removeEventListener("mouseup", mouseUp);
}

function mouseOut(e)
{
    if (clicking)
    {
        mouseUp(e);
    }
}

function keyDown(e)
{
    if (e.key == "ArrowLeft" && inputX.value > 0)
    {
        inputX.value = parseInt(inputX.value) - 1;
        updateCoordinates();
        fillCanvas();
    }
    else if (e.key == "ArrowUp" && inputY.value > 0)
    {
        inputY.value = parseInt(inputY.value) - 1;
        updateCoordinates();
        fillCanvas();
    }
    else if (e.key == "ArrowRight" && inputX.value < 639)
    {
        inputX.value = parseInt(inputX.value) + 1;
        updateCoordinates();
        fillCanvas();
    }
    else if (e.key == "ArrowDown" && inputY.value < 479)
    {
        inputY.value = parseInt(inputY.value) + 1;
        updateCoordinates();
        fillCanvas();
    }
    else if (e.key == "Enter" || e.key == " ")
    {
        setPixel();
    }
    /* For benchmark use only */
    else if (e.key == "b")
    {
        var t0 = performance.now();

        for (var i = 0; i < 20; i++)
            fillCanvas();

        var t1 = performance.now();
        alert("Time: " + (t1 - t0) / 20 + "ms");
    }
    // */
}

function updateZoom(x)
{
    var oldVal = zoomLabel.innerHTML;
    if (x != false)
    {
        xOffset = (xOffset / oldVal * zoomSlider.value) >> 0;
        yOffset = (yOffset / oldVal * zoomSlider.value) >> 0;
    }
    zoomLabel.innerHTML = zoomSlider.value;
    fillCanvas();
}

function updateCoordinates()
{
    document.getElementById("coords").innerHTML = inputX.value + "x" + inputY.value;
}

function loadCookies()
{
    var cookies = document.cookie.split(';');
    for (var i = 0; i < cookies.length; i++)
    {
        var c = cookies[i];
        while (c.charAt(0) == ' ')
        {
            c = c.substring(1);
        }

        var val = parseInt(c.substring(5, c.length));
        if (val == NaN)
            val = 0;

        if (c.indexOf("offX") == 0)
        {
            xOffset = val;
        }
        else if (c.indexOf("offY") == 0)
        {
            yOffset = val;
        }
        else if (c.indexOf("zoom") == 0)
        {
            if (val == 0)
                val = 800;
            zoomSlider.value = val;
        }
        else if (c.indexOf("selX") == 0)
        {
            inputX.value = val;
        }
        else if (c.indexOf("selY") == 0)
        {
            inputY.value = val;
        }
        else
        {
            var checked = document.querySelector('input[name="color"]:checked');
            if (checked != null)
                checked.removeAttribute("checked");
            var selColor = document.getElementById(c.substring(5, c.length));
            if (selColor != null)
                selColor.setAttribute("checked", "");
        }
    }
    updateZoom(false);
}

function setPixel()
{
    var x = parseInt(inputX.value);
    var y = parseInt(inputY.value);
    var col = document.querySelector('input[name="color"]:checked');
    var color = col == null ? 0 : col.value;
	var toServ = new ArrayBuffer(4);
	var view = new DataView(toServ);
	view.setUint8(0, (x & 4080) >>> 4);
	view.setUint8(1, ((x & 15) << 4) + ((y & 3840) >>> 8));
	view.setUint8(2, y & 255);
	view.setUint8(3, color.charCodeAt(0));
	ws.send(toServ);
}

function updateCookies()
{
    document.cookie = "offX=" + xOffset;
    document.cookie = "offY=" + yOffset;
    document.cookie = "zoom=" + zoomSlider.value;
    document.cookie = "selX=" + inputX.value;
    document.cookie = "selY=" + inputY.value;
    var col = document.querySelector('input[name="color"]:checked');
    document.cookie = "colo=" + (col == null ? 0 : col.value);
}

function fillCanvas()
{
    draw.fillStyle = "#FFF";
    draw.fillRect(0, 0, canvas.width, canvas.height);

    if (!loaded)
    {
        draw.fillStyle = "#000";
        draw.fillText("Loading...", 0, 10);
        return;
    }

    var prev = "0";
    var zoom = zoomSlider.value / 100;
    for (var i = 0, len = data.length; i < len; i++)
    {
        var dataX = i % 640;
        var dataY = (i / 640) >> 0;
        var x = xOffset + dataX * zoom;
        var y = yOffset + dataY * zoom
        if (y >= canvas.height)
            break;
        if (y + zoom < 0)
            i += 640;
        if (x >= canvas.width)
            i += (640 - dataX - 1);
        if (x + zoom >= 0)
        {
            if (data[i] != prev)
            {
                prev = data[i];
                draw.fillStyle = colors[base64.toInt(prev)];
            }
            draw.fillRect(x, y, zoom, zoom);
        }
    }
    var selX = xOffset + inputX.value * zoom;
    var selY = yOffset + inputY.value * zoom;
    var gradient = draw.createLinearGradient(selX, selY, selX + zoom, selY + zoom);
    gradient.addColorStop("0", "#FFF");
    gradient.addColorStop("1", "#000");
    draw.strokeStyle = gradient;
    draw.strokeRect(selX, selY, zoom, zoom);
    updateCookies();
}

ws.onmessage = function(event)
{
    var reader = new FileReader();
    reader.addEventListener("loadend", function() {
        //10110101 01001010 10010101 01001101
        //[    x      ][     y     ] [  c   ]
        var dataview = new DataView(reader.result);
        var x = (dataview.getUint8(0) << 4) + ((dataview.getUint8(1) & 240) >>> 4);
        var y = (dataview.getUint8(1) & 15) * 256 + dataview.getUint8(2);
        var c = dataview.getUint8(3);
		data = data.substr(0, y * 640 + x) + String.fromCharCode(c) + data.substr(y * 640 + x + 1);
		fillCanvas();
    });
    reader.readAsArrayBuffer(event.data);
}

