$(document).on("click", ".single-wishlist-icon-container", function () {

    let pid = $(this).data("product-id");
    const wishlistedIcon = $(this).find(".wishlisted-icon");
    const wishlistIcon = $(this).find(".wishlist-icon");

    $.ajax({
        url: '/Product/HandleSingleWishlist',
        type: 'POST',
        data: { pid: pid },
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
