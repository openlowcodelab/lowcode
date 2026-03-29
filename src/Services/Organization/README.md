在 src\\Services\\Organization 目录创建一个组织架构管理服务，包含组织架构的增删改和用户的添加，注意不直接提供用户新增功能，用户来源于 Account 服务。

技术要求：
1、基于 ASP.NET Core Minimal API、Blazor 等技术
2、非必要不引入第三方组件，也不要使用第三方框架
3、依赖 Account 的 SDK 获取用户信息

项目分层：
H.Organization.HttpApi：api 接口项目，提供给 web 调用
H.Organization.Application：业务逻辑
H.Organization.Application.Contracts：对象、接口定义
H.Organization.Domain：实体定义
H.Organization.EntityFrameworkCore
H.Organization.Web：页面，如部门管理、成员管理、角色管理等
H.Organization.Client：提供给外部应用对接的 SDK

## 功能模块

### 1. 组织架构
- 成员管理
- 部门管理
- 角色管理

### 2. API 接口
- 所有部门接口（父子层级结构）
- 部门下所有用户接口
- 