var bookingId = 0;
function checkBooking() {
    if (checkDateIsValid()) {
        getPlaygroundTimes();
    } else {
        document.getElementById("bookBtn").value = "Book";
    }
}

function onChangeSelectedPeriod(id) {
    if (checkDateIsValid()) {
        $.ajax({
            url: 'https://localhost:44379/Booking/GetBookings',
            type: "get", //send it through get method
            data: {
                playGroundId: document.getElementById("playground").value,
                date: document.getElementById("selDate").value,
                timeId: id
            },
            success: function (response) {
                console.log(response);
                if (response.bookingExist == true) {
                    document.getElementById("bookBtn").value = "Join Team";
                    removeMaxNumOfPlayers();
                    if (response.numOfPlayers < response.maxNumOfPlayers) { //bookingStatus ==1 means it's available
                        changeBtnandMessageState(false, "This period has beed booked if you'd like to join this team click Join team =D");

                        bookingId = response.bookingId;
                        // console.log(bookingId);
                    } else {
                        changeBtnandMessageState(true, "This period has beed booked and no available place :(");
                    }
                } else if (response.bookingId != 0) { // != 0 in this case user has booked before
                    bookingId = response.bookingId;
                    if (response.bookingOwner == false) {
                        removeMaxNumOfPlayers()
                        document.getElementById("bookBtn").value = "Leave Team";
                    } else {
                        removeMaxNumOfPlayers();
                        if (response.paid == 0) {
                            document.getElementById("bookBtn").value = "Cancel Booking";
                        } else {
                            var btn = document.getElementById("bookBtn");
                            btn.value = "Paid";
                            btn.disabled = true;
                        }

                        //console.log("Cancel My Booking Please");
                    }
                } else if (response.playgroundStatus == 1) {
                    changeBtnandMessageState(true, "We're in maintenance please be patient");
                } else {
                    document.getElementById("bookBtn").value = "Book";
                    addMaxNumOfplayersandChecking();
                }
            }
        });
    }
}

function addMaxNumOfplayersandChecking() {
    if (document.getElementsByName("maxNum").length == 0) {
        $("<label name='numOf' style='margin-top:10%;color:black;padding: 0' class='control - label'>num of players can join</label><input type='number' value='0' name='maxNum' min='0' max='9' style='width:50%;'  />").insertAfter("#message");

        document.getElementsByName("maxNum")[0].addEventListener("keypress", function (e) {
            e.preventDefault();
        });
    }
}

function removeMaxNumOfPlayers() {
    var inputNumber = document.getElementsByName("maxNum");
    var label = document.getElementsByName("numOf");
    if (inputNumber.length != 0 && label.length != 0) {
        inputNumber[0].remove();
        label[0].remove();
    }
}

function submit() {
    //createCookieForDateAndPeriod();
    $.ajax({
        type: "get",
        url: "https://localhost:44379/Booking/GetCurrentUser",

        success: function (response) {
            if (response == null) {
                window.location.href = "/Account/login?returnUrl=Booking/Index/" + document.getElementById("playground").value;
            } else {
                checkSubmition();
            }
        },
        error: function (req, status, error) {
            //console.log(msg);
        }
    });
}

function checkSubmition() {
    if (document.getElementById("bookBtn").value == "Book") {
        CreateBooking();
    } else if (document.getElementById("bookBtn").value == "Join Team") {
        joinTeam();
    } else if (document.getElementById("bookBtn").value == "Cancel Booking") {
        CancelBooking();
    } else if (document.getElementById("bookBtn").value == "Leave Team") {
        LeaveTeam();
    }
}

function CreateBooking() {
    $.ajax({
        type: "get",
        url: "https://localhost:44379/Booking/CreateBooking",
        data:
        {
            period: document.getElementById("periods").value,
            playgroundId: document.getElementById("playground").value,
            selectedDate: document.getElementById("selDate").value,
            numOfPlayers: document.getElementsByName("maxNum")[0].value
        },
        success: function (response) {
            if (response.error) {
                changeBtnandMessageState(true, response.error);
            } else {
                window.location.href = response.redirectToUrl;
            }
        },
        error: function (req, status, error) {
            //console.log(msg);
        }
    });
}

function joinTeam() {
    $.ajax({
        type: "get",
        url: "https://localhost:44379/Booking/JoinTeam",
        data:
        {
            bookingId: bookingId
        },
        success: function (response) {
            //console.log(response);
            if (response.error) {
                changeBtnandMessageState(true, response.error);
            } else {
                window.location.href = response.redirectToUrl;
            }
        },
        error: function (req, status, error) {
            //console.log(msg);
        }
    });
}

function CancelBooking() {
    $.ajax({
        type: "get",
        url: "https://localhost:44379/Booking/CancelBooking",
        data:
        {
            bookingId: bookingId
        },
        success: function (response) {
            //console.log(response);
            window.location.href = response.redirectToUrl;
        },
        error: function (req, status, error) {
            //console.log(msg);
        }
    });
}

function LeaveTeam() {
    $.ajax({
        type: "get",
        url: "https://localhost:44379/Booking/LeaveTeam",
        data:
        {
            bookingId: bookingId
        },
        success: function (response) {
            //console.log(response);
            window.location.href = response.redirectToUrl;
        },
        error: function (req, status, error) {
            //console.log(msg);
        }
    });
}

