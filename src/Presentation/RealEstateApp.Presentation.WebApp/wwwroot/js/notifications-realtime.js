// Real-time Push Notifications via SignalR Hub
(function () {
    if (typeof signalR === "undefined") {
        console.warn("SignalR not present for notifications.");
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notification")
        .withAutomaticReconnect([0, 2000, 5000, 10000])
        .build();

    connection.on("ReceiveNotification", function (data) {
        showToastNotification(data);
        updateNotificationBadge(1);
        addNotificationToDropdown(data);
    });

    connection.start().catch(err => console.error("NotificationHub connection error:", err));

    function showToastNotification(data) {
        let container = document.getElementById("toastContainer");
        if (!container) {
            container = document.createElement("div");
            container.id = "toastContainer";
            container.className = "toast-container position-fixed bottom-0 end-0 p-3";
            container.style.zIndex = "9999";
            document.body.appendChild(container);
        }

        const iconClass = data.type === "Offer" ? "bi-cash-coin text-success" :
            data.type === "Chat" ? "bi-chat-dots text-info" :
                data.type === "Account" ? "bi-shield-check text-warning" : "bi-bell text-primary";

        const toastId = "toast_" + Date.now();
        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-white bg-dark border-0 shadow-lg mb-2 rounded-4" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body d-flex align-items-center gap-3">
                        <div class="p-2 bg-secondary rounded-circle d-flex align-items-center justify-content-center">
                            <i class="bi ${iconClass} fs-4"></i>
                        </div>
                        <div>
                            <strong class="d-block text-white">${escapeHtml(data.title)}</strong>
                            <span class="small text-white-50">${escapeHtml(data.message)}</span>
                            ${data.redirectUrl ? `<a href="${data.redirectUrl}" class="d-block small text-info text-decoration-none mt-1 fw-bold">Ver detalles &rarr;</a>` : ''}
                        </div>
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

        container.insertAdjacentHTML("beforeend", toastHtml);
        const toastEl = document.getElementById(toastId);
        const bsToast = new bootstrap.Toast(toastEl, { delay: 6000 });
        bsToast.show();
    }

    function updateNotificationBadge(increment) {
        const badge = document.getElementById("notificationBadge");
        if (badge) {
            let current = parseInt(badge.textContent || "0");
            current += increment;
            badge.textContent = current;
            badge.classList.remove("d-none");
        }
    }

    function addNotificationToDropdown(data) {
        const menu = document.getElementById("notificationDropdownMenu");
        if (menu) {
            const noNotif = menu.querySelector(".no-notif-item");
            if (noNotif) noNotif.remove();

            const itemHtml = `
                <li>
                    <a class="dropdown-item py-2 px-3 border-bottom d-flex align-items-start gap-2" href="${data.redirectUrl || '#'}">
                        <i class="bi bi-bell-fill text-primary mt-1"></i>
                        <div>
                            <div class="fw-bold small">${escapeHtml(data.title)}</div>
                            <div class="text-muted extra-small">${escapeHtml(data.message)}</div>
                            <div class="text-secondary extra-small" style="font-size: 0.7rem;">${data.createdAtFormatted || 'Hace un momento'}</div>
                        </div>
                    </a>
                </li>
            `;
            menu.insertAdjacentHTML("afterbegin", itemHtml);
        }
    }

    function escapeHtml(text) {
        const div = document.createElement("div");
        div.textContent = text;
        return div.innerHTML;
    }
})();
