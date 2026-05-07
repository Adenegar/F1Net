(function () {
    const F1Net = window.F1Net = window.F1Net || {};
    let chart = null;
    let sessionId = 0;

    async function loadPace(driverId) {
        const r = await fetch(`/api/sessions/${sessionId}/drivers/${driverId}/pace`, { credentials: "same-origin" });
        if (!r.ok) return;
        const data = await r.json();
        const labels = data.laps.map(l => `Lap ${l.lapNumber}`);
        const times = data.laps.map(l => l.lapSeconds);
        const anomalyPoints = data.laps.map(l => l.isAnomaly ? l.lapSeconds : null);

        const ctx = document.getElementById("pace-chart").getContext("2d");
        if (chart) chart.destroy();
        chart = new Chart(ctx, {
            type: "line",
            data: {
                labels,
                datasets: [
                    { label: data.driverName, data: times, borderColor: "#e10600", tension: 0.15, pointRadius: 2 },
                    { label: "Anomaly", data: anomalyPoints, borderColor: "#ffd966", backgroundColor: "#ffd966",
                      pointRadius: 6, pointStyle: "rectRot", showLine: false }
                ]
            },
            options: {
                responsive: true,
                scales: { y: { title: { display: true, text: "Lap time (s)" } } },
                plugins: { legend: { labels: { color: "#e8eaed" } } }
            }
        });
    }

    F1Net.SessionDetails = {
        init(id) {
            sessionId = id;
            document.getElementById("loadPace").addEventListener("click", () => {
                const did = parseInt(document.getElementById("driverId").value, 10);
                if (did > 0) loadPace(did);
            });
        }
    };
})();
