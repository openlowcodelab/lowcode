export function initMonacoEditor(containerId, code, language, theme) {
    const loaderUrl = "_content/H.LowCode.Components.Extension/MonacoEditor/loader.min.js";

    loadScript(loaderUrl, function () {
        //如果cdn加载速度较快的话,更推荐使用cdn
        require.config({ paths: { 'vs': 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.30.1/min/vs' } });
        //require.config({ paths: { 'vs': '_content/H.LowCode.Components.Extension/MonacoEditor' } });

        require(['vs/editor/editor.main'], function () {
            // 初始化编辑器
            var editor = monaco.editor.create(document.getElementById(containerId), {
                value: code,
                language: language,
                theme: theme // 可选择其他主题，如'vs'为默认的浅色主题
            });
        });
    });
};

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