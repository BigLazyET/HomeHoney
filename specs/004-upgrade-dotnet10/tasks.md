# Tasks: 升级到 .NET 10

**Input**: Design documents from `/specs/004-upgrade-dotnet10/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: 本特性规范未要求新增测试优先开发流程，因此任务以版本基线升级、兼容性修复、现有测试回归和手动验收为主。

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **扁平结构**:
  - 主应用: `HomeHoney/`
  - 测试: `HomeHoney.Tests/`
  - 规范文档: `specs/004-upgrade-dotnet10/`

---

## Phase 1: Setup (环境准备)

**Purpose**: 为 .NET 10 升级建立统一的 SDK 基线和验证入口

- [X] T001 Update SDK pinning and rollout policy in `global.json` for the .NET 10 baseline
- [X] T002 [P] Review solution-level build entry points and version-sensitive references in `HomeHoney.sln` and `README.md`
- [X] T003 [P] Prepare upgrade validation artifacts in `specs/004-upgrade-dotnet10/plan.md`, `specs/004-upgrade-dotnet10/contracts/upgrade-validation-contract.md`, and `specs/004-upgrade-dotnet10/quickstart.md` for execution tracking

---

## Phase 2: Foundational (基础能力 — 阻塞型前置条件)

**Purpose**: 统一工程目标框架、依赖基线与多平台构建前提

**⚠️ CRITICAL**: 用户故事实现前必须完成此阶段

- [X] T004 Update application target frameworks, MAUI package alignment, and SDK-sensitive properties in `HomeHoney/HomeHoney.csproj`
- [X] T005 [P] Update test host target framework and test package compatibility in `HomeHoney.Tests/HomeHoney.Tests.csproj`
- [X] T006 [P] Adjust startup/build-touching code for .NET 10 compatibility in `HomeHoney/MauiProgram.cs`, `HomeHoney/App.xaml.cs`, and `HomeHoney/MainPage.xaml.cs`
- [X] T007 [P] Review platform configuration continuity for upgraded TFMs in `HomeHoney/Platforms/Android/AndroidManifest.xml`, `HomeHoney/Platforms/iOS/Info.plist`, and `HomeHoney/Platforms/MacCatalyst/Info.plist`
- [X] T008 Confirm build ignore coverage for upgraded outputs in `.gitignore`

**Checkpoint**: SDK、应用项目、测试项目和多平台基础配置已统一到 .NET 10，可开始按用户故事推进

---

## Phase 3: User Story 1 - 保持应用可持续构建与运行 (Priority: P1) 🎯 MVP

**Goal**: 升级后主应用与测试工程继续稳定构建、运行并通过核心自动化验证

**Independent Test**: 在新 SDK 环境下完成依赖还原、解决方案构建与测试工程运行，验证关键入口没有因升级而失效

### Implementation for User Story 1

- [X] T009 [P] [US1] Fix .NET 10 compilation or analyzer regressions in shared app entry files `HomeHoney/Components/Routes.razor`, `HomeHoney/Components/_Imports.razor`, and `HomeHoney/Components/Layout/MainLayout.razor`
- [X] T010 [P] [US1] Fix .NET 10 compatibility issues surfaced in core services `HomeHoney/Services/Documents/DocumentCatalogService.cs`, `HomeHoney/Services/Collaboration/FamilyCollaborationService.cs`, `HomeHoney/Services/Preferences/UserPreferenceService.cs`, `HomeHoney/Services/Search/SearchIndexService.cs`, and `HomeHoney/Services/Reminders/ReminderCenterService.cs`
- [X] T011 [P] [US1] Update test fixtures and version-sensitive assertions in `HomeHoney.Tests/Component/HomePageTests.cs`, `HomeHoney.Tests/Unit/GreetingServiceTests.cs`, `HomeHoney.Tests/Unit/AppRoutesTests.cs`, `HomeHoney.Tests/Unit/DocumentCatalogServiceTests.cs`, and `HomeHoney.Tests/Unit/FamilyCollaborationServiceTests.cs`
- [X] T012 [US1] Verify and document the upgraded command chain against `HomeHoney.sln` and `HomeHoney.Tests/HomeHoney.Tests.csproj`

**Checkpoint**: 主应用与测试工程在 .NET 10 下可独立构建并完成现有自动化验证

---

## Phase 4: User Story 2 - 保持多平台交付连续性 (Priority: P2)

**Goal**: 升级后现有目标平台仍保持一致的构建与主流程可验证能力

**Independent Test**: 针对解决方案中的目标平台完成构建检查，并抽样验证首页、欢迎流、资料、冰箱贴、提醒和设置入口可进入

### Implementation for User Story 2

- [X] T013 [P] [US2] Align platform-specific target framework expectations and minimum OS declarations in `HomeHoney/HomeHoney.csproj`
- [X] T014 [P] [US2] Resolve any platform startup or manifest regressions in `HomeHoney/Platforms/Android/MainActivity.cs`, `HomeHoney/Platforms/Android/MainApplication.cs`, `HomeHoney/Platforms/iOS/AppDelegate.cs`, and `HomeHoney/Platforms/MacCatalyst/AppDelegate.cs`
- [ ] T015 [P] [US2] Validate upgraded runtime behavior for primary navigation entry pages in `HomeHoney/Components/Pages/Home.razor`, `HomeHoney/Components/Pages/Documents/DocumentsHome.razor`, `HomeHoney/Components/Pages/FridgeNotes/FridgeNoteBoard.razor`, `HomeHoney/Components/Pages/Reminders/ReminderCenter.razor`, `HomeHoney/Components/Pages/Settings/SettingsHome.razor`, and `HomeHoney/Components/Pages/Welcome/Welcome.razor`
- [X] T016 [US2] Record multi-platform build and smoke-validation expectations in `specs/004-upgrade-dotnet10/contracts/upgrade-validation-contract.md` and `specs/004-upgrade-dotnet10/quickstart.md`

**Checkpoint**: 多平台构建连续性与核心入口抽样验收路径已明确且可执行

---

## Phase 5: User Story 3 - 降低后续协作与接手成本 (Priority: P3)

**Goal**: 升级后的开发前提、版本说明和验收方式在仓库中保持清晰一致

**Independent Test**: 新成员仅通过仓库文档即可完成环境准备、构建验证并理解升级边界

### Implementation for User Story 3

- [X] T017 [P] [US3] Update environment prerequisites, target framework references, and verification commands in `README.md`
- [X] T018 [P] [US3] Sync upgraded version baseline and validation expectations in `specs/004-upgrade-dotnet10/spec.md`, `specs/004-upgrade-dotnet10/plan.md`, and `specs/004-upgrade-dotnet10/research.md`
- [X] T019 [P] [US3] Refine upgrade state model and verification narrative in `specs/004-upgrade-dotnet10/data-model.md` and `specs/004-upgrade-dotnet10/quickstart.md`
- [X] T020 [US3] Refresh agent-facing repository guidance in `.github/agents/copilot-instructions.md`

**Checkpoint**: 代码、规范和仓库说明对 .NET 10 基线达成一致，可独立交接

---

## Phase 6: Polish & Cross-Cutting Concerns (优化与横切关注点)

**Purpose**: 完成最终回归、手动验收与任务收敛

- [ ] T021 [P] Run solution-level restore and build validation for the upgraded baseline against `HomeHoney.sln`
- [X] T022 [P] Run regression test validation for the upgraded baseline against `HomeHoney.Tests/HomeHoney.Tests.csproj`
- [ ] T023 Run manual validation scenarios from `specs/004-upgrade-dotnet10/quickstart.md`
- [X] T024 Update task completion status and final notes in `specs/004-upgrade-dotnet10/tasks.md`

**Current validation notes**:
- `dotnet build HomeHoney/HomeHoney.csproj -f net10.0` ✅
- `dotnet test HomeHoney.Tests/HomeHoney.Tests.csproj` ✅ (17/17)
- `dotnet build HomeHoney.sln` ⚠️ 当前环境受外部工具链阻塞：iOS / Mac Catalyst 需要 Xcode 26.2，而本机为 26.0；Android manifest merger 需要 Java 17+，而本机为 Java 11。
- `dotnet workload restore HomeHoney.sln` ⚠️ 当前终端权限不足，需在具备提升权限的环境下执行。

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 无依赖，可立即开始
- **Foundational (Phase 2)**: 依赖 Phase 1 完成，阻塞所有用户故事
- **User Story 1 (Phase 3)**: 依赖 Phase 2 完成，构成升级 MVP
- **User Story 2 (Phase 4)**: 依赖 Phase 2 完成；可与 US1 并行，但建议在主工程成功升级后推进平台抽样
- **User Story 3 (Phase 5)**: 依赖 Phase 2 完成；可与 US1/US2 并行推进文档与交接材料
- **Polish (Phase 6)**: 依赖目标用户故事完成后执行

### User Story Dependencies

- **User Story 1 (P1)**: 依赖统一 SDK 与项目目标框架，是整个升级工作的 MVP
- **User Story 2 (P2)**: 依赖应用项目已成功升级并可构建，之后才能验证多平台连续性
- **User Story 3 (P3)**: 依赖升级范围与验证链路基本稳定，之后才能同步仓库说明与交接材料

### Within Each User Story

- 先完成工程/平台配置，再修正代码兼容性
- 先确保构建与测试通过，再补文档与说明同步
- 完成故事独立验收后，再进入最终回归与手动验收

### Parallel Opportunities

- Phase 1: `T002` 与 `T003` 可并行
- Phase 2: `T005`、`T006`、`T007`、`T008` 可在 `T004` 后并行收敛
- Phase 3: `T009`、`T010`、`T011` 可按不同文件组并行
- Phase 4: `T013`、`T014`、`T015` 可并行，`T016` 最后收敛验证说明
- Phase 5: `T017`、`T018`、`T019` 可并行，`T020` 最后同步 agent 指南
- Phase 6: `T021` 与 `T022` 可并行

---

## Parallel Example: User Story 1

```text
# 可并行处理的 .NET 10 兼容修复：
T009 Shared app entry files
T010 Core services
T011 Test fixtures and assertions

