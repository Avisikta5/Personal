$(document).on("click", ".wishlist-icon-container", function () {
    let productId = $(this).data("product-id");
    const wishlistedIcon = $(this).find(".wishlisted-icon");
    const wishlistIcon = $(this).find(".wishlist-icon");

    $.ajax({
        url: '/Product/Wishlist',
        type: 'POST',
        data: { productId: productId },
        success: function (response) {
            if (response.redirect) {
                window.location.href = response.redirect;
            }
            else {
                if (wishlistedIcon.hasClass("display-block")) {
                    wishlistedIcon.removeClass("display-block");
                    wishlistIcon.addClass("display-block");
                }
                else if (wishlistIcon.hasClass("display-block")) {
                    wishlistIcon.removeClass("display-block");
                    wishlistedIcon.addClass("display-block");
                }
            }
        },
        error: function (error) {
            console.log(error);
        }
    });
});
