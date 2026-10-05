## 1. 系统/方案概述

本仓库为 .NET + Blazor 应用，**未引入任何第三方 UI 框架（AntDesign、Bootstrap、Tailwind 等）**。所有前端样式采用 **纯 CSS + Blazor Razor 组件 scoped CSS** 的方式实现，并围绕一个自研的轻量级 CSS 组件库构建视觉一致性。

- **设计语言**: 代码注释中明确标注对齐 **Arco Design Pro**（主色 `#165DFF`、圆角 `4px`、边框 `#E5E6EB`），在 `h-components.css` 与 `lc-components.css` 的头部注释中均有声明。
- **主题化方式**: 使用 CSS `:root` 自定义属性作为 **design tokens**，按不同作用域分别定义（`--h-*` 与 `--hc-*`）。
- **桌面端**: 通过 Avalonia (`H.AppLab.Desktop`) 独立实现，与 Web 端样式无关。

## 2. 关键文件

| 文件 | 职责 |
|---|---|
| `src/Utils/H.Util.Blazor/wwwroot/css/h-components.css` | 全局公共样式与通用组件（Button、Input、Card、Layout、Split Page、Card Grid 等），Token 源 `--h-*` |
| `src/LowCode/Common/H.LowCode.Components/wwwroot/lc-components.css` | 低代码渲染引擎使用的 Hc 组件库（hc-btn / hc-input / hc-select / hc-tabs / hc-tag / hc-alert / hc-descriptions 等），Token 源 `--hc-*` |
| `src/Components/AppDrawer/H.AppDrawer.Components/wwwroot/css/AppDrawer.css` | AppDrawer 抽屉组件样式 |
| `src/Host/Account/H.Account.Host/wwwroot/app.css` | Account Host 基础样式（字体、错误边界、验证颜色） |
| 各 `.razor.css` 文件 | Blazor 组件级 scoped CSS（如 `DraggableContainer.razor.css`、`PageSetting.razor.css`、`ApprovalLayout.razor.css` 等） |
| `src/LowCode/DesignEngine/H.LowCode.DesignEngine/wwwroot/designengine.css` | 设计器宿主样式入口 |
| `src/LowCode/RenderEngine/H.LowCode.RenderEngine/wwwroot/renderengine.css` | 渲染器宿主样式入口 |
| `src/LowCode/DesignEngine/H.LowCode.PartsDesignEngine/wwwroot/partsdesignengine.css` | 部件设计器样式入口 |
| `src/LowCode/DesignEngine/H.LowCode.MyApp/wwwroot/myapp.css` | 示例 MyApp 样式入口 |

## 3. 架构与约定

### 3.1 Token 分层
两个独立的 CSS 变量命名空间并存：
- `--h-*`：公用工具库 `H.Util.Blazor`，面向业务页面（`--h-primary`、`--h-border`、`--h-radius`、`--h-text`、`--h-shadow-*` 等）。
- `--hc-*`：低代码组件库 `H.LowCode.Components`，面向设计器/渲染器生成的组件（`--hc-primary`、`--hc-border`、`--hc-text-placeholder` 等）。
两者在主色 `#165DFF`、边框色 `#E5E6EB`、文本色 `#1D2129` 上保持一致，以维持跨模块视觉统一。

### 3.2 组件命名约定
- 公用组件类名统一以 `h-` 前缀（`h-btn`、`h-card`、`h-layout`、`h-split-page`、`h-card-grid`、`page-container`、`page-fill`）。
- 低代码组件库统一以 `hc-` 前缀（`hc-btn`、`hc-input`、`hc-select`、`hc-tabs`、`hc-tag`、`hc-alert`、`hc-descriptions`、`hc-statistic`）。
- AppDrawer 使用无前缀的语义类名（`.app-drawer-overlay`、`.drawer-header`、`.category-title`、`.app-item`）。

### 3.3 Scoped CSS 的使用
每个 `.razor` 组件可附带同名 `.razor.css` 文件（例如 `DraggableContainer.razor.css`、`PageSetting.razor.css`、`AddNodeDialog.razor.css`、`HModal.razor.css`、`HCard.razor.css`）。编译后由 Blazor 生成 `.rz.scp.css` 文件进行作用域隔离。

### 3.4 布局约定
- 页面容器：`page-container`（`width:80%; margin:0 auto`）、`page-fill`（白底卡片，带圆角与阴影）。
- 左右分栏：`h-split-page` + `h-card` 等高卡片网格，配合 `gap:2px` 形成灰色间隙效果。
- 栅格：提供 `hc-row` / `hc-col` 简易 flex 栅格，以及 `h-card-grid` 等高卡片栅格。
- 侧边布局：`hc-layout` + `hc-sider` + `hc-content` 三段式 Flex 布局。

### 3.5 颜色与状态
- 成功态：`#00B42A`；危险态：`#F53F3F`；警告态：`#FF7D00`。
- hover 状态普遍通过 `transition: all 0.2s` 或指定 `border-color` / `color` 过渡。
- 禁用态统一使用 `opacity: 0.5; cursor: not-allowed`。

## 4. 约定与约束

- **设计令牌来源唯一**：`h-components.css` 顶部注释明确声明“设计令牌 (Design Tokens) - 全局统一样式源”，如需切换主题色“仅修改 `--h-primary` 系列即可全局生效”。该注释即为主题变更的约定依据。
- **设计语言对齐 Arco Design Pro**：`h-components.css` 与 `lc-components.css` 的头部注释均声明设计语言为 Arco Design Pro，主色为 `#165DFF`，圆角 `4px`，边框 `#E5E6EB`。这是跨模块视觉一致性的约定来源。
- **组件库命名前缀不可混用**：公共组件使用 `h-`，低代码组件使用 `hc-`，二者 token 也各自独立（`--h-*` vs `--hc-*`），避免样式污染。
- **无第三方 UI 依赖**：经检查 `Directory.Packages.props` 及所有 `*.csproj`，未发现 AntDesign、Bootstrap、Tailwind 等包引用；UI 完全由自研 CSS 组件构成。
- **Scoped CSS 用于组件级样式**：Razor 组件与其同名 `.razor.css` 成对出现，由 Blazor 自动处理作用域，无需手动加 hash 类名。
- **响应式策略**：未见媒体查询或 Tailwind 断点；样式以固定宽度为主（如 AppDrawer `width:450px`），未观察到统一的响应式规范。
- **桌面端独立**：Avalonia 桌面客户端位于 `src/Host/H.AppLab.Desktop`，其样式通过 XAML Theme 管理，与 Web 端的 CSS 体系完全分离。