# Feature Specification: 项目脚手架搭建

**Feature Branch**: `001-project-scaffold`  
**Created**: 2026-03-09  
**Status**: Draft  
**Input**: User description: "项目脚手架搭建"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - 开发者克隆仓库后可立即构建运行 (Priority: P1)

作为开发者，我克隆 HomeHoney 仓库后，只需执行标准的 `dotnet` 命令即可在本地构建并运行 MAUI Blazor Hybrid 应用，无需额外的手动配置步骤。

**Why this priority**: 这是所有后续功能开发的基础。如果项目无法构建和运行，任何功能都无法开始。

**Independent Test**: 在全新环境中克隆仓库，执行 `dotnet build`，验证零错误零警告完成编译。

**Acceptance Scenarios**:

1. **Given** 一台安装了 .NET SDK 和目标平台 SDK 的开发机器，**When** 克隆仓库并执行 `dotnet build`，**Then** 项目编译成功，零错误零警告
2. **Given** 项目已构建成功，**When** 在 iOS 模拟器或 Android 模拟器上运行，**Then** 应用启动并显示一个包含 "HomeHoney" 标题的主页面
3. **Given** 项目已构建成功，**When** 在 macOS 上直接运行，**Then** 应用以桌面窗口形式启动，显示同样的主页面

---

### User Story 2 - 开发者可运行测试套件验证代码质量 (Priority: P2)

作为开发者，我希望项目初始就包含测试项目和示例测试，使我可以通过 `dotnet test` 验证项目健康状态，并为后续功能开发提供测试模板。

**Why this priority**: 测试基础设施应从项目之初就建立，避免后期补建的高成本。但优先级低于"能构建运行"。

**Independent Test**: 执行 `dotnet test`，验证所有示例测试通过。

**Acceptance Scenarios**:

1. **Given** 项目脚手架已就绪，**When** 执行 `dotnet test`，**Then** 所有测试通过且输出测试覆盖率报告入口
2. **Given** 测试项目存在，**When** 查看测试项目结构，**Then** 包含至少一个服务层单元测试示例和一个 Blazor 组件 bUnit 测试示例

---

### User Story 3 - 应用启动后展示空状态主页面 (Priority: P3)

作为用户，我第一次打开 HomeHoney App 时，看到一个简洁的主页面，清晰地告诉我这是一个家庭文档管理应用，并引导我开始添加第一个文档。

**Why this priority**: 确保脚手架不仅是技术空壳，而是能运行出一个有意义的 UI 骨架，为后续 UI 开发提供起点。

**Independent Test**: 启动应用，验证主页面展示应用名称、空状态提示和基础导航结构。

**Acceptance Scenarios**:

1. **Given** 用户首次启动应用，**When** 应用加载完成，**Then** 显示 "HomeHoney" 标题、空状态插图/文案（如"还没有文档，点击添加"）、底部导航栏骨架
2. **Given** 应用正在启动，**When** 加载过程中，**Then** 显示启动画面（Splash Screen）而非白屏

---

### Edge Cases

- 目标平台 SDK 未安装时，构建命令应给出清晰错误提示而非隐晦失败
- .NET SDK 版本不匹配时，`global.json` 应约束所需版本并给出明确提示
- 首次运行时无网络连接，应用仍应正常启动并展示空状态页面（离线优先原则）

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: 项目必须（MUST）使用 .NET MAUI Blazor Hybrid 模板创建，包含 `BlazorWebView` 作为主 UI 承载容器
- **FR-002**: 解决方案必须（MUST）包含至少两个项目：主应用项目和测试项目
- **FR-003**: 主应用项目必须（MUST）配置 iOS、Android 为目标平台，macOS/Windows 为可选目标平台
- **FR-004**: 项目必须（MUST）启用 `TreatWarningsAsErrors`，确保编译零警告
- **FR-005**: 项目必须（MUST）包含 `global.json` 锁定 .NET SDK 版本，确保团队开发环境一致
- **FR-006**: 测试项目必须（MUST）引用 xUnit、bUnit 和 Moq 依赖，并包含至少各一个示例测试
- **FR-007**: 项目必须（MUST）包含 `.editorconfig` 统一代码风格规范
- **FR-008**: 主应用必须（MUST）配置基础的依赖注入容器（`MauiProgram.cs`），为后续服务注册提供入口
- **FR-009**: 主应用必须（MUST）包含一个主页面（`MainPage.razor`），展示应用名称和空状态 UI
- **FR-010**: 项目必须（MUST）包含基础的导航结构（底部 Tab 骨架），为后续页面扩展预留入口
- **FR-011**: 应用必须（MUST）配置各平台的应用图标和启动画面占位资源
- **FR-012**: 项目必须（MUST）包含 `README.md`，说明构建、运行和测试的步骤

### Key Entities

- **解决方案（Solution）**：HomeHoney.sln — 包含所有项目的顶层解决方案文件
- **主应用项目（App Project）**：HomeHoney — MAUI Blazor Hybrid 应用项目，包含 Pages/、Shared/、Services/ 目录结构
- **测试项目（Test Project）**：HomeHoney.Tests — xUnit + bUnit 测试项目，包含 Unit/、Component/ 目录结构

## Assumptions

- 开发者使用 macOS（主要）或 Windows，已安装对应平台的 SDK（Xcode / Android SDK）
- 使用 .NET 9 或更高版本
- 项目初期不涉及后端服务或云端同步，仅搭建客户端应用骨架
- SQLite 数据库的集成将在后续需求中完成，本次仅预留服务接口

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 从克隆仓库到首次成功构建，耗时不超过 5 分钟（不含 SDK 安装时间）
- **SC-002**: `dotnet build` 零错误零警告完成
- **SC-003**: `dotnet test` 所有测试通过（100% 通过率）
- **SC-004**: 应用在 iOS 模拟器和 Android 模拟器上均可启动并展示主页面
- **SC-005**: 项目目录结构符合章程原则 I（组件化架构）的目录规范（Pages/、Shared/、Services/）
- **SC-006**: 新开发者阅读 README 后可独立完成环境搭建和首次运行
