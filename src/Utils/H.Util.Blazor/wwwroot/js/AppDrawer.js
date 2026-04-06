// AppDrawer 事件处理
window.AppDrawer = {
    _dotNetHelper: null,

    init: function (dotNetHelper) {
        this._dotNetHelper = dotNetHelper;
        this.bindEvents();
    },

    bindEvents: function () {
        if (!this._dotNetHelper) {
            console.warn('AppDrawer: dotNetHelper not initialized');
            return;
        }

        console.log('AppDrawer: Binding events...');

        // 关闭按钮
        const closeBtn = document.querySelector('.app-drawer .close-drawer');
        if (closeBtn) {
            console.log('AppDrawer: Close button found');
            closeBtn.onclick = () => {
                console.log('AppDrawer: Close button clicked');
                this._dotNetHelper.invokeMethodAsync('HandleClose')
                    .catch(err => console.error('AppDrawer: HandleClose failed', err));
            };
        } else {
            console.warn('AppDrawer: Close button NOT found');
        }

        // 遮罩层
        const overlay = document.querySelector('.app-drawer-overlay');
        if (overlay) {
            console.log('AppDrawer: Overlay found');
            overlay.onclick = () => {
                console.log('AppDrawer: Overlay clicked');
                this._dotNetHelper.invokeMethodAsync('HandleOverlayClick')
                    .catch(err => console.error('AppDrawer: HandleOverlayClick failed', err));
            };
        } else {
            console.warn('AppDrawer: Overlay NOT found');
        }

        // 应用项
        const appItems = document.querySelectorAll('.app-item');
        console.log(`AppDrawer: Found ${appItems.length} app items`);
        appItems.forEach((item, index) => {
            const appId = item.getAttribute('data-app-id');
            console.log(`AppDrawer: Binding app item ${index} - ${appId}`);
            item.onclick = () => {
                console.log(`AppDrawer: App item clicked - ${appId}`);
                this._dotNetHelper.invokeMethodAsync('HandleAppClickById', appId)
                    .catch(err => console.error('AppDrawer: HandleAppClickById failed', err));
            };
        });
    }
};