function checkDateIsValid() {
    //var myPeriods = document.getElementById("periods");
    var todayDate = new Date().getDate();
    var todayMonth = new Date().getMonth();
    var todayYear = new Date().getFullYear();

    var selectedDay = new Date(document.getElementById("selDate").value).getDate();
    var selectedMonth = new Date(document.getElementById("selDate").value).getMonth();
    var selectedYear = new Date(document.getElementById("selDate").value).getFullYear();

    if (selectedDay >= todayYear && selectedMonth >= todayMonth && selectedDay >= todayDate) {
        changeBtnandMessageState(true, "Please select a valid date");
        return false;
    }

    changeBtnandMessageState(false, "");

    return true;
}

function checkValidPeriod(time) {
    var todayDate = new Date().getDate();
    var selectedDate = new Date(document.getElementById("selDate").value).getDate();

    if (selectedDate == todayDate) {
        var curHour = new Date().getHours() % 12;
        curHour = curHour ? curHour : 12; // the hour '0' should be '12'
        var curState = new Date().getHours() >= 12 ? 'PM' : 'AM';
        var state = time.slice(-2);
        var optionHour = parseInt(time.substring(0, 2));
        if (curState == "PM" && state == "AM") {
            return false;
        } else if (curState == "PM" && state == "PM" && curHour != 12 && optionHour <= curHour) {
            return false;
        } else if (curState == "PM" && state == "PM" && optionHour == 12) {
            return false;
        }
        return true;
    }
    return true;
}

function getPlaygroundTimes() {
    $.ajax({
        type: "get",
        url: "https://localhost:44379/Booking/GetTimes",
        data:
        {
            id: parseInt(document.getElementById("playground").value)
        },
        success: function (response) {
            //console.log(response);
            resetAllOptions();
            var selectList = document.getElementById("periods");
            for (let i = 0; i < response.length; i++) {
                if (checkValidPeriod(response[i].time)) {
                    var option = document.createElement("option");
                    option.text = response[i].time;
                    option.value = response[i].id;
                    option.setAttribute("name", "options");
                    selectList.add(option);
                }
            }
            if (document.getElementsByName("options").length == 0) {
                changeBtnandMessageState(true, "No available booking on this day we are sorry");
            } else {
                var selecled = selectList.value;
                onChangeSelectedPeriod(selecled);
            }
        },
        error: function (req, status, error) {
            //console.log(msg);
        }
    });
}

function getSelectedOption(sel) {
    var opt;
    for (var i = 0, len = sel.options.length; i < len; i++) {
        opt = sel.options[i];
        if (opt.value === true) {
            break;
        }
    }
    //console.log(opt);
    return opt;
}

function resetAllOptions() {
    var curPeriods = document.getElementsByName("options");
    var periodsLength = curPeriods.length;

    for (let i = periodsLength - 1; i >= 0; i--) {
        curPeriods[i].remove();
    }
}

function changeBtnandMessageState(btnState, message) {
    document.getElementById("bookBtn").disabled = btnState;
    document.getElementById("message").innerText = message;
}

function CustomConfirm() {
    this.render = function (dialog) {
        dialog = "Click yes to confirm";
        var winW = window.innerWidth;
        var winH = window.innerHeight;
        var dialogoverlay = document.getElementById('dialogoverlay');
        var dialogbox = document.getElementById('dialogbox');
        dialogoverlay.style.display = "block";
        dialogoverlay.style.height = winH + "px";
        dialogbox.style.left = (winW / 2) - (550 * .5) + "px";
        dialogbox.style.top = "100px";
        dialogbox.style.display = "block";

        document.getElementById('dialogboxhead').innerHTML = "Confirm that action";
        document.getElementById('dialogboxbody').innerHTML = dialog;
        document.getElementById('dialogboxfoot').innerHTML = '<button onclick="Confirm.yes()">Yes</button> <button onclick="Confirm.no()">No</button>';
    }
    this.no = function () {
        document.getElementById('dialogbox').style.display = "none";
        document.getElementById('dialogoverlay').style.display = "none";
        //console.log("No");
        //noCallBack();
    }
    this.yes = function () {
        document.getElementById('dialogbox').style.display = "none";
        document.getElementById('dialogoverlay').style.display = "none";
        submit();
        //yesCallBack();
    }
}

function getCurrentRate() {
    $.ajax({
        type: "get",
        url: "https://localhost:44379/Booking/CheckRateBefore",
        data:
        {
            playgroundId: parseInt(document.getElementById("playground").value)
        },
        success: function (response) {
            if (response.rate > 0)
                updateSelectedStar(response.rate);
        },
        error: function (req, status, error) {
            //console.log(msg);
        }
    });
}

function updateRate(e) {
    switch (e.id) {
        case "1":
            updateSelectedStar("1");
            updateRateInDb(1);
            break;
        case "2":
            updateSelectedStar("2");
            updateRateInDb(2);
            break;
        case "3":
            updateSelectedStar("3");
            updateRateInDb(3);
            break;
        case "4":
            updateSelectedStar("4");
            updateRateInDb(4);
            break;
        case "5":
            updateSelectedStar("5");
            updateRateInDb(5);
            break;
    }
}

function updateSelectedStar(starNum) {
    var getOtherStars = document.getElementsByTagName("i");

    for (let i = 0; i < getOtherStars.length; i++) {
        if (getOtherStars[i].id <= starNum) {
            getOtherStars[i].style.color = "#fc0";
        } else {
            getOtherStars[i].style.color = "aliceblue";
        }
    }
}

function updateRateInDb(selRate) {
    $.ajax({
        type: "get",
        url: "https://localhost:44379/Booking/UpdateRate",
        data:
        {
            playgroundId: document.getElementById("playground").value,
            rate: selRate
        },
        success: function (response) {
            console.log("done");
        },
        error: function (req, status, error) {
            //console.log(msg);
        }
    });
}

var Confirm = new CustomConfirm();

checkBooking();
getCurrentRate();