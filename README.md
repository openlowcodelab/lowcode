# AppLab

### 介绍
* 应用开发实验性项目，基于 .NET + Blazor 实现

### 代码结构
- Host
    - 项目的宿主程序, 不包含任何业务逻辑, 仅进行业务应用的服务注册
    - H.AppLab.Host.All 为所有服务的宿主(单体应用), 其他 Host 项目为单服务宿主。

- LowCode
    - 低代码核心项目, 包含核心服务和领域模型

- Services 基础服务
    - Enterprise
    - Account
    - Organization
    - Approval

- Utils 工具类库

### 开发
#### 应用迁移
通过 Tools 目录下对应的 DbMigrator 创建数据库结构
