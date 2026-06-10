// Rion Daily – Dashboard Interactivity

document.addEventListener('DOMContentLoaded', () => {
    initThemeToggle();
    initSidebar();
    initF1Countdown();
    initLiveClock();
    initLazyF1();
    initGlossary();
    initWatchlist();
    initSmoothScroll();
});

function initHeroF1Countdown() {
    const el = document.getElementById('heroF1Countdown');
    if (!el) return;

    let seconds = parseInt(el.dataset.seconds, 10);
    if (isNaN(seconds) || seconds <= 0) return;

    const label = el.textContent;

    function update() {
        if (seconds <= 0) {
            el.textContent = 'Heute!';
            return;
        }
        const d = Math.floor(seconds / 86400);
        const h = Math.floor((seconds % 86400) / 3600);
        const m = Math.floor((seconds % 3600) / 60);
        el.textContent = d > 0 ? `${label.split(' in ')[0]} in ${d}d ${h}h` : `${h}h ${m}m`;
        seconds--;
    }

    update();
    setInterval(update, 60000);
}

function updateThemeIcon() {
    const btn = document.getElementById('themeToggle');
    if (!btn) return;
    const icon = btn.querySelector('i');
    if (!icon) return;
    const isDark = document.documentElement.getAttribute('data-theme') === 'dark';
    icon.className = isDark ? 'bi bi-sun-fill' : 'bi bi-moon-stars';
    btn.setAttribute('aria-label', isDark ? 'Hellmodus' : 'Dunkelmodus');
}

function initThemeToggle() {
    const btn = document.getElementById('themeToggle');
    if (!btn) return;

    const saved = localStorage.getItem('rd-theme');
    if (saved === 'dark') {
        document.documentElement.setAttribute('data-theme', 'dark');
    } else {
        document.documentElement.removeAttribute('data-theme');
    }
    updateThemeIcon();

    btn.addEventListener('click', () => {
        const isDark = document.documentElement.getAttribute('data-theme') === 'dark';
        if (isDark) {
            document.documentElement.removeAttribute('data-theme');
            localStorage.setItem('rd-theme', 'light');
        } else {
            document.documentElement.setAttribute('data-theme', 'dark');
            localStorage.setItem('rd-theme', 'dark');
        }
        updateThemeIcon();
    });
}

function initSidebar() {
    const toggle = document.getElementById('sidebarToggle');
    const sidebar = document.querySelector('.rd-sidebar');
    if (!toggle || !sidebar) return;

    toggle.addEventListener('click', () => sidebar.classList.toggle('open'));
}

function initF1Countdown() {
    const el = document.getElementById('f1Countdown');
    if (!el) return;

    let seconds = parseInt(el.dataset.seconds, 10);
    if (isNaN(seconds)) return;

    function update() {
        if (seconds <= 0) {
            el.textContent = 'Heute!';
            return;
        }
        const d = Math.floor(seconds / 86400);
        const h = Math.floor((seconds % 86400) / 3600);
        const m = Math.floor((seconds % 3600) / 60);
        const s = seconds % 60;
        el.textContent = d > 0
            ? `${d}d ${h}h ${m}m`
            : `${h}h ${m}m ${s}s`;
        seconds--;
    }

    update();
    setInterval(update, 1000);
}

function initLiveClock() {
    const el = document.getElementById('rdLiveDate');
    if (!el) return;

    const locale = 'de-CH';
    const dateOpts = { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' };

    function tick() {
        const now = new Date();
        const date = now.toLocaleDateString(locale, dateOpts);
        const time = now.toLocaleTimeString(locale, { hour: '2-digit', minute: '2-digit' });
        el.textContent = `${date} · ${time}`;
    }

    tick();
    setInterval(tick, 30000);
}

let f1CenterLoaded = false;

function initLazyF1() {
    const container = document.getElementById('f1-lazy-container');
    if (!container) return;

    const load = () => {
        if (f1CenterLoaded) return;
        f1CenterLoaded = true;
        fetch('/dashboard/f1')
            .then(r => r.text())
            .then(html => {
                container.innerHTML = html;
                initF1Countdown();
                if (typeof initF1Expand === 'function') {
                    initF1Expand('f1ExpandDrivers', 'f1DriversGrid', 'Fahrer');
                    initF1Expand('f1ExpandTeams', 'f1TeamsGrid', 'Teams');
                }
                if (typeof initF1CalendarToggle === 'function') initF1CalendarToggle();
                if (typeof initF1RaceModal === 'function') initF1RaceModal();
                if (typeof initF1NewsTranslate === 'function') initF1NewsTranslate();
            })
            .catch(() => {
                f1CenterLoaded = false;
                container.innerHTML = '<section class="rd-section" id="f1"><p class="rd-section-sub">Formula 1 Center konnte nicht geladen werden.</p></section>';
            });
    };

    if ('IntersectionObserver' in window) {
        const observer = new IntersectionObserver(entries => {
            if (entries.some(e => e.isIntersecting)) {
                observer.disconnect();
                load();
            }
        }, { rootMargin: '200px' });
        observer.observe(container);
    } else {
        setTimeout(load, 500);
    }
}

function initLeagueTabs() {
    const tabs = document.querySelectorAll('.rd-league-tab');
    const panels = document.querySelectorAll('.rd-league-panel');

    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            const league = tab.dataset.league;
            tabs.forEach(t => t.classList.remove('active'));
            tab.classList.add('active');
            panels.forEach(p => {
                p.style.display = p.dataset.league === league ? 'block' : 'none';
            });
        });
    });
}

