// Office 文件在线预览封装模块（docx / xlsx / pptx）
// 按需动态加载对应的第三方预览库，避免在应用启动时加载大体积脚本
const base = './_content/H.File.Web/js/';

const loadedScripts = {};

function loadScript(src) {
    return new Promise((resolve, reject) => {
        if (loadedScripts[src]) {
            resolve();
            return;
        }
        const s = document.createElement('script');
        s.src = src;
        s.onload = () => {
            loadedScripts[src] = true;
            resolve();
        };
        s.onerror = () => reject(new Error('预览组件加载失败: ' + src));
        document.head.appendChild(s);
    });
}

function base64ToUint8Array(base64) {
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }
    return bytes;
}

export async function previewOffice(base64, fileName, containerId) {
    const container = document.getElementById(containerId);
    if (!container) {
        throw new Error('预览容器不存在');
    }
    container.innerHTML = '';
    container.scrollTop = 0;

    const ext = (fileName.split('.').pop() || '').toLowerCase();
    const data = base64ToUint8Array(base64);

    if (ext === 'docx') {
        await loadScript(base + 'jszip.min.js');
        await loadScript(base + 'docx-preview.min.js');
        if (!window.docx) {
            throw new Error('docx 预览组件加载失败');
        }
        await window.docx.renderAsync(data, container, null, {
            className: 'docx',
            inWrapper: true,
            breakPages: true,
            ignoreWidth: false,
            ignoreHeight: false,
            useBase64URL: true
        });
    } else if (ext === 'xlsx' || ext === 'xls') {
        await loadScript(base + 'xlsx.full.min.js');
        if (!window.XLSX) {
            throw new Error('xlsx 预览组件加载失败');
        }
        const wb = window.XLSX.read(data, { type: 'array' });
        const firstSheet = wb.SheetNames[0];
        container.innerHTML = window.XLSX.utils.sheet_to_html(wb.Sheets[firstSheet], { id: 'file-sheet', editable: false });
    } else if (ext === 'pptx') {
        await loadScript(base + 'pptx-preview.umd.js');
        if (!window.pptxPreview) {
            throw new Error('pptx 预览组件加载失败');
        }
        const viewer = window.pptxPreview.init(container, { width: 900, height: 506 });
        await viewer.preview(data.buffer);
    } else {
        throw new Error('不支持的文件类型: ' + fileName);
    }
    return { ok: true };
}
