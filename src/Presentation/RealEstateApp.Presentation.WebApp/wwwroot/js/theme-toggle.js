// Dark Mode Theme Controller with LocalStorage Persistence
(function () {
    const THEME_KEY = "realestate_app_theme";

    function getPreferredTheme() {
        const savedTheme = localStorage.getItem(THEME_KEY);
        if (savedTheme) {
            return savedTheme;
        }
        return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute("data-theme", theme);
        document.body.setAttribute("data-theme", theme);
        localStorage.setItem(THEME_KEY, theme);
        updateToggleButtons(theme);
    }

    function updateToggleButtons(theme) {
        const toggleBtns = document.querySelectorAll(".theme-toggle-btn");
        toggleBtns.forEach(btn => {
            const icon = btn.querySelector("i");
            if (icon) {
                if (theme === "dark") {
                    icon.className = "bi bi-sun-fill text-warning";
                    btn.setAttribute("title", "Cambiar a Modo Claro");
                } else {
                    icon.className = "bi bi-moon-stars-fill text-info";
                    btn.setAttribute("title", "Cambiar a Modo Oscuro");
                }
            }
        });
    }

    // Use event delegation for maximum reliability
    document.addEventListener("click", function (e) {
        const btn = e.target.closest(".theme-toggle-btn");
        if (btn) {
            e.preventDefault();
            const activeTheme = document.documentElement.getAttribute("data-theme") || "light";
            const newTheme = activeTheme === "dark" ? "light" : "dark";
            applyTheme(newTheme);
        }
    });

    document.addEventListener("DOMContentLoaded", function () {
        const currentTheme = getPreferredTheme();
        applyTheme(currentTheme);
    });

    // Immediate execution to prevent flash of light content
    const initialTheme = getPreferredTheme();
    document.documentElement.setAttribute("data-theme", initialTheme);
})();
