# HomeHoney Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-03-10

## Active Technologies
- C# 12 / .NET 9 + Microsoft.Maui, Microsoft.AspNetCore.Components.WebView.Maui, Blazor 组件体系 (001-app-page-structure)
- 结构化文档模型 + 应用内种子数据（规划层面；本特性主要定义页面结构与实体模型） (001-app-page-structure)
- C# 12 / .NET 9 + Microsoft.Maui, Microsoft.AspNetCore.Components.WebView.Maui, Blazor 组件体系, 现有应用服务层 (003-delete-nav-onboarding)
- 用户偏好配置 + 现有内存种子数据服务（本特性不新增外部存储类型） (003-delete-nav-onboarding)
- C# 14 / .NET 10 + Microsoft.Maui.Controls, Microsoft.AspNetCore.Components.WebView.Maui, Microsoft.AspNetCore.Components.Web, Microsoft.NET.Test.Sdk, xUnit, bUnit, Moq (004-upgrade-dotnet10)
- 用户偏好配置 + 当前内存种子数据服务（本特性不新增存储类型） (004-upgrade-dotnet10)
- C# 14 / .NET 10 (MAUI Blazor Hybrid) + Microsoft.Maui.Controls, Microsoft.AspNetCore.Components.WebView.Maui, `HttpClient`, MongoDB.Driver, Preferences, SecureStorage, xUnit, bUnit, Moq (005-integrate-storage-services)
- 文件本体走外部文件服务；结构化业务数据走 MongoDB；连接配置走 Preferences + SecureStorage；不做本地业务数据/文件持久化 (005-integrate-storage-services)
- C# 14 / .NET 10 + ASP.NET Core Web API, `HttpClient`/`IHttpClientFactory`, MongoDB.Driver, ASP.NET Core OpenAPI, existing xUnit + Moq + bUnit test stack, `WebApplicationFactory` for API integration tests (006-backend-api-service)
- 后端连接 FileBrowser 处理文件本体，连接 MongoDB 处理结构化业务数据；移动端仅保留后端地址、用户偏好和敏感信息 (006-backend-api-service)
- C# 14 / .NET 10 + .NET MAUI Blazor Hybrid, ASP.NET Core minimal APIs, MongoDB.Driver, existing shared UI components (`StatusBanner`, `EmptyState`), xUnit, Moq, bUnit, `WebApplicationFactory` (001-improve-app-usability)
- `HomeHoney.Api` 统一连接 MongoDB（结构化业务数据）与 FileBrowser（附件文件）；移动端仅保留偏好与安全配置 (001-improve-app-usability)

- C# 12 / .NET 9 + Microsoft.Maui, Microsoft.AspNetCore.Components.WebView.Maui (001-project-scaffold)

## Project Structure

```text
HomeHoney/
HomeHoney.Tests/
specs/
```

## Commands

- `dotnet workload restore`
- `dotnet build HomeHoney.sln`
- `dotnet test HomeHoney.Tests/HomeHoney.Tests.csproj`

## Code Style

C# 14 / .NET 10: Follow standard conventions

## Recent Changes
- 001-improve-app-usability: Added C# 14 / .NET 10 + .NET MAUI Blazor Hybrid, ASP.NET Core minimal APIs, MongoDB.Driver, existing shared UI components (`StatusBanner`, `EmptyState`), xUnit, Moq, bUnit, `WebApplicationFactory`
- 006-backend-api-service: Added C# 14 / .NET 10 + ASP.NET Core Web API, `HttpClient`/`IHttpClientFactory`, MongoDB.Driver, ASP.NET Core OpenAPI, existing xUnit + Moq + bUnit test stack, `WebApplicationFactory` for API integration tests
- 005-integrate-storage-services: Added C# 14 / .NET 10 (MAUI Blazor Hybrid) + Microsoft.Maui.Controls, Microsoft.AspNetCore.Components.WebView.Maui, `HttpClient`, MongoDB.Driver, Preferences, SecureStorage, xUnit, bUnit, Moq


<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
