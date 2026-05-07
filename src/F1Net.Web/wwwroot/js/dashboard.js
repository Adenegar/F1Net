(function () {
    const F1Net = window.F1Net = window.F1Net || {};

    async function fetchJson(url) {
        const r = await fetch(url, { credentials: "same-origin" });
        if (!r.ok) throw new Error(`${url}: ${r.status}`);
        return r.json();
    }

    async function loadStandings(year) {
        const data = await fetchJson(`/api/standings/${year}`);
        const tbody = document.querySelector("#standings-table tbody");
        tbody.innerHTML = data.map(r =>
            `<tr><td>${r.position}</td><td>${r.driverName}</td><td>${r.teamName ?? ""}</td><td>${r.points}</td><td>${r.wins}</td></tr>`
        ).join("");
    }

    async function loadSessions() {
        const data = await fetchJson("/api/sessions/recent?take=10");
        const tbody = document.querySelector("#sessions-table tbody");
        tbody.innerHTML = data.map(s =>
            `<tr><td>${s.raceName}</td><td><a href="/Sessions/Details/${s.id}">${s.name}</a></td><td>${s.lapCount}</td><td>${s.anomalyCount}</td></tr>`
        ).join("");
        return data;
    }

    async function loadPaceForFirstSession(sessions) {
        if (!sessions.length) return;
        const first = sessions[0];
        const standings = await fetchJson(`/api/standings/${new Date().getUTCFullYear()}`).catch(() => []);
        if (!standings.length) return;
        // best-effort: just pick first session's first driver via /api/sessions/{id}/anomalies for any driver name
        // (the proper driver-id lookup would need another endpoint; skipped for v1)
    }

    F1Net.Dashboard = {
        async init(year) {
            await Promise.all([loadStandings(year), loadSessions().then(loadPaceForFirstSession)]);
        }
    };
})();