# 完成后统一收敛：
T012 Command-chain verification
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. 完成 Phase 1: Setup
2. 完成 Phase 2: Foundational
3. 完成 Phase 3: User Story 1
4. **停止并验证**：主应用与测试工程在 .NET 10 下独立可构建、可测试
5. 以此作为升级工作的 MVP 演示

### Incremental Delivery

1. Setup + Foundational → 统一 .NET 10 基线完成
2. User Story 1 → 构建与测试链路恢复
3. User Story 2 → 多平台连续性与核心入口验收完成
4. User Story 3 → 仓库说明、规范和交接文档完成
5. Polish → 最终回归与手动验收完成

### Parallel Team Strategy

1. 一名开发者完成 Phase 1 + Phase 2
2. 之后并行：
   - 开发者 A：US1 构建/测试兼容修复
   - 开发者 B：US2 多平台与主流程抽样验证
   - 开发者 C：US3 文档、规范与交接说明同步
3. 最后统一完成回归验证、手动验收与任务收敛

---

## Notes

- `[P]` tasks = 不同文件、依赖少，适合并行
- `[USx]` 标签保证任务可追溯到具体用户故事
- 本特性不要求新增测试优先开发流程，但要求现有构建与测试链路在升级后持续通过
- 每个故事都应能独立演示，不应依赖其他故事完成后才能确认价值
- SDK 基线、目标框架、平台连续性与仓库说明一致性是本特性的四条核心验收主线
