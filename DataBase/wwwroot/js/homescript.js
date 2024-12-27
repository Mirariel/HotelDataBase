let slideIndex = 1;
let slideInterval;

function plusSlides(n) {
    clearInterval(slideInterval);
    showSlides(slideIndex += n);
    slideInterval = setInterval(() => plusSlides(1), 3000);
}

function currentSlide(n) {
    clearInterval(slideInterval);
    showSlides(slideIndex = n);
    slideInterval = setInterval(() => plusSlides(1), 3000);
}

function showSlides(n) {
    let i;
    let slides = document.getElementsByClassName("slides");
    if (n > slides.length) { slideIndex = 1 }
    if (n < 1) { slideIndex = slides.length }
    for (i = 0; i < slides.length; i++) {
        slides[i].style.display = "none";
    }
    slides[slideIndex - 1].style.display = "block";
}

document.addEventListener("DOMContentLoaded", () => {
    showSlides(slideIndex);
    slideInterval = setInterval(() => plusSlides(1), 3000);
});
