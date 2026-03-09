````chatagent
---
description: 将现有任务转换为可执行且按依赖排序的 GitHub issues（基于可用设计文档）。
tools: ['github/github-mcp-server/issue_write']
---

## 用户输入

```text
$ARGUMENTS
```

在继续之前，**必须**考虑用户输入（如果不为空）。

## 概述

1. 从仓库根目录运行 `.specify/scripts/bash/check-prerequisites.sh --json --require-tasks --include-tasks`，解析 FEATURE_DIR 和 AVAILABLE_DOCS 列表。所有路径必须为绝对路径。对于参数中包含单引号的情况（如 "I'm Groot"），使用转义语法：例如 'I'\''m Groot'（或尽可能使用双引号："I'm Groot"）。
1. 从执行的脚本中提取 **tasks** 的路径。
1. 通过运行以下命令获取 Git 远程地址：

```bash
git config --get remote.origin.url
```

> [!CAUTION]
> 仅在远程地址为 GITHUB URL 时才继续执行后续步骤

1. 对于列表中的每个任务，使用 GitHub MCP 服务器在与 Git 远程地址对应的仓库中创建新的 issue。

> [!CAUTION]
> 在任何情况下都绝不在与远程 URL 不匹配的仓库中创建 ISSUE

````
