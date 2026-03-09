# Tasks: 项目脚手架搭建

**Input**: Design documents from `/specs/001-project-scaffold/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, quickstart.md ✅

**Tests**: spec.md FR-006 明确要求包含 xUnit + bUnit + Moq 示例测试，因此本任务列表包含测试任务。

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **扁平结构** (per research.md 决策 3):
  - 主应用: `HomeHoney/`
  - 测试: `HomeHoney.Tests/`
  - 解决方案级文件: 仓库根目录

---

## Phase 1: Setup (环境准备)

**Purpose**: 创建解决方案骨架和根目录级配置文件

- [X] T001 Create global.json at repository root with SDK version 9.0.100, rollForward latestFeature, allowPrerelease false (per research.md 决策 4)
- [X] T002 Create .editorconfig at repository root with C# coding style rules (indentation, naming conventions, using directives)
- [X] T003 Create HomeHoney solution file HomeHoney.sln at repository root using `dotnet new sln`
- [X] T004 Create MAUI Blazor Hybrid project using `dotnet new maui-blazor` in HomeHoney/ directory (per research.md 决策 1)
- [X] T005 Add HomeHoney project to HomeHoney.sln using `dotnet sln add HomeHoney/HomeHoney.csproj`

---

## Phase 2: Foundational (基础配置 — 阻塞型前置条件)

**Purpose**: 核心项目配置，所有用户故事的前置条件

**⚠️ CRITICAL**: 用户故事实现前必须完成此阶段

- [X] T006 Configure HomeHoney/HomeHoney.csproj: add net9.0 to TargetFrameworks (net9.0;net9.0-android;net9.0-ios;net9.0-maccatalyst), add TreatWarningsAsErrors=true, verify UseMaui=true and OutputType (per data-model.md and research.md 决策 2)
- [X] T007 Configure MauiProgram.cs in HomeHoney/MauiProgram.cs: verify AddMauiBlazorWebView() registration, AddBlazorWebViewDeveloperTools() in DEBUG, and font configuration (per research.md 决策 1)
- [X] T008 Reorganize Components directory: ensure HomeHoney/Components/ contains Pages/, Layout/, Shared/ subdirectories with _Imports.razor and Routes.razor (per research.md 决策 5)
- [X] T009 Remove template sample pages (Counter.razor, Weather.razor) from HomeHoney/Components/Pages/, keep only Home.razor placeholder
- [X] T010 [P] Configure platform resources: verify HomeHoney/Resources/AppIcon/, Splash/, Fonts/ directories contain placeholder assets for iOS and Android (per spec FR-011)
- [X] T011 [P] Configure App.xaml and MainPage.xaml in HomeHoney/: verify BlazorWebView hosts Routes component at #app selector pointing to wwwroot/index.html

**Checkpoint**: 项目可编译，`dotnet build` 通过（可能有警告待处理），基础目录结构就位

---

## Phase 3: User Story 1 — 开发者克隆仓库后可立即构建运行 (Priority: P1) 🎯 MVP

**Goal**: 克隆仓库 → `dotnet build` → 零错误零警告 → 模拟器可运行

**Independent Test**: `dotnet build` 零错误零警告完成编译；在 iOS/Android 模拟器上启动显示基础页面

### Implementation for User Story 1

- [X] T012 [US1] Fix all compiler warnings in HomeHoney/ to achieve zero-warning build with TreatWarningsAsErrors=true
- [X] T013 [US1] Verify and fix HomeHoney/wwwroot/index.html: ensure correct Blazor WebView script reference (_framework/blazor.webview.js) and #app root div
- [X] T014 [US1] Verify and fix HomeHoney/wwwroot/css/app.css: ensure base styles load correctly, remove unused Bootstrap references if present
- [X] T015 [US1] Create placeholder HomeHoney/Components/Pages/Home.razor with @page "/" route displaying "HomeHoney" title text
- [X] T016 [US1] Verify and configure HomeHoney/Platforms/iOS/ and HomeHoney/Platforms/Android/ platform entry points for correct app startup
- [X] T017 [US1] Run `dotnet build` and verify zero errors zero warnings across all target frameworks (net9.0, net9.0-ios, net9.0-android, net9.0-maccatalyst)
- [X] T018 [US1] Update README.md at repository root with build prerequisites (SDK, workloads), clone instructions, build command, and run instructions per platform (per spec FR-012 and quickstart.md)

**Checkpoint**: `dotnet build` 零错误零警告；应用可在模拟器上启动

---

## Phase 4: User Story 2 — 开发者可运行测试套件验证代码质量 (Priority: P2)

**Goal**: `dotnet test` 全部通过，包含服务层单元测试示例和 bUnit 组件测试示例

**Independent Test**: `dotnet test` 所有测试通过

### Implementation for User Story 2

- [X] T019 [US2] Create test project using `dotnet new xunit` in HomeHoney.Tests/ directory, then configure HomeHoney.Tests/HomeHoney.Tests.csproj with SDK Microsoft.NET.Sdk.Razor, TargetFramework net9.0, IsPackable false
- [X] T020 [US2] Add NuGet dependencies to HomeHoney.Tests/HomeHoney.Tests.csproj: bunit 2.5.3+, xunit 2.9.4+, xunit.runner.visualstudio 3.0.1+, Microsoft.NET.Test.Sdk 17.12.0+, coverlet.collector 6.0.4+, Moq 4.20+ (per research.md NuGet table)
- [X] T021 [US2] Add ProjectReference from HomeHoney.Tests/HomeHoney.Tests.csproj to HomeHoney/HomeHoney.csproj
- [X] T022 [US2] Add HomeHoney.Tests project to HomeHoney.sln using `dotnet sln add HomeHoney.Tests/HomeHoney.Tests.csproj`
- [X] T023 [US2] Create HomeHoney.Tests/_Imports.razor with global @using directives for bunit and project namespaces
- [X] T024 [P] [US2] Create sample service interface HomeHoney/Services/IGreetingService.cs and implementation HomeHoney/Services/GreetingService.cs (simple service returning app name) for unit test demonstration
- [X] T025 [P] [US2] Register IGreetingService/GreetingService in HomeHoney/MauiProgram.cs DI container as Singleton
- [X] T026 [US2] Create directory HomeHoney.Tests/Unit/ and sample unit test HomeHoney.Tests/Unit/GreetingServiceTests.cs testing GreetingService returns expected value (using xUnit + Moq pattern)
- [X] T027 [US2] Create directory HomeHoney.Tests/Component/ and sample bUnit test HomeHoney.Tests/Component/HomePageTests.cs testing Home.razor renders "HomeHoney" title markup
- [X] T028 [US2] Run `dotnet test` and verify all tests pass with 100% pass rate

**Checkpoint**: `dotnet test` 全部通过；Unit/ 和 Component/ 各含至少一个示例测试

---

## Phase 5: User Story 3 — 应用启动后展示空状态主页面 (Priority: P3)

**Goal**: 首次打开 App 看到 HomeHoney 标题、空状态引导文案、底部 Tab 导航骨架

**Independent Test**: 启动应用，验证主页面展示应用名称、空状态提示、底部导航栏

### Implementation for User Story 3

- [X] T029 [P] [US3] Create shared empty state component HomeHoney/Components/Shared/EmptyState.razor with parameters for icon, title, description (displaying "还没有文档，点击添加" by default)
- [X] T030 [US3] Implement main layout with bottom tab navigation in HomeHoney/Components/Layout/MainLayout.razor: bottom TabBar skeleton with 3 tabs (首页, 搜索, 我的) using mobile-first CSS (per Constitution V)
- [X] T031 [US3] Implement HomeHoney/Components/Layout/NavMenu.razor with navigation links mapped to bottom tabs
- [X] T032 [US3] Update HomeHoney/Components/Pages/Home.razor: render EmptyState component with HomeHoney branding, app title "HomeHoney", and empty state guidance text
- [X] T033 [US3] Style mobile-first layout in HomeHoney/wwwroot/css/app.css: bottom tab bar, touch-friendly tap targets (min 44px), safe area insets for iOS notch, responsive typography
- [X] T034 [US3] Configure splash screen resource in HomeHoney/Resources/Splash/ with HomeHoney branding placeholder (per spec FR-011)
- [X] T035 [US3] Configure app icon resource in HomeHoney/Resources/AppIcon/ with HomeHoney placeholder icon (per spec FR-011)
- [X] T036 [US3] Verify app launches with splash screen → home page with empty state + bottom tabs on iOS simulator **and** Android emulator (both platforms required per SC-004)

**Checkpoint**: 应用启动后显示 Splash → HomeHoney 主页 + 空状态提示 + 底部 Tab 导航

---

## Phase 6: Polish & Cross-Cutting Concerns (优化与横切关注点)

**Purpose**: 跨故事的优化和最终验证

- [X] T037 [P] Final `dotnet build` verification: zero errors, zero warnings across all target frameworks
- [X] T038 [P] Final `dotnet test` verification: all tests pass at 100% pass rate
- [X] T039 Finalize README.md at repository root: supplement T018 initial version with complete test instructions and project structure overview (per quickstart.md)
- [X] T040 Run quickstart.md full validation: clone → build → test → run on simulator end-to-end
- [X] T041 Verify project directory structure matches plan.md Source Code tree (Components/Pages/, Components/Layout/, Components/Shared/, Services/, HomeHoney.Tests/Unit/, HomeHoney.Tests/Component/)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 无依赖 — 可立即开始
- **Foundational (Phase 2)**: 依赖 Phase 1 完成 — **阻塞所有用户故事**
- **User Story 1 (Phase 3)**: 依赖 Phase 2 完成
- **User Story 2 (Phase 4)**: 依赖 Phase 3 完成（需要可编译的主项目才能创建 ProjectReference）
- **User Story 3 (Phase 5)**: 依赖 Phase 3 完成（需要可运行的基础 UI）；与 Phase 4 **可并行**
- **Polish (Phase 6)**: 依赖所有用户故事完成

### User Story Dependencies

- **User Story 1 (P1)**: Phase 2 完成后可开始。无故事间依赖。
- **User Story 2 (P2)**: 依赖 US1（需要可编译的主项目）。
- **User Story 3 (P3)**: 依赖 US1（需要基础页面结构）。与 US2 **无依赖**，可并行。

### Within Each User Story

- 配置文件先于源代码
- 模型/服务先于页面/UI
- 核心实现完成后再做集成验证
- 故事完成检查点通过后再进入下一阶段

### Parallel Opportunities

- Phase 1: T001、T002 可并行（不同文件）
- Phase 2: T010、T011 标记 [P]，可并行
- Phase 4 和 Phase 5: US2（测试）和 US3（UI）可并行执行（不同文件和目录）
- Phase 5: T029 标记 [P]，可与其他准备工作并行
- Phase 6: T037、T038 可并行

---

## Parallel Example: User Story 2 + User Story 3 (并行执行)

```text
# US2 和 US3 可在 US1 完成后同时开始（不同文件和目录）:

