const navUpperLinkWrapper = document.querySelector(".nav-upper-link-wrapper");
const navUpperLinkContainer = document.querySelector(".nav-upper-link-container");
const navUpperLinkBtn = document.querySelector(".nav-upper-link-btn");
const navUpperCloseBtn = document.querySelector(".nav-upper-close-btn");




/* This approach is not that good as js only re-renders whenever there is a change in the DOM element,
not in the style of the DOM element. So it is better to use max-content as height in the css*/

const navUpperHeight = navUpperLinkContainer.getBoundingClientRect().height;

navUpperLinkBtn.addEventListener("click", () => {
    navUpperLinkWrapper.style.height = `${navUpperHeight}px`;
    document.body.style.overflow = "hidden";
});

navUpperCloseBtn.addEventListener("click", () => {
    navUpperLinkWrapper.style.height = `0px`;
    document.body.style.overflow = "visible";
});

function changeNavText() {
    const navUpperText = document.querySelector(".nav-upper-text");

    const navTexts = ["Sign up & get 15% off", "Free delivery, return & exchange", "Net banking available"];
    let i = 0;

    setInterval(() => {
        navUpperText.textContent = navTexts[i];
        i += 1;

        if (i > 2) {
            i = 0;
        }

    }, 3000);

}

changeNavText();

/*
navUpperLinkBtn.addEventListener("click", () => {
    navUpperLinkWrapper.classList.add("show-nav-upper");
    document.body.style.overflow = "hidden";
});

navUpperCloseBtn.addEventListener("click", () => {
    navUpperLinkWrapper.classList.remove("show-nav-upper");
    document.body.style.overflow = "visible";
});
*/