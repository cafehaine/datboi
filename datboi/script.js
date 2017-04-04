var canvas = document.getElementById("mainCanvas");
var data = canvas.innerHTML;
var draw = canvas.getContext("2d");

function colorFromByte(byte)
{
    var r = (byte & 0x08) != 0 ? 15 : 0;
    var g = (byte & 0x04) != 0 ? 15 : 0;
    var b = (byte & 0x02) != 0 ? 15 : 0;
    if ((byte & 0x01) != 0)
    {
        r = Math.min(15, r + 7);
        g = Math.min(15, g + 7);
        b = Math.min(15, b + 7);
    }
    return "#" + r.toString(16) + g.toString(16) + b.toString(16);
}

for (var i = 0, len = data.length; i < len; i++)
{
    draw.fillStyle = colorFromByte(parseInt(data[i],16));
    draw.fillRect(i % 640, (i/640)>>0, 1, 1);
}