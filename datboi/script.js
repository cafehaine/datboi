var colors = ["000", "019", "091", "099", "A11", "A29", "992", "BBB",
              "777", "02F", "0F3", "0FF", "F31", "F4F", "FF3", "FFF"];

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
var canvas = document.getElementById("mainCanvas");
canvas.addEventListener("mousedown", mouseDown);
canvas.addEventListener("mouseout", mouseOut);
canvas.addEventListener("keydown", keyDown);
canvas.focus();
var clicking = false;
var moved = false;
var data = canvas.innerHTML;
canvas.innerHTML = "";
var draw = canvas.getContext("2d");
draw.imageSmoothingEnabled = false;
document.getElementById("form").addEventListener("click", updateCookies);
document.getElementById("submit").addEventListener("click", setPixel);
loadCookies();
updateCoordinates();

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
}

function mouseDown(e)
{
    clicking = true;
    canvas.addEventListener("mousemove", mouseMove);
    canvas.addEventListener("mouseup", mouseUp);
    xOrig = e.clientX;
    yOrig = e.clientY;
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
        inputX.value = (((e.clientX - canvas.getBoundingClientRect().left - xOffset) / (zoomSlider.value / 100)) >> 0);
        inputY.value = (((e.clientY - canvas.getBoundingClientRect().top - yOffset) / (zoomSlider.value / 100)) >> 0);
        fillCanvas();
        updateCoordinates();
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
    var xmlhttp = new XMLHttpRequest();
    var x = parseInt(inputX.value);
    var y = parseInt(inputY.value);
    var col = document.querySelector('input[name="color"]:checked');
    var color = col == null ? 0 : col.value;
    xmlhttp.onreadystatechange = function()
    {
        if (xmlhttp.readyState == 4)
            setPixelResponseHandler(xmlhttp.responseText, x, y, color);
    }
    var url = "/pixel";
    var params = "x=" + x + "&y=" + y + "&color=" + color;
    xmlhttp.open("POST", url, true);
    xmlhttp.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
    xmlhttp.send(params);
}

function setPixelResponseHandler(serv, x, y, color)
{
    if (serv == "ok")
        data = data.substr(0, y * 640 + x) + color + data.substr(y * 640 + x + 1);
    else
        alert(serv);
    fillCanvas();
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
    draw.clearRect(0, 0, canvas.width, canvas.height);
    var zoom = zoomSlider.value / 100;
    for (var i = 0, len = data.length; i < len; i++)
    {
        var x = xOffset + (i % 640) * zoom;
        var y = yOffset + ((i / 640) >> 0) * zoom
        if (x < 640 && x + zoom >= 0 && y < 480 && y + zoom >= 0)
        {
            draw.fillStyle = "#" + colors[parseInt(data[i], 16)];
            if (inputX.value == i % 640 && inputY.value == ((i / 640) >> 0))
                draw.fillRect(x + 1, y + 1, zoom - 2, zoom - 2);
            else
                draw.fillRect(x, y, zoom, zoom);
        }
    }
    updateCookies();
}