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

## 许可证

MIT
