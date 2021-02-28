$(document).ready(function () {
    // to get new stadium working hours

    var playgroundtimesList = new Array();
    /// get the current path to know if I;m working in create or edit page

    var path = window.location.pathname;
    var names = path.split('/');

    /// if the user press to add more times to the stadium

    $("#addDifferentPeriod").click(function () {
        if (($("#from").val() == "") || ($("#to").val() == ""))
            return;

        document.getElementById("timesRecord").style.display = "block";
        $("#header").css("display", "none");
        $("#table2").css("margin-top", "15px");

        var hoursfrom = $("#from").val().split(":");
        var hoursto = $("#to").val().split(":");

        var timesrecords = "<tr><td>" + "<input disabled type='text' value=" + $("#from").val() /*new Date($("#from").val()).toLocaleTimeString()*/ + "></td><td>" + "<td><input disabled type='text' value=" + $("#to").val() /*new Date($("#to").val()).toLocaleTimeString()*/ + "></td><td></td></tr>"
        $("#timesRecord").last().append(timesrecords);

        // Create object  similar to playgroundObject add times to it
        /// get the current time(hours & minutes) and convert it to datetime
        var From = new Date();
        var To = new Date();
        From.setHours(parseInt(hoursfrom[0]), parseInt(hoursfrom[1]));
        To.setHours(parseInt(hoursto[0]), parseInt(hoursto[1]));

        var PlaygroundTimes = {};
        PlaygroundTimes.From = From;
        PlaygroundTimes.To = To;
        playgroundtimesList.push(PlaygroundTimes);

        document.getElementById("from").value = "";
        document.getElementById("to").value = " ";
    });

    /// user prss create or update button to save playground info

    $("#AddPlayground").click(function () {
        /// get playground times from -- to
        /// create object similar to playground times and contains from to
        /// add the object the list that contains all playgroundtimes object to this playground

        /*$("#timesRecord").find("tr:gt(0)").each(function () {
            var From = $(this).find("td:eq(0)").text();
            var To = $(this).find("td:eq(1)").text();
            var PlaygroundTimes = {};
            PlaygroundTimes.From = From;
            PlaygroundTimes.To = To;
            playgroundtimesList.push(PlaygroundTimes);
        });*/

        // get the data of the current playground Object

        var Playground = {};
        Playground.Name = $("#Name").val();
        Playground.City = $("#City").val();
        Playground.StadiumArea = $("#StadiumArea").val();
        Playground.AmPrice = $("#AmPrice").val();
        Playground.PmPrice = $("#PmPrice").val();
        Playground.Services = $('input:checkbox:checked.services').map(function () {
            return this.value;
        }).get().join(",") ?? "0";
        Playground.PlaygroundStatus = $("#PlaygroundStatus").val();
        Playground.IsOffered = $("#IsOffered").val();
        Playground.CreatedOn = $("#CreatedOn").val();

        /// if current working page is create

        if (names[names.length - 1] == "Create") {
            // get the data of playground image
            var fileInput = document.getElementById('ImageFile');

            var reader = new FileReader();
            reader.readAsDataURL(fileInput.files[0]);

            ///
            reader.onload = function () {
                console.log(reader.result);//base64encoded string

                /// create ajax request to get playground and playgroundtimes data
                /// send to controller and update the database

                $.ajax({
                    type: 'POST',
                    dataType: 'text',

                    url: "https://localhost:44316/playgrounds/Create",
                    data: {
                        playgroundtimesinfo: JSON.stringify(playgroundtimesList),
                        Playground: JSON.stringify(Playground),
                        image: reader.result
                    },
                    success: function (data) {
                        location.href = 'index';
                    },
                    error: function () {
                        alert("something wrong Happens");
                    }
                });
            };
            reader.onerror = function (error) {
                console.log('Error: ', error);
            };
        }

        /// if the current working page is edit

        else if (names[names.length - 2] == "Edit") {
            Playground.PlaygroundId = $("#PlaygroundId").val();
            Playground.ImagePath = $("#ImagePath").val();
            $.ajax({
                type: 'POST',
                dataType: 'text',

                url: "https://localhost:44316/playgrounds/Edit",
                data: {
                    playgroundtimesinfo: JSON.stringify(playgroundtimesList),
                    Playground: JSON.stringify(Playground)
                },
                success: function (data) {
                    location.href = '../index';
                },
                error: function () {
                    alert("something wrong Happens");
                }
            });
        }
    });
});

function deleteRecorde(obj, timesID) {
    console.log(timesID);
    $.ajax({
        type: 'POST',
        dataType: 'text',

        url: "https://localhost:44316/playgrounds/DeletePlaygroundTimes",
        data: {
            pid: timesID
        },
        success: function (data) {
            obj.parentNode.parentNode.remove();
            console.log("Is it done");
        },
        error: function () {
            alert("something wrong Happens");
        }
    });
}

function updateRecorde(obj, timesID) {
    var hoursfrom = obj.parentNode.parentNode.children[0].children[0].value.split(":");
    var hoursto = obj.parentNode.parentNode.children[1].children[0].value.split(":");

    var From = new Date();
    var To = new Date();
    From.setHours(parseInt(hoursfrom[0]), parseInt(hoursfrom[1]));
    To.setHours(parseInt(hoursto[0]), parseInt(hoursto[1]));

    var PlaygroundTimes = {};
    PlaygroundTimes.From = From;
    PlaygroundTimes.To = To;
    PlaygroundTimes.PlaygroundTimesId = timesID;
    $.ajax({
        type: 'POST',
        dataType: 'text',
        url: "https://localhost:44316/playgrounds/UpdatePlayGroundTimes",
        data: {
            playgroundtimesinfo: JSON.stringify(PlaygroundTimes)
        },
        success: function (data) {
            console.log(data);
        },
        error: function () {
            alert("something wrong Happens");
        }
    });
}