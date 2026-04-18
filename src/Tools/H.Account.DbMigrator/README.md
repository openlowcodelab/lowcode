# H.Account.DbMigrator

数据库迁移工具项目，用于管理 Entity Framework Core 迁移。

## 项目结构

```
H.Account.DbMigrator/
├── H.Account.DbMigrator.csproj    # 迁移工具项目
├── Program.cs                      # 迁移执行入口
├── appsettings.json                # 配置文件
├── README.md                       # 使用说明
└── Migrations/                     # 迁移文件目录
    ├── 20260223064420_InitialCreate.cs
    ├── 20260223064420_InitialCreate.Designer.cs
    └── AccountDbContextModelSnapshot.cs
```

## 使用方法

### 1. 生成新迁移

在 `H.Account.DbMigrator` 目录下执行：

```bash
# 进入项目目录
cd src/Services/Account/H.Account.DbMigrator

# 先编译 DbMigrator 项目
dotnet build

# 生成新迁移（--startup-project 指向 HttpApi 以读取 DbContext 配置）
dotnet ef migrations add MigrationName --startup-project ../H.Account.HttpApi --output-dir Migrations
```

### 2. 应用迁移

```bash
# 方式一：运行 DbMigrator 程序
cd src/Services/Account/H.Account.DbMigrator
dotnet run

# 方式二：在 HttpApi 项目中执行（需要安装 EF Core Tools）
cd src/Services/Account/H.Account.HttpApi
dotnet ef database update --project ../H.Account.DbMigrator
```

### 3. 删除迁移

```bash
cd src/Services/Account/H.Account.DbMigrator
dotnet ef migrations remove --startup-project ../H.Account.HttpApi
```

### 4. 查看迁移脚本

```bash
cd src/Services/Account/H.Account.DbMigrator
dotnet ef migrations script --startup-project ../H.Account.HttpApi
```

### 5. 查看迁移列表

```bash
cd src/Services/Account/H.Account.DbMigrator
dotnet ef migrations list --startup-project ../H.Account.HttpApi
```

## 配置

在 `appsettings.json` 中配置数据库连接字符串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AccountDb;Trusted_Connection=true;"
  }
}
```

## 注意事项

1. **迁移文件位置**：所有迁移文件应存放在 `Migrations` 目录中
2. **版本控制**：迁移文件应提交到源代码管理
3. **备份**：运行迁移前请确保已备份数据库
4. **启动项目**：生成新迁移时，`--startup-project` 指向 `H.Account.HttpApi` 以便读取 DbContext 配置
5. **编译顺序**：执行 EF Core 命令前，请先编译 `H.Account.DbMigrator` 项目

## 常见问题

### 问题：File 'H.Account.DbMigrator.dll' not found

**解决方案**：在执行 EF Core 命令前，先编译 DbMigrator 项目：

```bash
dotnet build H.Account.DbMigrator.csproj
```

### 问题：无法找到 DbContext

**解决方案**：确保 `--startup-project` 参数指向包含 DbContext 配置的项目（通常是 `H.Account.HttpApi`）
