# Data Model: 资料删除、返回导航与首次引导优化

## 1. InsuranceRecord

**描述**：资料模块中的保险记录，现有对象继续作为删除目标之一。

**关键字段**
- `Id`
- `PolicyName`
- `Status`
- `EffectiveDate`
- `ExpiryDate`
- `LastUpdatedAt`

**本特性关注点**
- 支持从详情页和编辑页触发删除
- 删除后不得继续作为首页摘要、搜索结果和列表数据源
- 删除后所属详情路由应视为无效

## 2. ManualRecord

**描述**：资料模块中的说明书记录，现有对象继续作为删除目标之一。

**关键字段**
- `Id`
- `DeviceName`
- `Brand`
- `Model`
- `WarrantyExpiryDate`
- `LastUpdatedAt`

**本特性关注点**
- 支持从详情页和编辑页触发删除
- 删除后不得继续在资料列表、首页摘要和搜索结果中出现
- 删除后需要回到说明书列表或资料模块主页

## 3. FridgeNote

**描述**：家庭冰箱贴条目，支持快速查看、编辑和删除。

**关键字段**
- `Id`
- `Title`
- `Content`
- `Priority`
- `IsPinned`
- `IsCompleted`
- `DueAt`
- `UpdatedAt`

**本特性关注点**
- 支持从编辑页或可扩展的详情上下文触发删除
- 删除后不得继续出现在冰箱贴板、首页摘要和提醒聚合中
- 删除后回到冰箱贴主列表

## 4. UserPreference

**描述**：用户偏好对象，继续承载首次引导完成状态。

**关键字段**
- `ThemeMode`
- `NotificationSettings`
- `HomeModuleOrder`
- `HiddenHomeModules`
- `PrivacyMode`
- `OnboardingCompleted`

**本特性关注点**
- `OnboardingCompleted = false` 时，应用首次自动进入引导
- `OnboardingCompleted = true` 时，后续启动直接进入主页面
- 主动回看引导不得默认把 `OnboardingCompleted` 重置为 `false`

## 5. PageNavigationContext

**描述**：页面级导航上下文，不一定需要单独持久化实体，但在设计上需要明确“当前页面的上一级目标”。

**关键属性**
- `CurrentRoute`
- `ParentRoute`
- `ShowBackButton`
- `FallbackRoute`

**本特性关注点**
- 一级入口页默认 `ShowBackButton = false`
- 二级及更深页面默认 `ShowBackButton = true`
- 删除成功后优先回到 `ParentRoute` 或模块级 `FallbackRoute`

## 关系与状态规则

### 删除关系
- `InsuranceRecord` 删除后：
  - 从保险列表移除
  - 从首页资料摘要移除
  - 从搜索与提醒聚合中移除
- `ManualRecord` 删除后：
  - 从说明书列表移除
  - 从首页资料摘要移除
  - 从搜索与提醒聚合中移除
- `FridgeNote` 删除后：
  - 从冰箱贴页面移除
  - 从首页家庭留言摘要或提醒聚合中移除

### 引导状态规则
- `OnboardingCompleted = false` 且用户启动应用 → 自动进入引导流程
- 用户完成引导 → `OnboardingCompleted = true`
- `OnboardingCompleted = true` 后再次启动 → 默认进入主页面
- 用户主动回看引导 → 不改变默认启动规则

### 导航规则
- 一级页面：不强制显示返回按钮
- 二级页面：显示返回按钮，返回对应列表或模块主页
- 三级页面（如编辑页）：显示返回按钮，优先返回详情页或模块列表，避免直接跳首页
