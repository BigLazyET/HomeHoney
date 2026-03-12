# Tasks: 通过后端 API 托管存储集成

**Input**: Design documents from `/specs/006-backend-api-service/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: 本特性明确包含后端 API 契约、集成与下游适配器验证，因此任务中包含必要的 API 测试与回归验证任务。

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **移动端应用**: `HomeHoney/`
- **现有移动端测试**: `HomeHoney.Tests/`
- **新增后端 API**: `HomeHoney.Api/`
- **新增 API 测试**: `HomeHoney.Api.Tests/`
- **规范文档**: `specs/006-backend-api-service/`

---

## Phase 1: Setup (环境与治理准备)

**Purpose**: 建立后端项目、测试项目和实现前治理前置条件

- [X] T001 Resolve the current constitution conflict for remote-storage architecture in `.specify/memory/constitution.md` or record an approved exception before implementation proceeds
- [X] T002 Add `HomeHoney.Api` and `HomeHoney.Api.Tests` projects to `HomeHoney.sln`, create `HomeHoney.Api/HomeHoney.Api.csproj`, and create `HomeHoney.Api.Tests/HomeHoney.Api.Tests.csproj`
- [X] T003 [P] Create backend project scaffolding under `HomeHoney.Api/Application/`, `HomeHoney.Api/Infrastructure/`, `HomeHoney.Api/Contracts/`, and `HomeHoney.Api/Endpoints/`
- [X] T004 [P] Configure backend dependencies, local launch settings, and shared solution build wiring in `HomeHoney.Api/HomeHoney.Api.csproj`, `HomeHoney.Api.Tests/HomeHoney.Api.Tests.csproj`, and `HomeHoney.sln`
- [X] T005 [P] Update repository-level setup guidance for running mobile + API + downstream services in `README.md` and `specs/006-backend-api-service/quickstart.md`

---

## Phase 2: Foundational (基础能力 — 阻塞型前置条件)

**Purpose**: 建立后端契约、下游接入、统一错误处理和移动端 API 接入基础设施

**⚠️ CRITICAL**: 用户故事实现前必须完成此阶段

- [X] T006 Create shared backend configuration and contract base types in `HomeHoney.Api/Contracts/Common/ApiOperationResult.cs`, `HomeHoney.Api/Contracts/Common/ProblemDetailsExtensions.cs`, `HomeHoney.Api/Contracts/Storage/BackendServiceProfileDto.cs`, and `HomeHoney.Api/Contracts/Storage/DownstreamStorageSettingsDto.cs`
- [X] T007 [P] Implement backend downstream configuration, validation, and secret handling in `HomeHoney.Api/Application/Storage/IBackendStorageSettingsService.cs`, `HomeHoney.Api/Application/Storage/BackendStorageSettingsService.cs`, `HomeHoney.Api/Infrastructure/Configuration/BackendStorageOptions.cs`, and `HomeHoney.Api/Infrastructure/Configuration/SecureSettingsStore.cs`
- [X] T008 [P] Implement backend FileBrowser integration adapters in `HomeHoney.Api/Infrastructure/FileBrowser/IFileBrowserGateway.cs`, `HomeHoney.Api/Infrastructure/FileBrowser/FileBrowserGateway.cs`, and `HomeHoney.Api/Infrastructure/FileBrowser/FileBrowserModels.cs`
- [X] T009 [P] Implement backend Mongo access foundation in `HomeHoney.Api/Infrastructure/Mongo/IMongoDatabaseFactory.cs`, `HomeHoney.Api/Infrastructure/Mongo/MongoDatabaseFactory.cs`, `HomeHoney.Api/Infrastructure/Mongo/Collections/DocumentCollections.cs`, and `HomeHoney.Api/Infrastructure/Mongo/Collections/BusinessCollections.cs`
- [X] T010 [P] Add API-wide logging, exception mapping, `ProblemDetails`, OpenAPI, and health checks in `HomeHoney.Api/Program.cs`, `HomeHoney.Api/Infrastructure/Configuration/DependencyInjection.cs`, and `HomeHoney.Api/Endpoints/HealthEndpoints.cs`
- [X] T011 [P] Create API test host scaffolding and common fixtures in `HomeHoney.Api.Tests/Integration/ApiWebApplicationFactory.cs`, `HomeHoney.Api.Tests/Integration/TestServiceOverrides.cs`, and `HomeHoney.Api.Tests/Contract/OpenApiSnapshotTests.cs`
- [X] T012 Refactor mobile connection modeling from downstream storage targets to backend API targets in `HomeHoney/Models/StorageConnectionProfile.cs`, `HomeHoney/Models/UserPreference.cs`, `HomeHoney/Services/Storage/IStorageConnectionProfileService.cs`, and `HomeHoney/Services/Storage/StorageConnectionProfileService.cs`
- [X] T013 [P] Implement mobile backend HTTP client infrastructure in `HomeHoney/Services/Storage/BackendApiOptions.cs`, `HomeHoney/Services/Storage/BackendApiHttpClientFactory.cs`, and `HomeHoney/MauiProgram.cs`
- [X] T014 [P] Create mobile API client contracts for documents, collaboration, reminders, search, preferences, and admin storage in `HomeHoney/Services/Documents/IDocumentApiClient.cs`, `HomeHoney/Services/Collaboration/ICollaborationApiClient.cs`, `HomeHoney/Services/Reminders/IReminderApiClient.cs`, `HomeHoney/Services/Search/ISearchApiClient.cs`, `HomeHoney/Services/Preferences/IPreferenceApiClient.cs`, and `HomeHoney/Services/Storage/IAdminStorageApiClient.cs`

**Checkpoint**: 后端项目、下游适配器、统一错误基础设施与移动端 API 接入基线就绪，可开始实现用户故事

---

## Phase 3: User Story 1 - 统一通过后端访问业务能力 (Priority: P1) 🎯 MVP

**Goal**: 移动端只连接 HomeHoney 后端服务，并通过该后端承接现有核心业务读写路径

**Independent Test**: 仅配置后端服务地址即可在移动端读取资料列表和至少一类非文件业务数据，且客户端不再需要直连 FileBrowser/Mongo 配置

### Tests for User Story 1

- [X] T015 [P] [US1] Add contract tests for backend profile and admin storage endpoints in `HomeHoney.Api.Tests/Contract/Storage/AdminStorageContractTests.cs`
- [X] T016 [P] [US1] Add integration tests for mobile-facing backend profile validation and downstream setting updates in `HomeHoney.Api.Tests/Integration/Storage/AdminStorageIntegrationTests.cs`
- [X] T017 [P] [US1] Add mobile service tests for backend profile persistence and API client wiring in `HomeHoney.Tests/Unit/Storage/StorageConnectionProfileServiceTests.cs` and `HomeHoney.Tests/Unit/Storage/BackendApiHttpClientFactoryTests.cs`

### Implementation for User Story 1

- [X] T018 [US1] Implement backend admin storage application services in `HomeHoney.Api/Application/Storage/BackendProfileService.cs`, `HomeHoney.Api/Application/Storage/StorageValidationService.cs`, and `HomeHoney.Api/Application/Storage/StorageAdminMapper.cs`
- [X] T019 [US1] Implement backend admin endpoints for mobile profile and downstream storage settings in `HomeHoney.Api/Endpoints/Admin/BackendProfileEndpoints.cs` and `HomeHoney.Api/Endpoints/Admin/StorageAdminEndpoints.cs`
- [X] T020 [US1] Create mobile admin storage API client in `HomeHoney/Services/Storage/AdminStorageApiClient.cs` and connect it in `HomeHoney/MauiProgram.cs`
- [X] T021 [US1] Update the storage settings experience to edit backend API targets and backend-managed downstream settings in `HomeHoney/Components/Pages/Settings/StorageSettings.razor`, `HomeHoney/Components/Shared/StorageConnectionForm.razor`, and `HomeHoney/Services/Navigation/AppRoutes.cs`
- [X] T022 [US1] Remove remaining mobile direct validation dependencies on FileBrowser/Mongo in `HomeHoney/Services/Storage/StorageConfigurationValidator.cs`, `HomeHoney/Services/Storage/MongoContextFactory.cs`, and `HomeHoney/Services/Storage/FileBrowserFileStorageGateway.cs`
- [X] T023 [US1] Add backend startup validation, configuration binding, and safe fallback messaging in `HomeHoney.Api/Program.cs`, `HomeHoney.Api/appsettings.json`, and `HomeHoney.Api/appsettings.Development.json`

**Checkpoint**: 移动端已改为只连接后端服务，后端可统一承接连接管理与下游配置验证

---

## Phase 4: User Story 2 - 通过后端统一管理文件性质资源 (Priority: P2)

**Goal**: 保险合同和说明书等文件性质资源由后端统一提供列表、详情、上传、下载和文件状态能力

**Independent Test**: 在移动端仅连接后端的前提下，完成一条保险或说明书记录的列表读取、详情查看、文件上传和文件下载

### Tests for User Story 2

- [ ] T024 [P] [US2] Add contract tests for document and file endpoints in `HomeHoney.Api.Tests/Contract/Documents/InsuranceDocumentContractTests.cs` and `HomeHoney.Api.Tests/Contract/Documents/ManualDocumentContractTests.cs`
- [ ] T025 [P] [US2] Add integration tests for file upload/download and partial-success handling in `HomeHoney.Api.Tests/Integration/Documents/InsuranceFileFlowTests.cs` and `HomeHoney.Api.Tests/Integration/Documents/ManualFileFlowTests.cs`
- [ ] T026 [P] [US2] Add mobile regression tests for backend-backed document services in `HomeHoney.Tests/Unit/Documents/DocumentCatalogServiceApiTests.cs` and `HomeHoney.Tests/Unit/Search/SearchIndexServiceApiTests.cs`

### Implementation for User Story 2

- [X] T027 [P] [US2] Create backend document contracts and mappers in `HomeHoney.Api/Contracts/Documents/FileBackedDocumentDto.cs`, `HomeHoney.Api/Contracts/Documents/UpsertInsuranceDocumentRequest.cs`, `HomeHoney.Api/Contracts/Documents/UpsertManualDocumentRequest.cs`, and `HomeHoney.Api/Application/Documents/DocumentDtoMapper.cs`
- [X] T028 [P] [US2] Implement backend document repositories and file orchestration in `HomeHoney.Api/Infrastructure/Mongo/Repositories/DocumentRepository.cs`, `HomeHoney.Api/Application/Documents/DocumentFileOrchestrator.cs`, and `HomeHoney.Api/Application/Documents/FileStateEvaluator.cs`
- [X] T029 [US2] Implement insurance document application services and endpoints in `HomeHoney.Api/Application/Documents/InsuranceDocumentService.cs` and `HomeHoney.Api/Endpoints/Documents/InsuranceDocumentEndpoints.cs`
- [X] T030 [US2] Implement manual document application services and endpoints in `HomeHoney.Api/Application/Documents/ManualDocumentService.cs` and `HomeHoney.Api/Endpoints/Documents/ManualDocumentEndpoints.cs`
- [X] T031 [US2] Create mobile document API client and switch `DocumentCatalogService` to backend APIs in `HomeHoney/Services/Documents/DocumentApiClient.cs`, `HomeHoney/Services/Documents/DocumentCatalogService.cs`, and `HomeHoney/Services/Storage/DocumentFileOrchestrator.cs`
- [ ] T032 [US2] Update document pages to use backend file workflows and business error feedback in `HomeHoney/Components/Pages/Documents/Insurance/InsuranceList.razor`, `HomeHoney/Components/Pages/Documents/Insurance/InsuranceDetail.razor`, `HomeHoney/Components/Pages/Documents/Insurance/InsuranceForm.razor`, `HomeHoney/Components/Pages/Documents/Manuals/ManualList.razor`, `HomeHoney/Components/Pages/Documents/Manuals/ManualDetail.razor`, and `HomeHoney/Components/Pages/Documents/Manuals/ManualForm.razor`
- [ ] T033 [US2] Align file-state, integrity, and download messaging across mobile models and shared components in `HomeHoney/Models/FileResource.cs`, `HomeHoney/Models/InsuranceRecord.cs`, `HomeHoney/Models/ManualRecord.cs`, `HomeHoney/Components/Shared/EmptyState.razor`, and `HomeHoney/wwwroot/css/app.css`

**Checkpoint**: 文件性质资源已全部改由后端统一承接，移动端不再理解 FileBrowser 原始协议

---

## Phase 5: User Story 3 - 通过后端统一管理非文件业务数据 (Priority: P3)

**Goal**: 冰箱贴、备忘录、提醒、搜索、偏好和文档元数据等非文件业务全部通过后端完成 CRUD 与聚合

**Independent Test**: 仅通过后端接口即可完成至少一类非文件业务数据的完整 CRUD，并能读取提醒与搜索聚合结果

### Tests for User Story 3

- [ ] T034 [P] [US3] Add contract tests for collaboration, reminders, search, and preferences endpoints in `HomeHoney.Api.Tests/Contract/Collaboration/CollaborationContractTests.cs`, `HomeHoney.Api.Tests/Contract/Reminders/ReminderContractTests.cs`, `HomeHoney.Api.Tests/Contract/Search/SearchContractTests.cs`, and `HomeHoney.Api.Tests/Contract/Preferences/PreferenceContractTests.cs`
- [ ] T035 [P] [US3] Add integration tests for collaboration CRUD, reminder aggregation, and search semantics in `HomeHoney.Api.Tests/Integration/Collaboration/CollaborationFlowTests.cs`, `HomeHoney.Api.Tests/Integration/Reminders/ReminderAggregationTests.cs`, and `HomeHoney.Api.Tests/Integration/Search/SearchAggregationTests.cs`
- [ ] T036 [P] [US3] Add mobile regression tests for backend-backed collaboration, reminders, preferences, and search services in `HomeHoney.Tests/Unit/Collaboration/FamilyCollaborationServiceApiTests.cs`, `HomeHoney.Tests/Unit/Reminders/ReminderCenterServiceApiTests.cs`, `HomeHoney.Tests/Unit/Preferences/UserPreferenceServiceApiTests.cs`, and `HomeHoney.Tests/Unit/Search/SearchIndexServiceApiTests.cs`

### Implementation for User Story 3

- [X] T037 [P] [US3] Create backend contracts for collaboration, reminders, search, and preferences in `HomeHoney.Api/Contracts/Collaboration/FridgeNoteDto.cs`, `HomeHoney.Api/Contracts/Collaboration/MemoDto.cs`, `HomeHoney.Api/Contracts/Reminders/ReminderViewDto.cs`, `HomeHoney.Api/Contracts/Search/SearchGroupDto.cs`, and `HomeHoney.Api/Contracts/Preferences/UserPreferenceDto.cs`
- [X] T038 [P] [US3] Implement backend repositories for non-file aggregates in `HomeHoney.Api/Infrastructure/Mongo/Repositories/CollaborationRepository.cs`, `HomeHoney.Api/Infrastructure/Mongo/Repositories/PreferenceRepository.cs`, and `HomeHoney.Api/Infrastructure/Mongo/Repositories/ReferenceDataRepository.cs`
- [X] T039 [US3] Implement collaboration application services and endpoints in `HomeHoney.Api/Application/Collaboration/FridgeNoteService.cs`, `HomeHoney.Api/Application/Collaboration/MemoService.cs`, and `HomeHoney.Api/Endpoints/Collaboration/CollaborationEndpoints.cs`
- [X] T040 [US3] Implement preference, reminder, and search application services and endpoints in `HomeHoney.Api/Application/Preferences/UserPreferenceService.cs`, `HomeHoney.Api/Application/Reminders/ReminderAggregationService.cs`, `HomeHoney.Api/Application/Search/SearchAggregationService.cs`, `HomeHoney.Api/Endpoints/Preferences/PreferenceEndpoints.cs`, `HomeHoney.Api/Endpoints/Reminders/ReminderEndpoints.cs`, and `HomeHoney.Api/Endpoints/Search/SearchEndpoints.cs`
- [X] T041 [US3] Create mobile API clients for collaboration, preferences, reminders, and search in `HomeHoney/Services/Collaboration/CollaborationApiClient.cs`, `HomeHoney/Services/Preferences/PreferenceApiClient.cs`, `HomeHoney/Services/Reminders/ReminderApiClient.cs`, and `HomeHoney/Services/Search/SearchApiClient.cs`
- [X] T042 [US3] Refactor mobile business services to consume backend APIs instead of direct Mongo access in `HomeHoney/Services/Collaboration/FamilyCollaborationService.cs`, `HomeHoney/Services/Preferences/UserPreferenceService.cs`, `HomeHoney/Services/Reminders/ReminderCenterService.cs`, and `HomeHoney/Services/Search/SearchIndexService.cs`
- [ ] T043 [US3] Update fridge note, memo, reminder, search, and settings pages for backend-driven feedback in `HomeHoney/Components/Pages/FridgeNotes/FridgeNoteBoard.razor`, `HomeHoney/Components/Pages/FridgeNotes/FridgeNoteForm.razor`, `HomeHoney/Components/Pages/Memos/MemoList.razor`, `HomeHoney/Components/Pages/Memos/MemoForm.razor`, `HomeHoney/Components/Pages/Memos/MemoDetail.razor`, `HomeHoney/Components/Pages/Reminders/ReminderCenter.razor`, `HomeHoney/Components/Pages/Reminders/Search.razor`, `HomeHoney/Components/Pages/Settings/NotificationSettings.razor`, and `HomeHoney/Components/Pages/Settings/PrivacySettings.razor`
- [ ] T044 [US3] Remove residual mobile direct-storage repositories and old downstream-specific abstractions in `HomeHoney/Services/Storage/DocumentMetadataRepository.cs`, `HomeHoney/Services/Storage/BusinessAggregateRepository.cs`, `HomeHoney/Services/Storage/CollaborationRepository.cs`, `HomeHoney/Services/Storage/PreferenceRepository.cs`, and `HomeHoney/MauiProgram.cs`

**Checkpoint**: 非文件业务与聚合能力已迁移到后端，移动端只保留 UI 与后端 API 消费职责

---

## Phase 6: Polish & Cross-Cutting Concerns (收尾与横切关注点)

**Purpose**: 完成契约收敛、文档同步、回归验证和手动验收

- [ ] T045 [P] Finalize OpenAPI exposure, request tracing, and operational logging docs in `HomeHoney.Api/Program.cs`, `HomeHoney.Api/Endpoints/OpenApiEndpoints.cs`, and `README.md`
- [ ] T046 [P] Update implementation notes and migration guidance in `specs/006-backend-api-service/quickstart.md`, `specs/006-backend-api-service/research.md`, and `.github/agents/copilot-instructions.md`
- [ ] T047 Run build and automated regression validation for `HomeHoney.sln`, `HomeHoney.Api.Tests/HomeHoney.Api.Tests.csproj`, and `HomeHoney.Tests/HomeHoney.Tests.csproj`
- [ ] T048 Run the manual backend quickstart scenarios from `specs/006-backend-api-service/quickstart.md` against local FileBrowser, MongoDB, `HomeHoney.Api`, and the MAUI client

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 无依赖，可立即开始；`T001` 属于治理门控收敛任务
- **Foundational (Phase 2)**: 依赖 Phase 1 完成，阻塞所有用户故事
- **User Story 1 (Phase 3)**: 依赖 Phase 2 完成，是移动端切换到后端的 MVP
- **User Story 2 (Phase 4)**: 依赖 Phase 2 完成；建议在 US1 完成后推进，以便复用统一后端连接配置
- **User Story 3 (Phase 5)**: 依赖 Phase 2 完成；建议在 US1 完成后推进，以便复用统一后端连接配置与公共 API 基础设施
- **Polish (Phase 6)**: 依赖目标用户故事完成后执行

### User Story Dependencies

- **User Story 1 (P1)**: 无其他用户故事依赖；它建立移动端只连后端的主路径
- **User Story 2 (P2)**: 依赖 US1 的后端连接模型与公共 API 基线，但可独立完成文件性质资源闭环
- **User Story 3 (P3)**: 依赖 US1 的后端连接模型与公共 API 基线，但可独立完成非文件业务与聚合闭环

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

- 先完成后端契约与测试，再实现应用服务与端点
- 先完成后端业务编排，再切换移动端服务层
- 先完成服务切换，再更新页面与用户反馈
- 先完成自动化验证，再执行手动联调验收

### Parallel Opportunities

- Phase 1: `T003`、`T004`、`T005` 可并行，`T001` 应优先处理治理门控
- Phase 2: `T007`、`T008`、`T009`、`T010`、`T011`、`T013`、`T014` 可在 `T006`、`T012` 明确后并行
- Phase 3: `T015`、`T016`、`T017` 可并行；`T018`、`T019` 与 `T020`、`T021` 可分工推进，`T022`、`T023` 最后收敛
- Phase 4: `T024`、`T025`、`T026` 可并行；`T027` 与 `T028` 可并行；`T029`、`T030` 并行后再收敛到 `T031`、`T032`、`T033`
- Phase 5: `T034`、`T035`、`T036` 可并行；`T037`、`T038` 可并行；`T039` 与 `T040` 可并行；`T041`、`T042`、`T043` 可分模块推进，`T044` 最后清理旧路径
- Phase 6: `T045` 与 `T046` 可并行，`T047` 与 `T048` 在实现完成后执行

---

## Parallel Example: User Story 1

```text
# 可并行处理的 US1 测试与基础实现：
T015 Contract tests for admin storage endpoints
T016 Integration tests for backend profile and storage updates
T017 Mobile regression tests for backend profile persistence
T018 Backend profile and storage application services
T020 Mobile admin storage API client
```

## Parallel Example: User Story 2

```text
# 可并行处理的文件资源任务：
T024 Document/file contract tests
T025 File flow integration tests
T027 Backend document contracts and mappers
T028 Backend repositories and file orchestration
T029 Insurance endpoints
T030 Manual endpoints
```

## Parallel Example: User Story 3

```text
# 可并行处理的非文件业务与聚合任务：
T034 Collaboration/reminder/search/preference contract tests
T035 Aggregation integration tests
T037 Backend non-file contracts
T038 Backend non-file repositories
T039 Collaboration services and endpoints
T040 Preference/reminder/search services and endpoints
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. 完成 Phase 1: Setup
2. 完成 Phase 2: Foundational
3. 完成 Phase 3: User Story 1
4. **停止并验证**：移动端仅配置后端地址即可读取核心业务结果
5. 以此作为“移动端不再直连下游服务”的 MVP 演示

