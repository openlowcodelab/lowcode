---
kind: frontend_style
name: Blazor + AntDesign 主题化前端样式体系
category: frontend_style
scope:
    - '**'
source_files:
    - src/LowCode/RenderEngine/H.LowCode.Themes.AntBlazor/AntBlazorThemeModule.cs
    - src/LowCode/RenderEngine/H.LowCode.Themes.AntBlazor/Layout/AntBlazorThemeLayout.razor
    - src/Components/AppDrawer/H.AppDrawer.Components/wwwroot/css/AppDrawer.css
    - src/Components/AppDrawer/H.AppDrawer.Components/Components/DefaultLayoutComponent.razor
    - src/Host/H.AppLab.Host.All/H.AppLab.Host.All/Components/App.razor
    - src/LowCode/Common/H.LowCode.Components.Defaults/ExtraComponents/LcLakexEditor.razor
---

AppLab 平台采用 .NET Blazor WebAssembly 作为前端框架，基于 ABP Framework 模块化架构，前端样式体系围绕 AntDesign (AntBlazor) 组件库构建，形成可插拔的主题化渲染引擎。

**核心样式系统：**
- **主题包模式**：通过 `H.LowCode.Themes.AntBlazor` 主题包实现样式隔离与替换，支持运行时懒加载主题资源（`BlazorWebAssemblyLazyLoad`）
- **CSS 组织方式**：各组件包在 `wwwroot/css/` 目录下维护独立样式文件，如 `AppDrawer.css`、`h-components.css`、`renderengine.css` 等
- **样式引用策略**：通过 Razor 组件中的 `<link>` 标签按需引入样式，使用 `@Assets["..."]` 辅助方法解析静态资源路径

**设计系统与组件库：**
- **主 UI 框架**：AntDesign (AntBlazor) 提供企业级组件，包括表单、表格、布局等基础控件
- **自定义组件库**：`H.Util.Blazor` 封装通用业务组件，统一样式规范（`h-components.css`）
- **低代码组件**：`H.LowCode.Components.Defaults` 内置 36+ 个默认组件，支持 LaTeX、Monaco 编辑器等扩展
- **第三方样式集成**：集成 antd-4.24.13、LakexEditor 等外部样式资源

**布局与响应式策略：**
- **全局布局**：`DefaultLayoutComponent` 提供顶部导航栏 + 侧边菜单 + 内容区的标准布局结构
- **CSS 变量系统**：使用 CSS 自定义属性（如 `--h-bg-page: #F2F3F5`）管理主题色值
- **响应式设计**：通过 `@media` 查询适配移动端，最小断点 768px
- **动画效果**：统一的淡入淡出、滑入动画，提升用户体验

**样式开发约定：**
- 组件样式采用 BEM 命名规范（如 `.app-drawer-overlay`、`.drawer-header`）
- 颜色体系：主色调 `#165DFF`，背景色 `#F2F3F5`，文字色 `#1D2129` / `#86909C`
- 间距规范：统一使用 4px 倍数（8px、12px、16px、20px、24px）
- 阴影系统：使用 `box-shadow` 营造层次感，如 `6px 0 24px rgba(29, 33, 41, 0.12)`
- 圆角规范：统一使用 4px、8px 圆角，保持视觉一致性

**主题切换机制：**
- 通过 `ThemePartLayoutBase` 基类实现主题继承
- 支持运行时动态加载不同主题包的 CSS 和组件渲染逻辑
- 预留了主题配置接口，便于后续扩展多套视觉风格