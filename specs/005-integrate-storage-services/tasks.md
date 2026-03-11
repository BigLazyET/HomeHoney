# Tasks: 集成外部文件与数据存储

**Input**: Design documents from `/specs/005-integrate-storage-services/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: 本特性规范未要求新增测试优先开发流程，因此任务以存储接入实现、现有测试回归、连接验证和手动验收为主。

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **扁平结构**:
  - 主应用: `HomeHoney/`
  - 测试: `HomeHoney.Tests/`
  - 规范文档: `specs/005-integrate-storage-services/`

---

## Phase 1: Setup (环境准备)

**Purpose**: 为外部文件服务、MongoDB 与平台级偏好/密钥存储建立依赖基线和目录入口

- [ ] T001 Add storage integration package references and SDK-aligned dependency placeholders in `HomeHoney/HomeHoney.csproj` and `HomeHoney.Tests/HomeHoney.Tests.csproj`
- [X] T002 [P] Create storage feature scaffolding under `HomeHoney/Services/Storage/` and planned storage models under `HomeHoney/Models/`
- [X] T003 [P] Record local development assumptions for FileBrowser and Mongo-backed validation in `README.md` and `specs/005-integrate-storage-services/quickstart.md`

---

## Phase 2: Foundational (基础能力 — 阻塞型前置条件)

**Purpose**: 建立配置、远端接入和依赖注入基础设施，阻塞所有用户故事

**⚠️ CRITICAL**: 用户故事实现前必须完成此阶段

- [X] T004 Create core storage models in `HomeHoney/Models/StorageConnectionProfile.cs`, `HomeHoney/Models/FileResource.cs`, and `HomeHoney/Models/SyncOperation.cs`
- [X] T005 [P] Extend existing domain models for file references and sync state in `HomeHoney/Models/InsuranceRecord.cs`, `HomeHoney/Models/ManualRecord.cs`, `HomeHoney/Models/UserPreference.cs`, and `HomeHoney/Models/ReminderItem.cs`
- [X] T006 [P] Implement editable connection profile and secret abstractions in `HomeHoney/Services/Storage/IStorageConnectionProfileService.cs`, `HomeHoney/Services/Storage/StorageConnectionProfileService.cs`, `HomeHoney/Services/Storage/ISecretStore.cs`, and `HomeHoney/Services/Storage/SecureSecretStore.cs`
- [X] T007 [P] Implement file service gateway contracts and result mapping in `HomeHoney/Services/Storage/IFileStorageGateway.cs`, `HomeHoney/Services/Storage/FileBrowserFileStorageGateway.cs`, and `HomeHoney/Services/Storage/FileStorageResult.cs`
- [X] T008 [P] Implement Mongo client factory and repository interfaces in `HomeHoney/Services/Storage/IMongoContextFactory.cs`, `HomeHoney/Services/Storage/MongoContextFactory.cs`, `HomeHoney/Services/Storage/IDocumentMetadataRepository.cs`, and `HomeHoney/Services/Storage/IBusinessAggregateRepository.cs`
- [X] T009 [P] Replace local cache persistence with platform-safe Preferences / SecureStorage boundaries and remove file-backed cache services
- [X] T010 Wire storage registrations, `HttpClient`, and repository dependencies in `HomeHoney/MauiProgram.cs` and `HomeHoney/HomeHoney.csproj`
- [X] T011 Add storage settings navigation and route entry points in `HomeHoney/Services/Navigation/AppRoutes.cs`, `HomeHoney/Services/Navigation/NavigationStructureService.cs`, and `HomeHoney/Components/Pages/Settings/SettingsHome.razor`

**Checkpoint**: 连接配置、远端网关和依赖注入基础设施就绪，可开始按用户故事推进

---

## Phase 3: User Story 1 - 配置外部存储连接 (Priority: P1) 🎯 MVP

**Goal**: 用户可在应用内修改并持久保存文件服务与数据服务连接目标，后续能力自动复用最新配置

**Independent Test**: 在设置页修改文件服务和数据服务配置，保存后重启应用仍能看到最新值，并且后续服务调用使用该配置

### Implementation for User Story 1

- [X] T012 [US1] Implement storage configuration validation and activation workflow in `HomeHoney/Services/Storage/StorageConfigurationValidator.cs`, `HomeHoney/Services/Storage/StorageConnectionProfileService.cs`, and `HomeHoney/Services/Preferences/UserPreferenceService.cs`
- [X] T013 [P] [US1] Build storage configuration UI in `HomeHoney/Components/Pages/Settings/StorageSettings.razor` and `HomeHoney/Components/Shared/StorageConnectionForm.razor`
- [X] T014 [US1] Connect the storage settings page to app routing and save feedback in `HomeHoney/Components/Routes.razor`, `HomeHoney/Components/Pages/Settings/SettingsHome.razor`, and `HomeHoney/Services/Navigation/AppRoutes.cs`
- [X] T015 [US1] Ensure active storage profiles load on app startup and are reused across operations in `HomeHoney/MauiProgram.cs` and `HomeHoney/Services/Storage/StorageConnectionProfileService.cs`

**Checkpoint**: 用户无需重新发布应用即可切换外部文件服务和 MongoDB 连接目标

---

## Phase 4: User Story 2 - 统一管理文件性质资源 (Priority: P2)

**Goal**: 保险合同、说明书等文件性质资源以“Mongo 元数据 + 外部文件服务文件本体”方式完成列表、上传、下载和状态识别

**Independent Test**: 在已有有效存储配置前提下，完成保险或说明书文件列表读取、上传一个新文件、打开详情并下载原文件

### Implementation for User Story 2

- [X] T016 [P] [US2] Implement file-backed document repositories and orchestration in `HomeHoney/Services/Storage/DocumentMetadataRepository.cs`, `HomeHoney/Services/Storage/DocumentFileOrchestrator.cs`, and `HomeHoney/Services/Storage/FileSyncStateMapper.cs`
- [X] T017 [US2] Refactor document reads and writes to use Mongo metadata, file gateway, and remote-state feedback in `HomeHoney/Services/Documents/DocumentCatalogService.cs` and `HomeHoney/Services/Search/SearchIndexService.cs`
- [ ] T018 [P] [US2] Add insurance file upload, download, and sync-state UI in `HomeHoney/Components/Pages/Documents/Insurance/InsuranceList.razor`, `HomeHoney/Components/Pages/Documents/Insurance/InsuranceDetail.razor`, and `HomeHoney/Components/Pages/Documents/Insurance/InsuranceForm.razor`
- [ ] T019 [P] [US2] Add manual file upload, download, and sync-state UI in `HomeHoney/Components/Pages/Documents/Manuals/ManualList.razor`, `HomeHoney/Components/Pages/Documents/Manuals/ManualDetail.razor`, and `HomeHoney/Components/Pages/Documents/Manuals/ManualForm.razor`
- [ ] T020 [US2] Surface file integrity, attachment availability, and reminder compatibility in `HomeHoney/Services/Reminders/ReminderCenterService.cs`, `HomeHoney/Components/Pages/Documents/DocumentsHome.razor`, and `HomeHoney/Models/InsuranceRecord.cs`
- [ ] T021 [US2] Add shared missing-file and partial-sync feedback styles in `HomeHoney/Components/Shared/EmptyState.razor` and `HomeHoney/wwwroot/css/app.css`

**Checkpoint**: 文件性质资源可通过外部文件服务完成核心读写，且用户能看懂缺失文件与同步异常状态

---

## Phase 5: User Story 3 - 管理非文件业务数据 (Priority: P3)

**Goal**: 冰箱贴、备忘录、提醒、偏好等非文件业务数据改为 MongoDB 持久化，并保留缓存和同步状态反馈

**Independent Test**: 对至少一类非文件业务数据完成新增、查询、修改、删除，重启应用后仍能看到最终结果，并能在服务失败时看到缓存/同步提示

### Implementation for User Story 3

- [X] T022 [P] [US3] Implement Mongo-backed non-file repositories in `HomeHoney/Services/Storage/BusinessAggregateRepository.cs`, `HomeHoney/Services/Storage/CollaborationRepository.cs`, and `HomeHoney/Services/Storage/PreferenceRepository.cs`
- [X] T023 [US3] Refactor collaboration CRUD to use Mongo repositories and in-session fallback state in `HomeHoney/Services/Collaboration/FamilyCollaborationService.cs`, `HomeHoney/Models/FridgeNote.cs`, and `HomeHoney/Models/Memo.cs`
- [X] T024 [US3] Refactor reminder and preference persistence to use Mongo-aware services plus Preferences / SecureStorage boundaries in `HomeHoney/Services/Reminders/ReminderCenterService.cs`, `HomeHoney/Services/Preferences/UserPreferenceService.cs`, and `HomeHoney/Services/Storage/PreferenceRepository.cs`
- [ ] T025 [P] [US3] Update fridge note and memo pages for remote-backed CRUD feedback in `HomeHoney/Components/Pages/FridgeNotes/FridgeNoteBoard.razor`, `HomeHoney/Components/Pages/FridgeNotes/FridgeNoteForm.razor`, `HomeHoney/Components/Pages/Memos/MemoList.razor`, `HomeHoney/Components/Pages/Memos/MemoForm.razor`, and `HomeHoney/Components/Pages/Memos/MemoDetail.razor`
- [ ] T026 [P] [US3] Update reminder, search, and settings pages to reflect remote persistence and failure-state feedback in `HomeHoney/Components/Pages/Reminders/ReminderCenter.razor`, `HomeHoney/Components/Pages/Reminders/Search.razor`, `HomeHoney/Components/Pages/Settings/NotificationSettings.razor`, and `HomeHoney/Components/Pages/Settings/PrivacySettings.razor`
- [ ] T027 [US3] Preserve household-member, space, and document metadata relationships in `HomeHoney/Models/HouseholdMember.cs`, `HomeHoney/Models/Space.cs`, `HomeHoney/Models/ReminderItem.cs`, and `HomeHoney/Services/Storage/BusinessAggregateRepository.cs`

**Checkpoint**: 非文件业务数据已迁移到 MongoDB 持久化，同时保持明确的远端失败与同步状态可见性

---

## Phase 6: Polish & Cross-Cutting Concerns (优化与横切关注点)

**Purpose**: 完成文档同步、回归验证和手动验收

- [X] T028 [P] Update repository guidance and environment notes in `README.md` and `.github/agents/copilot-instructions.md`
- [X] T029 [P] Sync final storage architecture and validation notes in `specs/005-integrate-storage-services/research.md`, `specs/005-integrate-storage-services/data-model.md`, `specs/005-integrate-storage-services/contracts/storage-integration-contract.md`, and `specs/005-integrate-storage-services/quickstart.md`
- [X] T030 Run build and regression validation for `HomeHoney/HomeHoney.csproj` and `HomeHoney.Tests/HomeHoney.Tests.csproj`
- [ ] T031 Run manual storage scenarios from `specs/005-integrate-storage-services/quickstart.md` against the local FileBrowser and Mongo services

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 无依赖，可立即开始
- **Foundational (Phase 2)**: 依赖 Phase 1 完成，阻塞所有用户故事
- **User Story 1 (Phase 3)**: 依赖 Phase 2 完成，是后续远端接入的 MVP
- **User Story 2 (Phase 4)**: 依赖 Phase 2 完成；建议在 US1 完成后推进，以便直接复用可编辑配置
- **User Story 3 (Phase 5)**: 依赖 Phase 2 完成；建议在 US1 完成后推进，以便直接复用可编辑配置
- **Polish (Phase 6)**: 依赖目标用户故事完成后执行

### User Story Dependencies

- **User Story 1 (P1)**: 无其他用户故事依赖；它提供后续所有远端能力的配置入口
- **User Story 2 (P2)**: 依赖有效的活动存储配置，但在配置已就绪后可独立完成文件性质资源闭环
- **User Story 3 (P3)**: 依赖有效的活动存储配置，但在配置已就绪后可独立完成非文件数据 CRUD 闭环

### Story Completion Order Graph

```text
Setup -> Foundational -> US1
                      -> US2
                      -> US3
