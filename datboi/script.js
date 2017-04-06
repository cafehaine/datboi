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
/*loadCookies();*/
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

function updateZoom()
{
    zoomLabel.innerHTML = zoomSlider.value;
    draw.width = 640 * zoomSlider.value / 100 + "px";
    draw.height = 480 * zoomSlider.value / 100 + "px";
    fillCanvas();
    updateCookies();
}

function loadCookies()
{
    console.log("Loading cookies: " + document.cookie);
    var cookies = document.cookie.split(';');
    for (var i = 0; i < cookies.length; i++)
    {
        var c = cookies[i];
        while (c.charAt(0) == ' ')
        {
            c = c.substring(1);
        }

        if (c.indexOf("offX") == 0)
        {
            xOffset = c.substring(4, c.length);
        }
        else if (c.indexOf("offY") == 0)
        {
            yOffset = c.substring(4, c.length);
        }
        else if (c.indexOf("zoom") == 0)
        {
            zoomSlider.value = c.substring(4, c.length);
        }
        else if (c.indexOf("selX") == 0)
        {
            inputX.value = c.substring(4, c.length);
        }
        else if (c.indexOf("selY") == 0)
        {
            inputY.value = c.substring(4, c.length);
        }
    }
    if (xOffset == NaN)
        xOffset = 0;
    if (yOffset == NaN)
        yOffset = 0;
    if (zoomSlider.value == NaN)
        zoomSlider.value = 800;
    updateZoom();
}

function updateCookies()
{
    /*
    document.cookie = "offX=" + xOffset + "; offY=" + yOffset + "; zoom=" + zoomSlider.value + "; selX=" + inputX.value + "; selY=" + inputY.value;
    */
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
}