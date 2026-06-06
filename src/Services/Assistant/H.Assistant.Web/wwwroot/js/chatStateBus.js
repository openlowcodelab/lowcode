window.ChatStateBus = {
    _refs: {},
    _cachedSessions: null,

    register(id, dotNetRef) {
        this._refs[id] = dotNetRef;
    },

    unregister(id) {
        delete this._refs[id];
    },

    getCachedSessions() {
        return this._cachedSessions;
    },

    setCachedSessions(sessions) {
        this._cachedSessions = sessions;
    },

    showToast(message, type) {
        // Inject toast CSS on first use
        if (!document.getElementById('chat-state-bus-styles')) {
            const style = document.createElement('style');
            style.id = 'chat-state-bus-styles';
            style.textContent = `
                .csb-toast {
                    position: fixed;
                    top: 24px;
                    left: 50%;
                    transform: translateX(-50%) translateY(-100%);
                    padding: 10px 24px;
                    border-radius: 6px;
                    font-size: 14px;
                    color: #fff;
                    z-index: 9999;
                    opacity: 0;
                    transition: all 0.3s ease;
                    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                    pointer-events: none;
                }
                .csb-toast-show {
                    transform: translateX(-50%) translateY(0);
                    opacity: 1;
                }
                .csb-toast-success { background: #52c41a; }
                .csb-toast-error { background: #ff4d4f; }
                .csb-toast-warning { background: #faad14; }
                .csb-toast-info { background: #595959; }
            `;
            document.head.appendChild(style);
        }

        // Remove existing toast
        const existing = document.getElementById('csb-toast');
        if (existing) {
            existing.remove();
        }

        // Create new toast
        const toast = document.createElement('div');
        toast.id = 'csb-toast';
        toast.className = `csb-toast csb-toast-${type || 'info'}`;
        toast.textContent = message;
        document.body.appendChild(toast);

        // Trigger animation
        requestAnimationFrame(() => {
            requestAnimationFrame(() => {
                toast.classList.add('csb-toast-show');
            });
        });

        // Auto-hide after 3 seconds
        setTimeout(() => {
            toast.classList.remove('csb-toast-show');
            toast.addEventListener('transitionend', () => toast.remove(), { once: true });
            // Fallback removal if transitionend doesn't fire
            setTimeout(() => { if (toast.parentNode) toast.remove(); }, 500);
        }, 3000);
    },

    notifySessionsChanged(id) {
        const ref = this._refs[id];
        if (ref) {
            ref.invokeMethodAsync('RefreshSessions');
        }
    },

    insertSessionAtTop(id, json) {
        const ref = this._refs[id];
        if (ref) {
            ref.invokeMethodAsync('InsertSessionAtTop', json);
        }
        // Update cache
        if (this._cachedSessions) {
            try {
                const session = JSON.parse(json);
                this._cachedSessions.unshift(session);
            } catch {}
        }
    },

    notifyChatSelected(id, guidStr) {
        const ref = this._refs[id];
        if (ref) {
            ref.invokeMethodAsync('OnChatSelected', guidStr);
        }
    }
};
