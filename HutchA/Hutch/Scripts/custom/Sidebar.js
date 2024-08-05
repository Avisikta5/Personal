const hamburgerMenuBtn = document.querySelector(".hamburger-menu-btn");
const sidebarCloseBtn = document.querySelector(".sidebar-close-btn");

const sidebarModal = document.querySelector(".sidebar-modal");
const sidebar = document.querySelector(".sidebar");

hamburgerMenuBtn.addEventListener("click", () => {
    sidebar.classList.add("show-sidebar");
    sidebarModal.style.display = "block";
    document.body.style.overflow = "hidden";
})

sidebarCloseBtn.addEventListener("click", () => {
    sidebar.classList.remove("show-sidebar");
    sidebarModal.style.display = "none";
    document.body.style.overflow = "scroll";
})
