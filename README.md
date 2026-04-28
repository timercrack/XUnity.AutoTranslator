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

当前默认值现在主要由代码提供，配置文件默认只保留下面三项：

- `[LLMTranslate] Url=https://api.deepseek.com/chat/completions`
- `[LLMTranslate] ApiKey=`
- `[LLMTranslate] Model=deepseek-v4-flash`

如果你想手动指定目标语言，再额外自己加：

- `[General] Language=ru`（或其他目标语言代码）

其余默认值全部继续由代码提供，例如：

- 默认端点仍然是 `LLMTranslate`
- 默认源语言仍然是 `en`
- 默认字体映射仍然是 `OverrideFontTextMeshPro=zh=chinesefont;zh-TW=arialuni_sdf_u6000;default=arialuni_sdf_u6000`
- `LLMTranslate` 的 Prompt、`Temperature=0.2`、`MaxTokens=8192`、`BatchSize=100`、`MaxConcurrency=1`、`EnableShortDelay=False`、`DisableSpamChecks=False` 仍然固定写死在代码里
- Ostranauts 已验证的一组 `TextFrameworks` / `Behaviour` / `Files` 默认值也仍然固定保留在代码中

缺省项现在不会在启动时自动回填进配置文件，因此配置可以长期保持精简。

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

只需要保留下面这三项配置即可：

```ini
[LLMTranslate]
Url=https://api.deepseek.com/chat/completions
ApiKey=
Model=deepseek-v4-flash
```

如果你想固定目标语言，而不是跟随系统语言，再额外加上：

```ini
[General]
Language=ru
```

其中：

- 不写 `General.Language` 时，会自动跟随系统语言
- `LLMTranslate` 的 Prompt 和其他运行参数都由代码内置，无需再写到配置文件里
- 其他 `Service` / `General.FromLanguage` / `Behaviour` / `Files` / `TextFrameworks` 等项都继续使用代码默认值，不需要展开到配置文件里

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

## 附录：当前代码默认值清单

下面这份清单对应的是当前代码里真正生效的默认值，主要来源于：

- `src/XUnity.AutoTranslator.Plugin.Core/Configuration/Settings.cs`
- `src/Translators/LLMTranslate/LLMTranslateEndpoint.cs`

如果你手动在配置文件里补写其他**受支持**配置项，插件仍然会优先读取你写的值来覆盖这些默认值；只有没写出的项，才继续使用下面这套代码默认值。

### `Service` 与 `General`

```ini
[Service]
Endpoint=LLMTranslate
FallbackEndpoint=

[General]
Language=
FromLanguage=en
```

- `General.Language=` 留空时自动跟随系统语言
- 如果系统语言无法解析，才回退到 `zh`

### `LLMTranslate`

```ini
[LLMTranslate]
Url=https://api.deepseek.com/chat/completions
ApiKey=
Model=deepseek-v4-flash
```

另外这几个 `LLMTranslate` 运行参数固定写死在代码里：

- `Prompt`：代码内置模板，按目标语言自动替换语言名称
- `Temperature=0.2`
- `MaxTokens=8192`
- `BatchSize=100`
- `MaxConcurrency=1`
- `EnableShortDelay=False`
- `DisableSpamChecks=False`

### `TextFrameworks`

```ini
[TextFrameworks]
EnableIMGUI=True
EnableUGUI=True
EnableUIElements=True
EnableNGUI=True
EnableTextMeshPro=True
EnableTextMesh=True
EnableFairyGUI=True
```

### `Behaviour`

