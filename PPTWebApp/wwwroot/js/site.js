window.createChart = (canvasId, type, data, options) => {
    const canvasElement = document.getElementById(canvasId);

    if (!canvasElement) {
        console.error(`Canvas element with id '${canvasId}' not found. Cannot create chart.`);
        return;
    }

    const ctx = canvasElement.getContext('2d');
    if (!ctx) {
        console.error(`Unable to get '2d' context for canvas with id '${canvasId}'.`);
        return;
    }

    if (window[canvasId] instanceof Chart) {
        window[canvasId].destroy();
    }

    window[canvasId] = new Chart(ctx, {
        type: type,
        data: data,
        options: options
    });
};
