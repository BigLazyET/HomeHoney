# Data Model: App 页面结构与导航规划

## 1. HouseholdMember

**描述**：家庭中的个人主体，可作为保险被保人、提醒归属人、备忘责任人。

**字段**
- `id`: 唯一标识
- `displayName`: 展示名称
- `role`: 家庭角色（如本人、配偶、孩子、父母）
- `avatarColor`: 用于 UI 区分的视觉标识
- `isPrimary`: 是否主账号关注对象

**关系**
- 一个 `HouseholdMember` 可关联多个 `InsuranceRecord`
- 一个 `HouseholdMember` 可关联多个 `Memo`
- 一个 `HouseholdMember` 可关联多个 `ReminderItem`

## 2. Space

**描述**：家庭中的空间或区域，用于说明书和部分备忘的归类，如厨房、客厅、儿童房。

**字段**
- `id`
- `name`
- `icon`
- `sortOrder`

**关系**
- 一个 `Space` 可关联多个 `ManualRecord`
- 一个 `Space` 可关联多个 `FridgeNote`

## 3. InsuranceRecord

**描述**：保险资料记录。

**字段**
- `id`
- `policyName`
- `insuredMemberId`
- `insuranceCategory`: 医疗险、重疾险、意外险等
- `status`: 生效中、待续保、已失效、已归档
- `effectiveDate`
- `expiryDate`
- `providerName`
- `contactName`
- `contactPhone`
- `summary`
- `attachmentCount`
- `tags`
- `lastUpdatedAt`

**关系**
- 多对一关联 `HouseholdMember`
- 一对多派生 `ReminderItem`

**验证规则**
- `policyName` 必填
- `insuredMemberId` 必填
- `expiryDate` 不可早于 `effectiveDate`
- `status` 必须为预定义状态之一

## 4. ManualRecord

**描述**：说明书或设备资料记录。

**字段**
- `id`
- `deviceName`
- `brand`
- `model`
- `spaceId`
- `manualCategory`: 家电、家具、电子产品、儿童用品等
- `purchaseDate`
- `warrantyExpiryDate`
- `summary`
- `attachmentCount`
- `tags`
- `lastUpdatedAt`

**关系**
- 多对一关联 `Space`
- 一对多派生 `ReminderItem`

**验证规则**
- `deviceName` 必填
- `spaceId` 推荐必填
- `warrantyExpiryDate` 不可早于 `purchaseDate`

## 5. FridgeNote

**描述**：高频、短文本、家庭共享的便签式记录。

**字段**
- `id`
- `title`
- `content`
- `category`: 购物、提醒、留言、待办
- `priority`: 普通、重要、紧急
- `colorStyle`: 便签颜色
- `isPinned`
- `isCompleted`
- `ownerMemberId`
- `dueAt`
- `createdAt`
- `updatedAt`

**关系**
- 可选关联 `HouseholdMember`
- 可选派生 `ReminderItem`

**验证规则**
- `content` 必填
- `priority` 仅允许预定义枚举值

## 6. Memo

**描述**：较正式、较长、适合长期保存的家庭备忘记录。

**字段**
- `id`
- `title`
- `content`
- `memoCategory`: 家规、长期计划、采购计划、就医记录、教育提醒等
- `importance`: 普通、重要、关键
- `status`: 进行中、已完成、已归档
- `ownerMemberId`
- `relatedSpaceId`
- `dueAt`
- `createdAt`
- `updatedAt`

**关系**
- 可选关联 `HouseholdMember`
- 可选关联 `Space`
- 可派生 `ReminderItem`

## 7. ReminderItem

**描述**：聚合后的待处理事项，用于提醒中心和首页摘要。

**字段**
- `id`
- `sourceType`: Insurance / Manual / FridgeNote / Memo
- `sourceId`
- `title`
- `summary`
- `dueAt`
- `priority`
- `status`: 待处理、即将到期、已完成、已忽略
- `ownerMemberId`
- `createdAt`

**关系**
- 指向一个源实体
- 可选关联 `HouseholdMember`

**状态流转**
- `待处理` → `已完成`
- `待处理` → `已忽略`
- `即将到期` → `待处理`
- `即将到期` → `已完成`

## 8. HomeModule

**描述**：首页摘要区中的可配置模块。

**字段**
- `id`
- `moduleType`: 最近资料、即将到期、快捷操作、家庭留言、推荐操作
- `title`
- `isVisible`
- `sortOrder`
- `maxItems`

## 9. UserPreference

**描述**：用户个性化偏好。

**字段**
- `themeMode`: Light / Dark / System
- `notificationSettings`: 按模块划分的提醒开关与粒度
- `homeModuleOrder`: 首页模块顺序
- `hiddenHomeModules`: 被隐藏的首页模块集合
- `privacyMode`: 普通显示、部分遮挡、进入详情前验证
- `onboardingCompleted`: 是否完成首次引导

## 关系总览

- `HouseholdMember` 1:N `InsuranceRecord`
- `HouseholdMember` 1:N `Memo`
- `Space` 1:N `ManualRecord`
- `InsuranceRecord` / `ManualRecord` / `FridgeNote` / `Memo` 1:N `ReminderItem`
- `UserPreference` 1:1 当前用户
- `HomeModule` 作为首页可配置视图模型集合存在
