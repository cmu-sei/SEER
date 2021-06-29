$(function () {
    let groupId = $("#groupid").val();
    let metId = $("#metid").val();
    let assessmentId = $("#assessmentid").val();

    const hub = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/mets?groupid=" + groupId + "&metid=" + metId + "&assessmentid=" + assessmentId)
        .configureLogging(signalR.LogLevel.Information)
        .build();

    $(".option").on("change", null, function () {
        var o = Split(this.id);
        o = o[1];
        hub.invoke("AnswerChanged", Number(o), $("#comments-" + o).val(), $("#status-" + o).val());
    });

    hub.on("AnswerChanged", function (id, comment, status) {
        console.log("hub returned: " + id, comment + " " + status);
        Score();
    });

    hub.start().catch(err => console.error(err.toString()));

    Score();
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

function Score() {
    var notstarted = 0, go = 0, nogo = 0, partial = 0;
    $(".sct").each(function () {
        var i = $(this).data("id");
        var s = $("#status-" + i).val();
        var v = $("#visibility-" + i).val() === "Active";
        ClearCss(this);
        if (s === "1") {
            $(this).addClass("text-success");
            $(this).addClass("fa-check-circle");
            if (v) {
                go++;
            }
        } else if (s === "2") {
            $(this).addClass("text-danger");
            $(this).addClass("fa-times-circle");
            if (v) {
                nogo++;
            }
        } else if (s === "3") {
            $(this).addClass("text-warning");
            $(this).addClass("fa-minus-circle");
            if (v) {
                partial++;
            }
        } else {
            $(this).addClass("fa-circle-thin");
            if (v) {
                notstarted++;
            }
        }
    });
    $(".met").each(function () {
        var i = $(this).data("id");
        var status = 1;
        $(".met-" + i).each(function () {
            var s = $("#status-" + $(this).data("id")).val();
            if (s === "1") {
            } else if (s === "2") {
                status = 2;
                return false;
            } else if (s === "3") {
                status = 3;
            } else {
                status = 0;
                console.log($(this).data("id"), $("#status-" + $(this).data("id")).val());
                return false;
            }
        });
        ClearCss(this);
        if (status === 1) {
            $(this).addClass("text-success");
            $(this).addClass("fa-check-circle");
        } else if (status === 2) {
            $(this).addClass("text-danger");
            $(this).addClass("fa-times-circle");
        } else if (status === 3) {
            $(this).addClass("text-warning");
            $(this).addClass("fa-minus-circle");
        } else {
            $(this).addClass("fa-circle-thin");
        }
    });
    $("#complete").text(((go / (notstarted+go+nogo+partial)) * 100).toFixed(0));


    var ctx = document.getElementById("myChart").getContext('2d');
    var myChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ["GO", "PARTIAL", "NO GO", "NOT STARTED"],
            datasets: [{
                backgroundColor: ["#2ecc71", "#f1c40f", "#e74c3c", "#c0c0c0"],
                data: [go, partial, nogo, notstarted]
            }]
        },
        options: {
            legend: {
                display: false
            }
        }
    });
}

