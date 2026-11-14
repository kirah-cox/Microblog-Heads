// Small helpers called from Blazor with IJSRuntime.

// Scrolls the element with this id into the middle of the screen (used by the post page to show
// the comment a notification pointed to). Does nothing if the element isn't on the page.
window.scrollToElementId = function (id) {
    document.getElementById(id)?.scrollIntoView({ behavior: "smooth", block: "center" });
};
