function applyFadeInEffect() {
    function fadeIn(element) {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1 });
        observer.observe(element);
    }

    document.querySelectorAll('.fade-in-element').forEach(fadeIn);

    const mutationObserver = new MutationObserver((mutationsList) => {
        mutationsList.forEach(mutation => {
            mutation.addedNodes.forEach(node => {
                if (node.nodeType === 1 && node.classList.contains('fade-in-element')) {
                    fadeIn(node);
                }
            });
        });
    });

    mutationObserver.observe(document.body, { childList: true, subtree: true });
}
