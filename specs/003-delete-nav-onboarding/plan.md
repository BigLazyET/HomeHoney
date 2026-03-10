# Implementation Plan: 资料删除、返回导航与首次引导优化

**Branch**: `003-delete-nav-onboarding` | **Date**: 2026-03-10 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-delete-nav-onboarding/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

为 HomeHoney 增补三个关键体验闭环：一是为资料模块与冰箱贴增加删除能力，形成“新增/编辑/删除”完整生命周期；二是在所有二级及更深页面提供统一返回入口，保证移动端层级导航连续性；三是明确首次自动引导与后续正常启动的分离规则，确保用户首次可学习、后续不被重复打扰。实现上将沿用现有 MAUI Blazor Hybrid 页面架构，在现有服务层添加删除方法与引导状态约束，并通过共享页面头部/返回模式统一多层页面导航行为。

## Technical Context

**Language/Version**: C# 12 / .NET 9  
**Primary Dependencies**: Microsoft.Maui, Microsoft.AspNetCore.Components.WebView.Maui, Blazor 组件体系, 现有应用服务层  
**Storage**: 本地 JSON 偏好文件 + 现有内存种子数据服务（本特性不新增外部存储类型）  
**Testing**: xUnit 2.9+ / bUnit 2.5+ / Moq 4.20+  
**Target Platform**: iOS 16+、Android 13+ (API 33+)、macOS 15+（可选）  
**Project Type**: mobile-app (MAUI Blazor Hybrid)  
**Performance Goals**: 删除与返回操作保持单次点击可达；首次引导完成后后续启动直接进入主页面；删除后列表与摘要在当前会话即时刷新  
**Constraints**: 离线优先、移动端优先、导航层级清晰、删除操作需确认、防止引导状态被帮助入口误覆盖、编译零警告  
**Scale/Scope**: 影响 3 个服务域（资料/协作/偏好）、多组详情/编辑/设置子页面、少量组件测试与导航行为测试

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原则 | 合规状态 | 说明 |
|------|----------|------|
| I. 组件化架构 | ✅ 合规 | 删除与返回逻辑将落在现有页面组件和服务接口中，不直接耦合平台 API；可通过共享页面头部或局部组件复用导航模式 |
| II. 文档模型驱动 | ✅ 合规 | 资料删除围绕既有 `InsuranceRecord` 与 `ManualRecord` 生命周期扩展，不破坏当前文档建模 |
| III. 离线优先 | ✅ 合规 | 删除、返回和首次引导状态均在本地完成，不依赖网络；引导完成状态继续保存在本地 |
| IV. 类型安全与可测试性 | ✅ 合规 | 删除与引导逻辑将通过现有强类型服务扩展，适合继续用 xUnit/bUnit 验证状态与页面行为 |
| V. 移动端优先 UX | ✅ 合规 | 返回按钮与删除确认直接服务触控场景和多层页面浏览，符合移动端优先原则 |

**门控结果（Phase 0 前）**：PASS — 无宪章违规，可进入研究。  
**门控结果（Phase 1 后复核）**：PASS — 设计仍保持组件化、离线优先与移动端优先约束。

## Project Structure

### Documentation (this feature)

```text
specs/003-delete-nav-onboarding/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── ui-behavior-contract.md
└── tasks.md
```

### Source Code (repository root)

```text
HomeHoney/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   ├── Pages/
│   │   ├── Documents/
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
│   │   ├── Settings/
│   │   │   ├── SettingsHome.razor
│   │   │   ├── ThemeSettings.razor
│   │   │   ├── NotificationSettings.razor
│   │   │   ├── HomeLayoutSettings.razor
│   │   │   └── PrivacySettings.razor
│   │   └── Welcome/
│   │       ├── Welcome.razor
│   │       └── GetStarted.razor
│   └── Shared/
│       └── EmptyState.razor
├── Services/
│   ├── Documents/
│   │   └── DocumentCatalogService.cs
│   ├── Collaboration/
│   │   └── FamilyCollaborationService.cs
│   ├── Preferences/
│   │   └── UserPreferenceService.cs
│   └── Navigation/
│       └── AppRoutes.cs

HomeHoney.Tests/
├── Component/
│   └── HomePageTests.cs
└── Unit/
```

**Structure Decision**: 继续沿用当前单应用 MAUI Blazor Hybrid 扁平结构。实现主要集中在现有页面组件和服务层，不新增项目或子应用；文档删除与冰箱贴删除分别在现有 `Documents` 与 `FridgeNotes` 页面/服务中完成，返回导航通过页面级共享模式统一，首次引导规则继续归口到偏好服务。

## Complexity Tracking

> 无宪章违规，本表为空。
