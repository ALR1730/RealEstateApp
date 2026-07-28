// RealEstateApp JavaScript Global Utilities

document.addEventListener("DOMContentLoaded", function () {
    // Evento de clic en cualquier tarjeta interactiva (card-property, card-chat o elementos con data-href)
    $(document).on('click', '.card-property, .card-chat, [data-href]', function (e) {
        // Evitar la navegación si se hizo clic en un botón, enlace, formulario o control interactivo dentro de la tarjeta
        if ($(e.target).closest('button, a, form, input, label, select').length) {
            return;
        }

        // Buscar primero la URL en el atributo data-href de la tarjeta
        var targetUrl = $(this).attr('data-href');

        // Si no está en data-href, buscar el enlace dentro de la tarjeta
        if (!targetUrl) {
            targetUrl = $(this).find('a[href*="/Thread"], a[href*="/Details"], a[href*="Thread"], a[href*="Details"]').first().attr('href');
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
