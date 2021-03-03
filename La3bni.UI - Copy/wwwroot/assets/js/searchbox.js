//Search functionality wasn't working cause i'm using ajax request ,
//solution was to implement a filter on data recomended and implemented in this post
//https://stackoverflow.com/questions/46666150/search-is-not-working-in-jquery-autocomplete/46666591

//to filter my data
if (!RegExp.escape) {
    RegExp.escape = function (value) {
        return value.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, "\\$&")
    };
}

$.ajax({
    url: '/Home/getPlaygrounds/',
    type: 'GET',
    dataType: 'json',
    success: function (data) {
        playgrounds = data.value;
    },
    error: function (request, message, error) {
        handleException(request, message, error);
    }
});

var playgrounds;
$("#search").autocomplete({
    source: function (request, response) {
        //using the filter
        var regex = new RegExp(RegExp.escape(request.term), "i");
        var recs = $.grep(playgrounds, function (obj) {
            return regex.test(obj.name)
        });
        //now my data is clean, then set it as the source for autocomplete
        response($.map(recs, function (playground) {
            return { label: playground.name, value: playground.playgroundId }
        }));
    },
    select: function (e, ui) {
        $('#search').val(ui.item.label);
        location = "/Booking/Index/" + ui.item.value;
        return false;
    },
    focus: function (event, ui) {
        $("#search").val(ui.item.label);
        return false; // Prevent the widget from inserting the value.
    },
});

$(function () {
    $("#search").on('keypress', function (e) {
        var keyPressed = e.keyCode || e.which;
        if (keyPressed === 13) {
            e.preventDefault();
            return false;
        }
    })
});