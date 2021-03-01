$(document).ready(function () {
    // to store  stadium working hours
    var playgroundtimesList = new Array();

    /// get the current path to know if I'm working in create or edit page
    var path = window.location.pathname;
    var names = path.split('/');

    /// if the user press to add more times to the stadium
    $("#addDifferentPeriod").click(function () {

        /// check if there is no value do nothing 
        if (!($("#from").val()) || !($("#to").val()))
            return;
        /// else do the following 
        document.getElementById("timesRecord").style.display = "block";
        $("#header").css("display", "none");
        $("#table2").css("margin-top", "15px");

        var hoursfrom = $("#from").val().split(":");   // to ge hours and minutes seperately 
        var hoursto = $("#to").val().split(":");


        // add this record to table in the view 
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


        // reset these values
        document.getElementById("from").value = "";
        document.getElementById("to").value = " ";
    });

    /// user prss create or update button to save playground info

    $("#AddPlayground").click(function () {

        
        // get attributes values of the current playground

        var Playground = {};
        Playground.Name = $("#Name").val();
        Playground.City = $("#City").val();
        Playground.StadiumArea = $("#StadiumArea").val();
        Playground.AmPrice = $("#AmPrice").val();
        Playground.PmPrice = $("#PmPrice").val();

        // get services if owner doesn't check anything, just set it to 0
        Playground.Services = $('input:checkbox:checked.services').map(function () {
            return this.value;
        }).get().join(",") ?? "0";

        Playground.PlaygroundStatus = $("#PlaygroundStatus").val();
        Playground.IsOffered = $("#IsOffered").val();
        Playground.CreatedOn = $("#CreatedOn").val();

        /// if current working page is create
        if (names[names.length - 1] == "Create") {

            Playground.OverView = $("#OverView").val()??" ";
            // get the data of playground image
            var fileInput = document.getElementById('ImageFile');

            var reader = new FileReader();
            reader.readAsDataURL(fileInput.files[0]);

            ///
            reader.onload = function () {
         
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
                        location.href = 'Create';
                    }
                });
            };
            reader.onerror = function (error) {
                location.href = 'Create';
            };
        }

        /// if the current working page is edit
        
        else if(names[names.length-1] == "Edit") {
            
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
                    location.href = 'index';
                },
                error: function () {
                    location.href = 'Edit';
                }
            });

        }
    });
});


/// to delete the arget record
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
   
    ///get the hours and mintues (time)  and convert them to the current date
    var hoursfrom = obj.parentNode.parentNode.children[0].children[0].value.split(":");
    var hoursto = obj.parentNode.parentNode.children[1].children[0].value.split(":");

    var From = new Date();
    var To = new Date();
    
    From.setHours(parseInt(hoursfrom[0]), parseInt(hoursfrom[1]));
    To.setHours(parseInt(hoursto[0]), parseInt(hoursto[1]));

    // create object to store the updated attributes
    
    var PlaygroundTimes = {};
    PlaygroundTimes.From = From;
    PlaygroundTimes.To = To;
    PlaygroundTimes.PlaygroundTimesId = timesID;
    $.ajax({
        type:'POST',
        dataType: 'text',
        url: "https://localhost:44316/playgrounds/UpdatePlayGroundTimes",
        data: {
            playgroundtimesinfo: JSON.stringify(PlaygroundTimes)
        },
        success: function (data) {
            console(data);
        },
        error: function () {
            alert("something wrong Happens");
        }
    });
}