US1 -> US2
US1 -> US3
US2 + US3 -> Polish
```

### Within Each User Story

- 先完成模型与仓储/网关抽象，再接入现有应用服务
- 先完成服务编排，再更新页面与交互反馈
- 先打通缓存与同步状态，再执行回归与手动验收
- 完成故事独立验收后，再进入最终文档与验证收敛

### Parallel Opportunities

- Phase 1: `T002` 与 `T003` 可并行
- Phase 2: `T006`、`T007`、`T008`、`T009` 可在 `T004`、`T005` 后并行
- Phase 3: `T013` 可与 `T012` 并行，`T014`、`T015` 在其后收敛
- Phase 4: `T018` 与 `T019` 可并行，`T016`、`T017` 完成后再统一到 `T020`、`T021`
- Phase 5: `T025` 与 `T026` 可并行，`T022`、`T023`、`T024` 完成后再统一到 `T027`
- Phase 6: `T028` 与 `T029` 可并行，`T030` 与 `T031` 在实现完成后执行

---

## Parallel Example: User Story 1

```text
# 可并行处理的存储配置任务：
T012 Storage validation and activation workflow
T013 Storage settings UI

# 完成后统一收敛：
T014 Route and save feedback wiring
T015 Startup profile activation
```

## Parallel Example: User Story 2

```text
# 可并行处理的文件资源界面任务：
T018 Insurance file upload/download UI
T019 Manual file upload/download UI

