# Quickstart: 通过后端 API 托管存储集成

## 1. 目标

验证 HomeHoney 在引入 `HomeHoney.Api` 后满足以下要求：

- 移动端只连接统一后端 API
- 后端统一连接 FileBrowser 与 MongoDB
- 文件类业务和非文件类业务都通过后端接口完成
- 搜索、提醒和详情聚合结果由后端提供
- 下游失败时，移动端看到的是业务化反馈，而不是底层服务异常细节

## 2. 前置条件

- 已安装 .NET 10 SDK
- 本地 FileBrowser 服务可用
- 本地 MongoDB 服务可用
- 解决方案中已包含：
  - `HomeHoney`
  - `HomeHoney.Api`
  - `HomeHoney.Tests`
  - `HomeHoney.Api.Tests`

## 3. 建议本地配置

### 后端服务

- Backend Base URL: `https://localhost:7080` 或 `http://localhost:5080`

### 下游文件服务

- File service base URL: `http://localhost:8999`
- File service API path: `/api`

### 下游 MongoDB

- Mongo connection string: `mongodb://et:.netcore@localhost:27017/homehoney?authSource=homehoney`
- Mongo database name: `homehoney`

## 4. 启动顺序

1. 启动 FileBrowser
2. 启动 MongoDB
3. 启动 `HomeHoney.Api`
  - `dotnet run --project HomeHoney.Api/HomeHoney.Api.csproj`
  - 验证 `https://localhost:7080/health`
4. 通过后端管理接口或配置文件确认下游存储配置有效
5. 启动 `HomeHoney` 移动端，并仅配置后端 API 地址

## 4.1 快速自动化验证

建议先运行当前已经稳定的自动化回归，确认后端读写主链路可用：

1. 构建 API：`dotnet build HomeHoney.Api/HomeHoney.Api.csproj`
2. 构建移动端共享逻辑：`dotnet build HomeHoney/HomeHoney.csproj -f net10.0`
3. 运行 API 测试：`dotnet test HomeHoney.Api.Tests/HomeHoney.Api.Tests.csproj -v minimal`
4. 运行移动端单元测试：`dotnet test HomeHoney.Tests/HomeHoney.Tests.csproj -v minimal`

**当前应覆盖的自动化结果**：
- 后端管理配置接口回归
- 保险资料保存、文件上传、详情读取、文件下载链路
- 冰箱贴新增/删除链路
- 偏好读取/更新链路
- 移动端后端优先 API 客户端接线验证

## 5. 核心验收场景

### 场景 A：移动端只连接后端

1. 打开应用“设置”页
2. 确认界面优先保存的是后端 API 地址，并由后端托管 FileBrowser/Mongo 下游连接
3. 进入资料首页、提醒页、冰箱贴页
4. 确认数据可以正常显示

**预期结果**：移动端核心页面可正常使用，且客户端侧不再需要直连下游存储。

### 场景 B：保险文件列表、上传、下载

1. 进入保险列表
2. 读取一条保险记录详情
3. 上传一个新的 PDF 文件
4. 再次查看详情并下载该文件

**预期结果**：
- 文件上传通过后端完成
- 后端同时维护文档元数据与文件状态
- 下载动作由后端返回文件流
- 页面显示业务化状态而不是 FileBrowser 原始错误
- 当前已有自动化集成测试覆盖保险记录保存 → 文件上传 → 详情读取 → 文件下载闭环

### 场景 C：说明书文件状态异常

1. 让某条说明书记录仍存在，但其下游文件在 FileBrowser 中不可访问
2. 打开说明书详情页
3. 尝试下载附件

**预期结果**：
- 页面显示“文件不可用”或等价业务提示
- 不应显示原始下游资源路径或驱动异常堆栈

### 场景 D：非文件业务 CRUD

1. 新增一条冰箱贴
2. 修改该冰箱贴内容
3. 删除该冰箱贴
4. 新增并修改一条备忘录

**预期结果**：所有操作均通过后端接口完成，重新进入页面后结果一致。

补充说明：当前自动化测试已覆盖冰箱贴新增/删除与偏好更新/回读，备忘录、页面级反馈与更多 CRUD 语义仍在继续收敛。

### 场景 E：提醒与搜索聚合

1. 打开提醒中心
2. 确认保单到期、保修截止、冰箱贴、备忘录等提醒均可展示
3. 执行全局搜索
4. 验证保险、说明书、冰箱贴和备忘录结果可在一次搜索体验中统一展示

**预期结果**：提醒和搜索结果由后端统一聚合，移动端不需要自行多次拼装数据。

### 场景 F：下游失败反馈

1. 保持 `HomeHoney.Api` 可用
2. 停掉 FileBrowser 或 MongoDB 其中之一
3. 再次访问对应业务页面

**预期结果**：
- 后端返回明确业务失败状态
- 移动端展示“文件服务不可用”“数据服务不可用”或等价提示
- 不应表现为空白成功页，也不应暴露原始连接串、集合名或底层驱动异常细节

## 6. 调试建议

### 后端调试

- 使用 ASP.NET Core 开发环境日志
- 打开结构化请求日志与异常日志
- 使用健康检查端点确认下游存储连通性
- 通过 OpenAPI 页面验证接口契约

### 移动端调试

- 查看 IDE 调试输出
- 重点确认移动端请求目标已从 FileBrowser/Mongo 切换为 `HomeHoney.Api`
- 核对页面错误反馈是否来自后端统一结果，而不是客户端本地拼装错误

## 7. 回归检查

- 保险列表、详情、编辑仍可用
- 说明书列表、详情、编辑仍可用
- 冰箱贴与备忘录 CRUD 仍可用
- 提醒中心仍可展示跨模块提醒
- 搜索页仍可统一搜索资料与协作内容
- 偏好设置保存后仍可在重启后生效

## 8. 已知门控风险

- 当前 `.specify/memory/constitution.md` 仍写明“离线优先 + 本地 SQLite/本地文件系统”，与当前远端存储与后端代理方案冲突。
- 在进入正式实现前，应先完成章程修订或显式例外记录，否则本特性在治理层面仍处于 `ERROR` 状态。
- 本地如果直接构建完整 `HomeHoney.sln`，Android 目标仍依赖 Java 17+；当前更稳定的验证方式是分别构建 `HomeHoney.Api`、`HomeHoney.Api.Tests`、`HomeHoney.Tests`，以及 `HomeHoney` 的 `net10.0` 目标框架。