# 开发者 A: User Story 2 (测试基础设施)
Task T019: Create test project in HomeHoney.Tests/
Task T020: Add NuGet dependencies
Task T024: Create sample service interface in HomeHoney/Services/
Task T026: Create unit test in HomeHoney.Tests/Unit/
Task T027: Create bUnit test in HomeHoney.Tests/Component/

# 开发者 B: User Story 3 (UI 骨架)
Task T029: Create EmptyState.razor in HomeHoney/Components/Shared/
Task T030: Implement MainLayout.razor in HomeHoney/Components/Layout/
Task T032: Update Home.razor in HomeHoney/Components/Pages/
Task T033: Style app.css in HomeHoney/wwwroot/css/
```

---

## Implementation Strategy

### MVP First (仅 User Story 1)

1. 完成 Phase 1: Setup (T001-T005)
2. 完成 Phase 2: Foundational (T006-T011)
3. 完成 Phase 3: User Story 1 (T012-T018)
4. **停止并验证**: `dotnet build` 零错误零警告，模拟器可运行
5. 交付/演示 MVP

### Incremental Delivery (增量交付)

1. Setup + Foundational → 基础就绪
2. User Story 1 → `dotnet build` 通过 → **MVP 交付**
3. User Story 2 → `dotnet test` 通过 → 测试基础设施就绪
4. User Story 3 → 空状态 UI + 导航 → 可视化骨架就绪
5. Polish → 最终验证 → 完整脚手架交付

### Parallel Team Strategy (团队并行)

1. 全团队完成 Phase 1 + Phase 2
2. Phase 2 完成后全团队完成 Phase 3 (US1)
3. US1 完成后：
   - 开发者 A: User Story 2 (测试项目)
   - 开发者 B: User Story 3 (UI 骨架)
4. 两个故事独立完成和集成

---

## Notes

- [P] tasks = 不同文件，无依赖，可并行
- [Story] 标签将任务映射到具体用户故事，便于追溯
- 每个用户故事应可独立完成和测试
- 每个任务或逻辑组完成后提交 (Conventional Commits: feat:, chore:, test:)
- 可在任何检查点停止验证故事独立性
- 避免：模糊任务、同一文件冲突、跨故事依赖
