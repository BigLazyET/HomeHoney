# Tasks: Improve App Usability

**Input**: Design documents from `/specs/001-improve-app-usability/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/ui-state-contract.md, quickstart.md

**Tests**: 本特性未显式要求先写测试任务；以下任务以实现为主，并在最终阶段执行现有自动化与 quickstart 验证。  
**Organization**: Tasks are grouped by user story to enable independent implementation and validation.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g. `US1`, `US2`)
- Every task includes exact file paths

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 建立本特性会复用的共享状态模型与样式基础

- [X] T001 Create shared UX state models in HomeHoney/Models/ValidationIssue.cs, HomeHoney/Models/RetryableCollectionState.cs, and HomeHoney/Models/PresentationFallback.cs
- [X] T002 [P] Extend shared form, empty-state, and retry styling in HomeHoney/wwwroot/css/app.css

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 为所有用户故事准备统一校验、共享反馈和后端索引初始化骨架

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Create shared blank-draft and validation guard helpers in HomeHoney/Services/Documents/DocumentCatalogService.cs and HomeHoney/Services/Collaboration/FamilyCollaborationService.cs
- [X] T004 [P] Normalize shared banner and empty-state behavior in HomeHoney/Components/Shared/StatusBanner.razor and HomeHoney/Components/Shared/EmptyState.razor
- [X] T005 [P] Scaffold Mongo index bootstrap wiring in HomeHoney.Api/Infrastructure/Mongo/MongoIndexInitializer.cs, HomeHoney.Api/Infrastructure/Configuration/DependencyInjection.cs, and HomeHoney.Api/Program.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - 更清晰的表单录入 (Priority: P1) 🎯 MVP

**Goal**: 让说明书、保险、备忘录和冰箱贴录入流程从“误导性默认值”转为“空白起步 + 明确提示 + 保留已输入内容”

**Independent Test**: 分别打开四类新增/编辑表单并直接保存、部分填写后再保存，确认页面会阻止无效提交、保留已输入内容，并对历史空值显示明确缺省文案。

### Implementation for User Story 1

- [X] T006 [P] [US1] Remove misleading model and template defaults in HomeHoney/Models/InsuranceRecord.cs, HomeHoney/Models/ManualRecord.cs, HomeHoney/Models/FridgeNote.cs, and HomeHoney/Models/Memo.cs
- [X] T007 [US1] Rework insurance and manual form interactions for blank-start entry, required prompts, and preserved input in HomeHoney/Components/Pages/Documents/Insurance/InsuranceForm.razor and HomeHoney/Components/Pages/Documents/Manuals/ManualForm.razor
- [X] T008 [US1] Rework memo and fridge-note form interactions for blank-start entry, required prompts, and preserved input in HomeHoney/Components/Pages/Memos/MemoForm.razor and HomeHoney/Components/Pages/FridgeNotes/FridgeNoteForm.razor
- [X] T009 [P] [US1] Add user-friendly missing-value rendering in HomeHoney/Components/Pages/Documents/Insurance/InsuranceDetail.razor, HomeHoney/Components/Pages/Documents/Manuals/ManualDetail.razor, HomeHoney/Components/Pages/Memos/MemoDetail.razor, and HomeHoney/Components/Pages/FridgeNotes/FridgeNoteBoard.razor
- [X] T010 [US1] Harden save-time validation and user-visible field prompts in HomeHoney/Services/Documents/DocumentCatalogService.cs and HomeHoney/Services/Collaboration/FamilyCollaborationService.cs

**Checkpoint**: User Story 1 should now support empty-start forms, required-field prompts, and null-safe edit/detail rendering

---

## Phase 4: User Story 2 - 可感知的附件状态 (Priority: P2)

**Goal**: 让说明书和保险详情页直接展示附件文件名、大小、最后同步时间及明确状态

**Independent Test**: 打开有附件和无附件的保险/说明书详情页，确认附件区域均能展示元数据或缺省状态；上传附件后再次进入详情页可看到最新元数据。

### Implementation for User Story 2

- [X] T011 [US2] Populate insurance/manual API read models with filename, size, sync time, and fallback file state in HomeHoney.Api/Application/Documents/InsuranceDocumentService.cs, HomeHoney.Api/Application/Documents/ManualDocumentService.cs, and HomeHoney.Api/Contracts/Documents/FileBackedDocumentDto.cs
- [X] T012 [US2] Refresh upload/download result mapping to preserve attachment metadata in HomeHoney/Services/Documents/DocumentApiClient.cs and HomeHoney/Services/Documents/DocumentCatalogService.cs
- [X] T013 [US2] Display attachment filename, size, last sync, and partial-state copy in HomeHoney/Components/Pages/Documents/Insurance/InsuranceDetail.razor and HomeHoney/Components/Pages/Documents/Manuals/ManualDetail.razor
- [X] T014 [P] [US2] Add attachment metadata layout and fallback styling in HomeHoney/wwwroot/css/app.css

**Checkpoint**: User Story 2 should now make attachment state visible without requiring download attempts

---

## Phase 5: User Story 3 - 失败后的手动重试 (Priority: P3)

**Goal**: 让资料列表、提醒中心和搜索页在读取失败后支持页面内手动重试，并明确区分空状态与错误状态

**Independent Test**: 制造读取失败后打开资料列表、提醒中心和搜索页，确认每个页面都显示失败反馈和手动重试按钮；恢复服务后可在当前页成功重试。

### Implementation for User Story 3

- [X] T015 [US3] Add retryable documents-list state and retry actions in HomeHoney/Components/Pages/Documents/Insurance/InsuranceList.razor and HomeHoney/Components/Pages/Documents/Manuals/ManualList.razor
- [X] T016 [US3] Add retryable reminder and search page states in HomeHoney/Components/Pages/Reminders/ReminderCenter.razor and HomeHoney/Components/Pages/Reminders/Search.razor
- [X] T017 [US3] Support retained-data retries and state resets in HomeHoney/Services/Documents/DocumentCatalogService.cs, HomeHoney/Services/Reminders/ReminderCenterService.cs, and HomeHoney/Services/Search/SearchIndexService.cs
- [X] T018 [P] [US3] Extend retry-button and loading-state presentation in HomeHoney/Components/Shared/StatusBanner.razor and HomeHoney/wwwroot/css/app.css

**Checkpoint**: User Story 3 should now recover failed reads from within the active page without forcing a full app refresh

---

## Phase 6: User Story 4 - 更稳健的日常使用体验 (Priority: P4)

**Goal**: 补齐空数据、历史脏数据、异常隔离和 Mongo 索引策略，使应用在数据增长后仍保持稳定和可检索

**Independent Test**: 使用空集合、缺失字段、部分历史脏数据和较大数据量场景验证页面仍可显示；启动后端并确认索引初始化完成，常见列表与检索路径仍正常返回。

### Implementation for User Story 4

- [X] T019 [US4] Review and fix null-safe summaries, fallback copy, and module isolation in HomeHoney/Components/Pages/Home.razor, HomeHoney/Components/Pages/Documents/DocumentsHome.razor, HomeHoney/Components/Pages/Memos/MemoDetail.razor, and HomeHoney/Components/Pages/FridgeNotes/FridgeNoteBoard.razor
- [X] T020 [US4] Refine repository sorting and filter paths to align with indexed access in HomeHoney.Api/Infrastructure/Mongo/Repositories/DocumentRepository.cs, HomeHoney.Api/Infrastructure/Mongo/Repositories/CollaborationRepository.cs, and HomeHoney.Api/Infrastructure/Mongo/Repositories/PreferenceRepository.cs
- [X] T021 [P] [US4] Define and ensure collection indexes for all current Mongo collections in HomeHoney.Api/Infrastructure/Mongo/MongoIndexInitializer.cs, HomeHoney.Api/Infrastructure/Mongo/Collections/DocumentCollections.cs, and HomeHoney.Api/Infrastructure/Mongo/Collections/BusinessCollections.cs
- [X] T022 [US4] Align search and reminder aggregation with null-safe, index-friendly fields in HomeHoney.Api/Application/Search/SearchReadService.cs, HomeHoney.Api/Application/Search/SearchAggregationService.cs, HomeHoney.Api/Application/Reminders/ReminderReadService.cs, and HomeHoney.Api/Application/Reminders/ReminderAggregationService.cs
- [X] T023 [US4] Finalize backend startup wiring and logging for index initialization in HomeHoney.Api/Infrastructure/Configuration/DependencyInjection.cs and HomeHoney.Api/Program.cs

**Checkpoint**: User Story 4 should now improve resilience for empty/partial data and prepare Mongo for stable long-term query performance

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: 收尾文档、验证与跨故事一致性

- [X] T024 [P] Update feature verification notes in specs/001-improve-app-usability/quickstart.md and README.md
- [X] T025 Run the acceptance and regression checklist from specs/001-improve-app-usability/quickstart.md against HomeHoney/HomeHoney.csproj, HomeHoney.Api/HomeHoney.Api.csproj, HomeHoney.Tests/HomeHoney.Tests.csproj, and HomeHoney.Api.Tests/HomeHoney.Api.Tests.csproj

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1: Setup** — no dependencies, can start immediately
- **Phase 2: Foundational** — depends on Phase 1 and blocks all story work
- **Phase 3: US1** — depends on Phase 2 only; this is the MVP slice
- **Phase 4: US2** — depends on Phase 2; can run after or alongside US1 if capacity allows
- **Phase 5: US3** — depends on Phase 2; can run after or alongside US1/US2
- **Phase 6: US4** — depends on Phase 2 and should land after the core UX stories stabilize
- **Phase 7: Polish** — depends on all desired stories being complete

### User Story Dependencies

- **US1 (P1)**: No dependency on other stories after Phase 2
- **US2 (P2)**: Reuses document services but remains independently testable from the detail pages
- **US3 (P3)**: Reuses shared retry/banner patterns but remains independently testable from the affected list pages
- **US4 (P4)**: Builds on shared UX patterns and backend repository behavior, but can still be validated through empty/partial-data and indexing scenarios

### Within Each User Story

- Shared model/helper updates before page-level wiring
- Service/API adjustments before final UI polish for that story
- Story-specific validation before moving to the next priority slice

---

## Parallel Opportunities

- **Phase 1**: T001 and T002 can proceed in parallel once the shared UX approach is agreed
- **Phase 2**: T004 and T005 can run in parallel after T003 starts the shared guard patterns
- **US1**: T006 and T009 can run in parallel; T007 and T008 can be split by document vs collaboration forms
- **US2**: T014 can run in parallel with T011/T012 while API and UI metadata wiring is in progress
- **US3**: T015 and T016 can run in parallel; T018 can run alongside them after retry behavior is defined
- **US4**: T021 can run in parallel with T019/T020; T022 can begin once repository/index field choices are fixed

---

## Parallel Example: User Story 1

```bash
# Parallelizable form-foundation work for US1
Task: "Remove misleading model and template defaults in HomeHoney/Models/InsuranceRecord.cs, HomeHoney/Models/ManualRecord.cs, HomeHoney/Models/FridgeNote.cs, and HomeHoney/Models/Memo.cs"
Task: "Add user-friendly missing-value rendering in HomeHoney/Components/Pages/Documents/Insurance/InsuranceDetail.razor, HomeHoney/Components/Pages/Documents/Manuals/ManualDetail.razor, HomeHoney/Components/Pages/Memos/MemoDetail.razor, and HomeHoney/Components/Pages/FridgeNotes/FridgeNoteBoard.razor"

