// OutWit Web Framework - JavaScript Interop
// Functions called from Blazor via JS.InvokeVoidAsync

// OutWit namespace for framework functions
window.outwit = window.outwit || {};

/**
 * Get system theme preference (dark/light)
 * @returns {boolean} True if system prefers dark mode
 */
window.outwit.getSystemThemePreference = function () {
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
};

/**
 * Set theme attribute on document element
 * @param {string} theme - Theme name ('dark' or 'light')
 */
window.outwit.setThemeAttribute = function (theme) {
    document.documentElement.setAttribute('data-theme', theme);
};

/**
 * Scroll to an element by its ID with smooth animation
 * @param {string} elementId - The ID of the element to scroll to
 */
window.scrollToElement = function (elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({
            behavior: 'smooth',
            block: 'start'
        });
    }
};

/**
 * Set or toggle theme
 * @param {string} theme - 'dark' or 'light'
 */
window.setTheme = function (theme) {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
};

/**
 * Get current theme
 * @returns {string} Current theme
 */
window.getTheme = function () {
    return document.documentElement.getAttribute('data-theme') || 'dark';
};

/**
 * Render Cloudflare Turnstile widget explicitly for Blazor SPA
 * @param {object} dotNetRef - DotNet reference to the ContactForm component
 * @param {string} siteKey - Turnstile site key
 * @param {string} containerId - ID of the container element
 */
window.renderTurnstile = function (dotNetRef, siteKey, containerId) {
    // Wait for Turnstile to be available
    if (typeof turnstile === 'undefined') {
        // Retry after a short delay
        setTimeout(function () {
            window.renderTurnstile(dotNetRef, siteKey, containerId);
        }, 100);
        return;
    }

    const container = document.getElementById(containerId);
    if (!container) {
        console.error('Turnstile container not found:', containerId);
        return;
    }

    // Clear any existing widget
    container.innerHTML = '';

    // Render Turnstile widget
    turnstile.render(container, {
        sitekey: siteKey,
        theme: 'auto',
        callback: function (token) {
            dotNetRef.invokeMethodAsync('OnTurnstileSuccess', token);
        },
        'error-callback': function () {
            dotNetRef.invokeMethodAsync('OnTurnstileError');
        },
        'expired-callback': function () {
            dotNetRef.invokeMethodAsync('OnTurnstileExpired');
        }
    });
};

/**
 * Reset Turnstile widget
 * @param {string} containerId - ID of the container element
 */
window.resetTurnstile = function (containerId) {
    if (typeof turnstile !== 'undefined') {
        const container = document.getElementById(containerId);
        if (container) {
            turnstile.reset(container);
        }
    }
};
