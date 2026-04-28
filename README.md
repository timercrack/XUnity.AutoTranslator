# XUnity.AutoTranslator（Ostranauts 定制 fork 说明）

这个仓库是基于上游 `bbepis/XUnity.AutoTranslator` 的一个 **Ostranauts 定制 fork**。

它的维护目标不是继续覆盖上游 README 里的全部通用场景，而是把我当前为了 **翻译 `Ostranauts`** 实际踩过坑、调通过、能直接落地的那一套配置和代码改动整理出来。换句话说，这个 fork 的核心定位是：

- 优先服务 `Ostranauts`
- 优先保留当前已经验证可用的 `BepInEx + AutoTranslator` 工作流
- 优先收录和 `Ostranauts` 文本抓取、字体、日志解析、部署方式有关的定制

如果你需要的是原版 AutoTranslator 的安装方式、完整配置项说明、其他翻译端点、资源重定向、插件开发接口等通用内容，请直接参考上游 README：

- 上游 README：<https://github.com/bbepis/XUnity.AutoTranslator/blob/master/README.md>

## 相对原始 repo 的主要代码改动

### 1. 新增 `LLMTranslate`

- 新增 `LLMTranslate` 端点，并把它作为当前默认翻译端点
- 当前默认接的是 DeepSeek Chat Completions 接口
- Prompt、温度、最大 token、批大小、并发数等运行参数改成代码内置，而不是要求用户在配置里全部展开

### 2. 除 `LLMTranslate` 外，另外做过的定制

- **面向 Ostranauts 重写默认配置**
  - 默认源语言改为 `en`
  - `[General] Language=` 留空时自动跟随系统语言
  - `GameLogTextPaths`、`IgnoreStabilizationPaths`、`EnableTextPathLogging`、`PeriodicManualHookIntervalSeconds`、`TemplateAllNumberAway`、`DisableTextMeshProScrollInEffects` 等默认值都按 Ostranauts 当前实测口径调整

- **改了字体策略，按语言选择字体包**
  - `OverrideFont` / `OverrideFontTextMeshPro` / `FallbackFontTextMeshPro` 支持语言映射
  - 当前默认规则是：**只有简体中文** 使用 `chinesefont`，其他语言默认使用 `arialuni_sdf_u6000`

- **增强了文本抓取与解析逻辑，处理 Ostranauts 里的实际文本形态**
  - 调整了 `GameLogTextParser`，增强对游戏日志文本、重复计数文本、成对尖括号包裹纯文本等情况的处理
  - 调整了 `UGUIHooks`、`TextMeshHooks`、`TextMeshProHooks`、`NGUIHooks`、`ComponentExtensions`、`FontHelper` 等与 UI 文本抓取/回写有关的逻辑
  - 同步补了 `LanguageHelper`、`TextTranslationCache`、`SpamChecker` 等配套逻辑，减少实际游戏里的误判和重复处理

- **补了 `CustomTranslate` 的自定义批量接口能力**
  - 方便继续对接外部自建服务或批量翻译接口

- **裁剪了仓库和解决方案，收敛到当前真正维护的目标**
  - 移除了与当前用途无关的 `Koikatsu` 专用 formatter / resource redirector 工程
  - 移除了 `IPA`、`MelonMod`、`UnityInjector`、`BepInEx-IL2CPP`、`Setup`、`RuntimeHooker`、`TextureHashGenerator`、`XZipper`、测试/benchmark 等当前不再维护的项目或工具链
  - 解决方案现在更偏向当前这条 `BepInEx + Ostranauts` 的实际维护路径

- **构建与部署流程改成了 Ostranauts 优先**
  - 解决方案级构建会把插件和翻译器输出复制到本地 `Ostranauts\BepInEx`
  - 工作区也直接包含游戏目录，便于边改边测

- **README 和默认配置一起改成“fork 差异说明”模式**
  - 不再试图重复上游 README 的所有通用内容
  - 重点只保留这个 fork 相对上游的差异，以及当前对 Ostranauts 已验证的可用配置口径

## 这个 fork 当前额外提供了什么

当前仓库相对上游，主要新增/调整了这些当前可直接使用的能力：

- 内置 `LLMTranslate` 作为默认翻译端点
- 默认源语言为 `en`
- 当 `[General] Language` 留空时，自动使用系统语言作为目标语言
- `LLMTranslate` 的 Prompt 固定内置在代码中，并会按目标语言自动替换翻译目标语言名称
- 启动时会输出语言解析和 `LLMTranslate` 初始化的诊断日志，方便排查配置是否生效
- `OverrideFont` / `OverrideFontTextMeshPro` / `FallbackFontTextMeshPro` 支持按语言映射
- 当前默认配置已内置简中专用 TMP 字体覆盖：
  - 简体中文使用 `chinesefont`
  - 其他语言默认使用 `arialuni_sdf_u6000`
- 当前默认行为参数已同步到 Ostranauts 的实际可用配置口径（不包含 API key）

## 这个 fork 的默认口径

当前默认配置重点如下：

- `[Service] Endpoint=LLMTranslate`
- `[General] Language=`（留空时自动使用系统语言）
- `[General] FromLanguage=en`
- `[Behaviour] OverrideFontTextMeshPro=zh=chinesefont;zh-TW=arialuni_sdf_u6000;default=arialuni_sdf_u6000`
- `[LLMTranslate] Url=https://api.deepseek.com/chat/completions`
- `[LLMTranslate] Model=deepseek-v4-flash`

`LLMTranslate` 其余参数（Prompt、`Temperature=0.2`、`MaxTokens=8192`、`BatchSize=100`、`MaxConcurrency=1`、`EnableShortDelay=False`、`DisableSpamChecks=False`）现在都固定写死在代码里，不再要求写进配置文件。

另外，这个 fork 还把当前 Ostranauts 使用中的这些行为参数做成了默认值：

- `EnableIMGUI=True`
- `EnableTextMesh=True`
- `IgnoreWhitespaceInDialogue=False`
- `GameLogTextPaths` 使用当前已验证的一组路径
- `IgnoreStabilizationPaths=/Canvas Info(Clone)/Offset`
- `EnableTextPathLogging=True`
- `PeriodicManualHookIntervalSeconds=1.0`
- `TemplateAllNumberAway=True`
- `DisableTextMeshProScrollInEffects=True`

## 最小使用方法

只需要关注下面这几段配置即可：

```ini
[Service]
Endpoint=LLMTranslate
FallbackEndpoint=

[General]
Language=
FromLanguage=en

[Behaviour]
OverrideFontTextMeshPro=zh=chinesefont;zh-TW=arialuni_sdf_u6000;default=arialuni_sdf_u6000

[LLMTranslate]
Url=https://api.deepseek.com/chat/completions
ApiKey=
Model=deepseek-v4-flash
```

其中：

- `Language=` 留空表示自动跟随系统语言
- `LLMTranslate` 的 Prompt 和其他运行参数都由代码内置，无需再写到配置文件里

## 如何在中文和俄文之间切换

### 切换到中文

```ini
[General]
Language=zh
```

效果：

- 目标语言为中文
- 内置 Prompt 会自动要求 LLM 输出中文译文
- `OverrideFontTextMeshPro` 自动选中 `chinesefont`
- 翻译缓存和文本文件路径落到 `Translation\zh\...`

### 切换到俄文

```ini
[General]
Language=ru
```

效果：

- 目标语言为俄文
- 内置 Prompt 会自动要求 LLM 输出俄文译文
- `OverrideFontTextMeshPro` 自动落到默认的 `arialuni_sdf_u6000`
- 翻译缓存和文本文件路径落到 `Translation\ru\...`
