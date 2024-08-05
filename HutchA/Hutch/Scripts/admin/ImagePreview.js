function previewProductImage() {
    let imageReader = new FileReader();
    imageReader.readAsDataURL(document.getElementById("product-image-upload").files[0]);
    imageReader.onload = function (productImageEvent) {
        document.getElementById("image-preview").src = productImageEvent.target.result;
    };
};

function previewCategoryImage() {
    let imageReader = new FileReader();
    imageReader.readAsDataURL(document.getElementById("category-image-upload").files[0]);
    imageReader.onload = function (productImageEvent) {
        document.getElementById("image-preview").src = productImageEvent.target.result;
    };
};