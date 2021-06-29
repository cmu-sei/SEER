$(function () {

    $.ajaxSetup({ cache: false });
    var conn = $.hubConnection();

    var contextHub = conn.createHubProxy("Administration");

    conn.start().done(function () {
        go();
        setInterval(go, 5000);
    })
    .fail(function () { console.log('Could not connect'); });;

    contextSeer.on("QuizStats", function (o) {
        var s = "";
        console.log(o);
        var arrayLength = o.length;
        console.log(arrayLength);
        for (var i = 0; i < arrayLength; i++) {
            s += o[i] += "<br />";
        }

        if (o.length > 0) {
            o.forEach(function (n) {
                s += n += "<br />";
            });
            $("#current").html(s);
        }
        else {
            $("#current").html("There are no active evaluations");
        }
    });

    function go() {
        contextSeer.invoke('QuizStats', null);
    }
});