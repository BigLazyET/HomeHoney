---
description: Create or update the project constitution from interactive or provided principle inputs, ensuring all dependent templates stay in sync.
handoffs: 
  - label: Build Specification
    agent: speckit.specify
    prompt: Implement the feature specification based on the updated constitution. I want to build...
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Outline

You are updating the project constitution at `.specify/memory/constitution.md`. This file is a TEMPLATE containing placeholder tokens in square brackets (e.g. `[PROJECT_NAME]`, `[PRINCIPLE_1_NAME]`). Your job is to (a) collect/derive concrete values, (b) fill the template precisely, and (c) propagate any amendments across dependent artifacts.

**Note**: If `.specify/memory/constitution.md` does not exist yet, it should have been initialized from `.specify/templates/constitution-template.md` during project setup. If it's missing, copy the template first.

Follow this execution flow:

1. Load the existing constitution at `.specify/memory/constitution.md`.
   - Identify every placeholder token of the form `[ALL_CAPS_IDENTIFIER]`.
   **IMPORTANT**: The user might require less or more principles than the ones used in the template. If a number is specified, respect that - follow the general template. You will update the doc accordingly.

2. Collect/derive values for placeholders:
   - If user input (conversation) supplies a value, use it.
   - Otherwise infer from existing repo context (README, docs, prior constitution versions if embedded).
   - For governance dates: `RATIFICATION_DATE` is the original adoption date (if unknown ask or mark TODO), `LAST_AMENDED_DATE` is today if changes are made, otherwise keep previous.
   - `CONSTITUTION_VERSION` must increment according to semantic versioning rules:
     - MAJOR: Backward incompatible governance/principle removals or redefinitions.
     - MINOR: New principle/section added or materially expanded guidance.
     - PATCH: Clarifications, wording, typo fixes, non-semantic refinements.
   - If version bump type ambiguous, propose reasoning before finalizing.

3. Draft the updated constitution content:
   - Replace every placeholder with concrete text (no bracketed tokens left except intentionally retained template slots that the project has chosen not to define yet—explicitly justify any left).
   - Preserve heading hierarchy and comments can be removed once replaced unless they still add clarifying guidance.
   - Ensure each Principle section: succinct name line, paragraph (or bullet list) capturing non‑negotiable rules, explicit rationale if not obvious.
   - Ensure Governance section lists amendment procedure, versioning policy, and compliance review expectations.

4. Consistency propagation checklist (convert prior checklist into active validations):
   - Read `.specify/templates/plan-template.md` and ensure any "Constitution Check" or rules align with updated principles.
   - Read `.specify/templates/spec-template.md` for scope/requirements alignment—update if constitution adds/removes mandatory sections or constraints.
   - Read `.specify/templates/tasks-template.md` and ensure task categorization reflects new or removed principle-driven task types (e.g., observability, versioning, testing discipline).
   - Read each command file in `.specify/templates/commands/*.md` (including this one) to verify no outdated references (agent-specific names like CLAUDE only) remain when generic guidance is required.
   - Read any runtime guidance docs (e.g., `README.md`, `docs/quickstart.md`, or agent-specific guidance files if present). Update references to principles changed.

5. Produce a Sync Impact Report (prepend as an HTML comment at top of the constitution file after update):
   - Version change: old → new
   - List of modified principles (old title → new title if renamed)
   - Added sections
   - Removed sections
   - Templates requiring updates (✅ updated / ⚠ pending) with file paths
   - Follow-up TODOs if any placeholders intentionally deferred.

   - Version line matches report.
   - Dates ISO format YYYY-MM-DD.
   - Principles are declarative, testable, and free of vague language ("should" → replace with MUST/SHOULD rationale where appropriate).

7. Write the completed constitution back to `.specify/memory/constitution.md` (overwrite).
````chatagent
---
description: 从交互或提供的原则输入创建或更新项目章程，并确保所有依赖模板保持同步。
handoffs:
  - label: Build Specification
    agent: speckit.specify
    prompt: Implement the feature specification based on the updated constitution. I want to build...
---

## 用户输入

```text
$ARGUMENTS
```

在继续之前，**必须**考虑用户输入（如果不为空）。

## 概述

你正在更新位于 `.specify/memory/constitution.md` 的项目章程。该文件是一个模板，包含方括号内的占位符（例如 `[PROJECT_NAME]`、`[PRINCIPLE_1_NAME]`）。你的工作是：

