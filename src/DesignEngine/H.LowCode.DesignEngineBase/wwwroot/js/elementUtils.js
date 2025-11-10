window.elementUtils = {
    // 获取元素尺寸信息，包括margin
    getDimensions: function (element) {
        if (!element) return null;
        
        const rect = element.getBoundingClientRect();
        const computedStyle = window.getComputedStyle(element);
        const containerWidth = element.parentElement ? element.parentElement.getBoundingClientRect().width : 0;
        
        // 计算实际尺寸（包括margin）
        const margin = {
            top: parseFloat(computedStyle.marginTop),
            right: parseFloat(computedStyle.marginRight),
            bottom: parseFloat(computedStyle.marginBottom),
            left: parseFloat(computedStyle.marginLeft)
        };
        
        return {
            width: rect.width,
            height: rect.height,
            actualWidth: rect.width + margin.left + margin.right,
            actualHeight: rect.height + margin.top + margin.bottom,
            containerWidth: containerWidth,
            margin: margin,
            offsetTop: rect.top,
            offsetLeft: rect.left
        };
    },
    
    // 计算容器信息
    getContainerInfo: function (element) {
        if (!element || !element.parentElement) return null;
        
        const container = element.parentElement;
        const containerRect = container.getBoundingClientRect();
        const computedStyle = window.getComputedStyle(container);
        
        return {
            width: containerRect.width,
            height: containerRect.height,
            padding: {
                top: parseFloat(computedStyle.paddingTop),
                right: parseFloat(computedStyle.paddingRight),
                bottom: parseFloat(computedStyle.paddingBottom),
                left: parseFloat(computedStyle.paddingLeft)
            }
        };
    }
    ,
    // 获取鼠标下方的组件 id（忽略当前拖拽组件）
    getComponentIdByPoint: function (x, y, ignoreId) {
        try {
            const elements = document.elementsFromPoint(x, y);
            for (const el of elements) {
                if (!el || !el.classList) continue;
                if (el.classList.contains('draggableitem-box')) {
                    const id = el.getAttribute('data-component-id') || (el.dataset ? el.dataset.componentId : null);
                    if (id && id !== ignoreId) {
                        return id;
                    }
                }
            }
        } catch (e) {
            // ignore
        }
        return null;
    }
    ,
    // 为元素绑定 dragstart 事件，使用"整个 DraggableItem"作为拖拽图像
    attachDragImage: function (element, label) {
        if (!element) return;
        try {
            element.addEventListener('dragstart', function (e) {
                try {
                    // 1) 生成跟随鼠标的高对比覆盖层（而不是使用浏览器半透明的默认拖拽图像）
                    const target = element.querySelector('.draggableitem') || element;
                    const rect = target.getBoundingClientRect();

                    // 克隆原始元素以保持所有样式
                    const clone = target.cloneNode(true);
                    clone.style.width = '100%';
                    clone.style.height = '100%';
                    
                    // 创建一个包裹容器来增强视觉效果
                    const overlay = document.createElement('div');
                    overlay.style.cssText = [
                        'position:fixed',
                        'top:0',
                        'left:0',
                        'transform:translate(-9999px,-9999px)',
                        'will-change:transform',
                        'pointer-events:none',
                        'z-index:2147483647',
                        'width:' + rect.width + 'px',
                        'height:' + rect.height + 'px',
                        'background:transparent',
                        'border: 1px solid #409eff',  // 添加清晰的蓝色边框
                        'border-radius: 4px',
                        'opacity: 0.6',  // 调整透明度使内容更清晰
                        'box-shadow: 0 8px 16px rgba(0,0,0,0.2)',  // 添加阴影增强视觉效果
                        'filter: contrast(1.1) saturate(1.05)',  // 增强对比度和饱和度
                        '-webkit-font-smoothing: antialiased',  // 优化文字清晰度
                        '-moz-osx-font-smoothing: grayscale',
                        'text-rendering: optimizeLegibility'
                    ].join(';');
                    overlay.appendChild(clone);
                    document.body.appendChild(overlay);

                    // 特别优化克隆元素中的文字和图标清晰度
                    const texts = overlay.querySelectorAll('span, div, p, h1, h2, h3, h4, h5, h6, label, .ant-typography');
                    for (let i = 0; i < texts.length; i++) {
                        const textEl = texts[i];
                        textEl.style.webkitFontSmoothing = 'antialiased';
                        textEl.style.mozOsxFontSmoothing = 'grayscale';
                        textEl.style.textRendering = 'optimizeLegibility';
                        textEl.style.filter = 'contrast(1.15) saturate(1.1)';
                    }
                    
                    // 优化图标清晰度
                    const icons = overlay.querySelectorAll('.anticon, .icon, svg, i');
                    for (let i = 0; i < icons.length; i++) {
                        const iconEl = icons[i];
                        iconEl.style.webkitFontSmoothing = 'antialiased';
                        iconEl.style.mozOsxFontSmoothing = 'grayscale';
                        iconEl.style.filter = 'contrast(1.2) saturate(1.15)';
                    }

                    // 记录鼠标相对于组件中心的偏移，居中效果更直观
                    const offsetX = rect.width / 2;
                    const offsetY = rect.height / 2;
                    element.__dragOverlay = overlay;
                    element.__dragOffsetX = offsetX;
                    element.__dragOffsetY = offsetY;

                    // 设置 DataTransfer，提升不同浏览器/模式下的拖拽兼容性（WASM 下尤为重要）
                    if (e.dataTransfer) {
                        try {
                            e.dataTransfer.effectAllowed = 'move';
                            e.dataTransfer.dropEffect = 'move';
                            const id = element.getAttribute && element.getAttribute('data-component-id');
                            e.dataTransfer.setData('text/plain', (label || id || 'drag'));
                        } catch {}
                    }

                    // 2) 用一个 1x1 的透明元素作为 drag image，避免浏览器默认半透明图像影响清晰度
                    const tiny = document.createElement('div');
                    tiny.style.cssText = 'width:1px;height:1px;opacity:0;position:absolute;top:-9999px;left:-9999px';
                    document.body.appendChild(tiny);
                    if (e.dataTransfer && e.dataTransfer.setDragImage) {
                        e.dataTransfer.setDragImage(tiny, 0, 0);
                    }
                    setTimeout(function () { try { document.body.removeChild(tiny); } catch {} }, 0);

                    // 3) 跟随鼠标：在 WASM 模式下增加多事件回退以保证坐标获取
                    let raf = 0;
                    const getPoint = function (ev) {
                        try {
                            if (ev && ev.touches && ev.touches.length) {
                                return { x: ev.touches[0].clientX, y: ev.touches[0].clientY };
                            }
                            let x = typeof ev.clientX === 'number' ? ev.clientX : 0;
                            let y = typeof ev.clientY === 'number' ? ev.clientY : 0;
                            if ((!x && !y) && typeof ev.pageX === 'number' && typeof ev.pageY === 'number') {
                                const doc = document.documentElement || { scrollLeft: 0, scrollTop: 0 };
                                const body = document.body || { scrollLeft: 0, scrollTop: 0 };
                                const sl = doc.scrollLeft || body.scrollLeft || 0;
                                const st = doc.scrollTop || body.scrollTop || 0;
                                x = ev.pageX - sl;
                                y = ev.pageY - st;
                            }
                            return { x: x || 0, y: y || 0 };
                        } catch { return { x: 0, y: 0 }; }
                    };
                    const updateOverlay = function (ev) {
                        try {
                            if (!element.__dragOverlay) return;
                            if (raf) return;
                            raf = requestAnimationFrame(function () {
                                raf = 0;
                                const p = getPoint(ev);
                                const tx = p.x - element.__dragOffsetX;
                                const ty = p.y - element.__dragOffsetY;
                                element.__dragOverlay.style.transform = 'translate(' + tx + 'px,' + ty + 'px)';
                            });
                        } catch {}
                    };
                    const onDragOver = function (ev) { updateOverlay(ev); };
                    const onMouseMove = function (ev) { updateOverlay(ev); };
                    const onTouchMove = function (ev) { updateOverlay(ev); };
                    const onDragEnd = function () {
                        try {
                            window.removeEventListener('dragover', onDragOver, true);
                            document.removeEventListener('dragover', onDragOver, true);
                            element.removeEventListener('drag', onDragOver, true);
                            window.removeEventListener('mousemove', onMouseMove, true);
                            window.removeEventListener('touchmove', onTouchMove, true);
                            if (element.__dragOverlay && element.__dragOverlay.parentNode) {
                                element.__dragOverlay.parentNode.removeChild(element.__dragOverlay);
                            }
                            element.__dragOverlay = null;
                        } catch {}
                    };
                    // 优先使用 dragover，其次使用 document/window 的 mousemove/touchmove 作为回退
                    window.addEventListener('dragover', onDragOver, true);
                    document.addEventListener('dragover', onDragOver, true);
                    element.addEventListener('drag', onDragOver, true);
                    window.addEventListener('mousemove', onMouseMove, true);
                    window.addEventListener('touchmove', onTouchMove, { passive: true, capture: true });

                    // 始终允许页面上的 drop 行为，防止某些浏览器在未允许 drop 时过早结束拖拽
                    const dragoverAllow = function (ev) { try { if (ev && ev.preventDefault) ev.preventDefault(); if (ev && ev.dataTransfer) ev.dataTransfer.dropEffect = 'move'; } catch {} };
                    document.addEventListener('dragover', dragoverAllow, true);
                    document.addEventListener('dragenter', dragoverAllow, true);
                    document.addEventListener('drop', dragoverAllow, true);

                    window.addEventListener('dragend', function(){
                        try {
                            document.removeEventListener('dragover', dragoverAllow, true);
                            document.removeEventListener('dragenter', dragoverAllow, true);
                            document.removeEventListener('drop', dragoverAllow, true);
                        } catch {}
                        onDragEnd();
                    }, { once: true });
                    window.addEventListener('drop', function(){
                        try {
                            document.removeEventListener('dragover', dragoverAllow, true);
                            document.removeEventListener('dragenter', dragoverAllow, true);
                            document.removeEventListener('drop', dragoverAllow, true);
                        } catch {}
                        onDragEnd();
                    }, { once: true });
                } catch {}
            }, { passive: true });
        } catch {}
    }
};