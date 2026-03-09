````chatagent
---
description: 根据自然语言的功能描述创建或更新功能规范（spec）。
handoffs: 
  - label: Build Technical Plan
    agent: speckit.plan
    prompt: Create a plan for the spec. I am building with...
  - label: Clarify Spec Requirements
    agent: speckit.clarify
    prompt: Clarify specification requirements
    send: true
---

## 用户输入

```text
$ARGUMENTS
```

在继续之前，**必须**考虑用户输入（如果不为空）。

## 概述

用户在触发消息中 `/speckit.specify` 后面输入的文本**即为**功能描述。假设在本次对话中你始终拥有该信息，即使下方字面显示为 `$ARGUMENTS`。除非用户提供了空命令，否则不要要求用户重复输入。

给定该功能描述，执行以下操作：

1. **生成简洁的短名称**（2-4 个单词）用于分支命名：
   - 分析功能描述并提取最有意义的关键词
   - 创建一个 2-4 个单词的短名称来概括功能的核心
   - 尽可能使用"动作-名词"格式（例如 "add-user-auth"、"fix-payment-bug"）
   - 保留技术术语和缩写（OAuth2、API、JWT 等）
   - 保持简洁，但要有足够的描述性以便一目了然地理解功能
   - 示例：
     - "I want to add user authentication" → "user-auth"
     - "Implement OAuth2 integration for the API" → "oauth2-api-integration"
     - "Create a dashboard for analytics" → "analytics-dashboard"
     - "Fix payment processing timeout bug" → "fix-payment-timeout"

2. **创建新分支前先检查是否已存在同名分支**：

   a. 首先，获取所有远程分支以确保拥有最新信息：

      ```bash
      git fetch --all --prune
      ```

   b. 查找该 short-name 在所有来源中的最高功能编号：
      - 远程分支：`git ls-remote --heads origin | grep -E 'refs/heads/[0-9]+-<short-name>$'`
      - 本地分支：`git branch | grep -E '^[* ]*[0-9]+-<short-name>$'`
      - Specs 目录：检查匹配 `specs/[0-9]+-<short-name>` 的目录

   c. 确定下一个可用编号：
      - 从所有三个来源中提取全部编号
      - 找到最大编号 N
      - 使用 N+1 作为新分支编号

   d. 使用计算出的编号和 short-name 运行脚本 `.specify/scripts/bash/create-new-feature.sh --json "$ARGUMENTS"`：
      - 传入 `--number N+1` 和 `--short-name "your-short-name"` 以及功能描述
      - Bash 示例：`.specify/scripts/bash/create-new-feature.sh --json "$ARGUMENTS" --json --number 5 --short-name "user-auth" "Add user authentication"`
      - PowerShell 示例：`.specify/scripts/bash/create-new-feature.sh --json "$ARGUMENTS" -Json -Number 5 -ShortName "user-auth" "Add user authentication"`

   **重要提示**：
   - 检查所有三个来源（远程分支、本地分支、specs 目录）以找到最大编号
   - 仅匹配具有完全相同 short-name 模式的分支/目录
   - 如果未找到具有该 short-name 的现有分支/目录，则从编号 1 开始
   - 每个功能只能运行此脚本一次
   - JSON 作为终端输出提供——始终参考它来获取你需要的实际内容
   - JSON 输出将包含 BRANCH_NAME 和 SPEC_FILE 路径
   - 对于参数中的单引号（如 "I'm Groot"），请使用转义语法：例如 'I'\''m Groot'（或尽可能使用双引号："I'm Groot"）

3. 加载 `.specify/templates/spec-template.md` 以了解所需章节结构。

4. 按以下执行流程操作：

    1. 从输入中解析用户描述
       如果为空：ERROR "No feature description provided"
    2. 从描述中提取关键概念
       识别：角色、操作、数据、约束
    3. 对于不明确的方面：
       - 根据上下文和行业标准做出合理推测
       - 仅在以下情况标记 [NEEDS CLARIFICATION: 具体问题]：
         - 该选择显著影响功能范围或用户体验
         - 存在多种合理的解读且具有不同影响
         - 不存在合理的默认值
       - **限制：最多 3 个 [NEEDS CLARIFICATION] 标记**
       - 按影响程度排列澄清优先级：范围 > 安全/隐私 > 用户体验 > 技术细节
    4. 填写用户场景与测试章节
       如果无法确定明确的用户流程：ERROR "Cannot determine user scenarios"
    5. 生成功能需求
       每个需求必须可测试
       对未指定的细节使用合理默认值（在假设章节中记录假设）
    6. 定义成功标准
       创建可度量的、与技术无关的成果
       包含定量指标（时间、性能、容量）和定性度量（用户满意度、任务完成率）
       每个标准必须可验证且不包含实现细节
    7. 识别关键实体（如涉及数据）
    8. 返回：SUCCESS（规范已准备就绪，可进入规划阶段）