# Parallelizable page implementation split for US1
Task: "Rework insurance and manual form interactions in HomeHoney/Components/Pages/Documents/Insurance/InsuranceForm.razor and HomeHoney/Components/Pages/Documents/Manuals/ManualForm.razor"
Task: "Rework memo and fridge-note form interactions in HomeHoney/Components/Pages/Memos/MemoForm.razor and HomeHoney/Components/Pages/FridgeNotes/FridgeNoteForm.razor"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Validate the form-entry journey independently
5. Demo/deploy the MVP slice if ready

### Incremental Delivery

1. Setup + Foundational → shared UX and backend index bootstrap ready
2. Add US1 → validate blank-start forms and required prompts
3. Add US2 → validate visible attachment metadata
4. Add US3 → validate page-level retry recovery
5. Add US4 → validate resilience and indexing improvements
6. Finish with Phase 7 validation and documentation

### Suggested MVP Scope

- **MVP**: User Story 1 only
- **Next most valuable increment**: User Story 2
- **Operational recovery increment**: User Story 3
- **Scalability/resilience increment**: User Story 4

---

## Notes

- [P] tasks touch different files and can be assigned in parallel
- [US#] labels map directly to user stories in spec.md
- Each user story is independently testable using the acceptance approach described in spec.md and quickstart.md
- Avoid merging cross-story work before the current story’s checkpoint is validated
- The prerequisite script reported multiple `001-*` spec prefixes in the repository; tasks were generated for `/specs/001-improve-app-usability/` explicitly
