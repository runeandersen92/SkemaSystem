﻿(function ($) {
    var ajaxSemesterFormSubmit = function () {

        var $form = $(this);
        var options = {
            url: $form.attr("action"),
            type: $form.attr("method"),
            data: $form.serialize()
        };

        $.ajax(options).done(function (data) {
            var $target = $($form.attr("data-schemesys-target"));
            $target.html(data);
        });

        return false;
    }
    $(document).on('submit', "form[data-schemesys-ajax='true']", ajaxSemesterFormSubmit);

    $(document).on('submit', 'form#scheme-selector', function () {
        var form = $(this);

        $.ajax({
            url: form.attr("data-action-subject"),
            type: form.attr("method"),
            data: form.serialize()
        }).done(function (response) {
            $('#subject-selector').html(response);
        });

        $.ajax({
            url: form.attr("data-action-scheme"),
            type: form.attr("method"),
            data: form.serialize()
        }).done(function (response) {
            $('#schemes').html(response);
        });

        return false;
    });


})(jQuery);
