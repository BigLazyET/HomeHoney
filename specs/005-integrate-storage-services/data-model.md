# Data Model: 集成外部文件与数据存储

## 1. StorageConnectionProfile

**描述**：应用内可编辑的外部存储连接配置，用于决定当前文件服务和数据服务访问目标。

**关键字段**
- `ProfileId`
- `DisplayName`
- `FileServiceBaseUrl`
- `FileServiceApiPath`
- `FileServiceCredentialRef`
- `MongoHostOrConnectionRef`
- `MongoDatabaseName`
- `IsActive`
- `LastValidatedAt`
- `ValidationStatus`

**验证规则**
- 文件服务地址必须是可解析的绝对 URI
- Mongo 配置必须能唯一确定目标数据库
- 同一时间只能有一个激活配置
- 保存失败不得覆盖现有可用配置

**状态流转**
- `Draft` → `Saved`
- `Saved` → `Validated` / `ValidationFailed`
- `Validated` → `Active`
- `Active` → `Archived`

## 2. FileResource

**描述**：由外部文件服务托管的单个文件对象及其在应用中的可追踪引用。

**关键字段**
- `FileResourceId`
- `ExternalFileId`
- `ExternalPath`
- `FileName`
- `ContentType`
- `SizeBytes`
- `ChecksumOrEtag`
- `UploadedAt`
- `LastSyncedAt`
- `CachedLocalPath`
- `AvailabilityStatus`

**验证规则**
- `ExternalFileId` 或 `ExternalPath` 至少有一项可用于重新定位远端文件
- `FileName`、`ContentType`、`SizeBytes` 在上传完成后必须可追踪
- 文件状态异常时不得伪装成可正常下载

**状态流转**
- `PendingUpload` → `Available`
- `Available` → `Cached`
- `Available` / `Cached` → `Missing` / `SyncError`
- `Missing` / `SyncError` → `Recovered` / `Archived`

## 3. FileBackedDocumentRecord

**描述**：保险合同、说明书等文件性质资源在业务层的结构化记录，保存于 MongoDB，并关联一个或多个 `FileResource`。

**关键字段**
- `DocumentId`
- `DocumentType` (`Insurance` / `Manual` / future file-backed types)
- `Title`
- `Summary`
- `Category`
- `Tags`
- `OwnerMemberId`
- `RelatedSpaceId`
- `BusinessStatus`
- `PrimaryFileResourceId`
- `AdditionalFileResourceIds`
- `IntegrityStatus`
- `CreatedAt`
- `UpdatedAt`

**类型特有字段**
- 保险合同：`ProviderName`、`EffectiveDate`、`ExpiryDate`、`ContactName`、`ContactPhone`
- 说明书：`Brand`、`Model`、`PurchaseDate`、`WarrantyExpiryDate`

**验证规则**
- 文件性质资源必须至少关联一个主文件引用，除非记录仍处于草稿或待上传状态
- 标签、分类和关系字段必须以结构化方式保存，不能依赖文件名解析
- `IntegrityStatus` 必须反映元数据与文件本体是否一致

**状态流转**
- `Draft` → `PendingUpload`
- `PendingUpload` → `Healthy`
- `Healthy` → `MetadataOutOfSync` / `FileMissing` / `Archived`
- `MetadataOutOfSync` / `FileMissing` → `Healthy` / `Archived`

## 4. BusinessAggregateRecord

**描述**：除文件本体外需要持久化到 MongoDB 的业务数据记录集合，覆盖冰箱贴、备忘录、提醒、用户偏好、家庭成员、空间等实体。

**关键字段**
- `AggregateType`
- `RecordId`
- `Payload`
- `OwnerMemberId`
- `CreatedAt`
- `UpdatedAt`
- `DeletedAt`
- `SyncStatus`

**聚合映射建议**
- `FridgeNote`
- `Memo`
- `ReminderItem`
- `UserPreference`
- `HouseholdMember`
- `Space`

**验证规则**
- 每个聚合保持当前强类型字段结构，不降级为无约束 blob
- 软删除或硬删除策略必须在同一聚合内保持一致
- 需要支持标准新增、查询、修改、删除语义

**状态流转**
- `PendingCreate` → `Active`
- `Active` → `PendingUpdate` / `PendingDelete`
- `PendingUpdate` → `Active` / `SyncError`
- `PendingDelete` → `Deleted` / `SyncError`

## 5. SyncOperation

**描述**：跟踪本地缓存、文件服务和 MongoDB 之间一次操作的最终一致性结果。

**关键字段**
- `OperationId`
- `OperationType` (`UploadFile`, `SaveMetadata`, `DeleteRecord`, `DownloadFile`, `RefreshCache`)
- `TargetEntityType`
- `TargetEntityId`
- `StartedAt`
- `CompletedAt`
- `RemoteFileStatus`
- `RemoteDataStatus`
- `LocalCacheStatus`
- `UserVisibleMessage`
- `RetryCount`

**验证规则**
- 任一跨服务写操作都必须可追踪到一条同步结果
- 失败信息必须可归类为配置、认证、网络、远端异常或数据不一致
- 不能把双写中的部分成功误报为完全成功

**状态流转**
- `Queued` → `Running`
- `Running` → `Succeeded` / `PartiallySucceeded` / `Failed`
- `Failed` / `PartiallySucceeded` → `Retried` / `Dismissed`

## 关系与边界

- 一个 `StorageConnectionProfile` 驱动当前文件服务和 MongoDB 访问上下文
- 一个 `FileBackedDocumentRecord` 关联一个主 `FileResource`，并可扩展到多个附加文件
- 多个 `BusinessAggregateRecord` 可通过 `DocumentId`、`OwnerMemberId`、`RelatedSpaceId` 等字段关联文件性质资源
- 每次远端读写都应产生或更新一条 `SyncOperation`
- 本地 SQLite 缓存保存 `FileBackedDocumentRecord`、`BusinessAggregateRecord` 和 `SyncOperation` 的离线快照；本地文件缓存保存已下载文件副本

## 完成定义映射

- 配置功能完成：`StorageConnectionProfile` 可保存、激活并复用
- 文件功能完成：`FileResource` 与 `FileBackedDocumentRecord` 能稳定支持列表、上传、下载和状态识别
- 非文件数据功能完成：`BusinessAggregateRecord` 覆盖现有核心业务聚合并支持 CRUD
- 一致性设计完成：`SyncOperation` 能解释跨文件服务与 MongoDB 的成功、失败和补偿结果
