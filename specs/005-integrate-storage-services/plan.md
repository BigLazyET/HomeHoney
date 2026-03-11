# Implementation Plan: 集成外部文件与数据存储

**Branch**: `005-integrate-storage-services` | **Date**: 2026-03-11 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/005-integrate-storage-services/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

为 HomeHoney 引入可配置的外部文件服务与外部数据服务接入能力：保险合同、说明书等文件本体统一通过文件服务管理，其余结构化业务数据统一持久化到 MongoDB；同时在应用内提供可编辑的连接配置、面向文件资源的上传/下载/列表/详情能力，以及面向非文件数据的 CRUD 能力。为了与项目宪章中的离线优先要求保持一致，设计采用“远端主存储 + 本地缓存/同步状态”的分层方案：UI 通过应用服务访问仓储与文件网关，配置与凭据分离持久化，远端读写结果同步映射到本地缓存和一致性状态，避免将页面直接耦合到外部服务原始协议。

## Technical Context

**Language/Version**: C# 14 / .NET 10 (MAUI Blazor Hybrid)  
**Primary Dependencies**: Microsoft.Maui.Controls, Microsoft.AspNetCore.Components.WebView.Maui, `HttpClient`, MongoDB.Driver, 本地 SQLite 缓存库（按宪章引入）, xUnit, bUnit, Moq  
**Storage**: 文件本体走外部文件服务；结构化业务数据走 MongoDB；连接配置走本地偏好 + 安全凭据存储；离线缓存走本地 SQLite + 本地文件缓存  
**Testing**: xUnit / bUnit / Moq / 文件服务适配器测试 / 仓储与编排服务回归测试  
**Target Platform**: iOS 16+、Android 13+ (API 33+)、macOS 15+（可选）、Windows 条件构建、`net10.0` 测试宿主  
**Project Type**: mobile-app (MAUI Blazor Hybrid)  
**Performance Goals**: 本地配置保存应在 1 秒内完成；局域网环境下文件列表/元数据读取目标为 2 秒内返回首屏结果；上传/下载必须保持 UI 可响应；离线缓存列表打开应接近即时反馈  
**Constraints**: 必须保持离线优先、编译零警告、通过接口隔离外部服务、用户可修改连接目标、文件与非文件数据边界清晰、不得让 UI 直接依赖外部服务原始 payload、必须处理跨文件服务与 MongoDB 的部分成功/部分失败状态  
**Scale/Scope**: 1 个 MAUI 应用项目 + 1 个测试项目；覆盖资料、冰箱贴、备忘录、提醒、设置等现有模块；面向单家庭规模的数百条结构化记录与数百 MB 级文件资源

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原则 | 合规状态 | 说明 |
|------|----------|------|
| I. 组件化架构 | ✅ 合规 | 外部文件服务与 MongoDB 都通过注入式服务接口与适配器访问，Blazor 页面只消费应用服务，不直接调用 HTTP 或数据库驱动 |
| II. 文档模型驱动 | ✅ 合规 | 保险合同、说明书等仍以结构化文档元数据建模，并通过文件引用关联外部文件本体，维持清晰的文档生命周期 |
| III. 离线优先 | ✅ 合规（带设计约束） | 用户要求外部化存储与宪章存在天然张力，因此设计明确加入本地 SQLite 元数据缓存、本地文件缓存和同步状态；若后续实现删除本地缓存层，则必须先修订宪章 |
| IV. 类型安全与可测试性 | ✅ 合规 | 采用强类型模型、仓储/网关接口、可 mock 的配置提供器与适配器，并要求为配置、文件编排和 Mongo CRUD 提供自动化测试 |
| V. 移动端优先 UX | ✅ 合规 | 存储配置入口、文件状态、上传下载反馈和远端失败提示都将以内聚移动端页面状态呈现，保持三步内完成核心操作 |

**门控结果（Phase 0 前）**：PASS — 只要实现继续保留本地缓存与同步状态设计，即可满足宪章。  
**门控结果（Phase 1 后复核）**：PASS — 研究与设计已将远端存储、离线缓存、一致性状态和接口隔离纳入模型与契约，无未解释宪章违规。

## Project Structure

### Documentation (this feature)

```text
specs/005-integrate-storage-services/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── storage-integration-contract.md
└── tasks.md
```

### Source Code (repository root)
```text
HomeHoney/
├── Components/
│   ├── Layout/
│   ├── Pages/
│   │   ├── Documents/
│   │   ├── FridgeNotes/
│   │   ├── Reminders/
│   │   ├── Settings/
│   │   └── Welcome/
│   ├── Shared/
│   ├── Routes.razor
│   └── _Imports.razor
├── Models/
├── Platforms/
├── Resources/
├── Services/
│   ├── Collaboration/
│   ├── Documents/
│   ├── Navigation/
│   ├── Preferences/
│   ├── Reminders/
│   ├── Search/
│   └── Storage/              # planned external storage adapters / repositories
├── HomeHoney.csproj
├── MauiProgram.cs
└── wwwroot/

HomeHoney.Tests/
├── Component/
├── Unit/
└── HomeHoney.Tests.csproj

README.md
HomeHoney.sln
global.json
```

**Structure Decision**: 继续沿用当前单解决方案、双项目结构。UI 仍位于 `HomeHoney/Components/`，现有业务模型保留在 `HomeHoney/Models/`，新增的外部文件服务适配器、Mongo 仓储、本地缓存协调器和连接配置提供器收敛到 `HomeHoney/Services/Storage/`，再由现有 `Documents`、`Collaboration`、`Reminders`、`Preferences` 服务编排调用。测试继续集中在 `HomeHoney.Tests/Component/` 与 `HomeHoney.Tests/Unit/`，不新增独立后端项目。

## Complexity Tracking

> 无未解释的宪章违规，本表为空。
