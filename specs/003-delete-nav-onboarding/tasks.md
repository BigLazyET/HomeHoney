# Tasks: 资料删除、返回导航与首次引导优化

**Input**: Design documents from `/specs/003-delete-nav-onboarding/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: 本特性规范未明确要求新增自动化测试任务，因此本任务列表以功能实现、现有测试回归和手动验收为主，不单独生成测试优先任务。

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **扁平结构**:
  - 主应用: `HomeHoney/`
  - 测试: `HomeHoney.Tests/`
  - 规范文档: `specs/003-delete-nav-onboarding/`

---

## Phase 1: Setup (环境准备)

**Purpose**: 为删除操作与多层返回导航准备共享组件和基础路由钩子

- [X] T001 Create shared nested-page header component file `HomeHoney/Components/Shared/PageHeader.razor` for reusable title, back action, and page-level action slots
- [X] T002 [P] Extend route constants and parent-route helpers in `HomeHoney/Services/Navigation/AppRoutes.cs` for documents, fridge notes, settings, and onboarding preview flows
- [X] T003 [P] Update shared UI imports and base style hooks in `HomeHoney/Components/_Imports.razor` and `HomeHoney/wwwroot/css/app.css` for page header and destructive action styling

---

## Phase 2: Foundational (基础能力 — 阻塞型前置条件)

**Purpose**: 构建所有用户故事共用的删除、返回和引导状态基础能力

**⚠️ CRITICAL**: 用户故事实现前必须完成此阶段

- [X] T004 [P] Extend delete operations for insurance and manual records in `HomeHoney/Services/Documents/DocumentCatalogService.cs`
- [X] T005 [P] Extend fridge note delete operations and post-delete list behavior in `HomeHoney/Services/Collaboration/FamilyCollaborationService.cs`
- [X] T006 [P] Refine onboarding persistence semantics in `HomeHoney/Services/Preferences/UserPreferenceService.cs` so passive guide viewing does not reset default startup behavior
- [X] T007 Implement reusable nested-page header/back-navigation behavior in `HomeHoney/Components/Shared/PageHeader.razor` and `HomeHoney/wwwroot/css/app.css`
- [X] T008 Wire startup gating and manual guide access rules in `HomeHoney/Components/Routes.razor` together with route helpers in `HomeHoney/Services/Navigation/AppRoutes.cs`

**Checkpoint**: 删除 API、共享返回导航和首次引导规则已统一，后续用户故事可并行展开

---

## Phase 3: User Story 1 - 删除无用资料与冰箱贴 (Priority: P1) 🎯 MVP

**Goal**: 用户能够删除保险、说明书和冰箱贴内容，并在删除后回到仍然有效的模块页面

**Independent Test**: 从资料详情页或冰箱贴编辑页删除一条现有记录，验证内容从列表、首页摘要与查找结果中消失，且页面返回到所属模块列表

### Implementation for User Story 1

- [X] T009 [P] [US1] Add confirmed delete actions to `HomeHoney/Components/Pages/Documents/Insurance/InsuranceDetail.razor` and `HomeHoney/Components/Pages/Documents/Insurance/InsuranceForm.razor`
- [X] T010 [P] [US1] Add confirmed delete actions to `HomeHoney/Components/Pages/Documents/Manuals/ManualDetail.razor` and `HomeHoney/Components/Pages/Documents/Manuals/ManualForm.razor`
- [X] T011 [P] [US1] Add confirmed delete action to `HomeHoney/Components/Pages/FridgeNotes/FridgeNoteForm.razor`
- [X] T012 [US1] Update delete-after-return and empty-state handling in `HomeHoney/Components/Pages/Documents/Insurance/InsuranceList.razor`, `HomeHoney/Components/Pages/Documents/Manuals/ManualList.razor`, and `HomeHoney/Components/Pages/FridgeNotes/FridgeNoteBoard.razor`
- [X] T013 [US1] Ensure deleted records no longer surface in `HomeHoney/Components/Pages/Home.razor`, `HomeHoney/Components/Pages/Reminders/Search.razor`, and `HomeHoney/Components/Pages/Reminders/ReminderCenter.razor`

**Checkpoint**: 资料与冰箱贴删除闭环已可独立演示

---

## Phase 4: User Story 2 - 多层页面可返回上一级 (Priority: P1)

**Goal**: 用户在所有二级及更深页面都能通过统一返回按钮回到清晰的上一级页面

**Independent Test**: 从资料首页进入列表、详情、编辑页，以及从设置首页进入子页，验证每个多层页面都显示返回按钮并正确回到上一级上下文

### Implementation for User Story 2

- [X] T014 [P] [US2] Apply shared back-navigation header to `HomeHoney/Components/Pages/Documents/Insurance/InsuranceList.razor`, `HomeHoney/Components/Pages/Documents/Insurance/InsuranceDetail.razor`, and `HomeHoney/Components/Pages/Documents/Insurance/InsuranceForm.razor`
- [X] T015 [P] [US2] Apply shared back-navigation header to `HomeHoney/Components/Pages/Documents/Manuals/ManualList.razor`, `HomeHoney/Components/Pages/Documents/Manuals/ManualDetail.razor`, and `HomeHoney/Components/Pages/Documents/Manuals/ManualForm.razor`
- [X] T016 [P] [US2] Apply shared back-navigation header to `HomeHoney/Components/Pages/FridgeNotes/FridgeNoteForm.razor`, `HomeHoney/Components/Pages/Settings/ThemeSettings.razor`, `HomeHoney/Components/Pages/Settings/NotificationSettings.razor`, `HomeHoney/Components/Pages/Settings/HomeLayoutSettings.razor`, and `HomeHoney/Components/Pages/Settings/PrivacySettings.razor`
- [X] T017 [P] [US2] Apply shared back-navigation header to `HomeHoney/Components/Pages/Welcome/Features.razor` and `HomeHoney/Components/Pages/Welcome/GetStarted.razor`
- [X] T018 [US2] Refine page-header layout, back-button placement, and destructive action spacing in `HomeHoney/wwwroot/css/app.css` and `HomeHoney/Components/Layout/MainLayout.razor`

**Checkpoint**: 所有多层页面都具备一致、可预期的返回路径

---

## Phase 5: User Story 3 - 引导仅首次自动出现 (Priority: P2)

**Goal**: 引导只在首次自动启动时展示，后续再次打开应用直接进入主页面，而主动回看引导不改变默认启动行为

**Independent Test**: 首次完成引导后关闭并重新打开应用，验证不再自动进入引导；再从设置或帮助重新查看引导后再次启动，仍直接进入主页面

### Implementation for User Story 3

- [X] T019 [P] [US3] Separate onboarding completion from passive guide revisit behavior in `HomeHoney/Services/Preferences/UserPreferenceService.cs`
- [X] T020 [P] [US3] Update startup redirect and manual guide routing behavior in `HomeHoney/Components/Routes.razor` and `HomeHoney/Services/Navigation/AppRoutes.cs`
- [X] T021 [P] [US3] Replace onboarding-reset links with passive guide revisit flows in `HomeHoney/Components/Pages/Settings/SettingsHome.razor` and `HomeHoney/Components/Pages/Settings/PrivacySettings.razor`
- [X] T022 [US3] Align onboarding completion and revisit actions across `HomeHoney/Components/Pages/Welcome/Welcome.razor`, `HomeHoney/Components/Pages/Welcome/Features.razor`, and `HomeHoney/Components/Pages/Welcome/GetStarted.razor`

**Checkpoint**: 首次引导与后续启动逻辑已独立成立，可单独演示

---

## Phase 6: Polish & Cross-Cutting Concerns (优化与横切关注点)

**Purpose**: 完善文案、回归验证与最终验收

- [X] T023 [P] Refine destructive confirmation copy, empty states, and missing-record messaging in `HomeHoney/Components/Pages/Documents/**`, `HomeHoney/Components/Pages/FridgeNotes/FridgeNoteForm.razor`, and `HomeHoney/wwwroot/css/app.css`
- [X] T024 [P] Sync behavioral documentation in `README.md`, `specs/003-delete-nav-onboarding/contracts/ui-behavior-contract.md`, and `specs/003-delete-nav-onboarding/quickstart.md`
- [ ] T025 Run manual validation scenarios from `specs/003-delete-nav-onboarding/quickstart.md`
- [X] T026 Verify the feature build and regression test flow against `HomeHoney.sln` and `HomeHoney.Tests/HomeHoney.Tests.csproj`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 无依赖，可立即开始
- **Foundational (Phase 2)**: 依赖 Phase 1 完成，阻塞所有用户故事
- **User Story 1 (Phase 3)**: 依赖 Phase 2 完成，构成本特性 MVP
- **User Story 2 (Phase 4)**: 依赖 Phase 2 完成；可与 US1 并行，但建议在共享页头就位后统一推进
- **User Story 3 (Phase 5)**: 依赖 Phase 2 完成；与 US1/US2 逻辑独立，可并行实施
- **Polish (Phase 6)**: 依赖目标用户故事完成后执行

### User Story Dependencies

- **User Story 1 (P1)**: 依赖删除服务能力和共享导航基础，可独立交付删除闭环
- **User Story 2 (P1)**: 依赖共享页头与父级路由能力，可独立交付多层页面返回体验
- **User Story 3 (P2)**: 依赖引导状态持久化和路由分流能力，可独立交付首次启动行为优化

### Within Each User Story

- 先完成服务或路由基础，再接入页面行为
- 先统一共享导航模式，再逐页替换多层页面头部
- 完成故事独立验收后，再进入后续整体验证

### Parallel Opportunities

- Phase 1: `T002` 与 `T003` 可并行
- Phase 2: `T004`、`T005`、`T006` 可并行，`T007` 与 `T008` 可在其后收敛
- Phase 3: `T009`、`T010`、`T011` 可按不同页面组并行
- Phase 4: `T014`、`T015`、`T016`、`T017` 可按页面组并行
- Phase 5: `T019`、`T020`、`T021` 可并行，`T022` 最后收敛欢迎流
- Phase 6: `T023` 与 `T024` 可并行

---

## Parallel Example: User Story 2

```text
# 可并行处理的多层页面返回改造：
T014 Insurance pages
T015 Manual pages
T016 Fridge note + settings subpages
T017 Welcome nested pages

# 完成后统一收敛：
T018 Layout and style refinement
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. 完成 Phase 1: Setup
2. 完成 Phase 2: Foundational
3. 完成 Phase 3: User Story 1
4. **停止并验证**：资料和冰箱贴删除闭环独立可用
5. 以此作为本特性的 MVP 演示

### Incremental Delivery

1. Setup + Foundational → 删除、返回、引导的公共基础完成
2. User Story 1 → 删除闭环落地
3. User Story 2 → 多层页面返回体验落地
4. User Story 3 → 首次引导与后续启动行为落地
5. Polish → 文案、文档、回归验证与手动验收

### Parallel Team Strategy

1. 一名开发者完成 Phase 1 + Phase 2
2. 之后并行：
   - 开发者 A：US1 删除闭环
   - 开发者 B：US2 返回导航统一
   - 开发者 C：US3 引导状态与路由分流
3. 最后由一名开发者统一完成 polish、文档和回归验证

---

## Notes

- `[P]` tasks = 不同文件、依赖少，适合并行
- `[USx]` 标签保证任务可追溯到具体用户故事
- 本特性未显式要求新增测试文件，因此不单独创建测试优先任务
- 每个故事都应能独立演示，不应依赖其他故事完成后才能验证价值
- 删除后的跳转、返回按钮与首次引导状态是本特性最核心的三条验收主线
