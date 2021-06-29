$(function () {
    // Add image.
    $(document).on('change', '.option-image-file', function () {
        var $this = $(this);
        var $controlHolder = $this.closest('.controls');
        var $imgElem = $('img.option-image', $controlHolder);
        var $input = $('.option-image-hidden', $controlHolder);
        var $imgHolder = $('.image-holder', $controlHolder);

        var file = $this[0].files[0];
        var reader = new FileReader();
        $(reader).on('load', function () {
            $imgElem[0].src = this.result;
            $input.val(this.result);
            setTimeout(function () {
                if ($imgHolder.hasClass('no-image')) {
                    $imgElem.hide();
                    $imgHolder.removeClass("no-image");
                    $imgElem.slideDown('fast');
                }
            }, 0);
        });
        reader.readAsDataURL(file);
    });

    // Remove image.
    $(document).on('click', '.remove-image', function () {
        var $this = $(this);
        var $controlHolder = $this.closest('.controls');
        var $imgElem = $('img.option-image', $controlHolder);
        var $input = $('.option-image-hidden', $controlHolder);
        var $imgHolder = $('.image-holder', $controlHolder);

        $input.val('');
        $imgElem.slideUp('fast', function () {
            $imgHolder.addClass("no-image");
        });

        return false;
    });
});