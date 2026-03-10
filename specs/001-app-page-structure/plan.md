# Implementation Plan: App 页面结构与导航规划

**Branch**: `001-app-page-structure` | **Date**: 2026-03-10 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-app-page-structure/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

为 HomeHoney 规划一套面向移动端家庭场景的信息架构与页面体系：以 `首页`、`资料`、`冰箱贴`、`提醒`、`设置` 为一级导航，围绕欢迎引导、保险与说明书资料管理、家庭协作、跨模块搜索提醒、主题与个性化设置构建可扩展页面结构。技术上沿用现有 .NET MAUI Blazor Hybrid 架构，通过统一的文档型页面模式与横切能力设计，为后续组件实现、路由设计、状态管理与任务拆分提供基础。

## Technical Context

**Language/Version**: C# 12 / .NET 9  
**Primary Dependencies**: Microsoft.Maui, Microsoft.AspNetCore.Components.WebView.Maui, Blazor 组件体系  
**Storage**: SQLite + 本地文件系统（规划层面；本特性主要定义页面结构与实体模型）  
**Testing**: xUnit 2.9+ / bUnit 2.5+ / Moq 4.20+  
**Target Platform**: iOS 16+、Android 13+ (API 33+)、macOS 15+（可选）、Windows 10+（可选）
**Project Type**: mobile-app (MAUI Blazor Hybrid)  
**Performance Goals**: 首次用户在 2 分钟内理解核心入口；核心模块 2 次点击内可达；首页摘要适配移动端单屏优先浏览  
**Constraints**: 离线优先、移动端优先、统一导航稳定可见、编译零警告、页面职责清晰可组件化  
**Scale/Scope**: 1 套一级导航、5 个核心模块、10+ 二级页面、4 类核心内容实体、1 套跨模块搜索与提醒能力

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原则 | 合规状态 | 说明 |
|------|----------|------|
| I. 组件化架构 | ✅ 合规 | 页面结构明确分为首页、资料、冰箱贴、提醒、设置及其子页面；保险与说明书共享组件模式，利于后续抽象列表卡片、详情区块和筛选组件 |
| II. 文档模型驱动 | ✅ 合规 | 保险与说明书采用统一文档型建模；页面设计围绕列表、详情、录入、检索和归档语义展开 |
| III. 离线优先 | ✅ 合规 | 所有页面流程默认按本地浏览、录入、搜索和提醒可用来设计；提醒中心与资料查看不依赖实时网络 |
| IV. 类型安全与可测试性 | ✅ 合规 | 设计中保留明确页面契约、实体模型和可独立测试的用户路径，适合后续用强类型 ViewModel/Service 实现并使用 bUnit/xUnit 测试 |
| V. 移动端优先 UX | ✅ 合规 | 一级导航控制在 5 项以内；首页为摘要而非长列表；核心路径控制在 2~3 次触达，符合手机使用习惯 |

**门控结果（Phase 0 前）**：PASS — 无宪章违规，可进入研究与设计阶段。  
**门控结果（Phase 1 后复核）**：PASS — 研究、数据模型、UI 契约和 quickstart 与宪章保持一致，无需记录复杂度豁免。

## Project Structure

### Documentation (this feature)

```text
specs/001-app-page-structure/
├── plan.md                        # This file
├── research.md                    # Phase 0 output
├── data-model.md                  # Phase 1 output
├── quickstart.md                  # Phase 1 output
├── contracts/
│   └── ui-navigation-contract.md  # 页面与导航契约
└── tasks.md                       # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
HomeHoney/
├── Components/
│   ├── Pages/
│   │   ├── Home.razor
│   │   ├── Welcome/
│   │   │   ├── Welcome.razor
│   │   │   ├── Features.razor
│   │   │   └── GetStarted.razor
│   │   ├── Documents/
│   │   │   ├── DocumentsHome.razor
│   │   │   ├── Insurance/
│   │   │   │   ├── InsuranceList.razor
│   │   │   │   ├── InsuranceDetail.razor
│   │   │   │   └── InsuranceForm.razor
│   │   │   └── Manuals/
│   │   │       ├── ManualList.razor
│   │   │       ├── ManualDetail.razor
│   │   │       └── ManualForm.razor
│   │   ├── FridgeNotes/
│   │   │   ├── FridgeNoteBoard.razor
│   │   │   └── FridgeNoteForm.razor
│   │   ├── Memos/
│   │   │   ├── MemoList.razor
│   │   │   ├── MemoDetail.razor
│   │   │   └── MemoForm.razor
│   │   ├── Reminders/
│   │   │   ├── ReminderCenter.razor
│   │   │   └── Search.razor
│   │   └── Settings/
│   │       ├── SettingsHome.razor
│   │       ├── ThemeSettings.razor
│   │       ├── NotificationSettings.razor
│   │       ├── HomeLayoutSettings.razor
│   │       └── PrivacySettings.razor
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   └── Shared/
│       ├── EmptyState.razor
│       ├── DocumentCard.razor
│       ├── ReminderCard.razor
│       ├── QuickActionGrid.razor
│       └── FilterBar.razor
├── Services/
│   ├── Navigation/
│   ├── Search/
│   ├── Reminders/
│   └── Preferences/
└── wwwroot/
    └── css/

HomeHoney.Tests/
├── Component/
│   ├── Navigation/
│   ├── Home/
│   └── Documents/
└── Unit/
    ├── Search/
    ├── Reminders/
    └── Preferences/
```

**Structure Decision**: 继续沿用当前仓库的扁平结构与 MAUI Blazor Hybrid 单主项目布局，在 `HomeHoney/Components/Pages` 下按业务域拆分页面目录，在 `Shared/` 中沉淀跨模块组件，在 `Services/` 中为导航、搜索、提醒和偏好预留服务层目录。该结构既符合现有项目约定，也与本特性“页面结构优先、后续组件化实现”的目标一致。

## Complexity Tracking

> 无宪章违规，本表为空。
