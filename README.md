# H.LowCode

### 介绍
* 低代码实验性(功能仅达到demo级别,存在破坏性变更)项目，基于 .NET + Blazor 实现

### 分支规则
* master: 最新稳定代码
* dev: 开发分支

### 开发
#### 生成迁移
在 H.LowCode.DbMigrator 项目中执行以下命令，添加迁移文件：
dotnet ef migrations add <MigrationName>

#### 应用迁移
运行 H.LowCode.DbMigrator
