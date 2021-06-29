$(function() {

    let id = $("#g_assessmentid").val();
    if(id != undefined) {
        setupTimer(id);
    }
    
    function setupTimer(assessmentId){
        $.ajax({
            type: "GET",
            url: '/api/assessments/times/' + assessmentId,
            contentType: "application/json",
            success: function(e) {
                if(e === undefined)
                {
                    return false; //not setup
                }
                $("#timer_wrapper").show();
                
                $("#timer_elapsed").val(e.elapsedTime);
                $("#timer_status").val(e.status);

                setStatus(e.status)
            
                let isSet = false;
                let hub = new signalR.HubConnectionBuilder()
                    .withUrl("/hubs/time?assessmentid=" + assessmentId)
                    .configureLogging(signalR.LogLevel.Information)
                    .build();

                hub.on("Time",
                    function (s, e) {
                        if ($("#timer_status").val() !== s) {
                            setStatus(s)
                        }
                        $("#timer_status").val(s);
                        $("#timer_elapsed").val(e);
                    });

                $("#timer_status").change(function() {
                    hub.invoke('Time', null, -1);
                });

                setInterval(countTimer, 1000);

                let totalSeconds = $("#timer_elapsed").val();

                hub.logging = true;
                hub.start().catch(err => console.error(err.toString()));

                function countTimer() {
                    ++totalSeconds;
                    let hour = Math.round(Math.floor(totalSeconds / 3600));
                    let minute = Math.round(Math.floor((totalSeconds - hour * 3600) / 60));
                    let seconds = Math.round(totalSeconds - (hour * 3600 + minute * 60));
                    if(seconds === 1) {seconds = 0;}

                    if ($("#timer_status").val() === "Active" || !isSet) {
                        $("#timer_output").html(pad(hour) + ":" + pad(minute) + ":" + pad(seconds));
                        isSet = true;
                    }
                }
                function pad(val) { return val > 9 ? val : "0" + val; }
                
                function setStatus(status){
                    $("#timer_output_status").removeClass("fa-hourglass-hal");
                    $("#timer_output_status").removeClass("fa-hourglass-end");
                    $("#timer_output_status").removeClass("timer-success");
                    $("#timer_output_status").removeClass("timer-warning");
                    $("#timer_output_status").removeClass("timer-danger");

                    if (status === "Active") {
                        $("#timer_output_status").addClass("fa-hourglass-half");
                        $("#timer_output_status").addClass("timer-success");
                        $("#timer_output_def").html("Elapsed")
                    }
                    else if (status === "NotStarted") {
                        $("#timer_output_def").html("Not Started")
                        $("#timer_output_status").addClass("timer-warning");
                    }
                    else if (status === "Paused") {
                        $("#timer_output_def").html("PAUSEX")
                        $("#timer_output_status").addClass("timer-warning");
                    }
                    else if (status === "Ended") {
                        $("#timer_output_status").addClass("fa-hourglass-end");
                        $("#timer_output_status").addClass("timer-danger");
                        $("#timer_output_def").html("ENDEX")
                    }
                }
            },
            error: function() {
                console.log("ERR: Timer could not be setup");
            }
        });
    }
});