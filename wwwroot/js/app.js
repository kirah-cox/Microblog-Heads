// Small helpers called from Blazor with IJSRuntime.

// Scrolls the element with this id into the middle of the screen (used by the post page to show
// the comment a notification pointed to). Does nothing if the element isn't on the page.
window.scrollToElementId = function (id) {
    document.getElementById(id)?.scrollIntoView({ behavior: "smooth", block: "center" });
};

// Dark mode for every page. The Settings page saves "True"/"False" under "darkMode" in
// localStorage; this re-applies it on the first load and after each enhanced navigation
// (which can reset the <body> class), so pages don't each have to remember to do it.
window.applySavedDarkMode = function () {
    let isDark = false;
    try {
        isDark = (localStorage.getItem("darkMode") || "").toLowerCase() === "true";
    } catch {
        // Storage blocked (e.g. private mode): fall back to light.
    }
    document.body.classList.toggle("dark-mode", isDark);
};

window.applySavedDarkMode();
if (window.Blazor) {
    Blazor.addEventListener("enhancedload", window.applySavedDarkMode);
}
