var colors = ["000", "019", "091", "099", "A11", "A29", "992", "BBB",
              "777", "02F", "0F3", "0FF", "F31", "F4F", "FF3", "FFF"];

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
var clicking = false;
var moved = false;
var data = canvas.innerHTML;
var draw = canvas.getContext("2d");
draw.imageSmoothingEnabled = false;
fillCanvas();

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
        document.getElementById("submit").removeAttribute("disabled");
    }
    if (xOffset > 0 || yOffset > 0)
    {
        xOffset = Math.min(xOffset, 0);
        yOffset = Math.min(yOffset, 0);
        fillCanvas();
    }
    else if (xOffset < - (zoomLabel.value / 100 - 1) * 640)
    {
        console.log(xOffset, yOffset);
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

function updateZoom()
{
    xOffset = 0;
    yOffset = 0;
    zoomLabel.innerHTML = zoomSlider.value;
    draw.width = 640 * zoomSlider.value / 100 + "px";
    draw.height = 480 * zoomSlider.value / 100 + "px";
    fillCanvas();
}

function colorFromByte(byte)
{
    return "#" + colors[byte];
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
            draw.fillStyle = colorFromByte(parseInt(data[i], 16));
            draw.fillRect(x, y, zoom, zoom);
        }
    }
}