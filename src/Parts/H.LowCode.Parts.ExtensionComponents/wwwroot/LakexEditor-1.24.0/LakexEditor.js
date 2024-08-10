export function initLakexEditor(lakexId) {
    const reactJs = "_content/H.LowCode.Parts.ExtensionComponents/react-18/react.production.min.js";
    const reactDomJs = "_content/H.LowCode.Parts.ExtensionComponents/react-18/react-dom.production.min.js";
    const lakexJs = "_content/H.LowCode.Parts.ExtensionComponents/LakexEditor-1.24.0/doc.umd.js";

    loadScript(reactJs, function () {
        loadScript(reactDomJs, function () {
            loadScript(lakexJs, function () {
                initEditor(lakexId);
            });
        });
    });
}

function initEditor(lakexId) {
    const { createOpenEditor } = window.Doc;
    // 创建编辑器
    const editor = createOpenEditor(document.getElementById(lakexId), {
        input: {},
        image: {
            isCaptureImageURL() {
                return false;
            },
        },
    });
    var text = '<p><span style="color: rgb(255, 111, 4),rgb(243, 48, 171)">Welcome LakexEditor</span></p>'
    // 设置内容
    editor.setDocument('text/lake', text);
    // 监听内容变动
    editor.on('contentchange', () => {
        console.info(editor.getDocument('text/lake'));
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