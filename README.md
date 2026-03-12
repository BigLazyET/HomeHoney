# HomeHoney

家庭说明书和保险合同库 — 一站式管理家庭文档和保险合同的移动应用。

## 技术栈

- **框架**: .NET 10 / MAUI Blazor Hybrid
- **语言**: C# 14
- **测试**: xUnit + bUnit + Moq
- **目标平台**: iOS 16+、Android 13+ (API 33+)、macOS 15+ (可选)

## 前置条件

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (10.0.102+)
- MAUI 工作负载：

  ```bash
  dotnet workload restore
  ```

- **iOS 开发**: macOS + Xcode 15+
- **Android 开发**: Android SDK (API 33+)

> 当前仓库在 .NET 10 平台构建验证中额外观察到：
> - iOS / Mac Catalyst 使用当前 .NET 10 工具链时需要 **Xcode 26.2**
> - Android 构建链路中的 manifest merger 需要 **Java 17+**（当前 Java 11 会失败）

## 快速开始

```bash
# 克隆仓库
git clone https://github.com/BigLazyET/HomeHoney.git
cd HomeHoney

# 还原依赖
dotnet restore

# 恢复 MAUI 工作负载
dotnet workload restore

# 构建（所有平台）
dotnet build

# 运行测试
dotnet test

# macOS 运行
dotnet build -f net10.0-maccatalyst -t:Run

# iOS 模拟器运行
dotnet build -f net10.0-ios -t:Run -p:_DeviceName=:v2:udid=YOUR_SIMULATOR_UDID

# Android 模拟器运行
dotnet build -f net10.0-android -t:Run
```

## 升级后验证链路

建议按以下顺序验证当前仓库的 .NET 10 基线：

1. `dotnet --version` 确认当前 SDK 已切换到 10.0.102+
2. `dotnet workload restore` 恢复与 .NET 10 对应的 MAUI 工作负载
3. `dotnet build HomeHoney.sln` 验证主应用与测试工程构建
4. `dotnet test HomeHoney.Tests/HomeHoney.Tests.csproj` 验证现有回归测试
5. 手动抽样首页、欢迎流、资料、冰箱贴、提醒与设置入口

如果 `dotnet build HomeHoney.sln` 在 iOS / Mac Catalyst 或 Android 上失败，先确认是否满足以下平台工具前提：

- Xcode 已升级到 26.2 或更高
- `java -version` 输出为 17 或更高

## 后端与下游存储本地约定

当前仓库正在切换到“移动端 → HomeHoney.Api → 外部文件服务 / MongoDB”的集成方案，默认按以下本地开发约定验证：

- HomeHoney.Api 地址：`http://localhost:7080`
- 文件服务基础地址：`http://localhost:8999`
- 文件服务 API 路径：`/api`
- MongoDB 连接串：`mongodb://et:.netcore@localhost:27017/homehoney?authSource=homehoney`
- MongoDB 数据库名：`homehoney`

### 配置与本地保留范围

- 这些连接目标可在应用内的“设置 → 外部存储”页面修改并保存。
- 移动端优先保存和使用后端 API 地址，再由后端统一保存下游文件服务与 MongoDB 连接。
- 文件服务与 MongoDB 的敏感连接信息会与普通偏好分开保存。
- 应用本地只保留：
  - 用户偏好（Preferences）
  - 敏感连接信息（SecureStorage）

### 当前实现边界

- 保险合同、说明书等文件本体走外部文件服务。
- 冰箱贴、备忘录、提醒、偏好以及文档元数据走 MongoDB。
- 不再把业务数据或文件内容持久化到本地文件系统。

### 建议验证

1. 先确保本地 FileBrowser 和 MongoDB 服务已启动。
2. 启动 `HomeHoney.Api`，确认其可访问 `http://localhost:7080/health`。
3. 进入应用的“设置 → 外部存储”，先保存后端地址，再保存下游连接。
4. 再验证资料列表、详情、上传/下载，以及非文件数据的新增和读取流程。
5. 关闭任一远端服务后重新进入相关页面，确认页面显示明确失败反馈，而不是误显示为仍可正常读取远端数据。

## 项目结构

```text
HomeHoney.sln                # 解决方案文件
global.json                  # SDK 版本锁定
.editorconfig                # 代码风格规范

HomeHoney/                   # 主应用 (MAUI Blazor Hybrid)
├── Components/
│   ├── Pages/               # 页面组件
│   ├── Layout/              # 布局组件
│   └── Shared/              # 共享组件
├── Services/                # 服务层
├── Platforms/               # 平台特定代码
├── Resources/               # 应用资源 (图标、字体等)
└── wwwroot/                 # 静态资源

HomeHoney.Tests/             # 测试项目
├── Unit/                    # 单元测试
└── Component/               # bUnit 组件测试
```

## 页面结构

当前应用采用移动优先的信息架构：

- 一级导航：**首页 / 资料 / 冰箱贴 / 提醒 / 设置**
- 欢迎引导：`/welcome` → `/welcome/features` → `/welcome/get-started`
- 资料模块：`/documents` 下分为保险与说明书，两者都支持列表、详情、新增/编辑
- 家庭协作：冰箱贴用于短提醒，备忘录用于长期正式记录
- 跨模块能力：搜索页 `/search` 与提醒中心 `/reminders` 用于统一发现与跳转

### 当前交互规则

- 保险、说明书和冰箱贴均支持“确认后删除”，删除后返回仍然有效的模块页面
- 所有二级及更深页面统一显示返回按钮，按业务层级返回上一级，而不是强依赖历史栈
- 欢迎引导只在首次未完成时自动展示；后续可从设置中主动回看，但不会重置默认启动行为

### 主要页面路由

```text
/                         首页摘要
/welcome                  欢迎页
/welcome/features         功能介绍
/welcome/get-started      完成引导
/documents                资料首页
/documents/insurance      保险列表
/documents/manuals        说明书列表
/fridge-notes             冰箱贴
/memos                    家庭备忘录
/reminders                提醒中心
/search                   全局搜索
/settings                 设置首页
```

## 验证状态

- 升级目标：`.NET 10` 基线
- 推荐构建验证：`dotnet build HomeHoney.sln`
- 推荐测试验证：`dotnet test HomeHoney.Tests/HomeHoney.Tests.csproj`

## 许可证

MIT
