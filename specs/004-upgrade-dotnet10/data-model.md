# Data Model: 升级到 .NET 10

## 1. RuntimeBaseline

**描述**：仓库默认采用的开发与构建运行时基线，用于统一团队环境与 CI/本地执行行为。

**关键字段**
- `SdkVersion`
- `RollForwardPolicy`
- `AllowPrerelease`
- `RequiredWorkloads`

**本特性关注点**
- 从当前 .NET 9 基线切换到 .NET 10 基线
- 默认锁定到仓库已验证的 `10.0.102` SDK 补丁版本
- 需要与本地 MAUI 工作负载保持一致
- 若版本不一致，应能通过标准 CLI 验证快速暴露

## 2. ProjectTargetProfile

**描述**：单个项目在升级中的目标框架与平台配置快照。

**关键字段**
- `ProjectName`
- `ProjectPath`
- `TargetFrameworks`
- `UseMaui`
- `SupportedOSPlatformVersions`
- `TestHostFramework`

**本特性关注点**
- `HomeHoney.csproj` 从 `net9.0` / `net9.0-*` 迁移到 `net10.0` / `net10.0-*`
- `HomeHoney.Tests.csproj` 从 `net9.0` 迁移到 `net10.0`
- 条件 Windows TFM 与平台最低版本规则需要保留或明确调整

## 3. DependencyBaseline

**描述**：直接受 .NET 主版本升级影响的依赖与工具链清单。

**关键字段**
- `SdkProvidedDependencies`
- `PackageReferences`
- `MauiWorkloadState`
- `ToolingRequirements`

**本特性关注点**
- SDK 自带能力与 NuGet 依赖版本必须兼容 .NET 10
- 测试依赖需要继续支持 `dotnet test`
- MAUI 工作负载需要与新 SDK 对齐

## 4. ValidationScenario

**描述**：判断升级是否完成的一组可执行验证场景。

**关键字段**
- `ScenarioName`
- `CommandOrAction`
- `ExpectedOutcome`
- `Blocking`
- `AffectedScope`

**本特性关注点**
- 至少覆盖依赖还原、工作负载恢复、解决方案构建、测试项目运行和核心流程抽样
- 每个验证场景都必须有可判断的通过/失败结果
- 失败时能定位到环境、构建、测试或平台兼容维度

## 5. CompatibilityNote

**描述**：升级过程中需要记录的兼容性事项或 breaking changes 观察结果。

**关键字段**
- `Area`
- `Trigger`
- `Impact`
- `Mitigation`
- `Verification`

**本特性关注点**
- 记录 .NET 10 TFM 命名、MAUI 工作负载恢复要求与可能的行为差异
- 对当前仓库真正受影响的兼容点给出最小必要修复策略
- 需要能支撑后续任务拆分与回归验证

## 关系与状态规则

### 升级关系
- 一个 `RuntimeBaseline` 影响多个 `ProjectTargetProfile`
- 一个 `ProjectTargetProfile` 依赖一个对应的 `DependencyBaseline`
- 多个 `ValidationScenario` 共同判断升级是否完成
- 若存在兼容风险，则通过 `CompatibilityNote` 与具体验证场景关联

### 状态流转规则
- `RuntimeBaseline`：旧基线 → 新基线候选 → 新基线已验证
- `ProjectTargetProfile`：未升级 → 已更新目标框架 → 构建通过 → 验证通过
- `ValidationScenario`：未执行 → 通过 / 失败
- `CompatibilityNote`：已识别 → 已处理 → 已验证

### 完成定义映射
- 所有 `ProjectTargetProfile` 必须达到“验证通过”
- 所有阻塞型 `ValidationScenario` 必须为“通过”
- 所有关键 `CompatibilityNote` 必须达到“已验证”
