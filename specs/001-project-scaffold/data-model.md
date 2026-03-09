# Data Model: 项目脚手架搭建

**Branch**: `001-project-scaffold` | **Date**: 2026-03-09

## 概述

脚手架需求不涉及业务数据实体（文档、保单等将在后续需求中定义）。本文档记录项目级的"结构实体"和配置决策。

## 项目实体

### Solution: HomeHoney.sln

| 属性 | 值 |
|------|-----|
| 项目数量 | 2（主应用 + 测试） |
| SDK 版本 | .NET 9（通过 global.json 锁定） |

### Project: HomeHoney (主应用)

| 属性 | 值 |
|------|-----|
| SDK | Microsoft.NET.Sdk.Razor |
| TargetFrameworks | net9.0;net9.0-android;net9.0-ios;net9.0-maccatalyst |
| UseMaui | true |
| OutputType | Exe (平台 TFM) / Library (net9.0) |
| TreatWarningsAsErrors | true |
| Nullable | enable |
| ImplicitUsings | enable |

关键依赖：
- Microsoft.AspNetCore.Components.WebView.Maui
- Microsoft.Extensions.Logging.Debug (DEBUG only)

### Project: HomeHoney.Tests (测试)

| 属性 | 值 |
|------|-----|
| SDK | Microsoft.NET.Sdk.Razor |
| TargetFramework | net9.0 |
| IsPackable | false |

关键依赖：
- bunit, xunit, xunit.runner.visualstudio
- Microsoft.NET.Test.Sdk, coverlet.collector
- Moq
- ProjectReference → HomeHoney

## 配置模型

### global.json

```json
{
  "sdk": {
    "version": "9.0.100",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  }
}
```

### DI 容器初始注册 (MauiProgram.cs)

| 服务 | 生命周期 | 说明 |
|------|----------|------|
| BlazorWebView services | Scoped (框架管理) | `AddMauiBlazorWebView()` |
| BlazorWebViewDeveloperTools | Scoped (DEBUG only) | `AddBlazorWebViewDeveloperTools()` |

> **注意**: 后续需求将在此处注册业务服务（如 IDocumentService、ISqliteService 等）

## 状态转换

不适用 — 脚手架阶段无业务状态流转。

## 验证规则

| 规则 | 约束 |
|------|------|
| SDK 版本 | .NET 9.0.100+，不允许预览版 |
| 编译 | 零错误零警告 |
| 测试 | 100% 通过率 |
| TargetFrameworks 包含 net9.0 | 必须，用于 bUnit 测试引用 |
