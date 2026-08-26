# 设计器 DesignPage 功能完善与美化

## 背景与根因
- 组件元数据(`src/LowCode/meta/parts/componentParts/**/*.json`)的 `frag.dt` 仍指向已从解决方案移除的 AntDesign 类型(如 `AntDesign.Input\`1[System.String], AntDesign`)。拖入/单击都会把组件加入模型，但渲染时 `Type.GetType(frag.dt, true)`(位于 [DynamicComponentBase.cs:37](file:///d:/H/code/my/applab/src/LowCode/DesignEngine/H.LowCode.DesignEngine/Services/DynamicComponentBase.cs#L37))抛 `TypeLoadException` → 组件不显示(Bug1)。渲染引擎 [ComponentRender.razor](file:///d:/H/code/my/applab/src/LowCode/RenderEngine/H.LowCode.RenderEngine/ComponentRender/ComponentRender.razor) 走同一机制，同样受影响。
- Bug2(画布内拖拽无效)当前被 Bug1 掩盖(无组件可拖)；且画布项 `draggable="@Component.DesignState.IsSelected"`,需先选中才能拖。修复 Bug1 后再定位。
- 属性通过反射 `GetProperty(名称)` 绑定，未知属性会被静默跳过(见 [LowCodeDynamicComponentBase.cs](file:///d:/H/code/my/applab/src/LowCode/Common/H.LowCode.ComponentBase/LowCodeDynamicComponentBase.cs#L45-L47))，故自定义组件只需实现需要生效的参数名即可，不会因多余属性报错。

## 方案概述
在已存在的 Razor 库 `H.LowCode.Components` 中新建一套原生(纯 HTML+CSS，现代扁平风)低代码组件，改写全部 meta JSON 的类型名指向它们；不改渲染管线(设计器与渲染引擎都靠 `frag.dt` 反射解析，改数据即可)。

## Phase 1 — 原生组件库与解析链路(修复 Bug1 核心)
- 组件位置/命名：`H.LowCode.Components`,命名空间 `H.LowCode.Components`,前缀 `Hc`。
- 工程接线：
  - 在 [H.LowCode.DesignEngine.csproj](file:///d:/H/code/my/applab/src/LowCode/DesignEngine/H.LowCode.DesignEngine/H.LowCode.DesignEngine.csproj) 增加对 `H.LowCode.Components` 的 ProjectReference(渲染引擎侧已引用)。
  - 在两个 WASM 宿主客户端(`H.AppLab.Host.All.Client`、`H.LowCode.RenderEngine.Host.Client`)的 csproj 增加 `<TrimmerRootAssembly Include="H.LowCode.Components" />`,避免 Release/AOT 裁剪掉仅被字符串反射引用的组件类型(Debug 运行不裁剪，故开发期本就可用)。
- 先实现最小闭环验证：`HcButton`、`HcInput`,改写 `button.json`、`input.json` 的 `dt`,浏览器确认拖入后正常显示、无 `TypeLoadException`。

## Phase 2 — 原生组件实现 + 全量 meta 改写
实现以下原生组件(现代扁平风：中性灰白、1px 细边框、统一圆角 6px、`focus` 高亮、`hover` 反馈)：
- 表单类(完整实现)：`HcButton`(Text/Type/Disabled/Loading)、`HcInput`(Placeholder/Disabled/MaxLength/ReadOnly/AllowClear)、`HcTextarea`、`HcInputNumber`(Min/Max/Step)、`HcSelect`(+选项子片段/固定选项数据源)、`HcCheckbox`、`HcRadio`、`HcSwitch`、`HcDatePicker`(原生 date)、`HcTimePicker`(原生 time)。
- 容器类(实现 `Style`+`ChildContent`,配合 `content:"$(DraggableContainer)"` 机制)：`HcCard`(Title/Bordered)、`HcLayout`/`HcSider`/`HcContent`、`HcFlex`、`HcRow`/`HcCol`、`HcImage`。
- 复杂类(先做简洁可用的占位/降级实现，后续可增强)：`HcAutoComplete`/`HcCascader`/`HcTreeSelect`→复用 `HcSelect` 外观；`HcTree`/`HcTable`/`HcTabs`/`HcUpload`/`HcList`/`行政区划`→统一 `HcPlaceholder`(渲染带标签的规范占位框，不再崩溃)。
- 逐一改写 `meta/parts/componentParts/**/*.json`：
  - `frag.dt`(含嵌套 `childs[].dt`)→ `"H.LowCode.Components.<Hc组件>, H.LowCode.Components"`。
  - 审查 `frag.attrs` 及嵌套 `attrs` 中的 `attrt`(渲染期会 `Type.GetType(attrt,true)`),将任何 `AntDesign.*` CLR 类型替换为 BCL 类型或移除；移除 `input.json` 等中的泛型 `TValue` 相关项。
  - `attrdefgroups`(属性面板定义)中出现的 `AntDesign.*` 类型(如 `card.json` 的 `AntDesign.CardSize`)一并清理，避免属性面板异常。
- `common/conditional.json`(ComponentType=2)按现有“低代码组件渲染子节点”路径处理，无需 `dt`。

## Phase 3 — 修复 Bug2(画布内拖拽/排序)
- Bug1 修复后实测：选中→拖拽是否触发 [DraggableContainer](file:///d:/H/code/my/applab/src/LowCode/DesignEngine/H.LowCode.DesignEngine/DraggableComponents/DraggableContainer.razor) 的 `OnDrop`→`DraggableItem_Sorting` 并刷新。
- 预期修复点：确保排序后容器 `StateHasChanged` 生效；必要时让“拖拽手柄(⠿)”始终可发起拖拽(不强依赖先选中),并校正让位动画/`RefreshState` 时机，保证跨容器/同容器排序都能落位并即时刷新。

## Phase 4 — 现代简洁扁平风美化(#3)
在 [designengine.css](file:///d:/H/code/my/applab/src/LowCode/DesignEngine/H.LowCode.DesignEngine/wwwroot/designengine.css) 及相关 razor/scoped css 统一改造：
- 顶栏：白底细底边、`返回` 文本按钮、`预览`(次要)/`保存`(主色)按钮统一为扁平风(圆角、hover、主色 `#2f6feb` 一类中性蓝)。
- 左侧组件面板：卡片式组件项(细边框、hover 抬升/高亮)、分组标题清晰、图标 tab 规范。
- 中间画布：浅灰背景、留白、拖拽经过高亮统一为主色虚线,空态给出“拖拽组件到此处”引导。
- 右侧属性面板：分组标题、表单项间距/对齐统一,输入控件扁平化。

## Phase 5 — 其它功能修复与优化(#4)
在 [DesignPage.razor](file:///d:/H/code/my/applab/src/LowCode/DesignEngine/H.LowCode.DesignEngine/Pages/DesignPage.razor) 等：
- `保存` 按钮：`onclick="() => SavePageSchemaAsync()"`(HTML 属性，当前无效)→ Blazor `@onclick="SavePageSchemaAsync"`。
- `返回` 按钮：`<a href="javascript:void(0)">` 加 `@onclick`,导航回应用页面管理(或浏览器后退)。
- `预览` 按钮：接线到渲染引擎预览路由(或在无目标时禁用并提示)。
- 校验选中/删除(✕)/复制(📋)在真实组件渲染后工作正常。
- 已知的 `配置事件` → `IPageAppService` 服务端 500 属另一后端问题，本次仅记录，不在范围内(除非顺带定位成本低)。

## Phase 6 — 构建与浏览器验证
- 构建 `H.AppLab.Host.All`(客户端+服务端)0 错误。
- 运行并访问 `/designengine/designer/testapp/_new`,逐项验证：拖入各类组件正常显示、画布内拖拽排序生效、样式为现代扁平风、保存/返回/预览可用、控制台无 `TypeLoadException`。

## 假设与范围
- 采用“自定义原生组件 + 全量改写 meta”方案(用户已确认),不重新引入 AntDesign。
- 复杂组件(树/表格/标签页/上传/列表/行政区划等)先交付规范占位/降级实现，保证不崩溃、可拖拽布局,完整交互后续迭代。
- 同一套原生组件同时供设计器与渲染引擎复用(渲染引擎已引用 `Components.Defaults`)。