- (a) 收集/推导具体值，
- (b) 精确填充模板，
- (c) 将任何修改传播到依赖的文档和模板中。

注意：如果 `.specify/memory/constitution.md` 尚不存在，应在项目初始化时从 `.specify/templates/constitution-template.md` 初始化。如果缺失，请先复制模板。

请遵循以下执行流程：

1. 加载现有章程文件 `.specify/memory/constitution.md`。
   - 识别所有形如 `[ALL_CAPS_IDENTIFIER]` 的占位符。
   **重要**：用户可能需要的原则数量与模板中不同。若用户指定了数量，请遵从该数量并相应更新文档。

2. 为占位符收集/推导值：
   - 若用户输入提供了值，则使用它。
   - 否则从现有仓库上下文推断（README、文档、此前的章程版本等）。
   - 对于治理日期：`RATIFICATION_DATE` 为最初采纳日期（若未知请询问或标注 TODO），`LAST_AMENDED_DATE` 若修改则设为今天，否则保留原值。
   - `CONSTITUTION_VERSION` 必须按语义化版本规则递增：
     - MAJOR：不兼容的治理/原则删除或重定义。
     - MINOR：新增原则/章节或实质性扩展指导。
     - PATCH：说明性修改、措辞与拼写修正等。
   - 若版本提升类型不明确，应在最终确定前提出理由建议。

3. 起草更新后的章程内容：
   - 用具体文本替换所有占位符（除非项目有意保留某些模板槽位——对任何保留的占位符需给出明确理由）。
   - 保持标题层级不变；可以移除注释，除非注释仍有说明作用。
   - 确保每个原则部分包含：简短的名称行、明确的规则段落或要点，以及必要时的理由说明。
   - 确保治理部分列出修订流程、版本策略和合规检查期望。

4. 一致性传播检查清单（将先前的检查表转为活动校验项）：
   - 检阅 `.specify/templates/plan-template.md`，确保任何“章程检查”或规则与更新的原则保持一致。
   - 检阅 `.specify/templates/spec-template.md`，若章程新增/删除了强制性章节或约束，则更新此模板以反映变更。
   - 检阅 `.specify/templates/tasks-template.md`，确保任务分类反映出新增或删除的原则驱动任务类型（例如可观察性、版本控制、测试纪律）。
   - 检阅 `.specify/templates/commands/*.md`（包括本文件）以确认没有过时引用（例如仅针对特定 agent 的名称），需要时改为通用说明。
   - 检阅运行时指导性文档（如 `README.md`、`docs/quickstart.md`、或 agent 特定的指导文件），更新任何受影响的引用。

5. 生成同步影响报告（将其作为 HTML 注释插入到更新后的章程文件顶部）：
   - 版本变更：旧 → 新
   - 修改的原则清单（若重命名则列出旧名 → 新名）
   - 新增章节
   - 删除章节
   - 需要更新的模板文件清单（✅ 已更新 / ⚠ 待处理），并列出路径
   - 后续 TODO 列表（若有占位符有意保留，需在报告中标注）

6. 提交最终输出前的校验：
   - 不应保留未解释的方括号占位符。
   - 版本行应与报告一致。
   - 日期为 ISO 格式 YYYY-MM-DD。
   - 原则应以声明式、可测试的语言书写，避免含糊术语（必要时用 MUST/SHOULD 并附理由）。

7. 将完成的章程覆盖写回 `.specify/memory/constitution.md`。

8. 向用户输出最终摘要，包含：
   - 新版本与版本提升理由
   - 任何需人工跟进的文件
   - 建议的提交信息示例，例如：`docs: amend constitution to vX.Y.Z (principle additions + governance update)`

格式与风格要求：

- 保持模板原有的 Markdown 标题层级。
- 长段落尽量控制行长（推荐 <100 字符），但无需刻意换行导致断句不自然。
- 各节之间保留单空行。
- 避免行尾多余空格。

若用户只提供局部修改（例如仅修改一条原则），仍需执行版本判断与校验步骤。

若关键信息缺失（例如采纳日期确实未知），请插入 `TODO(<FIELD_NAME>): explanation` 并在同步影响报告的延期项中列出。

切勿创建新模板；始终在现有 `.specify/memory/constitution.md` 文件上操作。
````