```ini
[Behaviour]
MaxCharactersPerTranslation=2500
IgnoreWhitespaceInDialogue=False
MinDialogueChars=20
ForceSplitTextAfterCharacters=0
CopyToClipboard=False
MaxClipboardCopyCharacters=2500
ClipboardDebounceTime=1.25
EnableUIResizing=True
EnableBatching=True
UseStaticTranslations=True
OverrideFont=
OverrideFontSize=
OverrideFontTextMeshPro=zh=chinesefont;zh-TW=arialuni_sdf_u6000;default=arialuni_sdf_u6000
FallbackFontTextMeshPro=
ResizeUILineSpacingScale=
ForceUIResizing=False
IgnoreTextStartingWith=Age:;C:;D:;E:;F:;G:;UTC;Mass:;Condition:
IgnoreTextRegexes=^\s*[xX]\d+\s*$
TextGetterCompatibilityMode=False
WhitelistPaths=
BlacklistPaths=
GameLogTextPaths=/Canvas Stack/Canvas Crew Bar/GUICrewStatus/pnlMessageScroll;/Canvas Stack/Canvas Tooltip Compact/ItemList;/OffscreenDraw/CanvasDockSysDraw;/OffscreenDraw/CanvasATCDraw;/Canvas Stack/Canvas Objectives;/Canvas Stack/Canvas Helmet/bmpHelmet/pnlHUD;/Canvas Stack/Canvas GUI
IgnoreStabilizationPaths=/Canvas Info(Clone)/Offset
RomajiPostProcessing=ReplaceMacronWithCircumflex;RemoveApostrophes;ReplaceHtmlEntities
TranslationPostProcessing=ReplaceMacronWithCircumflex;ReplaceHtmlEntities
RegexPostProcessing=None
CacheRegexPatternResults=False
PersistRichTextMode=Final
CacheRegexLookups=False
CacheWhitespaceDifferences=False
GenerateStaticSubstitutionTranslations=False
GeneratePartialTranslations=False
EnableTranslationScoping=True
EnableSilentMode=True
BlacklistedIMGUIPlugins=
EnableTextPathLogging=True
EnableUIPathInspector=False
TextPathLoggingIgnoredPaths=
PeriodicManualHookIntervalSeconds=1.0
OutputUntranslatableText=False
IgnoreVirtualTextSetterCallingRules=False
MaxTextParserRecursion=1
HtmlEntityPreprocessing=True
HandleRichText=True
EnableTranslationHelper=False
ForceMonoModHooks=False
InitializeHarmonyDetourBridge=False
RedirectedResourceDetectionStrategy=AppendMongolianVowelSeparatorAndRemoveAll
OutputTooLongText=False
TemplateAllNumberAway=True
ReloadTranslationsOnFileChange=False
DisableTextMeshProScrollInEffects=True
CacheParsedTranslations=False
```

### `Files`

```ini
[Files]
Directory=Translation\{Lang}\Text
OutputFile=Translation\{Lang}\Text\_AutoGeneratedTranslations.txt
SubstitutionFile=Translation\{Lang}\Text\_Substitutions.txt
PreprocessorsFile=Translation\{Lang}\Text\_Preprocessors.txt
PostprocessorsFile=Translation\{Lang}\Text\_Postprocessors.txt
```

### `Texture`

```ini
[Texture]
TextureDirectory=Translation\{Lang}\Texture
EnableTextureDumping=False
EnableTextureToggling=False
EnableTextureScanOnSceneLoad=False
EnableSpriteRendererHooking=False
LoadUnmodifiedTextures=False
DetectDuplicateTextureNames=False
DuplicateTextureNames=
EnableLegacyTextureLoading=False
TextureHashGenerationStrategy=FromImageName
CacheTexturesInMemory=True
EnableSpriteHooking=False
```

- `EnableTextureTranslation` 的默认值是动态的：当 `Translation\{Lang}\Texture` 目录存在时为 `True`，否则为 `False`

### `ResourceRedirector`

```ini
[ResourceRedirector]
PreferredStoragePath=Translation\{Lang}\RedirectedResources
EnableTextAssetRedirector=False
LogAllLoadedResources=False
EnableDumping=False
CacheMetadataForAllFiles=True
```

### `Http`、`TranslationAggregator`、`Debug`、`Migrations`

```ini
[Http]
UserAgent=
DisableCertificateValidation=True

[TranslationAggregator]
Width=542
Height=300
EnabledTranslators=

[Debug]
EnableConsole=False

[Migrations]
Enable=True
Tag=<当前插件版本>
```

- `Migrations.Tag` 在运行时会被设置为当前插件版本
- 由于缺省项不会再自动回填，这些 section 默认不会主动出现在配置文件里
