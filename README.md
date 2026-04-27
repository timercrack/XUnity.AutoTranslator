# XUnity.AutoTranslator（LLMTranslate fork 说明）

这个仓库现在只额外说明 **相对上游原版 AutoTranslator 的改动部分**，重点是内置 `LLMTranslate` 的默认配置、多目标语言预设，以及 Ostranauts 当前实战可用的配置口径。

原版 AutoTranslator 的安装方式、完整配置项说明、其他翻译端点、资源重定向、插件开发接口等通用内容，请直接参考上游 README：

- 上游 README：<https://github.com/bbepis/XUnity.AutoTranslator/blob/master/README.md>

## 这个 fork 额外改了什么

当前仓库相对上游，主要新增/调整了这些和 `LLMTranslate` 有关的能力：

- 内置 `LLMTranslate` 作为默认翻译端点
- 默认目标语言改为 `zh`，默认源语言改为 `en`
- `LLMTranslate` 支持按目标语言自动切换 Prompt
- 支持语言专用 Prompt 键：
  - `SystemPrompt_zh`
  - `SystemPrompt_ru`
- `SystemPrompt` 支持占位符：
  - `{SourceLanguageCode}`
  - `{SourceLanguageName}`
  - `{DestinationLanguageCode}`
  - `{DestinationLanguageName}`
- `OverrideFont` / `OverrideFontTextMeshPro` / `FallbackFontTextMeshPro` 支持按语言映射
- 当前默认配置已内置中俄双预设：
  - 中文使用 `chinesefont`
  - 俄文使用 `arialuni_sdf_u6000`
- 当前默认行为参数已同步到 Ostranauts 的实际可用配置口径（不包含 API key）

## 这个 fork 的默认口径

当前默认配置重点如下：

- `[Service] Endpoint=LLMTranslate`
- `[General] Language=zh`
- `[General] FromLanguage=en`
- `[Behaviour] OverrideFontTextMeshPro=zh=chinesefont;ru=arialuni_sdf_u6000`
- `[LLMTranslate] Url=https://api.deepseek.com/chat/completions`
- `[LLMTranslate] Model=deepseek-v4-flash`
- `[LLMTranslate] Temperature=0.2`
- `[LLMTranslate] MaxTokens=8192`
- `[LLMTranslate] BatchSize=100`
- `[LLMTranslate] MaxConcurrency=1`

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
Language=zh
FromLanguage=en

[Behaviour]
OverrideFontTextMeshPro=zh=chinesefont;ru=arialuni_sdf_u6000

[LLMTranslate]
Url=https://api.deepseek.com/chat/completions
ApiKey=
Model=deepseek-v4-flash
SystemPrompt=You are a dedicated translator for game UI text. The input is a JSON array of strings. Translate each array element into {DestinationLanguageName}. Return exactly one valid JSON array with the same number of elements and the same order as the input. Output only the JSON array. Do not output markdown, code fences, comments, explanations, or any extra text. Preserve escape sequences such as \n, \r, and \t exactly as they appear. Preserve placeholders and markup such as <...> exactly. Keep personal names translated or transliterated consistently into {DestinationLanguageName}, unless the text is a hotkey, single-letter label, ID, serial number, or obvious technical identifier.
SystemPrompt_zh=你是Ostranauts（星际漂流者）的专用翻译器。输入是一个JSON数组，数组中的每个元素都是一个待翻译字符串。你的任务是返回一个JSON数组，元素数量、顺序必须与输入完全一致，每个元素都是对应的简体中文译文字符串。除 JSON 数组外禁止输出任何其他内容。
SystemPrompt_ru=你是Ostranauts（星际漂流者）的专用翻译器。输入是一个JSON数组，数组中的每个元素都是一个待翻译字符串。你的任务是返回一个JSON数组，元素数量、顺序必须与输入完全一致，每个元素都是对应的俄文译文字符串。除 JSON 数组外禁止输出任何其他内容。
Temperature=0.2
MaxTokens=8192
BatchSize=100
MaxConcurrency=1
EnableShortDelay=False
DisableSpamChecks=False
```

## 如何在中文和俄文之间切换

### 切换到中文

```ini
[General]
Language=zh
```

效果：

- 目标语言为中文
- 使用 `SystemPrompt_zh`
- `OverrideFontTextMeshPro` 自动选中 `chinesefont`
- 翻译缓存和文本文件路径落到 `Translation\zh\...`

### 切换到俄文

```ini
[General]
Language=ru
```

效果：

- 目标语言为俄文
- 使用 `SystemPrompt_ru`
- `OverrideFontTextMeshPro` 自动选中 `arialuni_sdf_u6000`
- 翻译缓存和文本文件路径落到 `Translation\ru\...`
