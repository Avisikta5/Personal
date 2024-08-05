const loginAlert = document.querySelector(".error-message");

if (loginAlert) {
    setTimeout(() => {
        loginAlert.style.opacity = 0;
        loginAlert.style.visibility = 'hidden';
    }, 3000);
}