function initGlossary() {
    const tags = document.querySelectorAll('.rd-glossary-tag');
    const overlay = document.getElementById('glossaryModal');
    if (!overlay) return;

    const title = overlay.querySelector('.rd-modal-title');
    const body = overlay.querySelector('.rd-modal-body');
    const closeBtn = overlay.querySelector('.rd-modal-close');

    tags.forEach(tag => {
        tag.addEventListener('click', async () => {
            const slug = tag.dataset.slug;
            try {
                const res = await fetch(`/api/glossary/${slug}`);
                if (!res.ok) return;
                const data = await res.json();
                title.textContent = data.term;
                body.innerHTML = `
                    <p>${escapeHtml(data.simpleExplanation)}</p>
                    ${data.whyImportant ? `<div class="rd-modal-section"><strong>Warum wichtig?</strong><p>${escapeHtml(data.whyImportant)}</p></div>` : ''}
                    ${data.practicalExample ? `<div class="rd-modal-section"><strong>Praxisbeispiel</strong><p>${escapeHtml(data.practicalExample)}</p></div>` : ''}
                `;
                overlay.classList.add('show');
            } catch { /* silent */ }
        });
    });

    closeBtn?.addEventListener('click', () => overlay.classList.remove('show'));
    overlay.addEventListener('click', e => {
        if (e.target === overlay) overlay.classList.remove('show');
    });
}

function initWatchlist() {
    const form = document.getElementById('addWatchlistForm');
    const grid = document.getElementById('watchlistGrid');
    if (!form || !grid) return;

    const storageKey = 'rion-daily-watchlist';
    const defaults = [...grid.querySelectorAll('[data-symbol]')].map(el => el.dataset.symbol);

    function loadExtra() {
        try { return JSON.parse(localStorage.getItem(storageKey) || '[]'); }
        catch { return []; }
    }

    function saveExtra(symbols) {
        localStorage.setItem(storageKey, JSON.stringify(symbols));
    }

    loadExtra().forEach(symbol => {
        if (!defaults.includes(symbol)) addCard(symbol);
    });

    form.addEventListener('submit', e => {
        e.preventDefault();
        const input = form.querySelector('[name="symbol"]');
        const symbol = input.value.trim().toUpperCase();
        if (!symbol) return;

        const extra = loadExtra();
        if (defaults.includes(symbol) || extra.includes(symbol)) {
            alert('Symbol bereits in der Watchlist.');
            return;
        }
        extra.push(symbol);
        saveExtra(extra);
        addCard(symbol);
        input.value = '';
    });

    function addCard(symbol) {
        const card = document.createElement('div');
        card.className = 'rd-card';
        card.dataset.symbol = symbol;
        card.innerHTML = `
            <div class="d-flex justify-content-between">
                <div><div style="font-size:.8rem;color:var(--rd-text-muted)">${symbol}</div>
                <div style="font-weight:600">${symbol}</div></div>
                <div class="text-end"><div style="font-weight:700">–</div>
                <span class="rd-change hold">API laden…</span></div>
            </div>
            <button class="rd-btn mt-2" style="background:var(--rd-negative);font-size:.75rem;padding:.3rem .6rem"
                onclick="removeWatchlistLocal('${symbol}')">Entfernen</button>`;
        grid.appendChild(card);
    }
}

function removeWatchlistLocal(symbol) {
    const storageKey = 'rion-daily-watchlist';
    const extra = JSON.parse(localStorage.getItem(storageKey) || '[]').filter(s => s !== symbol);
    localStorage.setItem(storageKey, JSON.stringify(extra));
    document.querySelector(`[data-symbol="${symbol}"]`)?.remove();
}

function escapeHtml(str) {
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

function initSmoothScroll() {
    document.querySelectorAll('.rd-nav-link[href^="#"]').forEach(link => {
        link.addEventListener('click', e => {
            e.preventDefault();
            const target = document.querySelector(link.getAttribute('href'));
            target?.scrollIntoView({ behavior: 'smooth', block: 'start' });
            document.querySelector('.rd-sidebar')?.classList.remove('open');
        });
    });
}

