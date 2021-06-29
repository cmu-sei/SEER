$(function () {
    var uid = uuidv4();

    var quizId = $("#quizid").val();
    $.ajaxSetup({ cache: false });

    const hub = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/quizzes?quizid=" + quizId)
        .configureLogging(signalR.LogLevel.Information	)
        .build();

    hub.on("CurrentStateChanged", function (s) {
        if (s != "InProgress") {
            window.location.replace("../inactive?previous=" + $("#currentstate").val() + "&n=" + s);
        }
    });

    $("#currentstate").change(function () {
        console.log("CurrentStateChanged up: " + $("#currentstate").val());
        hub.invoke("CurrentStateChanged", $("#currentstate").val());
    });

    $(".option").on("change", null, function () {
        var o = Split(this.id);

        if (this != null && this.type === "text") {
            //control is textbox
            console.log("Called hub:", uid, null, quizId, o[1], null, $(this).val());
            hub.invoke("AnswerChanged", uid, null, quizId, o[1], 0, $(this).val()).catch(err => console.error(err.toString()));
        }
        else {
            console.log("Called hub 2:", uid, null, quizId, o[1], o[2], null);
            hub.invoke("AnswerChanged", uid, null, quizId, o[1], o[2], null).catch(err => console.error(err.toString()));
        }
    });

    hub.on("AnswerChanged", function (userId, who, quizId, questionId, optionId, answer) {
        $(".slot" + questionId).empty();
        if (userId != uid) {
            Slot(questionId, optionId.toString(), who);
            if (answer != null) {
                document.getElementById("text-" + questionId).value = answer;
            }
            else {
                RadionButtonSelectedValueSet(questionId.toString(), optionId.toString());
            }
        }
    });

    $(".hint").on("click", null, function () {
        var id = this.id.replace("hint-show-", "");
        hub.invoke("Hint", quizId, id, "");
    });

    hub.on("Hint", function (quizId, questionId, hint) {
        var t = $("#hint-" + questionId);
        t.show();
        t.text(hint);
    });

    $("#btnClose").on("click", null, function () {
        if (confirm("Are you sure you want to submit and close this quiz for your entire group?")) {
            contextSeer.invoke("Close", quizId, uid);
        }
    });

    hub.on("Close", function (quizId) {
        window.location = '/home/assessment';
    });

    hub.start().catch(err => console.error(err.toString()));

    window.onbeforeunload = function () {
        try {
            $.connection.Seer.stop();
        }
        catch (e) {
            console.log(e);
        }
    }
});

function Split(o) {
    return o.toString().split("-");
}

function RadionButtonSelectedValueSet(questionId, optionId) {
    document.getElementById("option-" + questionId + "-" + optionId).checked = true;
}

function Slot(questionId, optionId, t) {
    var o = document.getElementById("slot-" + questionId + "-" + optionId);
    o.innerHTML = t;
    o.className += " alert alert-success";
    window.setTimeout(function () {
        o.classList.remove("alert");
        o.classList.remove("alert-success");
    }, 500);
}

/// called by mouse.js
function uuidv4() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}