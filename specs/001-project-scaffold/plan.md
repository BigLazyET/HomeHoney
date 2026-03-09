# Implementation Plan: 项目脚手架搭建

**Branch**: `001-project-scaffold` | **Date**: 2026-03-09 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-project-scaffold/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

搭建 HomeHoney 项目的 .NET MAUI Blazor Hybrid 脚手架，包含解决方案结构（主应用 + 测试项目）、基础 DI 容器、空状态主页面 UI 骨架、底部 Tab 导航、平台资源配置、以及 xUnit + bUnit 示例测试。确保克隆即可构建运行。

## Technical Context

**Language/Version**: C# 12 / .NET 9  
**Primary Dependencies**: Microsoft.Maui, Microsoft.AspNetCore.Components.WebView.Maui  
**Storage**: 本次不涉及（SQLite 将在后续需求集成，本次仅预留服务接口）  
**Testing**: xUnit 2.9+ / bUnit 1.31+ / Moq 4.20+  
**Target Platform**: iOS 16+、Android 13+ (API 33+)、macOS 14+（可选）、Windows 10+（可选）  
**Project Type**: mobile-app (MAUI Blazor Hybrid)  
**Performance Goals**: 应用冷启动 < 3 秒（模拟器环境）  
**Constraints**: 离线可用、零后端依赖、编译零警告（TreatWarningsAsErrors）  
**Scale/Scope**: 单人开发/小团队、初始仅含 1 个页面 + 导航骨架

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原则 | 合规状态 | 说明 |
|------|----------|------|
| I. 组件化架构 | ✅ 合规 | Pages/、Shared/、Services/ 目录结构；UI 通过 Blazor 组件构建；原生服务通过接口注入 |
| II. 文档模型驱动 | ⬜ 不适用 | 脚手架阶段不涉及业务数据模型，仅预留目录结构 |
| III. 离线优先 | ✅ 合规 | 纯客户端应用，无网络依赖；SQLite 接口预留但不实现 |
| IV. 类型安全与可测试性 | ✅ 合规 | TreatWarningsAsErrors 强制开启；服务通过接口定义；测试项目含 xUnit + bUnit + Moq |
| V. 移动端优先 UX | ✅ 合规 | iOS 为首要目标平台；底部 Tab 导航符合移动端交互模式；空状态 UI 针对触控优化 |

**门控结果：PASS** — 无违规，可进入阶段 0。

## Project Structure

### Documentation (this feature)

```text
specs/001-project-scaffold/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (minimal — scaffold has no data entities)
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
HomeHoney.sln                             # 顶层解决方案文件
global.json                               # .NET 9 SDK 版本锁定
.editorconfig                             # 代码风格规范
README.md                                 # 构建/运行/测试说明

HomeHoney/                                # 主应用项目 (MAUI Blazor Hybrid)
├── HomeHoney.csproj                      # SDK: Microsoft.NET.Sdk.Razor
├── MauiProgram.cs                        # DI 容器 + MAUI 配置入口
├── App.xaml / App.xaml.cs                # MAUI 应用入口
├── MainPage.xaml / MainPage.xaml.cs      # BlazorWebView 宿主页面
├── wwwroot/
│   ├── css/
│   │   └── app.css                       # 全局样式
│   └── index.html                        # Blazor 入口 HTML
├── Components/
│   ├── _Imports.razor                    # 全局 @using 指令
│   ├── Routes.razor                      # Blazor 路由
│   ├── Pages/
│   │   └── Home.razor                    # 主页面（空状态 UI）
│   ├── Layout/
│   │   ├── MainLayout.razor              # 主布局（含底部 Tab 导航）
│   │   └── NavMenu.razor                 # 导航菜单
│   └── Shared/
│       └── EmptyState.razor              # 空状态组件
├── Services/                             # 服务接口（预留目录）
├── Platforms/
│   ├── Android/
│   ├── iOS/
│   ├── MacCatalyst/
│   └── Windows/
└── Resources/
    ├── AppIcon/
    ├── Splash/
    ├── Fonts/
    └── Raw/

HomeHoney.Tests/                          # 测试项目
├── HomeHoney.Tests.csproj                # SDK: Microsoft.NET.Sdk.Razor, TFM: net9.0
├── Unit/
│   └── SampleServiceTests.cs             # 服务层单元测试示例
└── Component/
    └── HomePageTests.cs                  # bUnit 组件测试示例
```

**Structure Decision**: 采用**扁平结构**（项目文件夹在仓库根目录），符合 MAUI 模板和 `dotnet` CLI 默认约定（见 research.md 决策 3）。Blazor 组件统一放在 `Components/` 下，用 Pages/、Layout/、Shared/ 子目录分离（见 research.md 决策 5），满足 Constitution 原则 I 的目录规范。MAUI 项目的 `TargetFrameworks` 中增加 `net9.0` 以支持 bUnit 测试引用（见 research.md 决策 2）。

## Complexity Tracking

> 无违规，本表格为空。
