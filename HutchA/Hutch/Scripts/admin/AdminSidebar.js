const adminSidebar = document.querySelector(".dashboard-sidebar");
const sidebarHamburgerBtn = document.querySelector(".sidebar-hamburger-btn");
const sidebarCloseBtn = document.querySelector(".sidebar-close-btn");

const sidebarModal = document.querySelector(".sidebar-modal");

sidebarHamburgerBtn.addEventListener("click", () => {
    adminSidebar.classList.add("show-sidebar");
    sidebarModal.style.display = "block";
})

sidebarCloseBtn.addEventListener("click", () => {
    adminSidebar.classList.remove("show-sidebar");
    sidebarModal.style.display = "none";
})

