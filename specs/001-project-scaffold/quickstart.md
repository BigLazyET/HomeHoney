# Quickstart: 项目脚手架搭建

**Branch**: `001-project-scaffold` | **Date**: 2026-03-09

## 前提条件

| 工具 | 版本 | 安装方式 |
|------|------|---------|
| .NET SDK | 9.0.100+ | https://dot.net/download |
| Xcode | 15+ (含 iOS 模拟器) | Mac App Store |
| Android SDK | API 33+ | Android Studio 或 `dotnet workload install android` |
| MAUI 工作负载 | - | `dotnet workload install maui` |

验证安装：
```bash
dotnet --version          # 应显示 9.0.xxx
dotnet workload list      # 应包含 maui
```

## 快速开始

### 1. 克隆并构建

```bash
git clone https://github.com/BigLazyET/HomeHoney.git
cd HomeHoney
dotnet build
```

预期结果：编译成功，零错误零警告。

### 2. 运行测试

```bash
dotnet test
```

预期结果：所有测试通过。

### 3. 在模拟器上运行

**iOS 模拟器（macOS）**:
```bash
dotnet build -t:Run -f net9.0-ios
```

**Android 模拟器**:
```bash
dotnet build -t:Run -f net9.0-android
```

**macOS 桌面**:
```bash
dotnet build -t:Run -f net9.0-maccatalyst
```

预期结果：应用启动，显示 "HomeHoney" 标题和空状态页面。

## 项目结构一览

```
HomeHoney.sln              → 解决方案入口
HomeHoney/                  → 主应用 (MAUI Blazor Hybrid)
  Components/Pages/         → 页面级 Razor 组件
  Components/Layout/        → 布局组件
  Components/Shared/        → 共享组件
  Services/                 → 服务接口（预留）
HomeHoney.Tests/            → xUnit + bUnit 测试
  Unit/                     → 服务层单元测试
  Component/                → Blazor 组件测试
```

## 常见问题

| 问题 | 解决方案 |
|------|---------|
| `global.json` 版本不匹配 | 安装 .NET 9 SDK: `dotnet --list-sdks` 检查 |
| MAUI 工作负载缺失 | `dotnet workload install maui` |
| iOS 构建失败 | 确保 Xcode 已安装，运行 `xcode-select --install` |
| Android 模拟器无法启动 | 确保 Android SDK 已安装，`$ANDROID_HOME` 已设置 |
