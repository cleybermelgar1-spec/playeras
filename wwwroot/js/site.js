// site.js - Animaciones y comportamiento del menú
document.addEventListener("DOMContentLoaded", function () {
    
    // 1. Efecto de entrada suave (Fade-in) cuando carga la página
    document.body.style.opacity = "0";
    document.body.style.transition = "opacity 0.4s ease-in-out";
    setTimeout(() => {
        document.body.style.opacity = "1";
    }, 50);

    // 2. Animación de encogimiento y desvanecimiento al hacer clic en las opciones del menú
    const menuLinks = document.querySelectorAll(".nav-link");

    menuLinks.forEach(link => {
        link.addEventListener("click", function (e) {
            // Efecto visual al presionar el botón
            this.style.transform = "scale(0.92)";
            this.style.transition = "transform 0.15s ease";

            // Si es un enlace normal, desvanece la pantalla antes de cambiar de página
            if (this.href && !this.href.includes("#") && this.target !== "_blank") {
                e.preventDefault();
                const destination = this.href;
                
                document.body.style.opacity = "0"; // Efecto Fade-out
                
                setTimeout(() => {
                    window.location.href = destination;
                }, 300); // Espera 300 milisegundos para permitir la animación
            }
        });
    });

    // 3. Desplazamiento suave para enlaces con ancla (scroll suave)
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth'
                });
            }
        });
    });
});