# 先由服务层打底：
T016 Document repositories and orchestration
T017 Document service refactor

# 最后统一收敛：
T020 Reminder/document compatibility
T021 Shared missing-file feedback
```

## Parallel Example: User Story 3

```text
# 可并行处理的非文件模块页面任务：
T025 Fridge note and memo remote CRUD UI
T026 Reminder, search, and settings cache-state UI

# 先由仓储/服务层打底：
T022 Mongo-backed repositories
T023 Collaboration service refactor
T024 Reminder and preference persistence refactor

# 最后统一收敛：
T027 Relationship preservation and aggregate consistency
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. 完成 Phase 1: Setup
2. 完成 Phase 2: Foundational
3. 完成 Phase 3: User Story 1
4. **停止并验证**：应用内可修改并持久保存文件服务与数据服务连接配置
5. 以此作为外部存储集成的 MVP 演示

### Incremental Delivery

1. Setup + Foundational → 建立远端接入与平台级偏好/密钥持久化基础设施
2. User Story 1 → 配置入口与激活机制完成
3. User Story 2 → 文件性质资源迁移到“文件服务 + Mongo 元数据”闭环
4. User Story 3 → 非文件业务数据迁移到 MongoDB CRUD 闭环
5. Polish → 文档同步、回归验证和手动验收完成

### Parallel Team Strategy

1. 一名开发者先完成 Phase 1 + Phase 2
2. 之后并行：
   - 开发者 A：US1 配置与激活流程
   - 开发者 B：US2 文件性质资源接入
   - 开发者 C：US3 非文件数据 CRUD 接入
3. 最后统一完成文档同步、自动化验证与手动验收

---

## Notes

- `[P]` tasks = 不同文件、依赖少，适合并行
- `[USx]` 标签保证任务可追溯到具体用户故事
- 本特性未要求新增测试优先开发流程，因此未单独拆分测试先行任务
- `T030` 负责运行现有测试与构建回归，不代表新增测试覆盖范围自动满足
- 应用本地只保留 Preferences / SecureStorage；不要重新引入业务数据或文件内容落盘
- 建议先用默认本地 FileBrowser / Mongo 连接完成最小闭环，再扩展到更完整的远端异常处理