### Incremental Delivery

1. Setup + Foundational → 建立后端项目、契约、下游适配器和移动端 API 基线
2. User Story 1 → 移动端只连接后端
3. User Story 2 → 文件性质资源迁移到后端统一承接
4. User Story 3 → 非文件业务与聚合迁移到后端统一承接
5. Polish → 文档同步、自动化验证和手动联调验收完成

### Parallel Team Strategy

1. 一名开发者先完成 Phase 1 + Phase 2
2. 之后并行：
   - 开发者 A：US1 后端连接配置与移动端切换
   - 开发者 B：US2 文件文档与文件流接口
   - 开发者 C：US3 非文件 CRUD 与聚合接口
3. 最后统一完成文档、验证与联调验收

---

## Notes

- 当前代码已打通“移动端 -> HomeHoney.Api -> FileBrowser/Mongo”的主链路，保险资料已支持后端保存、上传、下载，冰箱贴与偏好也已支持后端写入。
- 当前实现已完成 `T027`、`T028`、`T029`、`T030`、`T031`、`T037`、`T038`、`T039`、`T040` 的 DTO、仓储、文件编排、服务与端点拆分；提醒、搜索、协作和偏好接口已切换到独立契约类型。
- 提醒与搜索读取链路已经切换为后端优先，但页面反馈统一化、旧仓储清理与契约测试补齐仍待后续阶段完成。

- `[P]` tasks = 不同文件、依赖少，适合并行
- `[USx]` 标签保证任务可追溯到具体用户故事
- 本特性包含 API 契约与集成验证，因此测试任务不是可选补充，而是交付基线的一部分
- `T001` 代表当前章程门控冲突的处理入口；在它解决前，不应把本特性视为完全合规实现
- 迁移完成后，移动端不得保留任何正式业务路径直连 FileBrowser 或 MongoDB
- 建议先打通 US1，再逐步替换文档业务与非文件业务，避免一次性大切换导致回归面过大
