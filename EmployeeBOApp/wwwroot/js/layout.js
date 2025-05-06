document.addEventListener('DOMContentLoaded', function () {
    const button = document.querySelector('[data-bs-target="#gdoRequestMenu"]');
    const icon = button.querySelector('i');
    const gdoMenu = document.getElementById('gdoRequestMenu');

    if (gdoMenu && button && icon) {
        // Set initial expand/collapse based on URL
        const currentPath = window.location.pathname.toLowerCase();
        if (currentPath.includes('/allocation') || currentPath.includes('/deallocation') || currentPath.includes('/reporting') || currentPath.includes('/view')) {
            // If user is in Allocation/Deallocation/Reporting, expand GDO Request menu
            const collapseInstance = bootstrap.Collapse.getOrCreateInstance(gdoMenu, { toggle: false });
            collapseInstance.show();
            icon.classList.remove('fa-caret-down');
            icon.classList.add('fa-caret-up');
        }

        // Update icon when user manually clicks button
        gdoMenu.addEventListener('show.bs.collapse', function () {
            icon.classList.remove('fa-caret-down');
            icon.classList.add('fa-caret-up');
        });

        gdoMenu.addEventListener('hide.bs.collapse', function () {
            icon.classList.remove('fa-caret-up');
            icon.classList.add('fa-caret-down');
        });
    }
});
