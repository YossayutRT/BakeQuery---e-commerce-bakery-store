// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minification static web assets.

// Write your JavaScript code.

// ===== CAROUSEL FUNCTIONALITY =====
document.addEventListener('DOMContentLoaded', function() {
    const carousel = document.getElementById('promotionCarousel');
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    const dotsContainer = document.getElementById('carouselDots');

    if (!carousel) return;

    const items = carousel.querySelectorAll('.offer-item');
    let currentIndex = 0;

    // Create dots
    items.forEach((_, index) => {
        const dot = document.createElement('button');
        dot.classList.add('carousel-dot');
        if (index === 0) dot.classList.add('active');
        dot.addEventListener('click', () => goToSlide(index));
        dotsContainer.appendChild(dot);
    });

    function updateCarousel() {
        const offset = -currentIndex * 100;
        carousel.style.transform = `translateX(${offset}%)`;
        
        // Update dots
        document.querySelectorAll('.carousel-dot').forEach((dot, index) => {
            dot.classList.toggle('active', index === currentIndex);
        });
    }

    function nextSlide() {
        currentIndex = (currentIndex + 1) % items.length;
        updateCarousel();
    }

    function prevSlide() {
        currentIndex = (currentIndex - 1 + items.length) % items.length;
        updateCarousel();
    }

    function goToSlide(index) {
        currentIndex = index;
        updateCarousel();
    }

    prevBtn.addEventListener('click', prevSlide);
    nextBtn.addEventListener('click', nextSlide);

    // Auto-slide every 5 seconds
    setInterval(nextSlide, 5000);
});
