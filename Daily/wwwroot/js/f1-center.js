document.addEventListener('DOMContentLoaded', () => {
    initF1Expand('f1ExpandDrivers', 'f1DriversGrid', 'Fahrer');
    initF1Expand('f1ExpandTeams', 'f1TeamsGrid', 'Teams');
    initF1CalendarToggle();
    initF1RaceModal();
    initF1NewsTranslate();
});

function initF1Expand(btnId, gridId, label) {
    const btn = document.getElementById(btnId);
    const grid = document.getElementById(gridId);
    if (!btn || !grid) return;

    const total = grid.querySelectorAll('.f1-team-card, .f1-driver-card').length;

    btn.addEventListener('click', () => {
        const expanded = grid.classList.toggle('expanded');
        btn.innerHTML = expanded
            ? `<i class="bi bi-chevron-up"></i> Weniger anzeigen`
            : `<i class="bi bi-chevron-down"></i> Alle ${label} anzeigen (${total})`;
    });
}

function initF1CalendarToggle() {
    const toggle = document.getElementById('f1CalendarToggle');
    const block = toggle?.closest('.f1-calendar-block');
    if (!toggle || !block) return;

    toggle.addEventListener('click', () => {
        const expanded = block.classList.toggle('expanded');
        toggle.setAttribute('aria-expanded', expanded ? 'true' : 'false');
    });
}

function initF1RaceModal() {
    const dataEl = document.getElementById('f1PastRacesData');
    const overlay = document.getElementById('f1RaceModal');
    if (!dataEl || !overlay) return;

    let races = [];
    try {
        races = JSON.parse(dataEl.textContent);
    } catch {
        return;
    }

    const raceMap = Object.fromEntries(races.map(r => [String(r.Round), r]));
    const title = overlay.querySelector('.rd-modal-title');
    const body = overlay.querySelector('.rd-modal-body');
    const closeBtn = overlay.querySelector('.rd-modal-close');

    document.querySelectorAll('.f1-cal-past').forEach(btn => {
        btn.addEventListener('click', () => {
            const race = raceMap[btn.dataset.round];
            if (!race) return;

            title.textContent = race.RaceName;

            const circuitHtml = race.CircuitImageUrl
                ? `<div class="f1-race-detail-circuit"><img src="${escapeHtml(race.CircuitImageUrl)}" alt="${escapeHtml(race.CircuitName)}" /></div>`
                : '';

            const podiumHtml = (race.Podium || []).map((name, i) =>
                `<span class="f1-podium-pos p${i + 1}">${i + 1}. ${escapeHtml(name)}</span>`
            ).join('');

            const highlightsHtml = (race.Highlights || []).length
                ? `<div class="f1-race-highlights"><div class="f1-story-label">Highlights</div><ul>${race.Highlights.map(h => `<li>${escapeHtml(h)}</li>`).join('')}</ul></div>`
                : '';

            body.innerHTML = `
                ${circuitHtml}
                <div class="f1-race-detail-grid">
                    <div class="f1-race-detail-item"><span>Sieger</span><strong>${escapeHtml(race.Winner || '–')}</strong></div>
                    <div class="f1-race-detail-item"><span>Pole</span><strong>${escapeHtml(race.PolePosition || '–')}</strong></div>
                    <div class="f1-race-detail-item"><span>Schnellste Runde</span><strong>${escapeHtml(race.FastestLap || '–')}</strong></div>
                    <div class="f1-race-detail-item"><span>Datum</span><strong>${escapeHtml(race.DateDisplay)}</strong></div>
                </div>
                <div class="f1-race-podium">${podiumHtml}</div>
                ${highlightsHtml}
            `;

            overlay.classList.add('show');
        });
    });

    closeBtn?.addEventListener('click', () => overlay.classList.remove('show'));
    overlay.addEventListener('click', e => {
        if (e.target === overlay) overlay.classList.remove('show');
    });
}

function initF1NewsTranslate() {
    document.querySelectorAll('.f1-translate-btn').forEach(btn => {
        btn.addEventListener('click', async () => {
            const story = btn.closest('.f1-story');
            if (!story) return;

            const translationBox = story.querySelector('.f1-story-translation');
            const content = story.querySelector('.f1-translation-content');
            if (!translationBox || !content) return;

            if (!translationBox.classList.contains('hidden')) {
                translationBox.classList.add('hidden');
                btn.classList.remove('active');
                return;
            }

            const cacheKey = `f1tr_${story.dataset.title}`;
            const cached = sessionStorage.getItem(cacheKey);
            if (cached) {
                content.innerHTML = cached;
                translationBox.classList.remove('hidden');
                btn.classList.add('active');
                return;
            }

            btn.classList.add('loading');
            btn.innerHTML = '<i class="bi bi-hourglass-split"></i>';

            try {
                const fields = {
                    title: story.dataset.title || '',
                    summary: story.dataset.summary || '',
                    why: story.dataset.why || '',
                    impact: story.dataset.impact || '',
                    benefits: story.dataset.benefits || ''
                };

                const response = await fetch('/f1/translate', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ fields })
                });

                if (!response.ok) throw new Error('Translation failed');

                const translated = await response.json();
                const html = [
                    translated.title && `<p><strong>Titel</strong>${escapeHtml(translated.title)}</p>`,
                    translated.summary && `<p><strong>Zusammenfassung</strong>${escapeHtml(translated.summary)}</p>`,
                    translated.why && `<p><strong>Warum wichtig?</strong>${escapeHtml(translated.why)}</p>`,
                    translated.impact && `<p><strong>WM-Auswirkung</strong>${escapeHtml(translated.impact)}</p>`,
                    translated.benefits && `<p><strong>Wer profitiert?</strong>${escapeHtml(translated.benefits)}</p>`
                ].filter(Boolean).join('');

                content.innerHTML = html;
                sessionStorage.setItem(cacheKey, html);
                translationBox.classList.remove('hidden');
                btn.classList.add('active');
            } catch {
                content.innerHTML = '<p>Übersetzung fehlgeschlagen. Bitte später erneut versuchen.</p>';
                translationBox.classList.remove('hidden');
            } finally {
                btn.classList.remove('loading');
                btn.innerHTML = '<i class="bi bi-translate"></i> DE';
            }
        });
    });
}

function escapeHtml(str) {
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}
