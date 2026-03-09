````chatagent
---
description: 使用计划模板执行实施规划工作流以生成设计文档。
handoffs: 
  - label: Create Tasks
    agent: speckit.tasks
    prompt: Break the plan into tasks
    send: true
  - label: Create Checklist
    agent: speckit.checklist
    prompt: Create a checklist for the following domain...
---

## 用户输入

```text
$ARGUMENTS
```

在继续之前，**必须**考虑用户输入（如果不为空）。

## 概述

1. **设置**：从仓库根目录运行 `.specify/scripts/bash/setup-plan.sh --json`，解析 JSON 获取 FEATURE_SPEC、IMPL_PLAN、SPECS_DIR、BRANCH。对于参数中包含单引号的情况（如 "I'm Groot"），使用转义语法：例如 'I'\''m Groot'（或尽可能使用双引号："I'm Groot"）。

2. **加载上下文**：读取 FEATURE_SPEC 和 `.specify/memory/constitution.md`。加载 IMPL_PLAN 模板（已复制）。

3. **执行计划工作流**：按照 IMPL_PLAN 模板中的结构：
   - 填写技术上下文（将未知项标记为 "NEEDS CLARIFICATION"）
   - 从 constitution 填写 Constitution Check 部分
   - 评估门控条件（如有未经合理说明的违规则报 ERROR）
   - 阶段 0：生成 research.md（解决所有 NEEDS CLARIFICATION）
   - 阶段 1：生成 data-model.md、contracts/、quickstart.md
   - 阶段 1：通过运行代理脚本更新代理上下文
   - 设计完成后重新评估 Constitution Check

4. **停止并报告**：命令在阶段 2 规划后结束。报告分支、IMPL_PLAN 路径和生成的产物。

## 阶段

### 阶段 0：概要与研究

1. **从上方的技术上下文中提取未知项**：
   - 对于每个 NEEDS CLARIFICATION → 研究任务
   - 对于每个依赖项 → 最佳实践任务
   - 对于每个集成项 → 模式任务

2. **生成并分发研究代理**：

   ```text
   For each unknown in Technical Context:
     Task: "Research {unknown} for {feature context}"
   For each technology choice:
     Task: "Find best practices for {tech} in {domain}"
   ```

3. **将研究成果整合**到 `research.md`，使用以下格式：
   - 决策：[选择了什么]
   - 理由：[为什么选择]
   - 考虑过的替代方案：[评估了哪些其他方案]

**输出**：research.md，所有 NEEDS CLARIFICATION 已解决

### 阶段 1：设计与契约

**前提条件：** `research.md` 已完成

1. **从功能规范中提取实体** → `data-model.md`：
   - 实体名称、字段、关系
   - 来自需求的验证规则
   - 适用时的状态转换

2. **定义接口契约**（如果项目有外部接口） → `/contracts/`：
   - 确定项目向用户或其他系统暴露的接口
   - 记录适合项目类型的契约格式
   - 示例：库的公共 API、CLI 工具的命令模式、Web 服务的端点、解析器的语法、应用程序的 UI 契约
   - 如果项目纯粹是内部使用的（构建脚本、一次性工具等），则跳过

3. **代理上下文更新**：
   - 运行 `.specify/scripts/bash/update-agent-context.sh copilot`
   - 这些脚本会检测当前使用的 AI 代理
   - 更新相应的代理专用上下文文件
   - 仅从当前计划中添加新技术
   - 保留标记之间的手动添加内容

**输出**：data-model.md、/contracts/*、quickstart.md、代理专用文件

## 关键规则

- 使用绝对路径
- 门控失败或未解决的澄清项应报 ERROR

````
