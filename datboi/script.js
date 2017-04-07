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
var clicking = false;
var moved = false;
var data = canvas.innerHTML;
var draw = canvas.getContext("2d");
draw.imageSmoothingEnabled = false;
document.getElementById("form").addEventListener("click", updateCookies);
loadCookies();

function resetView()
{
    xOffset = 0;
    yOffset = 0;
    inputX.value = 0;
    inputY.value = 0
    zoomSlider.value = 800;
    updateZoom();
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
    updateZoom();
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