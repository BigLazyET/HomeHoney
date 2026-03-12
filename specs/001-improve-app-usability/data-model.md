# Data Model: Improve App Usability

## 1. RecordDraft

**描述**：用户在说明书、保险、备忘录、冰箱贴页面中可编辑的草稿状态，表示“尚未最终提交、但应被完整保留”的录入内容。

**关键字段**
- `DraftType`（Manual / Insurance / Memo / FridgeNote）
- `RecordId`
- `TitleOrPrimaryLabel`
- `SecondaryFields`
- `OptionalFields`
- `DirtyFields`
- `ValidationIssues`
- `IsNewRecord`
- `LastEditedAt`

**验证规则**
- 必填字段不得为空、全空白字符或仅由分隔符组成
- 新增表单初始值必须反映“未选择 / 未填写”状态，而不是误导性业务默认值
- 校验失败时必须保留用户已输入的其他有效字段
- 历史记录中的空值必须可被读取并映射到可展示的缺省状态

**状态流转**
- `Pristine` → `Editing`
- `Editing` → `ValidationFailed` / `ReadyToSave`
- `ValidationFailed` → `Editing`
- `ReadyToSave` → `Saving`
- `Saving` → `Saved` / `SaveFailed`
- `SaveFailed` → `Editing`

## 2. ValidationIssue

**描述**：页面或服务层识别到的单项输入问题，用于驱动表单内即时提示与保存阻断。

**关键字段**
- `FieldKey`
- `Severity`（Info / Warning / Error）
- `Message`
- `Code`
- `IsBlocking`

**验证规则**
- 必填缺失必须标记为 `Error` 且 `IsBlocking=true`
- 非阻断提示不得被误显示为保存失败
- 同一字段的多条问题应能稳定聚合展示，避免重复轰炸用户

## 3. AttachmentSummaryView

**描述**：资料详情页附件区域面向用户展示的摘要状态，统一表达文件是否存在、文件元数据是否完整以及附件当前是否可用。

**关键字段**
- `DocumentId`
- `DocumentType`
- `HasAttachment`
- `FileName`
- `SizeBytes`
- `LastSyncedAt`
- `AvailabilityStatus`
- `UserVisibleStatusMessage`

**验证规则**
- 若 `HasAttachment=false`，必须显示明确的“暂无附件”语义
- 若元数据仅部分存在，页面必须区分“未知”与“未同步”而非显示错误数据
- 当附件不可下载时，状态展示不得误导用户认为文件仍然可用

**状态流转**
- `NoAttachment`
- `AttachmentAvailable`
- `AttachmentMetadataPartial`
- `AttachmentSyncUnknown`
- `AttachmentUnavailable`

## 4. RetryableCollectionState

**描述**：搜索、提醒和资料列表页面共享的读取状态模型，用于区分真正空结果与读取失败，并支持显式手动重试。

**关键字段**
- `ViewScope`（Documents / Reminders / Search）
- `Status`（Loading / Ready / Empty / Error / Retrying）
- `LastAttemptAt`
- `LastSuccessAt`
- `Message`
- `CanRetry`
- `RetainedItems`

**验证规则**
- `Empty` 与 `Error` 必须可区分，不能用同一提示替代
- `Retrying` 时页面必须仍保留上下文，不进入无响应状态
- 存在上一次成功数据时，失败反馈不应强制清空 `RetainedItems`

**状态流转**
- `Loading` → `Ready` / `Empty` / `Error`
- `Error` → `Retrying`
- `Retrying` → `Ready` / `Empty` / `Error`
- `Ready` → `Loading`（主动刷新）

## 5. PresentationFallback

**描述**：页面针对空字段、缺失元数据和历史脏数据给出的统一缺省展示语义。

**关键字段**
- `FieldKey`
- `FallbackKind`（MissingValue / UnknownValue / NotUploaded / NotSynced / NotApplicable）
- `DisplayText`
- `IconOrTone`
- `IsUserActionSuggested`

**验证规则**
- 同类缺省语义在不同页面中的表达应保持一致
- 缺省展示不得暴露技术异常、集合名或底层驱动信息
- 对用户可补救的问题，应给出后续动作指引

## 6. CollectionIndexProfile

**描述**：后端为 Mongo 集合维护的统一索引定义，面向当前稳定读取路径和后续可扩展检索路径。

**关键字段**
- `CollectionName`
- `PrimaryAccessPatterns`
- `SortKeys`
- `FilterKeys`
- `SearchCandidateFields`
- `IndexDefinitions`
- `LastEnsuredAt`

**建议索引关注点**
- `insurance_records`：`Id`、`ExpiryDate`、`Status`、`InsuredMemberId`、`LastUpdatedAt`、文本字段（`PolicyName`/`ProviderName`/`Summary`/`Tags`）
- `manual_records`：`Id`、`SpaceId`、`WarrantyExpiryDate`、`Brand`、`LastUpdatedAt`、文本字段（`DeviceName`/`Brand`/`Summary`/`Tags`）
- `fridge_notes`：`Id`、`IsPinned`、`IsCompleted`、`DueAt`、`UpdatedAt`、文本字段（`Title`/`Content`）
- `memos`：`Id`、`Status`、`DueAt`、`UpdatedAt`、文本字段（`Title`/`Content`）
- `user_preferences`：单记录读取保证与更新时间字段
- `household_members`：`Id`、`IsPrimary`、`DisplayName`
- `spaces`：`Id`、`SortOrder`、`Name`

**验证规则**
- 每个集合的索引必须与明确的读取/排序/筛选场景对应，避免无意义堆叠
- 索引初始化必须可重复执行且不会破坏现有数据
- 对搜索候选字段的索引策略必须为后续查询下推预留空间，但不强制本次同步重写全部搜索实现

## 关系与边界

- `RecordDraft` 在保存前会生成 0..n 条 `ValidationIssue`
- `AttachmentSummaryView` 来源于资料记录及其关联文件元数据
- `RetryableCollectionState` 驱动搜索、提醒和资料列表的可见状态与重试行为
- `PresentationFallback` 为 `RecordDraft` 和 `AttachmentSummaryView` 中的缺失数据提供统一显示语义
- `CollectionIndexProfile` 仅存在于后端边界，用于保障列表读取和未来检索演进

## 完成定义映射

- 表单体验完成：`RecordDraft` 与 `ValidationIssue` 能支撑空白起步、必填提示和失败后保留输入
- 附件体验完成：`AttachmentSummaryView` 能稳定展示文件名、大小、最后同步时间及缺省状态
- 列表恢复完成：`RetryableCollectionState` 能让搜索、提醒和资料列表在失败后手动恢复
- 稳定性完成：`PresentationFallback` 能覆盖历史脏数据与空字段展示
- 检索准备完成：`CollectionIndexProfile` 覆盖所有现有 Mongo 集合的索引规划
