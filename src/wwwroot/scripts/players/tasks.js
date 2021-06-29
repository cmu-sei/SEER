$(function () {
    var groupId = $("#groupid").val();

    const hub = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/tasks?groupid=" + groupId)
        .configureLogging(signalR.LogLevel.Information)
        .build();

    $(".option").on("change", null, function (e) {
        var o = Split(e.target.id);
        o = o[1];
        console.log("hub invoked: " + o, $("#chk-" + o).prop('checked') + " " + $("#txt-" + o).val());
        hub.invoke("AnswerChanged", o, $("#chk-" + o).prop('checked'), $("#txt-" + o).val());
    });

    hub.on("AnswerChanged", function (id, comment, status) {
        console.log("hub returned: " + id, comment + " " + status);
    });

    hub.start().catch(err => console.error(err.toString()));
});

function Split(o) {
    return o.toString().split("-");
}

function ClearCss(o) {
    $(o).removeClass("text-success");
    $(o).removeClass("text-warning");
    $(o).removeClass("text-danger");

    $(o).removeClass("fa-circle-thin");
    $(o).removeClass("fa-check-circle");
    $(o).removeClass("fa-minus-circle");
    $(o).removeClass("fa-times-circle");
}