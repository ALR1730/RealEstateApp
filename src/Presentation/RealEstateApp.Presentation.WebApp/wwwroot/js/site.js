// RealEstateApp JavaScript Global Utilities

document.addEventListener("DOMContentLoaded", function () {
    // Evento de clic en cualquier parte de una tarjeta de propiedad (card-property)
    $(document).on('click', '.card-property', function (e) {
        // Evitar la navegación si se hizo clic en un botón, enlace, formulario o control interactivo dentro de la tarjeta
        if ($(e.target).closest('button, a, form, input, label, select').length) {
            return;
        }

        // Buscar primero la URL en el atributo data-href de la tarjeta
        var targetUrl = $(this).attr('data-href');

        // Si no está en data-href, buscar el enlace de detalles dentro de la tarjeta
        if (!targetUrl) {
            targetUrl = $(this).find('a[href*="/Details/"], a[href*="/Home/Details"], a[href*="Details"]').first().attr('href');
        }

        // Si aún no se encuentra, usar el primer enlace disponible en la tarjeta
        if (!targetUrl) {
            targetUrl = $(this).find('a').first().attr('href');
        }

        // Navegar a la página correspondiente
        if (targetUrl) {
            window.location.href = targetUrl;
        }
    });
});
