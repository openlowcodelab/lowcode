### 待办 
#### 公共 
* [已解决]GUID 替换为 6位随机字符串，减少字符长度
* [不处理]razor 项目减少第三方包依赖（如 abp）
* 自身、第三方 版本更新产生的兼容问题
* [已解决]Auto 模式，页面调用2次问题优化
* [已解决]添加 HttpClientInterceptor，实现调用 ApplicationService 时 Server 模式本地请求，WebAssembly 模式 http 请求代理


#### DesignEngine
* 设计通用的组件物料结构，并加载到设计器
** 当前存在问题：
*** 组件类型为固定值，无法变更
*** 组件物料支持新设置项后，历史元数据中的属性设置看不到
* 将组件分为原子组件、物料组件，物料组件使用 json 格式存储，并进行渲染

#### RenderEngine



### 性能优化
#### 优化组件呈现
* 避免不必要地呈现组件子树
    * 子组件参数为基元不可变类型
        * TablePropertyItem.razor
    * 重写 ShouldRender
* 创建轻型组件 
    * 在代码中定义可重用的 RenderFragments 
        * ComponentItem.razor
    * 不接收太多参数
* 确保级联参数是固定的（IsFixed="true"）
* 手动实现 SetParametersAsync 避免呈现器通过反射写入 [Parameter] 属性值
* 不要过快触发事件（onmousemove,onscroll 等）

#### 优化下载大小
* 中间语言裁剪
* 延迟加载程序集
* 压缩