5. 使用模板结构将规范写入 SPEC_FILE，用从功能描述（参数）中推导出的具体细节替换占位符，同时保留章节顺序和标题。

6. **规范质量验证**：编写初始规范后，对照质量标准进行验证：

   a. **创建规范质量检查清单**：使用检查清单模板结构在 `FEATURE_DIR/checklists/requirements.md` 生成检查清单文件，包含以下验证项：

      ```markdown
      # Specification Quality Checklist: [FEATURE NAME]
      
      **Purpose**: Validate specification completeness and quality before proceeding to planning
      **Created**: [DATE]
      **Feature**: [Link to spec.md]
      
      ## Content Quality
      
      - [ ] No implementation details (languages, frameworks, APIs)
      - [ ] Focused on user value and business needs
      - [ ] Written for non-technical stakeholders
      - [ ] All mandatory sections completed
      
      ## Requirement Completeness
      
      - [ ] No [NEEDS CLARIFICATION] markers remain
      - [ ] Requirements are testable and unambiguous
      - [ ] Success criteria are measurable
      - [ ] Success criteria are technology-agnostic (no implementation details)
      - [ ] All acceptance scenarios are defined
      - [ ] Edge cases are identified
      - [ ] Scope is clearly bounded
      - [ ] Dependencies and assumptions identified
      
      ## Feature Readiness
      
      - [ ] All functional requirements have clear acceptance criteria
      - [ ] User scenarios cover primary flows
      - [ ] Feature meets measurable outcomes defined in Success Criteria
      - [ ] No implementation details leak into specification
      
      ## Notes
      
      - Items marked incomplete require spec updates before `/speckit.clarify` or `/speckit.plan`
      ```

   b. **运行验证检查**：将规范与每个检查项进行对照审查：
      - 对于每个检查项，判断通过或未通过
      - 记录发现的具体问题（引用相关规范章节）

   c. **处理验证结果**：

      - **如果所有项均通过**：标记检查清单为完成并进入步骤 6

      - **如果存在未通过项（不含 [NEEDS CLARIFICATION]）**：
        1. 列出未通过的项目和具体问题
        2. 更新规范以解决每个问题
        3. 重新运行验证直到所有项通过（最多 3 次迭代）
        4. 如果 3 次迭代后仍有未通过项，在检查清单备注中记录剩余问题并警告用户

      - **如果存在 [NEEDS CLARIFICATION] 标记**：
        1. 从规范中提取所有 [NEEDS CLARIFICATION: ...] 标记
        2. **数量检查**：如果标记超过 3 个，仅保留最关键的 3 个（按范围/安全/用户体验影响排序），其余部分做出合理推测
        3. 对于每个需要澄清的问题（最多 3 个），按以下格式向用户展示选项：

           ```markdown
           ## Question [N]: [Topic]
           
           **Context**: [Quote relevant spec section]
           
           **What we need to know**: [Specific question from NEEDS CLARIFICATION marker]
           
           **Suggested Answers**:
           
           | Option | Answer | Implications |
           |--------|--------|--------------|
           | A      | [First suggested answer] | [What this means for the feature] |
           | B      | [Second suggested answer] | [What this means for the feature] |
           | C      | [Third suggested answer] | [What this means for the feature] |
           | Custom | Provide your own answer | [Explain how to provide custom input] |
           
           **Your choice**: _[Wait for user response]_
           ```

        4. **关键提示 - 表格格式化**：确保 Markdown 表格格式正确：
           - 使用一致的间距并对齐管道符
           - 每个单元格内容周围应有空格：`| Content |` 而非 `|Content|`
           - 标题分隔行至少使用 3 个破折号：`|--------|`
           - 验证表格在 Markdown 预览中正确渲染
        5. 按顺序编号问题（Q1、Q2、Q3——最多 3 个）
        6. 在等待回复之前一次性展示所有问题
        7. 等待用户回复所有问题的选择（例如 "Q1: A, Q2: Custom - [详情], Q3: B"）
        8. 用用户选择或提供的答案替换规范中的每个 [NEEDS CLARIFICATION] 标记来更新规范
        9. 所有澄清解决后重新运行验证

   d. **更新检查清单**：每次验证迭代后，更新检查清单文件的当前通过/未通过状态

7. 报告完成情况，包括分支名称、规范文件路径、检查清单结果，以及进入下一阶段的就绪状态（`/speckit.clarify` 或 `/speckit.plan`）。

**注意**：脚本会创建并切换到新分支，并在写入之前初始化规范文件。

## 通用准则

## 快速指南

````