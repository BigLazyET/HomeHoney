# Implementation Plan: 集成外部文件与数据存储

**Branch**: `005-integrate-storage-services` | **Date**: 2026-03-11 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/005-integrate-storage-services/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

为 HomeHoney 引入可配置的外部文件服务与外部数据服务接入能力：保险合同、说明书等文件本体统一通过文件服务管理，其余结构化业务数据统一持久化到 MongoDB；同时在应用内提供可编辑的连接配置、面向文件资源的上传/下载/列表/详情能力，以及面向非文件数据的 CRUD 能力。当前实现不再保留本地业务数据或文件持久化层，UI 通过应用服务访问仓储与文件网关，配置与凭据分离保存在 Preferences / SecureStorage，避免将页面直接耦合到外部服务原始协议。

## Implementation Adjustments

### 2026-03-11

- 移除了此前引入的本地缓存服务、文件缓存服务和提醒快照仓储，避免与当前“远端为正式来源”的边界冲突。
- 将用户偏好统一收敛到 Preferences，将敏感连接信息统一收敛到 SecureStorage。
- 修正了运行时诊断方案：不再写本地日志文件，改为通过错误边界和 IDE 调试输出暴露异常，避免再引入新的本地落盘点。
- 修正了平台兼容性风险：不再使用 `AppContext.BaseDirectory` 作为任何运行时写入位置，消除 Mac Catalyst `.app` 包只读目录权限问题。

## Technical Context

**Language/Version**: C# 14 / .NET 10 (MAUI Blazor Hybrid)  
**Primary Dependencies**: Microsoft.Maui.Controls, Microsoft.AspNetCore.Components.WebView.Maui, `HttpClient`, MongoDB.Driver, Preferences, SecureStorage, xUnit, bUnit, Moq  
**Storage**: 文件本体走外部文件服务；结构化业务数据走 MongoDB；连接配置走 Preferences + SecureStorage；不做本地业务数据/文件持久化  
**Testing**: xUnit / bUnit / Moq / 文件服务适配器测试 / 仓储与编排服务回归测试  
**Target Platform**: iOS 16+、Android 13+ (API 33+)、macOS 15+（可选）、Windows 条件构建、`net10.0` 测试宿主  
**Project Type**: mobile-app (MAUI Blazor Hybrid)  
**Performance Goals**: 本地配置保存应在 1 秒内完成；局域网环境下文件列表/元数据读取目标为 2 秒内返回首屏结果；上传/下载必须保持 UI 可响应  
**Constraints**: 编译零警告、通过接口隔离外部服务、用户可修改连接目标、文件与非文件数据边界清晰、不得让 UI 直接依赖外部服务原始 payload、必须处理跨文件服务与 MongoDB 的部分成功/部分失败状态、不得新增本地业务数据/文件持久化  
**Scale/Scope**: 1 个 MAUI 应用项目 + 1 个测试项目；覆盖资料、冰箱贴、备忘录、提醒、设置等现有模块；面向单家庭规模的数百条结构化记录与数百 MB 级文件资源

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原则 | 合规状态 | 说明 |
|------|----------|------|
| I. 组件化架构 | ✅ 合规 | 外部文件服务与 MongoDB 都通过注入式服务接口与适配器访问，Blazor 页面只消费应用服务，不直接调用 HTTP 或数据库驱动 |
| II. 文档模型驱动 | ✅ 合规 | 保险合同、说明书等仍以结构化文档元数据建模，并通过文件引用关联外部文件本体，维持清晰的文档生命周期 |
| III. 存储边界清晰 | ✅ 合规 | 当前实现明确限制本地只保留 Preferences 与 SecureStorage，业务数据和文件内容分别由 MongoDB 与 FileBrowser 承担 |
| IV. 类型安全与可测试性 | ✅ 合规 | 采用强类型模型、仓储/网关接口、可 mock 的配置提供器与适配器，并要求为配置、文件编排和 Mongo CRUD 提供自动化测试 |
| V. 移动端优先 UX | ✅ 合规 | 存储配置入口、文件状态、上传下载反馈和远端失败提示都将以内聚移动端页面状态呈现，保持三步内完成核心操作 |

**门控结果（Phase 0 前）**：PASS — 当前方案与“远端数据为正式来源，本地仅保留偏好/密钥”一致。  
**门控结果（Phase 1 后复核）**：PASS — 研究与设计已将远端存储、配置持久化和接口隔离纳入模型与契约。

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

**Structure Decision**: 继续沿用当前单解决方案、双项目结构。UI 仍位于 `HomeHoney/Components/`，现有业务模型保留在 `HomeHoney/Models/`，新增的外部文件服务适配器、Mongo 仓储和连接配置提供器收敛到 `HomeHoney/Services/Storage/`，再由现有 `Documents`、`Collaboration`、`Reminders`、`Preferences` 服务编排调用。测试继续集中在 `HomeHoney.Tests/Component/` 与 `HomeHoney.Tests/Unit/`，不新增独立后端项目。

## Complexity Tracking

> 无未解释的宪章违规，本表为空。
