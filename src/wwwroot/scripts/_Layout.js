$(function () {
    $('#logout').click(function () {
        if (confirm('Are you sure you want to logout?')) {
            $.post($(this).attr('href'))
                .done(function (data) {
                    location.href = data.url;
                });
        }
        return false;
    });

})