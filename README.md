# H.LowCode

### 介绍
* 低代码实验性(功能仅达到demo级别,存在破坏性变更)项目，基于 .NET + Blazor 实现

### 分支规则
* master: 最新稳定代码
* dev: 开发分支

### 代码结构
* Host
** Host 目录下所有项目的宿主程序, 不包含任何业务逻辑, 仅进行业务应用的服务注册
** H.LowCode.Host.All 为所有服务的宿主(单体应用), 其他 Host 项目为单服务宿主。

* LowCode
** 低代码核心项目, 包含核心服务和领域模型

* Services
** 配套基础服务

* Utils
** 配套工具类库

### 开发
#### 生成迁移
在 H.LowCode.DbMigrator 项目中执行以下命令，添加迁移文件：
dotnet ef migrations add <MigrationName>

#### 应用迁移
运行 H.LowCode.DbMigrator
