window.createChart = (canvasId, type, data, options) => {
    const ctx = document.getElementById(canvasId).getContext('2d');

    if (window[canvasId] instanceof Chart) {
        window[canvasId].destroy();
    }

    window[canvasId] = new Chart(ctx, {
        type: type,
        data: data,
        options: options
    });
};
