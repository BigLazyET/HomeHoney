# HomeHoney

家庭说明书和保险合同库 — 一站式管理家庭文档和保险合同的移动应用。

## 技术栈

- **框架**: .NET 9 / MAUI Blazor Hybrid
- **语言**: C# 12
- **测试**: xUnit + bUnit + Moq
- **目标平台**: iOS 16+、Android 13+ (API 33+)、macOS 15+ (可选)

## 前置条件

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (9.0.100+)
- MAUI 工作负载：

  ```bash
  dotnet workload install maui
  ```

- **iOS 开发**: macOS + Xcode 15+
- **Android 开发**: Android SDK (API 33+)

## 快速开始

```bash
# 克隆仓库
git clone https://github.com/BigLazyET/HomeHoney.git
cd HomeHoney

# 还原依赖
dotnet restore

# 构建（所有平台）
dotnet build

# 运行测试
dotnet test

# macOS 运行
dotnet build -f net9.0-maccatalyst -t:Run

# iOS 模拟器运行
dotnet build -f net9.0-ios -t:Run -p:_DeviceName=:v2:udid=YOUR_SIMULATOR_UDID

# Android 模拟器运行
dotnet build -f net9.0-android -t:Run
```

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

- 解决方案构建：`dotnet build HomeHoney.sln` ✅
- 测试项目：`dotnet test HomeHoney.Tests/HomeHoney.Tests.csproj` ✅

## 许可证

MIT
