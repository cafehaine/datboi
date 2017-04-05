var colors = ["000", "019", "091", "099", "A11", "A29", "992", "BBB",
              "777", "02F", "0F3", "0FF", "F31", "F4F", "FF3", "FFF"];

var zoomLabel = document.getElementById("zoomLabel");
var zoomSlider = document.getElementById("zoomSlider");
zoomSlider.value = 100;
zoomSlider.addEventListener("change", updateZoom);
var canvas = document.getElementById("mainCanvas");
var data = canvas.innerHTML;
var draw = canvas.getContext("2d");
fillCanvas();

function updateZoom()
{
    zoomLabel.innerHTML = zoomSlider.value;
    canvas.style.width = 640 * zoomSlider.value / 100 + "px";
    canvas.style.height = 480 * zoomSlider.value / 100 + "px";
    fillCanvas();
}

function colorFromByte(byte)
{
    return "#" + colors[byte];
}

function fillCanvas()
{
    var zoom = zoomSlider.value / 100;
    for (var i = 0, len = data.length; i < len; i++)
    {
        draw.fillStyle = colorFromByte(parseInt(data[i], 16));
        draw.fillRect((i % 640) * zoom, ((i / 640) >> 0) * zoom, zoom, zoom);
    }
}