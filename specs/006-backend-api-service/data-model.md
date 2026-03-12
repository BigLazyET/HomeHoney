# Data Model: 通过后端 API 托管存储集成

## 1. BackendServiceProfile

**描述**：移动端用于连接统一后端服务的目标配置，替代当前面向 FileBrowser/Mongo 的下游连接概念。

**关键字段**
- `ProfileId`
- `DisplayName`
- `ApiBaseUrl`
- `IsActive`
- `LastValidatedAt`
- `ValidationStatus`
- `ValidationMessage`

**验证规则**
- `ApiBaseUrl` 必须是可解析的绝对 URI
- 同一时间只能有一个激活的后端连接配置
- 保存失败不得覆盖最近一次可用配置

**状态流转**
- `Draft` → `Saved`
- `Saved` → `Validated` / `ValidationFailed`
- `Validated` → `Active`
- `Active` → `Archived`

## 2. DownstreamStorageSettings

**描述**：后端用于访问 FileBrowser 与 MongoDB 的下游连接设置，由后端维护并在需要时通过管理接口读写。

**关键字段**
- `SettingsId`
- `FileServiceBaseUrl`
- `FileServiceApiPath`
- `FileServiceCredentialRef`
- `MongoConnectionStringRef`
- `MongoDatabaseName`
- `IsEditableAtRuntime`
- `LastValidatedAt`
- `ValidationStatus`

**验证规则**
- 文件服务地址必须能构成有效 API 基地址
- Mongo 连接必须唯一确定目标数据库
- 敏感连接信息不得以普通明文形式返回给移动端

**状态流转**
- `Draft` → `Validated`
- `Validated` → `Active`
- `Active` → `Invalid`
- `Invalid` → `Validated`

## 3. FileBackedDocumentDto

**描述**：后端返回给移动端的文件性质资源读模型，覆盖保险合同、说明书等文档记录与其附件状态。

**关键字段**
- `DocumentId`
- `DocumentType`
- `Title`
- `Summary`
- `Tags`
- `OwnerMemberId`
- `RelatedSpaceId`
- `BusinessStatus`
- `PrimaryFile`
- `AttachmentCount`
- `IntegrityStatus`
- `SyncMessage`
- `UpdatedAt`

**子对象：PrimaryFile**
- `FileId`
- `FileName`
- `ContentType`
- `SizeBytes`
- `AvailabilityStatus`
- `LastSyncedAt`
- `DownloadUrl` 或 `DownloadAction`

**验证规则**
- 文件性质资源必须有明确的业务类型（如 `Insurance`、`Manual`）
- 文件状态异常时不得伪装为可正常下载
- `IntegrityStatus` 必须反映元数据与文件本体的一致性

**状态流转**
- `Draft` → `PendingUpload`
- `PendingUpload` → `Healthy`
- `Healthy` → `FileMissing` / `MetadataOutOfSync` / `SyncError`
- `FileMissing` / `MetadataOutOfSync` / `SyncError` → `Healthy` / `Archived`

## 4. NonFileAggregateDto

**描述**：后端返回给移动端的非文件业务聚合结果，覆盖冰箱贴、备忘录、提醒、偏好、参考数据等结构化业务内容。

**关键字段**
- `AggregateType`
- `RecordId`
- `Title`
- `Summary`
- `Payload`
- `OwnerMemberId`
- `RelatedDocumentIds`
- `CreatedAt`
- `UpdatedAt`
- `OperationStatus`
- `UserVisibleMessage`

**聚合映射建议**
- `FridgeNote`
- `Memo`
- `ReminderItem`
- `UserPreference`
- `HouseholdMember`
- `Space`

**验证规则**
- 每类聚合仍保持强类型结构，不退化为无约束 blob
- 移动端不应直接感知 Mongo 集合名或原始持久化字段
- 错误消息必须是业务可见语义，而不是下游驱动异常文本直通

**状态流转**
- `PendingCreate` → `Active`
- `Active` → `PendingUpdate` / `PendingDelete`
- `PendingUpdate` → `Active` / `SyncError`
- `PendingDelete` → `Deleted` / `SyncError`

## 5. AggregatedReminderView

**描述**：后端为提醒中心生成的聚合读模型，将保险、说明书、冰箱贴、备忘录等来源统一成可直接展示的提醒结果。

**关键字段**
- `ReminderId`
- `SourceType`
- `SourceId`
- `Title`
- `Summary`
- `DueAt`
- `Priority`
- `Status`
- `Href` 或 `NavigationTarget`
- `RelatedDocumentState`

**验证规则**
- 后端必须保证提醒结果能直接驱动页面展示，不要求移动端重新聚合
- 如果提醒依赖的文件资源异常，应通过 `RelatedDocumentState` 等字段体现

## 6. AggregatedSearchResult

**描述**：后端为全局搜索生成的统一检索结果，聚合资料、协作与其他业务模块。

**关键字段**
- `GroupTitle`
- `Items[]`

**搜索项字段**
- `ItemId`
- `Category`
- `Title`
- `Summary`
- `NavigationTarget`
- `StatusMessage`

**验证规则**
- 结果分组必须体现业务类别，而不是底层存储来源
- 文件状态或同步状态仅以业务可理解形式返回

## 7. ApiOperationResult

**描述**：后端统一返回给移动端的业务操作结果，覆盖成功、失败、部分成功、待修复等状态。

**关键字段**
- `OperationId`
- `OperationType`
- `TargetType`
- `TargetId`
- `IsSuccess`
- `IsPartialSuccess`
- `Code`
- `Message`
- `Details`
- `OccurredAt`

**验证规则**
- 任何跨 FileBrowser 与 MongoDB 的写操作都必须能映射到统一业务结果
- 不得把下游部分成功错误地表示为完全成功
- 移动端可以直接根据结果字段展示用户可理解反馈

## 关系与边界

- 一个 `BackendServiceProfile` 决定移动端当前连接的后端目标
- 一个 `DownstreamStorageSettings` 决定后端当前访问 FileBrowser 与 MongoDB 的方式
- 一个 `FileBackedDocumentDto` 可关联一个主文件以及多个额外附件
- `AggregatedReminderView` 与 `AggregatedSearchResult` 来源于 `FileBackedDocumentDto` 和 `NonFileAggregateDto` 的后端聚合
- 每次写操作都应产生一个 `ApiOperationResult`
- 下游存储细节只存在于后端边界内，不进入移动端公共契约

## 完成定义映射

- 连接迁移完成：移动端只使用 `BackendServiceProfile`
- 文件接口完成：`FileBackedDocumentDto` 支持列表、详情、上传、下载与状态识别
- 非文件接口完成：`NonFileAggregateDto` 覆盖现有核心业务聚合并支持 CRUD
- 聚合接口完成：`AggregatedReminderView` 与 `AggregatedSearchResult` 可直接供页面使用
- 结果语义完成：`ApiOperationResult` 能统一表达成功、失败与部分成功
