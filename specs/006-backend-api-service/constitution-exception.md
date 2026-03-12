# Constitution Exception Record: 006-backend-api-service

**Date**: 2026-03-11  
**Feature**: [spec.md](spec.md)  
**Related Plan**: [plan.md](plan.md)

## Exception Summary

当前特性允许不遵循 [`.specify/memory/constitution.md`](../../.specify/memory/constitution.md) 中“离线优先 + 本地 SQLite/本地文件系统承担核心业务存储”的第 III 条要求。

## Reason

仓库已在 `005-integrate-storage-services` 中明确采用以下实际边界：

- 文件本体走 FileBrowser
- 结构化业务数据走 MongoDB
- 本地仅保留 Preferences / SecureStorage

`006-backend-api-service` 并未进一步削弱本地存储能力，而是把移动端与下游存储之间再收敛到统一后端 API，以减少客户端集成复杂度并改善安全边界。

## Approved Alternative

在本特性范围内，以下替代原则生效：

- 移动端只连接 `HomeHoney.Api`
- `HomeHoney.Api` 统一连接 FileBrowser 与 MongoDB
- 移动端本地仅保留用户偏好、敏感信息和必要的后端连接配置
- 不重新引入本地 SQLite 或本地业务文件持久化

## Scope

此例外仅适用于 `006-backend-api-service` 及其直接依赖实现。

## Follow-up

后续应通过正式章程修订，把当前实际架构边界同步回治理文件。
