function applyDarkMode() {
    const darkModeSetting = localStorage.getItem('darkMode');
    const isDarkMode = darkModeSetting === 'True';

    if (isDarkMode) {
        document.body.classList.add('dark-mode');
    } else {
        document.body.classList.remove('dark-mode');
    }
}

// Apply on initial load
applyDarkMode();

// Listen for Blazor navigation events
document.addEventListener('enhancedload', function() {
    setTimeout(applyDarkMode, 0);
});

// Listen for Blazor enhanced navigation completion
Blazor?.addEventListener?.('enhancedload', () => {
    setTimeout(applyDarkMode, 0);
});

