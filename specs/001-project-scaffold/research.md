# Research: 项目脚手架搭建

**Branch**: `001-project-scaffold` | **Date**: 2026-03-09

## 决策 1：.NET 9 MAUI Blazor Hybrid 模板与配置

- **决策**: 使用 `dotnet new maui-blazor --framework net9.0` 作为起点，保留模板的 `Microsoft.NET.Sdk.Razor` SDK、`AddMauiBlazorWebView()` 服务注册模式和 `Components/` 文件夹约定。在 `.csproj` 的 `<PropertyGroup>` 中添加 `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`。
- **理由**: 官方模板已遵循 Microsoft 最新约定（单项目、Components/ 子文件夹、平台文件夹）。`AddMauiBlazorWebView()` 是 .NET MAUI RC1 以来唯一支持的 BlazorWebView 服务注册方式。模板不含 `TreatWarningsAsErrors`，但规范 FR-004 明确要求编译零警告。
- **考虑过的替代方案**:
  - `dotnet new maui`（纯 XAML）+ 手动添加 Blazor → 不必要的复杂度
  - 从第一天就分离 Razor Class Library (RCL) → 脚手架阶段过度工程化
  - 仅 `<WarningsAsErrors>` 指定特定代码 → 规范要求零警告，`TreatWarningsAsErrors` 更简单严格

## 决策 2：bUnit 与 MAUI Blazor Hybrid 的测试策略

- **决策**: 在 MAUI 项目的 `<TargetFrameworks>` 中添加 `net9.0`，使 bUnit 测试项目可以引用它。脚手架阶段不引入独立的 RCL。
- **理由**: 规范 FR-002 要求**恰好两个项目**（主应用 + 测试项目）。引入第三个 RCL 增加了不必要的复杂度。在 `TargetFrameworks` 中添加 `net9.0` 是已知的可行方案，允许 `dotnet test` 针对 `net9.0` 切片运行。脚手架阶段的 Razor 组件不含平台特定的 MAUI 代码，因此安全可行。
- **考虑过的替代方案**:
  - 从第一天就分离 RCL → 增加第三个项目，过度工程化
  - 仅用 xUnit 测试非 Blazor 代码（跳过 bUnit）→ 规范 FR-006 明确要求 bUnit + 组件测试示例
- **已知限制**:
  - bUnit 在模拟环境中渲染组件，**无法**测试 MAUI 原生元素（BlazorWebView、XAML 控件、平台 API）
  - 通过 `AddMauiBlazorWebView()` 注入的服务（如 NavigationManager）必须在 bUnit 测试中 mock
  - JSInterop 调用需使用 bUnit 内置的 JSInterop mock
  - 后续添加平台相关组件时，可考虑提取共享组件到 RCL

## 决策 3：解决方案目录结构

- **决策**: 采用**扁平结构**，项目文件夹直接位于仓库根目录：
  ```
  HomeHoney.sln
  global.json
  HomeHoney/HomeHoney.csproj
  HomeHoney.Tests/HomeHoney.Tests.csproj
  ```
- **理由**: 规范定义恰好两个项目，不需要 `src/` + `tests/` 层级。扁平结构符合官方 MAUI 模板约定和 `dotnet` CLI 默认行为，是小型项目的最佳实践。
- **考虑过的替代方案**:
  - `src/` + `tests/` 子文件夹层级 → 对两个项目而言过于结构化，不符合 MAUI 模板约定
  - 后续如解决方案增长（如添加 RCL 或 Web 项目），可迁移到 `src/` 结构

## 决策 4：global.json SDK 锁定策略

- **决策**: 在仓库根目录放置 `global.json`，使用 `"version": "9.0.100"` + `"rollForward": "latestFeature"` + `"allowPrerelease": false`。
- **理由**: `latestFeature` 在锁定与灵活性之间取得平衡 — 接受任何 .NET 9 SDK 补丁/功能版本，但不会意外使用 .NET 10。`allowPrerelease: false` 确保 CI 构建稳定性。
- **考虑过的替代方案**:
  - 不要 `global.json` → 规范 FR-005 强制要求 SDK 锁定
  - `rollForward: disable`（精确版本） → 过于严格，开发者安装了 9.0.200 就会失败
  - `rollForward: latestMajor` → 过于宽松，可能意外使用 .NET 10

## 决策 5：组件目录命名 — Components/ vs Pages/+Shared/

- **决策**: 采用 MAUI Blazor 模板的 `Components/` 根目录约定，Constitution 的 Pages/ 和 Shared/ 作为 `Components/` 的子目录：`Components/Pages/`、`Components/Layout/`、`Components/Shared/`。
- **理由**: .NET 9 MAUI Blazor 模板默认使用 `Components/` 作为所有 Razor 组件的根目录，这是 Microsoft 的官方模式。Constitution 原则 I 的本质要求是"页面组件和共享组件分离"，在 `Components/` 下用子目录实现完全满足这一要求。
- **考虑过的替代方案**:
  - 直接在项目根创建 `Pages/` 和 `Shared/` → 不符合 MAUI Blazor 模板约定，可能与 MAUI XAML 的 Platforms/ 产生路径混淆

## 测试项目 NuGet 包参考

| 包名 | 版本 | 用途 |
|------|------|------|
| `bunit` | 2.5.3+ | Blazor 组件测试 |
| `Microsoft.NET.Test.Sdk` | 17.12.0+ | .NET 测试基础设施 |
| `xunit` | 2.9.4+ | 测试框架 |
| `xunit.runner.visualstudio` | 3.0.1+ | VS/CLI 测试运行器 |
| `coverlet.collector` | 6.0.4+ | 代码覆盖率 |
| `Moq` | 4.20+ | Mock 框架（规范 FR-006 要求）|
