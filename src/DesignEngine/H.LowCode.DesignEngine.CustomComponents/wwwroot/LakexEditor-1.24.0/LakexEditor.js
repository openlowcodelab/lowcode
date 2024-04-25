export function initLakexEditor() {
    const reactJs = "_content/H.LowCode.DesignEngine.CustomComponents/react-18/react.production.min.js";
    const reactDomJs = "_content/H.LowCode.DesignEngine.CustomComponents/react-18/react-dom.production.min.js";
    const lakexJs = "_content/H.LowCode.DesignEngine.CustomComponents/LakexEditor-1.24.0/doc.umd.js";

    //使用 JSRuntime.InvokeAsync 代替，此种方式亦有效
    //loadScript(reactJs);
    //loadScript(reactDomJs);
    //loadScript(lakexJs, function () {
        initEditor();
    //});
}

function initEditor() {
    const { createOpenEditor } = window.Doc;
    // 创建编辑器
    const editor = createOpenEditor(document.getElementById('lakexEditor'), {
        input: {},
        image: {
            isCaptureImageURL() {
                return false;
            },
        },
    });
    // 设置内容
    editor.setDocument('text/lake', '<p><span style="color: rgb(255, 111, 4),rgb(243, 48, 171)">欢迎来到yuque编辑器</span></p>');
    // 监听内容变动
    editor.on('contentchange', () => {
        console.info(editor.getDocument('text/lake'));
    });
}

function loadScript(url, callback) {
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