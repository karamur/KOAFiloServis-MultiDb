// Filo KPI Dashboard chart helpers
window.filoKpiCharts = (function () {
    const instances = {};

    function destroy(id) {
        if (instances[id]) {
            try { instances[id].destroy(); } catch (e) { }
            delete instances[id];
        }
    }

    function destroyAll() {
        Object.keys(instances).forEach(destroy);
    }

    function fmtTL(v) {
        return new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(v) + ' ₺';
    }

    function renderHakedisTip(canvasId, labels, data) {
        destroy(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        instances[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: ['#198754', '#dc3545', '#0dcaf0'],
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'bottom' },
                    tooltip: {
                        callbacks: {
                            label: function (ctx) {
                                return ctx.label + ': ' + fmtTL(ctx.parsed);
                            }
                        }
                    }
                }
            }
        });
    }

    function renderSeferTrend(canvasId, labels, data) {
        destroy(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        instances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Günlük Sefer',
                    data: data,
                    backgroundColor: 'rgba(13, 110, 253, 0.6)',
                    borderColor: '#0d6efd',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    y: { beginAtZero: true, ticks: { precision: 0 } }
                }
            }
        });
    }

    return {
        renderHakedisTip: renderHakedisTip,
        renderSeferTrend: renderSeferTrend,
        destroyAll: destroyAll
    };
})();
