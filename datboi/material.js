document.getElementById("shadowLayer").onclick = hideMenu;
document.getElementById("menuIcon").onclick = showMenu;
window.onresize = resized;

function resized()
{
    if (window.innerWidth >= 1600)
    {
        document.getElementById("menu").style.display = "block";
        document.getElementById("shadowLayer").style.display = "none";
    }
    else
    {
        document.getElementById("menu").style.display = "none";
    }
}

function showMenu()
{
    document.getElementById("menu").style.display = "block";
    document.getElementById("shadowLayer").style.display = "block";
}

function hideMenu()
{
    document.getElementById("menu").style.display = "none";
    document.getElementById("shadowLayer").style.display = "none";
}