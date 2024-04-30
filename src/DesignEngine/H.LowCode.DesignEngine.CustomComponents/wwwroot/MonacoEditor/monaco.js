export function initMonacoEditor() {
    const js1 = "_content/BlazorMonaco/jsInterop.js";
    const js2 = "_content/BlazorMonaco/lib/monaco-editor/min/vs/loader.js";
    const js3 = "_content/BlazorMonaco/lib/monaco-editor/min/vs/editor/editor.main.js";

    loadScript(js1, function () {
        loadScript(js2, function () {
            loadScript(js3);
        });
    });
}

function loadScript(url, callback) {
    if (document.querySelector('[src$="' + url + '"]')) {
        if (typeof callback === 'function') {
            callback();
        }
        return;
    }

    const script = document.createElement('script');
    script.type = 'text/javascript';
    script.src = url;

    script.onload = function () {
        if (typeof callback === 'function') {
            callback();
        }
    }

    const jsMark = document.querySelector("script");
    if (jsMark) {
        jsMark.after(script);
    }
    else {
        document.body.appendChild(script);
    }
}