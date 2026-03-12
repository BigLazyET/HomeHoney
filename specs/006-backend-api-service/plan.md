# Implementation Plan: 通过后端 API 托管存储集成

**Branch**: `006-backend-api-service` | **Date**: 2026-03-11 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/006-backend-api-service/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

为 HomeHoney 新增一个独立的 `HomeHoney.Api` 后端项目，使用 .NET 10 ASP.NET Core Web API 作为移动端唯一业务接入点。当前移动端中直接访问 FileBrowser 与 MongoDB 的逻辑将迁移到后端，由后端负责下游存储连接、业务聚合、文件上传下载编排、异常分类与统一接口契约；移动端改为仅保存并使用后端服务地址，通过按业务域划分的 API 完成资料、文件、协作、提醒、搜索和偏好等能力。

## Technical Context

**Language/Version**: C# 14 / .NET 10  
**Primary Dependencies**: ASP.NET Core Web API, `HttpClient`/`IHttpClientFactory`, MongoDB.Driver, ASP.NET Core OpenAPI, existing xUnit + Moq + bUnit test stack, `WebApplicationFactory` for API integration tests  
**Storage**: 后端连接 FileBrowser 处理文件本体，连接 MongoDB 处理结构化业务数据；移动端仅保留后端地址、用户偏好和敏感信息  
**Testing**: xUnit, Moq, bUnit, API integration tests with `WebApplicationFactory`, downstream adapter tests for FileBrowser/Mongo interactions  
**Target Platform**: MAUI Blazor Hybrid mobile client + ASP.NET Core backend deployable on macOS/Linux/Windows  
**Project Type**: mobile-app + web-service  
**Performance Goals**: 文档列表与聚合读取在局域网环境下目标 p95 2 秒内返回首屏结果；文件上传下载采用流式转发，避免将大文件完整缓冲到内存；提醒/搜索聚合保持单次 API 调用可满足页面展示  
**Constraints**: 移动端不得再直接访问 FileBrowser 或 MongoDB；后端必须返回业务语义稳定的 DTO 与错误结果；需保留当前文件走 FileBrowser、数据走 MongoDB 的职责边界；必须处理跨文件服务与 MongoDB 的部分成功/部分失败状态；移动端仍只允许本地保存 Preferences / SecureStorage 范围内的数据  
**Scale/Scope**: 1 个现有 MAUI 应用 + 1 个新增 Web API 项目 + 1 个新增 API 测试项目；覆盖保险、说明书、冰箱贴、备忘录、提醒、搜索、偏好、存储设置等现有模块；面向单家庭规模但要求可平滑扩展到更多资料类型

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原则 | 合规状态 | 说明 |
|------|----------|------|
| I. 组件化架构 | ✅ 合规 | 移动端 UI 仍以 Blazor 组件为边界，新增后端服务不会破坏组件化结构，反而进一步把外部集成从 UI 中剥离 |
| II. 文档模型驱动 | ✅ 合规 | 文档元数据、文件状态、提醒与搜索聚合仍围绕明确的文档模型与业务状态设计 |
| III. 离线优先 | ❌ ERROR | 当前章程要求本地 SQLite 和本地文件系统承担核心离线存储，但仓库已在 `005-integrate-storage-services` 中确立“文件走 FileBrowser、数据走 Mongo、本地仅保留 Preferences / SecureStorage”的边界，本特性继续沿该方向演进，因此与章程文本直接冲突，需要后续章程修订或显式豁免 |
| IV. 类型安全与可测试性 | ✅ 合规 | 后端将通过接口、DTO、适配器与 API 测试保持强类型与可测试性 |
| V. 移动端优先 UX | ✅ 合规 | 移动端仍保持原有操作路径，后端负责聚合与错误分类，减少页面层复杂度并提升可理解反馈 |

**门控结果（Phase 0 前）**：ERROR — 章程第 III 条仍要求离线优先与本地 SQLite/文件系统，这与当前已采纳的远端存储架构以及本特性目标不一致。规划继续输出以支持实现设计，但实现前应先修订章程或记录正式例外。  
**门控结果（Phase 1 后复核）**：ERROR — 设计已完整围绕“移动端仅连后端、后端统一承接 FileBrowser/Mongo”展开，仍与现行章程第 III 条冲突；其余原则均满足。

## Project Structure

### Documentation (this feature)

```text
specs/006-backend-api-service/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── homehoney-backend-openapi.yaml
└── tasks.md
```

### Source Code (repository root)

```text
HomeHoney/
├── Components/
├── Models/
├── Services/
│   ├── Collaboration/
│   ├── Documents/
│   ├── Preferences/
│   ├── Reminders/
│   ├── Search/
│   └── Storage/
├── HomeHoney.csproj
└── MauiProgram.cs

HomeHoney.Tests/
├── Component/
├── Unit/
└── HomeHoney.Tests.csproj

HomeHoney.Api/
├── Controllers/ or Endpoints/
├── Contracts/
├── Application/
│   ├── Documents/
│   ├── Collaboration/
│   ├── Preferences/
│   ├── Reminders/
│   ├── Search/
│   └── Storage/
├── Infrastructure/
│   ├── FileBrowser/
│   ├── Mongo/
│   └── Configuration/
├── Program.cs
└── HomeHoney.Api.csproj

HomeHoney.Api.Tests/
├── Contract/
├── Integration/
└── HomeHoney.Api.Tests.csproj
```

**Structure Decision**: 采用“现有移动端 + 新增 Web API + 新增 API 测试项目”的单解决方案结构。`HomeHoney.Api` 负责公开业务接口、组织应用层用例与下游集成；`HomeHoney` 移动端移除直连 FileBrowser/Mongo 的业务路径，替换为后端 API 客户端；`HomeHoney.Api.Tests` 承担 API 集成与契约验证，避免将服务器测试与 MAUI/bUnit 测试混在同一个项目中。

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Constitution III requires offline-first local SQLite/file storage | 仓库当前已采纳远端 FileBrowser + Mongo 架构，本特性进一步将移动端下游集成收敛到后端统一承接 | 继续按章程保留本地 SQLite/文件系统会重新引入已被明确取消的本地业务持久化边界，与用户最新架构要求冲突 |
