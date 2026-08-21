// Property Comparator Dock and LocalStorage Manager
(function () {
    const STORAGE_KEY = "realestate_comparator_ids";

    window.PropertyComparator = {
        getIds: function () {
            const data = localStorage.getItem(STORAGE_KEY);
            return data ? JSON.parse(data) : [];
        },

        add: function (id, title, price, img) {
            let ids = this.getIds();
            if (ids.length >= 4) {
                alert("Puedes comparar hasta 4 propiedades como máximo.");
                return false;
            }
            if (!ids.some(item => item.id === id)) {
                ids.push({ id, title, price, img });
                localStorage.setItem(STORAGE_KEY, JSON.stringify(ids));
            }
            this.renderDrawer();
            return true;
        },

        remove: function (id) {
            let ids = this.getIds();
            ids = ids.filter(item => item.id !== id);
            localStorage.setItem(STORAGE_KEY, JSON.stringify(ids));
            this.renderDrawer();
        },

        clear: function () {
            localStorage.removeItem(STORAGE_KEY);
            this.renderDrawer();
        },

        renderDrawer: function () {
            let dock = document.getElementById("comparatorDock");
            const ids = this.getIds();

            if (ids.length === 0) {
                if (dock) dock.remove();
                return;
            }

            if (!dock) {
                dock = document.createElement("div");
                dock.id = "comparatorDock";
                dock.className = "position-fixed bottom-0 start-50 translate-middle-x bg-dark text-white rounded-pill px-4 py-3 shadow-lg d-flex align-items-center gap-3 border border-secondary";
                dock.style.zIndex = "9990";
                dock.style.marginBottom = "1.5rem";
                document.body.appendChild(dock);
            }

            const thumbnailsHtml = ids.map(item => `
                <div class="position-relative d-inline-block">
                    <img src="${item.img || '/images/default-property.jpg'}" class="rounded-circle border border-2 border-info" style="width: 42px; height: 42px; object-fit: cover;" title="${escapeHtml(item.title)} - ${item.price}" />
                    <button onclick="PropertyComparator.remove(${item.id})" class="btn btn-danger btn-sm p-0 rounded-circle position-absolute top-0 start-100 translate-middle" style="width: 18px; height: 18px; line-height: 14px; font-size: 10px;">&times;</button>
                </div>
            `).join("");

            const idsQuery = ids.map(i => i.id).join(",");

            dock.innerHTML = `
                <div class="d-flex align-items-center gap-2">
                    <i class="bi bi-arrow-left-right text-info fs-5"></i>
                    <span class="fw-bold small">Comparador (${ids.length}/4)</span>
                </div>
                <div class="d-flex align-items-center gap-2">
                    ${thumbnailsHtml}
                </div>
                <div class="d-flex align-items-center gap-2 ms-2">
                    <a href="/Home/Compare?ids=${idsQuery}" class="btn btn-info btn-sm rounded-pill px-3 fw-bold">Comparar Ahora &rarr;</a>
                    <button onclick="PropertyComparator.clear()" class="btn btn-outline-light btn-sm rounded-circle p-1" title="Limpiar"><i class="bi bi-trash"></i></button>
                </div>
            `;
        }
    };

    function escapeHtml(text) {
        const div = document.createElement("div");
        div.textContent = text;
        return div.innerHTML;
    }

    document.addEventListener("DOMContentLoaded", function () {
        window.PropertyComparator.renderDrawer();
    });
})();
