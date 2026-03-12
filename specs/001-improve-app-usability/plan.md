# Implementation Plan: Improve App Usability

**Branch**: `[001-improve-app-usability]` | **Date**: 2026-03-12 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-improve-app-usability/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

提升 HomeHoney 在高频录入与日常浏览场景中的可用性和稳定性：移除说明书、保险、备忘录、冰箱贴表单中的误导性默认值，统一缺省显示与必填提示；在附件区域补充最后同步时间、文件名与大小展示；为搜索、提醒和资料列表增加页面内手动重试能力；补齐空值、异常和历史脏数据处理；并在 `HomeHoney.Api` 中为现有 Mongo 集合建立统一索引策略，以支撑后续列表读取与检索扩展。实现上保持“移动端仅通过后端 API 访问业务数据”的既有边界，在共享组件、页面状态模型、服务层校验与后端索引初始化之间协同完成。

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# 14 / .NET 10  
**Primary Dependencies**: .NET MAUI Blazor Hybrid, ASP.NET Core minimal APIs, MongoDB.Driver, existing shared UI components (`StatusBanner`, `EmptyState`), xUnit, Moq, bUnit, `WebApplicationFactory`  
**Storage**: `HomeHoney.Api` 统一连接 MongoDB（结构化业务数据）与 FileBrowser（附件文件）；移动端仅保留偏好与安全配置  
**Testing**: xUnit, Moq, bUnit, `HomeHoney.Api.Tests` 集成/契约测试，`HomeHoney.Tests` 单元/组件测试  
**Target Platform**: iOS 16+、Android 13+、macOS 15+ 的 MAUI 客户端 + 可运行于 macOS/Linux/Windows 的 ASP.NET Core 后端  
**Project Type**: mobile-app + web-service  
**Performance Goals**: 表单校验反馈应在同次交互内即时可见；搜索/提醒/资料列表失败后应支持一次页面内重试恢复；随着资料量增长，常用列表排序与常见筛选不得出现明显响应退化  
**Constraints**: 移动端不得绕过后端直接访问 FileBrowser/MongoDB；必须保留用户已输入内容；必须明确区分空状态与错误状态；必须避免向页面暴露下游存储内部细节；继续满足零警告构建与测试回归要求  
**Scale/Scope**: 覆盖 4 类核心表单（说明书、保险、备忘录、冰箱贴）、2 类附件详情页、3 类可重试列表页、共享组件/服务层以及 Mongo 的 7 个现有业务集合（insurance/manual/fridge/memo/preferences/members/spaces）

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原则 | 合规状态 | 说明 |
|------|----------|------|
| I. 组件化架构 | ✅ 合规 | 方案主要落在现有 Razor 页面、共享组件和注入式服务层，不引入组件间耦合或平台 API 直调 |
| II. 文档模型驱动 | ✅ 合规 | 表单、附件元数据和缺省显示均围绕现有文档/协作实体扩展，不以底层集合或文件路径驱动交互 |
| III. 类型安全与可测试性 | ✅ 合规 | 改动可通过强类型模型、共享状态对象、服务接口和现有单元/组件/API 测试体系验证 |
| IV. 移动端优先 UX | ✅ 合规 | 需求直接强化移动端默认值、空状态、错误状态、重试与附件可见信息，符合移动端优先体验原则 |

**门控结果（Phase 0 前）**：PASS — 当前方案符合更新后的章程边界：移动端仅通过 `HomeHoney.Api` 访问业务数据与附件能力，其余原则均满足。  
**门控结果（Phase 1 后复核）**：PASS — 设计产物完整覆盖表单、页面状态、附件元数据与 Mongo 索引，同时持续符合现行章程要求。

## Project Structure

### Documentation (this feature)

```text
specs/001-improve-app-usability/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── ui-state-contract.md
└── tasks.md
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
HomeHoney/
├── Components/
│   ├── Layout/
│   ├── Pages/
│   │   ├── Documents/
│   │   ├── FridgeNotes/
│   │   ├── Memos/
│   │   ├── Reminders/
│   │   └── Settings/
│   └── Shared/
├── Models/
├── Services/
│   ├── Collaboration/
│   ├── Documents/
│   ├── Preferences/
│   ├── Reminders/
│   ├── Search/
│   └── Storage/
├── wwwroot/
└── HomeHoney.csproj

HomeHoney.Tests/
├── Component/
├── Unit/
│   ├── Collaboration/
│   ├── Documents/
│   ├── Reminders/
│   ├── Search/
│   └── Storage/
└── HomeHoney.Tests.csproj

HomeHoney.Api/
├── Application/
│   ├── Collaboration/
│   ├── Documents/
│   ├── Preferences/
│   ├── Reminders/
│   ├── Search/
│   └── Storage/
├── Contracts/
├── Endpoints/
├── Infrastructure/
│   ├── Configuration/
│   ├── FileBrowser/
│   └── Mongo/
└── HomeHoney.Api.csproj

HomeHoney.Api.Tests/
├── Contract/
└── Integration/
```

**Structure Decision**: 采用现有“MAUI 客户端 + ASP.NET Core 后端 + 双测试工程”的单解决方案结构。表单默认值、缺省显示、附件可见状态和手动重试主要落在 `HomeHoney/Components` 与对应服务层；Mongo 索引策略、附件元数据供给和后端兼容处理落在 `HomeHoney.Api/Application`、`HomeHoney.Api/Infrastructure/Mongo` 与现有端点层；回归验证分别落在 `HomeHoney.Tests` 与 `HomeHoney.Api.Tests`。

## Complexity Tracking

本特性当前无额外章程豁免项。
