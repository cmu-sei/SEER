/// <reference path="../../scripts/jquery-1.8.2.js" />
/// <reference path="../../scripts/jquery.signalR.js" />

$(function () {
    var uid = uuidv4();

    var quizId = $("#quizid").val();

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/mouse?quizid=" + quizId)
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on("move", (e, x, y, u) => {
        updateCursor(e, x, y, u);
    });

    document.addEventListener("mousemove", event => {
        updateCursor(connection.id, event.pageX, event.pageY, uid);
        connection.invoke("Move", event.pageX, event.pageY, uid).catch(err => console.error(err.toString()));
        event.preventDefault();

    });

    connection.start().catch(err => console.error(err.toString()));
    
    function updateCursor(id, x, y, u) {
        if (u != uid) {
            var e = document.getElementById(id);
            if (!e) {
                e = $('<div class="mouse" id="' + id + '"><i class="fa fa-user-o" aria-hidden="true"></i> ' + id + '</div>').appendTo(document.body);
                e.css('position', 'absolute');
            }
            else {
                e = $(e);
            }
            e.css({ left: x , top: y });
        }
    }
});
