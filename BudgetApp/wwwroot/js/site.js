/* site.js shared utilities */

const SiteUtils = (() => {
    const PIE_COLORS = [
        "#0d6efd","#198754","#dc3545","#ffc107","#0dcaf0",
        "#6f42c1","#fd7e14","#20c997","#d63384","#6c757d"
    ];

    function getAntiForgeryToken() {
        const el = document.querySelector("input[name=__RequestVerificationToken]");
        return el ? el.value : "";
    }

    async function postJson(url, data) {
        const resp = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": getAntiForgeryToken()
            },
            body: JSON.stringify(data)
        });
        return resp.json();
    }

    async function getJson(url) {
        const resp = await fetch(url);
        return resp.json();
    }

    function showToast(message, isError) {
        isError = isError || false;
        const container = document.getElementById("toastContainer");
        const id = "toast-" + Date.now();
        const typeClass = isError ? "toast-error" : "toast-success";
        const icon = isError ? "bi-exclamation-circle text-danger" : "bi-check-circle text-success";
        container.insertAdjacentHTML("beforeend",
            "<div id=" + id + " class=\"toast " + typeClass + "\" role=\"alert\">" +
            "<div class=\"toast-header\">" +
            "<i class=\"bi " + icon + " me-2\"></i>" +
            "<strong class=\"me-auto\">" + (isError ? "Error" : "Success") + "</strong>" +
            "<button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"toast\"></button>" +
            "</div><div class=\"toast-body\">" + message + "</div></div>");
        const toastEl = document.getElementById(id);
        new bootstrap.Toast(toastEl, { delay: 3500 }).show();
        toastEl.addEventListener("hidden.bs.toast", function() { toastEl.remove(); });
    }

    function buildPieChart(canvasId, slices) {
        const canvas = document.getElementById(canvasId);
        if (!canvas || !slices || !slices.length) return;
        const existing = Chart.getChart(canvas);
        if (existing) existing.destroy();
        new Chart(canvas, {
            type: "pie",
            data: {
                labels: slices.map(function(s) { return s.label || s.Label; }),
                datasets: [{
                    data: slices.map(function(s) { return s.value || s.Value; }),
                    backgroundColor: PIE_COLORS.slice(0, slices.length),
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { position: "bottom", labels: { font: { size: 12 } } },
                    tooltip: {
                        callbacks: {
                            label: function(ctx) { return " $" + ctx.parsed.toFixed(2); }
                        }
                    }
                }
            }
        });
    }

    (function storeTz() {
        if (document.cookie.indexOf("userTimeZone") === -1) {
            var tz = Intl.DateTimeFormat().resolvedOptions().timeZone;
            document.cookie = "userTimeZone=" + encodeURIComponent(tz) + ";path=/;max-age=31536000";
        }
    })();

    return { postJson: postJson, getJson: getJson, showToast: showToast, buildPieChart: buildPieChart };
})();
