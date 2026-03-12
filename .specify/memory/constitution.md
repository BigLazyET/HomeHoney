<!--
  === 同步影响报告 ===
  版本变更：2.0.0 → 3.0.0 (MAJOR)
  变更理由：删除“离线优先 + 本地 SQLite/文件系统”核心原则，并将章程边界调整为与当前后端托管存储架构一致
  修改的原则：
    - 删除 III. 离线优先（Offline-First）
    - IV. 类型安全与可测试性 → III. 类型安全与可测试性（移除 SQLite 特定表述）
    - V. 移动端优先的用户体验 → IV. 移动端优先的用户体验
  新增章节：无
  删除章节：III. 离线优先（Offline-First）
  模板更新状态：
    ✅ .specify/templates/plan-template.md — 无需修改，Constitution Check 动态引用
    ✅ .specify/templates/spec-template.md — 无需修改
    ✅ .specify/templates/tasks-template.md — 无需修改
  后续 TODO：无
-->

# HomeHoney Constitution

## Core Principles

### I. 组件化架构（Component-Based Architecture）

所有 UI 功能必须（MUST）以独立的 Blazor 组件（`.razor`）形式构建，运行在 MAUI Blazor Hybrid 的 `BlazorWebView` 容器中。每个组件必须：

- 职责单一，可独立渲染和测试
- 通过参数（`[Parameter]`）和事件回调（`EventCallback`）进行数据通信，禁止组件间直接引用内部状态
- 共享组件放入 `Shared/` 目录，页面级组件放入 `Pages/` 目录
- 需要调用原生设备能力（相机、文件系统等）时，必须（MUST）通过注入的 C# 服务接口访问，禁止在 Blazor 组件中直接调用平台 API

**理由**：家庭说明书和保险合同涉及多种文档类型（说明书、保单、理赔记录等），组件化确保每种文档卡片、查看器、编辑器可独立演进和复用。Blazor 组件与平台服务解耦后，UI 层可直接复用到未来的 Blazor Web 版本。

### II. 文档模型驱动（Document-Model Driven）

系统的核心实体是"文档"。所有功能必须（MUST）围绕文档生命周期设计：

- 每种文档类型（家电说明书、保险合同、保单附件等）必须有明确的数据模型定义
- 文档的创建、分类、检索、归档必须有清晰的状态流转
- 文档元数据（标题、类别、关联设备/保单、有效期等）必须结构化存储，不依赖文件名约定

**理由**：这是一个文档管理系统，数据模型的严谨性直接决定后续检索、提醒、统计等功能的可实现性。

### III. 类型安全与可测试性（Type Safety & Testability）

所有业务逻辑必须（MUST）使用 C# 强类型编写，禁止使用 `dynamic` 或绕过编译器类型检查：

- 服务层必须通过接口（`interface`）定义契约，支持依赖注入和单元测试的 mock 替换
- 平台相关服务（相机、文件系统等）必须（MUST）通过接口抽象，以便在单元测试中 mock
- 每个服务至少包含针对核心方法的单元测试
- Blazor 组件应（SHOULD）使用 bUnit 进行组件级测试
- 集成测试应（SHOULD）覆盖关键的用户端到端流程

**理由**：C# 的强类型系统是 .NET 生态的核心优势。MAUI Hybrid 架构中平台服务与 UI 解耦后，业务逻辑可完全在非设备环境中测试。

### IV. 移动端优先的用户体验（Mobile-First UX）

面向家庭成员（包括非技术用户）的界面必须（MUST）遵循以下原则：

- **移动端优先**：UI 设计以手机屏幕为基准，所有布局和交互必须（MUST）针对触控操作优化
- 核心操作（查看文档、搜索、添加文档）必须在 3 次点击/触摸内完成
- 必须（MUST）充分利用原生能力：相机拍照添加文档、分享、本地通知（到期提醒）
- 文档分类和搜索必须提供直观的视觉反馈（加载状态、空状态、错误状态）
- 必须（MUST）遵循各平台设计规范（iOS Human Interface Guidelines / Material Design 基本原则）
- 中文为默认语言；国际化支持为可选扩展

**理由**：家庭成员技术水平参差不齐，手机是最常用的设备。原生 App 的交互体验远优于 Web，触控优化和原生能力（相机、通知）是家庭文档管理场景的刚需。

## 技术约束

- **语言与框架**：C# + .NET MAUI Blazor Hybrid
- **UI 技术**：Blazor 组件（HTML/CSS/Razor），运行在 MAUI `BlazorWebView` 中
- **目标平台**（按优先级）：
  1. iOS（iPhone）— 主要目标，须满足 App Store 上架要求
  2. Android — 须满足 Google Play 上架要求
  3. macOS / Windows — 可选桌面适配（MAUI 原生支持）
- **构建工具**：`dotnet` CLI + 各平台 SDK（Xcode / Android SDK）
- **测试框架**：xUnit + bUnit（UI 组件）+ Moq（设备服务 mock）
- **包管理**：NuGet
- **数据边界**：移动端通过 `HomeHoney.Api` 访问业务数据与附件能力；本地仅保留必要的偏好与安全配置
- **部署模式**：
  - iOS → App Store（需 Apple Developer 账号）
  - Android → Google Play（需 Google Play Developer 账号）
  - 可选：TestFlight / Firebase App Distribution 用于内测
- **未来扩展**：Blazor 组件可复用到 Blazor WebAssembly 项目，实现 Web 版共享 UI

## 开发工作流

- **分支策略**：功能分支从 `main` 切出，命名格式 `<number>-<short-name>`（由 SpecKit 自动管理）
- **代码审查**：所有合并到 `main` 的 PR 必须（MUST）通过 CI 构建和测试
- **提交规范**：遵循 Conventional Commits 格式（`feat:`, `fix:`, `docs:`, `refactor:` 等）
- **质量门禁**：
  - 编译零警告（`TreatWarningsAsErrors` 启用）
  - 单元测试通过率 100%
  - 新增功能必须附带对应测试（视具体功能而定）

## Governance

本章程是 HomeHoney 项目的最高开发准则。所有规范（spec）、计划（plan）和任务（tasks）必须（MUST）与章程原则保持一致。

- **修订流程**：对章程的任何修改必须通过 `/speckit.constitution` 命令执行，并生成同步影响报告
- **版本策略**：遵循语义化版本控制（SemVer）——MAJOR 代表原则删除或不兼容变更，MINOR 代表新增原则或重要扩展，PATCH 代表措辞和格式修正
- **合规检查**：每次 `/speckit.plan` 执行时会自动校验 Constitution Check，违反原则的设计必须提供书面理由或调整方案

**Version**: 3.0.0 | **Ratified**: 2026-03-09 | **Last Amended**: 2026-03-12
