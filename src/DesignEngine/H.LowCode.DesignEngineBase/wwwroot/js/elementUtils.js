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
};