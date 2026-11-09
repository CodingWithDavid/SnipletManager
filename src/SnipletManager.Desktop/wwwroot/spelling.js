// Draws spelling underlines for fields wrapped in .spell-field and tracks the word under
// the pointer for the native context menu. Words are checked by the Windows spell checker
// in .NET because WebView2 does not check text that was loaded into a field.
window.snipletSpelling = (() => {
    const wordPattern = /[\p{L}\p{M}]+(?:['\u2019][\p{L}\p{M}]+)*/gu;
    const batchSize = 500;
    const maxCachedWords = 20000;
    const cache = new Map();
    const fields = new Map();
    let menuTarget = null;
    let menuOffset = -1;

    const isTextField = el =>
        el instanceof HTMLTextAreaElement ||
        (el instanceof HTMLInputElement && ["text", "search"].includes(el.type));

    const isSkippedChunk = chunk => /:\/\/|^www\.|@|[\/\\]|\p{L}\.\p{L}/u.test(chunk);

    function isCheckable(text, start, end) {
        const word = text.slice(start, end);
        if (word.length < 2 || word.length > 64 || /\p{Lu}/u.test(word.slice(1))) return false;
        if (/[\d_]/.test(text[start - 1] ?? "") || /[\d_]/.test(text[end] ?? "")) return false;
        let chunkStart = start;
        let chunkEnd = end;
        while (chunkStart > 0 && !/\s/.test(text[chunkStart - 1])) chunkStart--;
        while (chunkEnd < text.length && !/\s/.test(text[chunkEnd])) chunkEnd++;
        return !isSkippedChunk(text.slice(chunkStart, chunkEnd));
    }

    function tokens(text, from = 0, to = text.length) {
        const result = [];
        for (const match of text.slice(from, to).matchAll(wordPattern)) {
            const start = from + match.index;
            const end = start + match[0].length;
            if (isCheckable(text, start, end)) result.push({ start, end, word: match[0] });
        }
        return result;
    }

    function tokenAt(text, offset) {
        if (offset < 0 || offset > text.length) return null;
        const lineStart = text.lastIndexOf("\n", offset - 1) + 1;
        const lineEnd = text.indexOf("\n", offset);
        return tokens(text, lineStart, lineEnd < 0 ? text.length : lineEnd)
            .find(t => t.start <= offset && offset <= t.end) ?? null;
    }

    function syncLayout(el, mirror) {
        const style = getComputedStyle(el);
        for (const name of ["fontFamily", "fontSize", "fontWeight", "fontStyle", "fontKerning", "fontVariantLigatures",
            "fontFeatureSettings", "letterSpacing", "wordSpacing", "textTransform", "textIndent", "textAlign", "tabSize",
            "paddingTop", "paddingLeft", "paddingBottom", "borderTopWidth", "borderRightWidth", "borderBottomWidth",
            "borderLeftWidth", "boxSizing"]) {
            mirror.style[name] = style[name];
        }

        const isTextArea = el instanceof HTMLTextAreaElement;
        const borders = parseFloat(style.borderLeftWidth) + parseFloat(style.borderRightWidth);
        const scrollbar = Math.max(0, el.offsetWidth - el.clientWidth - borders);
        mirror.style.paddingRight = `${parseFloat(style.paddingRight) + scrollbar}px`;
        mirror.style.whiteSpace = isTextArea ? "pre-wrap" : "pre";
        mirror.style.overflowWrap = isTextArea ? "break-word" : "normal";
        mirror.style.lineHeight = isTextArea
            ? style.lineHeight
            : `${el.clientHeight - parseFloat(style.paddingTop) - parseFloat(style.paddingBottom)}px`;
        mirror.style.top = `${el.offsetTop}px`;
        mirror.style.left = `${el.offsetLeft}px`;
        mirror.style.width = `${el.offsetWidth}px`;
        mirror.style.height = `${el.offsetHeight}px`;
        mirror.scrollTop = el.scrollTop;
        mirror.scrollLeft = el.scrollLeft;
    }

    function render(el, state) {
        const text = el.value;
        const typingAt = document.activeElement === el && el.selectionStart === el.selectionEnd ? el.selectionStart : -1;
        const fragment = document.createDocumentFragment();
        let position = 0;
        for (const token of tokens(text)) {
            if (cache.get(token.word) !== true || token.end === typingAt) continue;
            fragment.append(text.slice(position, token.start));
            const mark = document.createElement("span");
            mark.className = "spell-error";
            mark.textContent = token.word;
            fragment.append(mark);
            position = token.end;
        }
        fragment.append(text.slice(position) + (text.endsWith("\n") ? " " : ""));
        state.mirror.replaceChildren(fragment);
        syncLayout(el, state.mirror);
    }

    async function check(el) {
        const state = fields.get(el);
        if (!state) return;
        state.timer = 0;
        const text = el.value;
        state.checked = text;
        const unknown = [...new Set(tokens(text).map(t => t.word).filter(w => !cache.has(w)))];
        try {
            for (let i = 0; i < unknown.length; i += batchSize) {
                const batch = unknown.slice(i, i + batchSize);
                const misspelled = new Set(await DotNet.invokeMethodAsync("SnipletManager.Desktop", "FindMisspelledWords", batch));
                if (cache.size > maxCachedWords) cache.clear();
                for (const word of batch) cache.set(word, misspelled.has(word));
            }
        } catch {
            state.checked = null;
            return;
        }
        if (fields.get(el) === state && el.value === text) render(el, state);
    }

    function schedule(el, delay) {
        const state = fields.get(el);
        if (!state) return;
        clearTimeout(state.timer);
        state.timer = setTimeout(() => check(el), delay);
    }

    function register(wrapper) {
        const el = wrapper.querySelector(".form-control");
        const mirror = wrapper.querySelector(".spell-mirror");
        if (!el || !mirror || fields.has(el)) return;
        const state = { mirror, checked: null, timer: 0, observer: new ResizeObserver(() => syncLayout(el, mirror)) };
        fields.set(el, state);
        state.observer.observe(el);
        el.addEventListener("scroll", () => { mirror.scrollTop = el.scrollTop; mirror.scrollLeft = el.scrollLeft; });
        el.addEventListener("input", () => { render(el, state); schedule(el, 300); });
        el.addEventListener("blur", () => render(el, state));
        schedule(el, 0);
    }

    // Blazor can replace field values (opening the editor, pasting from the clipboard)
    // without raising input events, so poll for new fields and changed values.
    setInterval(() => {
        document.querySelectorAll(".spell-field").forEach(register);
        for (const [el, state] of fields) {
            if (!el.isConnected) {
                state.observer.disconnect();
                clearTimeout(state.timer);
                fields.delete(el);
            } else if (!state.timer && el.value !== state.checked) {
                schedule(el, 0);
            }
        }
    }, 400);

    document.addEventListener("contextmenu", e => {
        menuTarget = isTextField(e.target) ? e.target : null;
        menuOffset = -1;
        if (!menuTarget) return;
        const position = document.caretPositionFromPoint?.(e.clientX, e.clientY);
        menuOffset = position && position.offsetNode === menuTarget ? position.offset : menuTarget.selectionStart;
    }, true);

    return {
        wordAtMenu() {
            if (!menuTarget || !menuTarget.isConnected || menuTarget.disabled || menuTarget.readOnly) return null;
            return tokenAt(menuTarget.value, menuOffset)?.word ?? null;
        },
        replaceWord(original, replacement) {
            if (!menuTarget || !menuTarget.isConnected) return false;
            const token = tokenAt(menuTarget.value, menuOffset);
            if (!token || token.word !== original) return false;
            menuTarget.focus();
            menuTarget.setRangeText(replacement, token.start, token.end, "end");
            menuTarget.dispatchEvent(new Event("input", { bubbles: true }));
            menuTarget.dispatchEvent(new Event("change", { bubbles: true }));
            return true;
        }
    };
})();
