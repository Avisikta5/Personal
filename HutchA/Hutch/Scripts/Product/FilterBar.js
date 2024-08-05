const filterOpenBtn = document.querySelector(".product-filter-bar-open-btn"); filterCloseBtn = document.querySelector(".product-filter-bar-close-btn");
const filterBar = document.querySelector(".product-filter-bar");

filterOpenBtn.addEventListener("click", () => {
    console.log("Hi")
    filterBar.classList.add("show-filter-bar");
})

filterCloseBtn.addEventListener("click", () => {
    filterBar.classList.remove("show-filter-bar");
})