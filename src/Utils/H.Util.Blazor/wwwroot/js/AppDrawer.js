// AppDrawer 事件处理
window.AppDrawer = {
    _dotNetHelper: null,

    init: function (dotNetHelper) {
        this._dotNetHelper = dotNetHelper;
        this.bindEvents();
    },

    bindEvents: function () {
        if (!this._dotNetHelper) {
            return;
        }

        // 关闭按钮
        const closeBtn = document.querySelector('.app-drawer .close-drawer');
        if (closeBtn) {
            closeBtn.onclick = () => {
                this._dotNetHelper.invokeMethodAsync('HandleClose')
                    .catch(err => console.error('AppDrawer: HandleClose failed', err));
            };
        } else {
        }

        // 遮罩层
        const overlay = document.querySelector('.app-drawer-overlay');
        if (overlay) {
            overlay.onclick = () => {
                this._dotNetHelper.invokeMethodAsync('HandleOverlayClick')
                    .catch(err => console.error('AppDrawer: HandleOverlayClick failed', err));
            };
        } else {
        }

        // 应用项
        const appItems = document.querySelectorAll('.app-item');
        appItems.forEach((item, index) => {
            const appId = item.getAttribute('data-app-id');
            item.onclick = () => {
                this._dotNetHelper.invokeMethodAsync('HandleAppClickById', appId)
                    .catch(err => console.error('AppDrawer: HandleAppClickById failed', err));
            };
        });
    }
};
