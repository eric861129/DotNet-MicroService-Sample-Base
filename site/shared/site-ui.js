(() => {
  const storageKey = "enterprise-docs-theme";

  function getPreferredTheme() {
    const saved = localStorage.getItem(storageKey);
    if (saved === "light" || saved === "dark") {
      return saved;
    }

    return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
  }

  function applyTheme(theme) {
    document.documentElement.setAttribute("data-theme", theme);
    localStorage.setItem(storageKey, theme);

    document.querySelectorAll("[data-theme-toggle]").forEach((button) => {
      const nextMode = theme === "dark" ? "light" : "dark";
      button.setAttribute("aria-label", `切換到${nextMode === "dark" ? "深色" : "淺色"}模式`);
      button.setAttribute("title", `切換到${nextMode === "dark" ? "深色" : "淺色"}模式`);
      const label = button.querySelector("[data-theme-toggle-label]");

      if (label) {
        label.textContent = theme === "dark" ? "深色模式" : "淺色模式";
      }
    });
  }

  function toggleTheme() {
    const current = document.documentElement.getAttribute("data-theme") || getPreferredTheme();
    applyTheme(current === "dark" ? "light" : "dark");
  }

  document.addEventListener("DOMContentLoaded", () => {
    applyTheme(getPreferredTheme());

    document.querySelectorAll("[data-theme-toggle]").forEach((button) => {
      button.addEventListener("click", toggleTheme);
    });
  });
})();
