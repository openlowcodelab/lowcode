# H.Organization.DbMigrator

组织架构服务的数据库迁移工具。

## 功能说明

本项目用于自动创建和更新 Organization 服务的数据库结构。运行此项目会：

1. 读取 `appsettings.json` 中的数据库连接字符串
2. 应用所有 Entity Framework Core 迁移
3. 自动创建数据库（如果不存在）
4. 创建所有表结构

## 使用方法

### 方法 1：直接运行项目

```bash
dotnet run --project src/Services/Organization/H.Organization.DbMigrator/H.Organization.DbMigrator.csproj
```

### 方法 2：在 Visual Studio 中运行

1. 右键点击 `H.Organization.DbMigrator` 项目
2. 选择"设为启动项目"
3. 按 F5 或 Ctrl+F5 运行

### 方法 3：运行编译后的程序

```bash
cd src/Services/Organization/H.Organization.DbMigrator/bin/Debug/net10.0
./H.Organization.DbMigrator.exe
```

## 配置文件

编辑 `appsettings.json` 可以修改数据库连接字符串：

```json
{
  "ConnectionStrings": {
    "OrganizationDb": "Server=(localdb)\\mssqllocaldb;Database=OrganizationDb;Trusted_Connection=true;"
  }
}
```

支持以下 SQL Server 连接方式：

- **LocalDB**（开发环境推荐）：
  ```
  Server=(localdb)\\mssqllocaldb;Database=OrganizationDb;Trusted_Connection=true;
  ```

- **SQL Server**：
  ```
  Server=localhost;Database=OrganizationDb;Trusted_Connection=true;
  ```

- **SQL Server（指定用户名密码）**：
  ```
  Server=localhost;Database=OrganizationDb;User Id=sa;Password=YourPassword;TrustServerCertificate=true;
  ```

## 输出示例

```
==========================================
  H.Organization.DbMigrator - 数据库迁移工具
==========================================

连接字符串：Server=(localdb)\mssqllocaldb;Database=OrganizationDb;Trusted_Connection=true;

正在应用迁移...
迁移应用成功！

按任意键退出...
```

## 包含的数据表

运行成功后，会在数据库中创建以下表：

- **Organizations** - 部门表
- **Members** - 成员表
- **Roles** - 角色表

## 注意事项

1. 首次运行前请确保 SQL Server 或 LocalDB 已安装并运行
2. 需要有数据库创建权限
3. 重复运行会自动应用新的迁移（如果有的话）
4. 生产环境建议使用专门的迁移脚本管理工具

## 故障排除

### 问题：无法连接到数据库

**解决方案**：
- 检查 SQL Server 服务是否运行
- 验证连接字符串是否正确
- 确认防火墙设置

### 问题：权限不足

**解决方案**：
- 使用 Windows 身份验证（Trusted_Connection=true）
- 或提供具有足够权限的 SQL Server 账号

### 问题：数据库已存在

**解决方案**：
- 工具会自动检测并使用现有数据库
- 如需重置，可手动删除数据库后重新运行
