# Tasks: App 页面结构与导航规划

**Input**: Design documents from `/specs/002-app-page-structure/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: 本特性规范未明确要求新增自动化测试任务，因此本任务列表以页面结构实现与手动验收为主，不单独生成测试优先任务。

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **扁平结构**:
  - 主应用: `HomeHoney/`
  - 测试: `HomeHoney.Tests/`
  - 规范文档: `specs/002-app-page-structure/`

---

## Phase 1: Setup (环境准备)

**Purpose**: 创建本特性所需的页面、组件、服务与模型目录骨架

- [X] T001 Create feature page folders and placeholder files under `HomeHoney/Components/Pages/Welcome/`, `HomeHoney/Components/Pages/Documents/Insurance/`, `HomeHoney/Components/Pages/Documents/Manuals/`, `HomeHoney/Components/Pages/FridgeNotes/`, `HomeHoney/Components/Pages/Memos/`, `HomeHoney/Components/Pages/Reminders/`, and `HomeHoney/Components/Pages/Settings/`
- [X] T002 Create shared component placeholder files `HomeHoney/Components/Shared/DocumentCard.razor`, `HomeHoney/Components/Shared/ReminderCard.razor`, `HomeHoney/Components/Shared/QuickActionGrid.razor`, and `HomeHoney/Components/Shared/FilterBar.razor`
- [X] T003 Create service and model folders `HomeHoney/Services/Navigation/`, `HomeHoney/Services/Documents/`, `HomeHoney/Services/Search/`, `HomeHoney/Services/Reminders/`, `HomeHoney/Services/Preferences/`, `HomeHoney/Services/Collaboration/`, and `HomeHoney/Models/`
- [X] T004 [P] Update `HomeHoney/Components/_Imports.razor` to include namespaces for new page groups, shared components, services, and models
- [X] T005 [P] Create route constants file `HomeHoney/Services/Navigation/AppRoutes.cs` covering welcome, documents, fridge notes, memos, reminders/search, and settings paths from `specs/002-app-page-structure/contracts/ui-navigation-contract.md`

---

## Phase 2: Foundational (基础能力 — 阻塞型前置条件)

**Purpose**: 构建所有用户故事共用的实体、服务和共享组件

**⚠️ CRITICAL**: 用户故事实现前必须完成此阶段

- [X] T006 [P] Create foundational model files `HomeHoney/Models/HouseholdMember.cs`, `HomeHoney/Models/Space.cs`, `HomeHoney/Models/HomeModule.cs`, and `HomeHoney/Models/UserPreference.cs`
- [X] T007 [P] Create document model files `HomeHoney/Models/InsuranceRecord.cs` and `HomeHoney/Models/ManualRecord.cs`
- [X] T008 [P] Create collaboration model files `HomeHoney/Models/FridgeNote.cs`, `HomeHoney/Models/Memo.cs`, and `HomeHoney/Models/ReminderItem.cs`
- [X] T009 [P] Implement navigation metadata service in `HomeHoney/Services/Navigation/NavigationStructureService.cs` for bottom tabs, documents sub-navigation, and deep-link descriptors
- [X] T010 [P] Implement preference service in `HomeHoney/Services/Preferences/UserPreferenceService.cs` for theme mode, onboarding completion, home module visibility, notification settings, and privacy mode
- [X] T011 [P] Implement search source contracts in `HomeHoney/Services/Search/SearchIndexService.cs` for grouped cross-module search results
- [X] T012 [P] Implement reminder aggregation service in `HomeHoney/Services/Reminders/ReminderCenterService.cs` for upcoming 30-day items and source jump targets
- [X] T013 [P] Implement collaboration data service in `HomeHoney/Services/Collaboration/FamilyCollaborationService.cs` for fridge notes and memos sample data plus state transitions
- [X] T014 Implement shared UI shells in `HomeHoney/Components/Shared/DocumentCard.razor`, `HomeHoney/Components/Shared/ReminderCard.razor`, `HomeHoney/Components/Shared/QuickActionGrid.razor`, and `HomeHoney/Components/Shared/FilterBar.razor`
- [X] T015 Register navigation, preference, search, reminder, and collaboration services in `HomeHoney/MauiProgram.cs`

**Checkpoint**: 基础模型、服务、共享组件与路由常量就位，后续页面可并行实现

---

## Phase 3: User Story 1 - 欢迎引导与首页导航 (Priority: P1) 🎯 MVP

**Goal**: 首次使用者能够完成欢迎引导并进入带有稳定一级导航的首页摘要页

**Independent Test**: 首次启动应用后完成引导进入首页，能直接找到资料、冰箱贴、提醒和设置入口；再次进入应用默认回到首页

### Implementation for User Story 1

- [X] T016 [P] [US1] Implement welcome landing page in `HomeHoney/Components/Pages/Welcome/Welcome.razor`
- [X] T017 [P] [US1] Implement feature introduction page in `HomeHoney/Components/Pages/Welcome/Features.razor`
- [X] T018 [P] [US1] Implement onboarding completion page in `HomeHoney/Components/Pages/Welcome/GetStarted.razor`
- [X] T019 [US1] Update `HomeHoney/Components/Routes.razor` to support welcome flow routes and default redirect behavior based on onboarding completion from `UserPreferenceService`
- [X] T020 [US1] Implement dashboard-style home page in `HomeHoney/Components/Pages/Home.razor` with next actions, module entry cards, and empty-state guidance
- [X] T021 [US1] Update `HomeHoney/Components/Layout/MainLayout.razor` to provide the final mobile-first page shell for welcome/home/navigation flows
- [X] T022 [US1] Update `HomeHoney/Components/Layout/NavMenu.razor` and `HomeHoney/Components/Layout/NavMenu.razor.css` to render the five-tab bottom navigation defined in `specs/002-app-page-structure/contracts/ui-navigation-contract.md`
- [X] T023 [US1] Update `HomeHoney/wwwroot/css/app.css` to style onboarding, home dashboard, quick actions, and stable bottom navigation for mobile-first use

**Checkpoint**: 欢迎引导和首页导航可独立演示，形成 MVP 入口层

---

## Phase 4: User Story 2 - 保险与说明书资料管理 (Priority: P1)

**Goal**: 用户能够在资料模块内管理保险和说明书，并使用统一的文档型交互模式完成浏览、查看与录入

**Independent Test**: 从资料入口分别进入保险与说明书，验证列表、详情、新增三类页面都存在且能形成完整闭环

### Implementation for User Story 2

- [X] T024 [P] [US2] Implement documents landing page in `HomeHoney/Components/Pages/Documents/DocumentsHome.razor` with insurance/manual entry cards
- [X] T025 [P] [US2] Implement insurance list page in `HomeHoney/Components/Pages/Documents/Insurance/InsuranceList.razor`
- [X] T026 [P] [US2] Implement insurance detail page in `HomeHoney/Components/Pages/Documents/Insurance/InsuranceDetail.razor`
- [X] T027 [P] [US2] Implement insurance create/edit form page in `HomeHoney/Components/Pages/Documents/Insurance/InsuranceForm.razor`
- [X] T028 [P] [US2] Implement manual list page in `HomeHoney/Components/Pages/Documents/Manuals/ManualList.razor`
- [X] T029 [P] [US2] Implement manual detail page in `HomeHoney/Components/Pages/Documents/Manuals/ManualDetail.razor`
- [X] T030 [P] [US2] Implement manual create/edit form page in `HomeHoney/Components/Pages/Documents/Manuals/ManualForm.razor`
- [X] T031 [US2] Create seeded document catalog service in `HomeHoney/Services/Documents/DocumentCatalogService.cs` to provide list/detail/form data contracts for insurance and manuals
- [X] T032 [US2] Integrate `HomeHoney/Components/Shared/DocumentCard.razor` and `HomeHoney/Components/Shared/FilterBar.razor` into all document list/detail pages under `HomeHoney/Components/Pages/Documents/`
- [X] T033 [US2] Wire document routes and deep links in `HomeHoney/Components/Routes.razor`, `HomeHoney/Components/Pages/Documents/DocumentsHome.razor`, and `HomeHoney/Components/Pages/Home.razor`

**Checkpoint**: 资料模块可独立使用，保险与说明书共享一致的信息架构与录入闭环

---

## Phase 5: User Story 3 - 家庭冰箱贴与备忘录协作 (Priority: P2)

**Goal**: 用户能够用冰箱贴记录高频短事项，用备忘录管理正式长期事项，并支持重要/待办/归档状态

**Independent Test**: 分别进入冰箱贴与备忘录页面，完成创建、查看、标记状态与回到列表的完整闭环

### Implementation for User Story 3

- [X] T034 [P] [US3] Implement fridge note board page in `HomeHoney/Components/Pages/FridgeNotes/FridgeNoteBoard.razor`
- [X] T035 [P] [US3] Implement fridge note create/edit page in `HomeHoney/Components/Pages/FridgeNotes/FridgeNoteForm.razor`
- [X] T036 [P] [US3] Implement memo list page in `HomeHoney/Components/Pages/Memos/MemoList.razor`
- [X] T037 [P] [US3] Implement memo detail page in `HomeHoney/Components/Pages/Memos/MemoDetail.razor`
- [X] T038 [P] [US3] Implement memo create/edit page in `HomeHoney/Components/Pages/Memos/MemoForm.razor`
- [X] T039 [US3] Extend `HomeHoney/Services/Collaboration/FamilyCollaborationService.cs` to support pinned, important, todo, completed, and archived states for fridge notes and memos
- [X] T040 [US3] Apply collaboration visual rules in `HomeHoney/Components/Pages/FridgeNotes/FridgeNoteBoard.razor`, `HomeHoney/Components/Pages/Memos/MemoList.razor`, `HomeHoney/Components/Pages/Memos/MemoDetail.razor`, and `HomeHoney/wwwroot/css/app.css`
- [X] T041 [US3] Wire fridge note and memo routes from `HomeHoney/Components/Routes.razor`, `HomeHoney/Components/Layout/NavMenu.razor`, and `HomeHoney/Components/Pages/Home.razor`

**Checkpoint**: 家庭协作模块可独立演示，支持短便签与正式备忘两种使用模式

---

## Phase 6: User Story 4 - 搜索、提醒与家庭首页摘要 (Priority: P2)

**Goal**: 用户能够通过首页摘要、搜索和提醒中心快速发现跨模块内容并跳转到目标详情页

**Independent Test**: 在存在多模块数据时，使用搜索和提醒中心在 30 秒内定位目标内容并跳转到对应详情页

### Implementation for User Story 4

- [X] T042 [P] [US4] Implement reminder center page in `HomeHoney/Components/Pages/Reminders/ReminderCenter.razor`
- [X] T043 [P] [US4] Implement global search page in `HomeHoney/Components/Pages/Reminders/Search.razor`
- [X] T044 [US4] Extend `HomeHoney/Services/Search/SearchIndexService.cs` to group keyword results by insurance, manuals, fridge notes, and memos
- [X] T045 [US4] Extend `HomeHoney/Services/Reminders/ReminderCenterService.cs` to classify upcoming, urgent, and completed items with detail jump targets
- [X] T046 [US4] Update `HomeHoney/Components/Pages/Home.razor` to render recent documents, upcoming reminders, and recommended quick actions using `ReminderCard` and `QuickActionGrid`
- [X] T047 [US4] Wire search and reminder deep links in `HomeHoney/Components/Routes.razor`, `HomeHoney/Components/Pages/Reminders/ReminderCenter.razor`, `HomeHoney/Components/Pages/Reminders/Search.razor`, and detail pages under `HomeHoney/Components/Pages/Documents/` and `HomeHoney/Components/Pages/Memos/`

**Checkpoint**: 首页摘要、统一搜索与提醒中心形成完整的跨模块发现与跳转闭环

---

## Phase 7: User Story 5 - 设置与个性化体验 (Priority: P3)

**Goal**: 用户能够管理主题、通知、首页布局与隐私偏好，并立即看到设置结果反映到页面结构中

**Independent Test**: 在设置页修改主题、通知和首页布局后，回到首页与相关页面验证结果已生效

### Implementation for User Story 5

- [X] T048 [P] [US5] Implement settings overview page in `HomeHoney/Components/Pages/Settings/SettingsHome.razor`
- [X] T049 [P] [US5] Implement theme settings page in `HomeHoney/Components/Pages/Settings/ThemeSettings.razor`
- [X] T050 [P] [US5] Implement notification settings page in `HomeHoney/Components/Pages/Settings/NotificationSettings.razor`
- [X] T051 [P] [US5] Implement home layout settings page in `HomeHoney/Components/Pages/Settings/HomeLayoutSettings.razor`
- [X] T052 [P] [US5] Implement privacy settings page in `HomeHoney/Components/Pages/Settings/PrivacySettings.razor`
- [X] T053 [US5] Extend `HomeHoney/Services/Preferences/UserPreferenceService.cs` to persist theme, notification, home-layout, privacy, and onboarding re-entry preferences
- [X] T054 [US5] Apply theme mode and home module visibility across `HomeHoney/App.xaml`, `HomeHoney/MainPage.xaml`, `HomeHoney/Components/Layout/MainLayout.razor`, `HomeHoney/Components/Pages/Home.razor`, and settings pages under `HomeHoney/Components/Pages/Settings/`
- [X] T055 [US5] Add settings routes and onboarding/help re-entry links in `HomeHoney/Components/Routes.razor`, `HomeHoney/Components/Pages/Settings/SettingsHome.razor`, and files under `HomeHoney/Components/Pages/Welcome/`

**Checkpoint**: 设置模块可独立演示，并能驱动首页与整体视觉的个性化变化

---

## Phase 8: Polish & Cross-Cutting Concerns (优化与横切关注点)

**Purpose**: 完善跨模块一致性、文档与最终验收

- [X] T056 [P] Refine empty states, Chinese labels, and visual consistency across `HomeHoney/Components/Pages/**`, `HomeHoney/Components/Shared/*.razor`, and `HomeHoney/wwwroot/css/app.css`
- [X] T057 [P] Sync route map and page responsibilities in `specs/002-app-page-structure/contracts/ui-navigation-contract.md`, `specs/002-app-page-structure/quickstart.md`, and `README.md`
- [ ] T058 Run the manual validation scenarios in `specs/002-app-page-structure/quickstart.md` for onboarding, documents, collaboration, search/reminders, and settings flows
- [X] T059 Verify implemented page tree and service layout against `specs/002-app-page-structure/plan.md` source structure section

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 无依赖，可立即开始
- **Foundational (Phase 2)**: 依赖 Phase 1 完成，阻塞所有用户故事
- **User Story 1 (Phase 3)**: 依赖 Phase 2 完成，构成 MVP
- **User Story 2 (Phase 4)**: 依赖 Phase 2 完成；与 US1 在开发上有协同，但应保持独立可演示
- **User Story 3 (Phase 5)**: 依赖 Phase 2 完成；可与 US2 并行
- **User Story 4 (Phase 6)**: 依赖 US2 与 US3 至少有基础页面和数据源可供聚合
- **User Story 5 (Phase 7)**: 依赖 US1 完成，并可在 US2/US3/US4 基础上逐步接入偏好能力
- **Polish (Phase 8)**: 依赖所有目标用户故事完成

### User Story Dependencies

- **User Story 1 (P1)**: 仅依赖基础能力，可单独形成 MVP
- **User Story 2 (P1)**: 依赖基础模型、文档卡片、筛选组件和文档数据服务
- **User Story 3 (P2)**: 依赖基础模型、协作服务和底部导航
- **User Story 4 (P2)**: 依赖 US2 和 US3 提供可被搜索与聚合的内容源
- **User Story 5 (P3)**: 依赖 US1 的导航与页面骨架，并逐步接入其余模块偏好设置

### Within Each User Story

- 先完成页面文件与路由，再接入服务与状态
- 先落地共享组件使用，再做跨页面联动
- 完成故事独立验收后，再进入下一优先级故事或横切整合

### Parallel Opportunities

- Phase 1: `T004`、`T005` 可并行
- Phase 2: `T006`~`T013` 大多可并行（不同文件）
- Phase 3: `T016`、`T017`、`T018` 可并行
- Phase 4: `T024`~`T030` 可按页面并行拆分
- Phase 5: `T034`~`T038` 可按冰箱贴 / 备忘录页面并行拆分
- Phase 6: `T042`、`T043` 可并行
- Phase 7: `T048`~`T052` 可并行
- Polish: `T056`、`T057` 可并行

---

## Parallel Example: User Story 2

```text
# 可并行开发的资料模块页面：
T025 InsuranceList.razor
T026 InsuranceDetail.razor
T027 InsuranceForm.razor
T028 ManualList.razor
T029 ManualDetail.razor
T030 ManualForm.razor

# 在页面文件完成后，再串联：
T031 DocumentCatalogService.cs
T032 DocumentCard/FilterBar integration
T033 Routes + deep links
```

---

## Implementation Strategy

### MVP First (仅 User Story 1)

1. 完成 Phase 1: Setup
2. 完成 Phase 2: Foundational
3. 完成 Phase 3: User Story 1
4. **停止并验证**：欢迎引导、首页摘要、一级导航独立可用
5. 以此作为页面架构 MVP 演示

### Incremental Delivery

1. Setup + Foundational → 页面骨架、模型、服务契约就绪
2. User Story 1 → 首次使用路径与导航落地
3. User Story 2 → 资料模块落地
4. User Story 3 → 家庭协作模块落地
5. User Story 4 → 搜索/提醒/摘要闭环落地
6. User Story 5 → 设置与个性化落地
7. Polish → 一致性收尾与手动验收

### Parallel Team Strategy

1. 一名开发者完成 Phase 1 + Phase 2
2. 之后并行：
   - 开发者 A：US1 / US5
   - 开发者 B：US2
   - 开发者 C：US3
3. US2 和 US3 完成后，由一名开发者收拢 US4 的跨模块搜索与提醒整合

---

## Notes

- `[P]` tasks = 不同文件、依赖少，适合并行
- `[USx]` 标签保证每个任务可追溯到明确的用户故事
- 本任务列表以页面结构实现为核心，自动化测试任务未作为本特性的显式范围
- 每个故事都应能独立展示并用 quickstart 手动脚本验证
- 避免在未完成基础服务前直接实现跨模块联动，以减少返工
