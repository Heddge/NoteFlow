(function (global) {
    const STORAGE_KEY = 'noteFlow.uiSettings';
    const DEFAULTS = Object.freeze({
        theme: 'dark',
        font: 'inter',
        editorSidebarVisible: true
    });

    function normalize(input) {
        const source = input && typeof input === 'object' ? input : {};
        const theme = source.theme === 'light' ? 'light' : DEFAULTS.theme;
        const font = ['inter', 'manrope', 'plex'].includes(source.font) ? source.font : DEFAULTS.font;
        const editorSidebarVisible = typeof source.editorSidebarVisible === 'boolean'
            ? source.editorSidebarVisible
            : DEFAULTS.editorSidebarVisible;

        return {
            theme,
            font,
            editorSidebarVisible
        };
    }

    function read() {
        try {
            const raw = global.localStorage.getItem(STORAGE_KEY);
            return normalize(raw ? JSON.parse(raw) : {});
        } catch {
            return normalize({});
        }
    }

    function apply(settings) {
        const normalized = normalize(settings);
        const root = global.document.documentElement;

        root.dataset.theme = normalized.theme;
        root.dataset.font = normalized.font;
        root.dataset.editorSidebarVisible = normalized.editorSidebarVisible ? 'true' : 'false';

        return normalized;
    }

    function write(partialSettings) {
        const nextSettings = normalize({
            ...read(),
            ...(partialSettings && typeof partialSettings === 'object' ? partialSettings : {})
        });

        try {
            global.localStorage.setItem(STORAGE_KEY, JSON.stringify(nextSettings));
        } catch {
            // Ignore storage write failures and still apply in-memory settings.
        }

        return apply(nextSettings);
    }

    function boot() {
        return apply(read());
    }

    global.NoteFlowUiSettings = {
        boot,
        read,
        write,
        apply,
        defaults: DEFAULTS
    };
})(window);
