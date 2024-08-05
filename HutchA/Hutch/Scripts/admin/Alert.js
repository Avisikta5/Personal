const productAddAlert = document.querySelector(".admin-add-product-alert");

if (productAddAlert) {
    setTimeout(() => {
        productAddAlert.style.opacity = 1;
        productAddAlert.style.visibility = "hidden";
    }, 3000)
}
