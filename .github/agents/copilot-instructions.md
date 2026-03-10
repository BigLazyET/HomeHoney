# HomeHoney Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-03-10

## Active Technologies
- C# 12 / .NET 9 + Microsoft.Maui, Microsoft.AspNetCore.Components.WebView.Maui, Blazor 组件体系 (001-app-page-structure)
- SQLite + 本地文件系统（规划层面；本特性主要定义页面结构与实体模型） (001-app-page-structure)
- C# 12 / .NET 9 + Microsoft.Maui, Microsoft.AspNetCore.Components.WebView.Maui, Blazor 组件体系, 现有应用服务层 (003-delete-nav-onboarding)
- 本地 JSON 偏好文件 + 现有内存种子数据服务（本特性不新增外部存储类型） (003-delete-nav-onboarding)
- C# 14 / .NET 10 + Microsoft.Maui.Controls, Microsoft.AspNetCore.Components.WebView.Maui, Microsoft.AspNetCore.Components.Web, Microsoft.NET.Test.Sdk, xUnit, bUnit, Moq (004-upgrade-dotnet10)
- 本地 JSON 偏好文件 + 当前内存种子数据服务（本特性不新增存储类型） (004-upgrade-dotnet10)

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
- 004-upgrade-dotnet10: Added C# 14 / .NET 10 + Microsoft.Maui.Controls, Microsoft.AspNetCore.Components.WebView.Maui, Microsoft.AspNetCore.Components.Web, Microsoft.NET.Test.Sdk, xUnit, bUnit, Moq
- 003-delete-nav-onboarding: Added C# 12 / .NET 9 + Microsoft.Maui, Microsoft.AspNetCore.Components.WebView.Maui, Blazor 组件体系, 现有应用服务层
- 002-app-page-structure: Added C# 12 / .NET 9 + Microsoft.Maui, Microsoft.AspNetCore.Components.WebView.Maui, Blazor 组件体系


<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
