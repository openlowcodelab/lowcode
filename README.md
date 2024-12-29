# H.LowCode

### 介绍
* 低代码实验性项目
* 基于 .NET 8.0 + Blazor 实现

### 分支规则
* master: 最新稳定代码
* release: 发布分支，基于 release 创建版本（当 release 分支足够稳定后，合并到 dev、master 分支）
* dev: 开发分支（所有的 feature、hotfix 合并至 dev 分支，进行统一验证，并合并到 release 分支）
* feature: 新特性分支（基于 dev 分支创建）
* hotfix: 热修复分支（基于 dev 或 release 分支创建）

### 版本规则
* 正式版本: 遵循语义化版本 2.0 规范 (v0.0.1)
* 非正式版本: v0.0.1-preview.1
