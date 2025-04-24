document.addEventListener('DOMContentLoaded', function () {
    const gdoMenu = document.getElementById('gdoRequestMenu');
    const button = document.querySelector('[data-bs-target="#gdoRequestMenu"]');
    const links = gdoMenu.querySelectorAll('a');

    // Force the menu to collapse every time the page is loaded
    gdoMenu.classList.remove('show');
    gdoMenu.setAttribute('aria-expanded', 'false');  // Ensure the aria-expanded state is false (collapsed)

    // Remove any previously saved state from localStorage (ensuring collapse on every login)
    localStorage.removeItem('gdoMenuExpanded');

    // Initialize collapse state based on the above logic
    let isExpanded = localStorage.getItem('gdoMenuExpanded');
    if (isExpanded === null) {
        localStorage.setItem('gdoMenuExpanded', 'false'); // Default state: collapsed
        isExpanded = 'false';
    }

    // Update localStorage when the collapse button is clicked
    button.addEventListener('click', function () {
        setTimeout(() => {
            const currentlyExpanded = gdoMenu.classList.contains('show');
            localStorage.setItem('gdoMenuExpanded', currentlyExpanded);
        }, 300); // Wait for transition to complete
    });

    // Save collapse state before navigating away
    links.forEach(link => {
        link.addEventListener('pointerdown', function () {
            const currentlyExpanded = gdoMenu.classList.contains('show');
            localStorage.setItem('gdoMenuExpanded', currentlyExpanded);
        });
    });
});
