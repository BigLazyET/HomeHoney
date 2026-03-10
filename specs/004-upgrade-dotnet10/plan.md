# Implementation Plan: 升级到 .NET 10

**Branch**: `004-upgrade-dotnet10` | **Date**: 2026-03-10 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/004-upgrade-dotnet10/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

将 HomeHoney 从当前 .NET 9 / MAUI Blazor Hybrid 基线升级到 .NET 10，目标是在不改变现有业务能力边界的前提下，统一更新 SDK 锁定、应用与测试工程目标框架、相关依赖版本说明与验证流程，并确保多平台构建与核心回归验证继续通过。实现上采用“先统一版本基线，再修复构建/兼容性，再同步文档与验证入口”的策略，避免局部升级造成 SDK、工作负载与测试工程脱节。

## Technical Context

**Language/Version**: C# 14 / .NET 10 (SDK 10.0.102)  
**Primary Dependencies**: Microsoft.Maui.Controls, Microsoft.AspNetCore.Components.WebView.Maui, Microsoft.AspNetCore.Components.Web, Microsoft.NET.Test.Sdk, xUnit, bUnit, Moq  
**Storage**: 本地 JSON 偏好文件 + 当前内存种子数据服务（本特性不新增存储类型）  
**Testing**: xUnit 2.9+ / bUnit 2.5+ / Moq 4.20+ / `dotnet test`  
**Target Platform**: iOS 16+、Android 13+ (API 33+)、macOS 15+（可选）、Windows 条件构建、`net10.0` 测试宿主  
**Project Type**: mobile-app (MAUI Blazor Hybrid)  
**Performance Goals**: 升级后标准 `restore/build/test` 流程一次通过；核心入口无可见启动或导航退化；多平台构建时间和体验与现有基线同量级  
**Constraints**: 编译零警告、保持既有功能范围、工作负载与 SDK 版本一致、尽量不调整平台最低支持范围、离线优先能力不退化、需使用 `dotnet workload restore` 对齐 MAUI 10 工作负载、iOS/Mac Catalyst 构建需满足 Xcode 26.2、Android 构建链路需满足 Java 17+  
**Scale/Scope**: 2 个项目（应用 + 测试）、1 个解决方案、4+ 目标框架/平台 TFM、README 与规范文档同步更新

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原则 | 合规状态 | 说明 |
|------|----------|------|
| I. 组件化架构 | ✅ 合规 | 本特性聚焦运行时和工程基线升级，不改变现有组件边界；如需兼容性修复，也将局限在现有组件/服务职责内 |
| II. 文档模型驱动 | ✅ 合规 | 升级不改变 `InsuranceRecord`、`ManualRecord`、`FridgeNote` 等核心文档模型，只保证其在新运行时下持续可用 |
| III. 离线优先 | ✅ 合规 | 升级范围不引入网络依赖；验证要求覆盖本地启动、页面访问和现有本地状态持久化行为 |
| IV. 类型安全与可测试性 | ✅ 合规 | 升级后仍以强类型 C# 工程、`dotnet test`、xUnit/bUnit 验证为主，并补足必要的升级回归检查 |
| V. 移动端优先 UX | ✅ 合规 | 升级目标之一是保持首页、资料、冰箱贴、提醒、设置与欢迎流在移动端入口下无明显退化 |

**门控结果（Phase 0 前）**：PASS — 无宪章违规，可进入研究。  
**门控结果（Phase 1 后复核）**：PASS — 设计仍围绕工程基线升级、验证连续性与移动端主流程稳定性展开。

## Project Structure

### Documentation (this feature)

```text
specs/004-upgrade-dotnet10/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── upgrade-validation-contract.md
└── tasks.md
```

### Source Code (repository root)
```text
HomeHoney/
├── Components/
│   ├── Layout/
│   ├── Pages/
│   ├── Shared/
│   ├── Routes.razor
│   └── _Imports.razor
├── Models/
├── Platforms/
├── Resources/
├── Services/
├── HomeHoney.csproj
├── MauiProgram.cs
└── wwwroot/

HomeHoney.Tests/
├── Component/
├── Unit/
└── HomeHoney.Tests.csproj

global.json
README.md
HomeHoney.sln
```

**Structure Decision**: 继续沿用当前单解决方案、双项目结构：`HomeHoney/` 承载 MAUI Blazor Hybrid 应用，`HomeHoney.Tests/` 承载单元与组件测试。此次升级主要修改 `global.json`、两个项目文件、可能受 SDK 变更影响的少量启动/平台配置，以及 README/规范文档，不新增子应用或服务。

## Complexity Tracking

> 无宪章违规，本表为空。
