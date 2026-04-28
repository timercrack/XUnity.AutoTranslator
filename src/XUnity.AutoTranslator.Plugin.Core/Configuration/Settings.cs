using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Debugging;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Parsing;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Configuration
{
   internal static class Settings
   {
      // cannot be changed
      public static string TextMeshProVersion = null;
      public static readonly int MaxFailuresForSameTextPerEndpoint = 3;
      public static readonly string TranslatorsFolder = "Translators";
      public static readonly int MaxMaxCharactersPerTranslation = 2500;
      public static readonly string DefaultLanguage = "zh";
      public static readonly string DefaultFromLanguage = "en";
      public static readonly string EnglishLanguage = "en";
      public static readonly string Romaji = "romaji";
      public static readonly int MaxErrors = 5;
      public static readonly int MaxTranslationsBeforeShutdown = 8000;
      public static readonly int MaxUnstartedJobs = 4000;
      public static readonly float IncreaseBatchOperationsEvery = 30;
      public static readonly int MaximumStaggers = 6;
      public static readonly int PreviousTextStaggerCount = 3;
      public static readonly int MaximumConsecutiveFramesTranslated = 90;
      public static readonly int MaximumConsecutiveSecondsTranslated = 60;
      public static bool FromLanguageUsesWhitespaceBetweenWords = false;
      public static bool ToLanguageUsesWhitespaceBetweenWords = false;
      public static string ApplicationName;
      public static float Timeout = 150.0f;
      public static string RedirectedResourcesPath;
      public static readonly int MaxImguiKeyCacheCount = 10000;
      public static readonly float DefaultTranslationDelay = 0.9f;
      public static readonly int DefaultMaxRetries = 67;

      public static Dictionary<string, string> Replacements = new Dictionary<string, string>();
      public static Dictionary<string, string> Preprocessors = new Dictionary<string, string>();
      public static Dictionary<string, string> Postprocessors = new Dictionary<string, string>();
      //public static List<RegexTranslationSplitter> Patterns = new List<RegexTranslationSplitter>();

      public static bool SimulateError = false;
      public static bool SimulateDelayedError = false;

      public static bool InvokeEvents = true;
      public static Action<object> RemakeTextData = null;
      public static Action<object> SetCurText = null;

      public static bool IsShutdown = false;
      public static int TranslationCount = 0;
      public static int MaxAvailableBatchOperations = 50;

      public static readonly float MaxTranslationsQueuedPerSecond = 5;
      public static readonly int MaxSecondsAboveTranslationThreshold = 30;
      public static readonly int TranslationQueueWatchWindow = 6;

      // can be changed
      public static string ServiceEndpoint;
      public static string FallbackServiceEndpoint;
      public static string Language;
      public static string FromLanguage;
      public static string ConfiguredLanguage;
      public static string DetectedSystemLanguageCode;
      public static string DetectedSystemLanguageContext;
      public static bool UsingSystemLanguageAsDefault;
      public static string OutputFile;
      public static string SubstitutionFile;
      public static string PreprocessorsFile;
      public static string PostprocessorsFile;
      public static string TranslationDirectory;
      public static int MaxCharactersPerTranslation;
      public static bool EnableConsole;
      public static string AutoTranslationsFilePath;
      public static string SubstitutionFilePath;
      public static string PreprocessorsFilePath;
      public static string PostprocessorsFilePath;
      public static string TranslationsPath;
      public static string TexturesPath;
      public static string TranslatorsPath;
      public static bool EnableIMGUI;
      public static bool EnableUGUI;
      public static bool EnableUIElements;
      public static bool EnableNGUI;
      public static bool EnableTextMeshPro;
      public static bool EnableTextMesh;
      public static bool EnableFairyGUI;
      public static bool InitializeHarmonyDetourBridge;
      public static bool IgnoreWhitespaceInDialogue;
      public static int MinDialogueChars;
      public static int ForceSplitTextAfterCharacters;
      public static bool EnableMigrations;
      public static string MigrationsTag;
      public static bool EnableBatching;
      public static bool EnableUIResizing;
      public static bool UseStaticTranslations;
      public static string OverrideFont;
      public static int? OverrideFontSize;
      public static string OverrideFontTextMeshPro;
      public static string FallbackFontTextMeshPro;
      public static string UserAgent;
      public static bool DisableCertificateValidation;
      public static float? ResizeUILineSpacingScale;
      public static bool ForceUIResizing;
      public static string[] IgnoreTextStartingWith;
      public static List<Regex> IgnoreTextRegexes;
      public static HashSet<string> GameLogTextPaths;
      public static HashSet<string> IgnoreStabilizationPaths;
      public static HashSet<string> BlacklistPaths;
      public static HashSet<string> WhitelistPaths;
      public static bool TextGetterCompatibilityMode;
      public static TextPostProcessing RomajiPostProcessing;
      public static TextPostProcessing TranslationPostProcessing;
      public static TextPostProcessing RegexPostProcessing;
      public static bool ForceMonoModHooks;
      public static bool CacheRegexPatternResults;
      public static bool CacheRegexLookups;
      public static bool CacheWhitespaceDifferences;
      public static bool GenerateStaticSubstitutionTranslations;
      public static bool GeneratePartialTranslations;
      public static bool EnableTranslationScoping;
      public static bool EnableSilentMode;
      public static HashSet<string> BlacklistedIMGUIPlugins;
      public static bool EnableTextPathLogging;
      public static bool EnableUIPathInspector;
      public static HashSet<string> TextPathLoggingIgnoredPaths;
      public static float PeriodicManualHookIntervalSeconds;
      public static bool OutputUntranslatableText;
      public static bool IgnoreVirtualTextSetterCallingRules;
      public static int MaxTextParserRecursion;
      public static bool HtmlEntityPreprocessing;
      public static bool HandleRichText;
      public static PersistRichTextMode PersistRichTextMode;
      public static bool EnableTranslationHelper;
      public static RedirectedResourceDetection RedirectedResourceDetectionStrategy;
      public static bool OutputTooLongText;
      public static bool TemplateAllNumberAway;
      public static bool ReloadTranslationsOnFileChange;
      public static bool DisableTextMeshProScrollInEffects;
      public static bool CacheParsedTranslations;

      public static string TextureDirectory;
      public static bool EnableTextureTranslation;
      public static bool EnableTextureDumping;
      public static bool EnableTextureToggling;
      public static bool EnableTextureScanOnSceneLoad;
      public static bool EnableSpriteRendererHooking;
      public static bool LoadUnmodifiedTextures;
      public static bool DetectDuplicateTextureNames;
      public static bool EnableLegacyTextureLoading;
      public static HashSet<string> DuplicateTextureNames;
      public static TextureHashGenerationStrategy TextureHashGenerationStrategy;
      public static bool CacheTexturesInMemory;
      public static bool EnableSpriteHooking;

      public static string PreferredStoragePath;
      public static bool EnableDumping;
      public static bool EnableTextAssetRedirector;
      public static bool LogAllLoadedResources;
      public static bool CacheMetadataForAllFiles;


      public static float Height;
      public static float Width;
      public static HashSet<string> EnabledTranslators;

      public static bool CopyToClipboard;
      public static int MaxClipboardCopyCharacters;
      public static float ClipboardDebounceTime;

      public static void Configure()
      {
         try
         {
            var fi = new FileInfo( typeof( Settings ).Assembly.Location );
            var di = fi.Directory;
            TranslatorsPath = Path.Combine( di.FullName, TranslatorsFolder );

            try
            {
               try
               {
                  ApplicationName = Path.GetFileNameWithoutExtension( ApplicationInformation.StartupPath );
               }
               catch( Exception )
               {
                  ApplicationName = Path.GetFileNameWithoutExtension( Process.GetCurrentProcess().MainModule.FileName );
               }
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Warn( e, "An error occurred while obtaining the path to the executable. {GameExeName} variable in configuration files will not work correctly." );

               ApplicationName = string.Empty;
            }

            try
            {
               XuaLogger.AutoTranslator.Debug( "Screen resolution determine to be: " + Screen.width + "x" + Screen.height );
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Warn( e, "An error occurred while trying to determine the game resolution." );
            }

            try
            {
               var versionProperty = UnityTypes.TMP_Settings_Properties.Version;
               if( versionProperty != null )
               {
                  TextMeshProVersion = (string)versionProperty.Get( null );
                  XuaLogger.AutoTranslator.Info( $"Version of TextMesh Pro: {TextMeshProVersion}." );
               }
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Warn( e, "An error occurred while trying to determine TextMesh Pro version." );
            }

            ServiceEndpoint = PluginEnvironment.Current.Preferences.GetOrDefault( "Service", "Endpoint", KnownTranslateEndpointNames.LLMTranslate );
            FallbackServiceEndpoint = PluginEnvironment.Current.Preferences.GetOrDefault( "Service", "FallbackEndpoint", string.Empty );

            var configuredLanguage = PluginEnvironment.Current.Preferences.GetOrDefault( "General", "Language", string.Empty );
            DetectSystemLanguage( out var detectedSystemLanguageCode, out var detectedSystemLanguageContext );
            ConfiguredLanguage = NormalizeConfiguredLanguageCode( configuredLanguage );
            DetectedSystemLanguageCode = detectedSystemLanguageCode ?? string.Empty;
            DetectedSystemLanguageContext = detectedSystemLanguageContext ?? string.Empty;
            UsingSystemLanguageAsDefault = string.IsNullOrEmpty( ConfiguredLanguage );
            Language = string.Intern( ResolveConfiguredLanguageOrSystemLanguage( ConfiguredLanguage, DetectedSystemLanguageCode ) );
            FromLanguage = string.Intern( PluginEnvironment.Current.Preferences.GetOrDefault( "General", "FromLanguage", DefaultFromLanguage ) );
            LogLanguageConfiguration();

            TranslationDirectory = PluginEnvironment.Current.Preferences.GetOrDefault( "Files", "Directory", Path.Combine( "Translation", Path.Combine( "{Lang}", "Text" ) ) );
            OutputFile = PluginEnvironment.Current.Preferences.GetOrDefault( "Files", "OutputFile", Path.Combine( "Translation", Path.Combine( "{Lang}", Path.Combine( "Text", "_AutoGeneratedTranslations.txt" ) ) ) );
            SubstitutionFile = PluginEnvironment.Current.Preferences.GetOrDefault( "Files", "SubstitutionFile", Path.Combine( "Translation", Path.Combine( "{Lang}", Path.Combine( "Text", "_Substitutions.txt" ) ) ) );
            PreprocessorsFile = PluginEnvironment.Current.Preferences.GetOrDefault( "Files", "PreprocessorsFile", Path.Combine( "Translation", Path.Combine( "{Lang}", Path.Combine( "Text", "_Preprocessors.txt" ) ) ) );
            PostprocessorsFile = PluginEnvironment.Current.Preferences.GetOrDefault( "Files", "PostprocessorsFile", Path.Combine( "Translation", Path.Combine( "{Lang}", Path.Combine( "Text", "_Postprocessors.txt" ) ) ) );

            EnableIMGUI = PluginEnvironment.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableIMGUI", true );
            EnableUGUI = PluginEnvironment.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableUGUI", true );
            EnableUIElements = PluginEnvironment.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableUIElements", true );
            EnableNGUI = PluginEnvironment.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableNGUI", true );
            EnableTextMeshPro = PluginEnvironment.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableTextMeshPro", true );
            EnableTextMesh = PluginEnvironment.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableTextMesh", true );
            EnableFairyGUI = PluginEnvironment.Current.Preferences.GetOrDefault( "TextFrameworks", "EnableFairyGUI", true );

            MaxCharactersPerTranslation = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "MaxCharactersPerTranslation", 2500 );
            IgnoreWhitespaceInDialogue = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "IgnoreWhitespaceInDialogue", false );
            MinDialogueChars = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "MinDialogueChars", 20 );
            ForceSplitTextAfterCharacters = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "ForceSplitTextAfterCharacters", 0 );
            CopyToClipboard = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "CopyToClipboard", false );
            MaxClipboardCopyCharacters = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "MaxClipboardCopyCharacters", 2500 );
            ClipboardDebounceTime = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "ClipboardDebounceTime", 1.25f );
            EnableUIResizing = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "EnableUIResizing", true );
            EnableBatching = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "EnableBatching", true );
            UseStaticTranslations = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "UseStaticTranslations", true );
            OverrideFont = ResolveLanguageSpecificSetting( "OverrideFont", PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "OverrideFont", string.Empty ) );
            OverrideFontSize = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "OverrideFontSize", (int?)null );
            OverrideFontTextMeshPro = ResolveLanguageSpecificSetting( "OverrideFontTextMeshPro", PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "OverrideFontTextMeshPro", "zh=chinesefont;zh-TW=arialuni_sdf_u6000;default=arialuni_sdf_u6000" ) );
            FallbackFontTextMeshPro = ResolveLanguageSpecificSetting( "FallbackFontTextMeshPro", PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "FallbackFontTextMeshPro", string.Empty ) );
            ResizeUILineSpacingScale = PluginEnvironment.Current.Preferences.GetOrDefault<float?>( "Behaviour", "ResizeUILineSpacingScale", null );
            ForceUIResizing = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "ForceUIResizing", false );
            IgnoreTextStartingWith = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "IgnoreTextStartingWith", "Age:;C:;D:;E:;F:;G:;UTC;Mass:;Condition:" )
               ?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).Select( x => JsonHelper.Unescape( x ) ).ToArray() ?? new string[ 0 ];
            IgnoreTextRegexes = ParseRegexCollection( PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "IgnoreTextRegexes", @"^\s*[xX]\d+\s*$" ), "IgnoreTextRegexes" );
            TextGetterCompatibilityMode = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "TextGetterCompatibilityMode", false );
            GameLogTextPaths = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "GameLogTextPaths", "/Canvas Stack/Canvas Crew Bar/GUICrewStatus/pnlMessageScroll;/Canvas Stack/Canvas Tooltip Compact/ItemList;/OffscreenDraw/CanvasDockSysDraw;/OffscreenDraw/CanvasATCDraw;/Canvas Stack/Canvas Objectives;/Canvas Stack/Canvas Helmet/bmpHelmet/pnlHUD;/Canvas Stack/Canvas GUI" )
               ?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).ToHashSet() ?? new HashSet<string>();
            GameLogTextPaths.RemoveWhere( x => !x.StartsWith( "/" ) ); // clean up to ensure no 'empty' entries
            IgnoreStabilizationPaths = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "IgnoreStabilizationPaths", "/Canvas Info(Clone)/Offset" )
               ?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).ToHashSet() ?? new HashSet<string>();
            IgnoreStabilizationPaths.RemoveWhere( x => !x.StartsWith( "/" ) );
            BlacklistPaths = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "BlacklistPaths", string.Empty )
               ?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).ToHashSet() ?? new HashSet<string>();
            BlacklistPaths.RemoveWhere( x => !x.StartsWith( "/" ) );
            WhitelistPaths = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "WhitelistPaths", string.Empty )
               ?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).ToHashSet() ?? new HashSet<string>();
            WhitelistPaths.RemoveWhere( x => !x.StartsWith( "/" ) );
            RomajiPostProcessing = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "RomajiPostProcessing", TextPostProcessing.ReplaceMacronWithCircumflex | TextPostProcessing.RemoveApostrophes | TextPostProcessing.ReplaceHtmlEntities );
            TranslationPostProcessing = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "TranslationPostProcessing", TextPostProcessing.ReplaceMacronWithCircumflex | TextPostProcessing.ReplaceHtmlEntities );
            RegexPostProcessing = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "RegexPostProcessing", TextPostProcessing.None );
            CacheRegexPatternResults = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "CacheRegexPatternResults", false );
            PersistRichTextMode = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "PersistRichTextMode", PersistRichTextMode.Final );
            CacheRegexLookups = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "CacheRegexLookups", false );
            CacheWhitespaceDifferences = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "CacheWhitespaceDifferences", false );
            GenerateStaticSubstitutionTranslations = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "GenerateStaticSubstitutionTranslations", false );
            GeneratePartialTranslations = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "GeneratePartialTranslations", false );
            EnableTranslationScoping = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "EnableTranslationScoping", true );
            EnableSilentMode = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "EnableSilentMode", true );
            BlacklistedIMGUIPlugins = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "BlacklistedIMGUIPlugins", string.Empty )
               ?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries )
               .Select( x => x.Trim() )
               .Where( x => !string.IsNullOrEmpty( x ) )
               .ToHashSet( StringComparer.OrdinalIgnoreCase ) ?? new HashSet<string>( StringComparer.OrdinalIgnoreCase );
            EnableTextPathLogging = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "EnableTextPathLogging", true );
            EnableUIPathInspector = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "EnableUIPathInspector", false );
            TextPathLoggingIgnoredPaths = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "TextPathLoggingIgnoredPaths", string.Empty )
               ?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).ToHashSet() ?? new HashSet<string>();
            TextPathLoggingIgnoredPaths.RemoveWhere( x => !x.StartsWith( "/" ) );
            PeriodicManualHookIntervalSeconds = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "PeriodicManualHookIntervalSeconds", 1.0f );
            OutputUntranslatableText = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "OutputUntranslatableText", false );
            IgnoreVirtualTextSetterCallingRules = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "IgnoreVirtualTextSetterCallingRules", false );
            MaxTextParserRecursion = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "MaxTextParserRecursion", 1 );
            HtmlEntityPreprocessing = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "HtmlEntityPreprocessing", true );
            HandleRichText = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "HandleRichText", true );
            EnableTranslationHelper = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "EnableTranslationHelper", false );
            ForceMonoModHooks = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "ForceMonoModHooks", false );
            InitializeHarmonyDetourBridge = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "InitializeHarmonyDetourBridge", !ClrFeatures.SupportsReflectionEmit && PluginEnvironment.Current.AllowDefaultInitializeHarmonyDetourBridge );
            RedirectedResourceDetectionStrategy = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "RedirectedResourceDetectionStrategy", RedirectedResourceDetection.AppendMongolianVowelSeparatorAndRemoveAll );
            OutputTooLongText = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "OutputTooLongText", false );
            TemplateAllNumberAway = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "TemplateAllNumberAway", true );
            ReloadTranslationsOnFileChange = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "ReloadTranslationsOnFileChange", false );
            DisableTextMeshProScrollInEffects = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "DisableTextMeshProScrollInEffects", true );
            CacheParsedTranslations = PluginEnvironment.Current.Preferences.GetOrDefault( "Behaviour", "CacheParsedTranslations", false );

            TextureDirectory = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "TextureDirectory", Path.Combine( "Translation", Path.Combine( "{Lang}", "Texture" ) ) );
            TexturesPath = Path.Combine( PluginEnvironment.Current.TranslationPath, Settings.TextureDirectory ).Parameterize();

            EnableTextureTranslation = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "EnableTextureTranslation", Directory.Exists( TexturesPath ) );
            EnableTextureDumping = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "EnableTextureDumping", false );
            EnableTextureToggling = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "EnableTextureToggling", false );
            EnableTextureScanOnSceneLoad = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "EnableTextureScanOnSceneLoad", false );
            EnableSpriteRendererHooking = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "EnableSpriteRendererHooking", false );
            LoadUnmodifiedTextures = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "LoadUnmodifiedTextures", false );
            DetectDuplicateTextureNames = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "DetectDuplicateTextureNames", false );
            DuplicateTextureNames = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "DuplicateTextureNames", string.Empty )
               ?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).ToHashSet() ?? new HashSet<string>();
            EnableLegacyTextureLoading = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "EnableLegacyTextureLoading", false );
            TextureHashGenerationStrategy = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "TextureHashGenerationStrategy", TextureHashGenerationStrategy.FromImageName );
            CacheTexturesInMemory = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "CacheTexturesInMemory", true );
            EnableSpriteHooking = PluginEnvironment.Current.Preferences.GetOrDefault( "Texture", "EnableSpriteHooking", false );

            PreferredStoragePath = PluginEnvironment.Current.Preferences.GetOrDefault( "ResourceRedirector", "PreferredStoragePath", Path.Combine( "Translation", Path.Combine( "{Lang}", "RedirectedResources" ) ) );
            EnableTextAssetRedirector = PluginEnvironment.Current.Preferences.GetOrDefault( "ResourceRedirector", "EnableTextAssetRedirector", false );
            LogAllLoadedResources = PluginEnvironment.Current.Preferences.GetOrDefault( "ResourceRedirector", "LogAllLoadedResources", false );
            EnableDumping = PluginEnvironment.Current.Preferences.GetOrDefault( "ResourceRedirector", "EnableDumping", false );
            CacheMetadataForAllFiles = PluginEnvironment.Current.Preferences.GetOrDefault( "ResourceRedirector", "CacheMetadataForAllFiles", true );

            if( ClipboardDebounceTime < 0.1f )
            {
               XuaLogger.AutoTranslator.Warn( "'ClipboardDebounceTime' must not be lower than 0.1. Setting it to that..." );

               ClipboardDebounceTime = 0.1f;

            }

            if( PeriodicManualHookIntervalSeconds < 0f )
            {
               XuaLogger.AutoTranslator.Warn( "'PeriodicManualHookIntervalSeconds' must not be negative. Disabling periodic manual hook..." );

               PeriodicManualHookIntervalSeconds = 0f;
            }
            else if( PeriodicManualHookIntervalSeconds > 0f && PeriodicManualHookIntervalSeconds < 0.1f )
            {
               XuaLogger.AutoTranslator.Warn( "'PeriodicManualHookIntervalSeconds' must not be lower than 0.1 when enabled. Setting it to that..." );

               PeriodicManualHookIntervalSeconds = 0.1f;
            }

            if( CacheMetadataForAllFiles && EnableDumping )
            {
               XuaLogger.AutoTranslator.Warn( "'EnableDumping' and 'CacheMetadataForAllFiles' cannot be enabled at the same time. Disabling 'CacheMetadataForAllFiles'..." );

               CacheMetadataForAllFiles = false;
            }

            // FIXME: UseCorrectDirectorySeparators() is called on entire path because redirected resource path lookups has the same changes (requires v. 5.x.x)
            RedirectedResourcesPath = Path.Combine( PluginEnvironment.Current.TranslationPath, PreferredStoragePath ).UseCorrectDirectorySeparators().Parameterize();

            if( MaxCharactersPerTranslation > MaxMaxCharactersPerTranslation )
            {
               PluginEnvironment.Current.Preferences[ "Behaviour" ][ "MaxCharactersPerTranslation" ].Value = MaxMaxCharactersPerTranslation.ToString( CultureInfo.InvariantCulture );
               MaxCharactersPerTranslation = MaxMaxCharactersPerTranslation;
            }

            UserAgent = PluginEnvironment.Current.Preferences.GetOrDefault( "Http", "UserAgent", string.Empty );
            DisableCertificateValidation = PluginEnvironment.Current.Preferences.GetOrDefault( "Http", "DisableCertificateValidation", true );


            Width = PluginEnvironment.Current.Preferences.GetOrDefault( "TranslationAggregator", "Width", 542.0f );
            Height = PluginEnvironment.Current.Preferences.GetOrDefault( "TranslationAggregator", "Height", 300.0f );
            EnabledTranslators = PluginEnvironment.Current.Preferences.GetOrDefault( "TranslationAggregator", "EnabledTranslators", string.Empty )
               ?.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries ).ToHashSet() ?? new HashSet<string>();


            EnableConsole = PluginEnvironment.Current.Preferences.GetOrDefault( "Debug", "EnableConsole", false );

            EnableMigrations = PluginEnvironment.Current.Preferences.GetOrDefault( "Migrations", "Enable", true );
            MigrationsTag = PluginEnvironment.Current.Preferences.GetOrDefault( "Migrations", "Tag", string.Empty );

            AutoTranslationsFilePath = Path.Combine( PluginEnvironment.Current.TranslationPath, OutputFile.UseCorrectDirectorySeparators() ).Parameterize();
            SubstitutionFilePath = Path.Combine( PluginEnvironment.Current.TranslationPath, SubstitutionFile.UseCorrectDirectorySeparators() ).Parameterize();
            PreprocessorsFilePath = Path.Combine( PluginEnvironment.Current.TranslationPath, PreprocessorsFile.UseCorrectDirectorySeparators() ).Parameterize();
            PostprocessorsFilePath = Path.Combine( PluginEnvironment.Current.TranslationPath, PostprocessorsFile.UseCorrectDirectorySeparators() ).Parameterize();

            TranslationsPath = Path.Combine( PluginEnvironment.Current.TranslationPath, Settings.TranslationDirectory.UseCorrectDirectorySeparators() ).Parameterize();

            FromLanguageUsesWhitespaceBetweenWords = LanguageHelper.RequiresWhitespaceUponLineMerging( FromLanguage );
            ToLanguageUsesWhitespaceBetweenWords = LanguageHelper.RequiresWhitespaceUponLineMerging( Language );

            if( EnableTranslationScoping && !UnityFeatures.SupportsSceneManager )
            {
               EnableTranslationScoping = false;

               XuaLogger.AutoTranslator.Warn( "Disabling translation scoping because the SceneManager API is not supported in this version of Unity." );
            }

            if( EnableMigrations )
            {
               Migrate();
            }

            MigrationsTag = PluginData.Version;

            if( PluginEnvironment.Current.Preferences.HasSection( "Migrations" ) )
            {
               var migrationsSection = PluginEnvironment.Current.Preferences.GetSection( "Migrations" );
               if( migrationsSection != null && migrationsSection.HasKey( "Tag" ) )
               {
                  migrationsSection.GetKey( "Tag" ).Value = PluginData.Version;
               }
            }

            Save();
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred during configuration. Shutting plugin down." );

            IsShutdown = true;
         }
      }

      public static void AddDuplicateName( string name )
      {
         DuplicateTextureNames.Add( name );
         PluginEnvironment.Current.Preferences[ "Texture" ][ "DuplicateTextureNames" ].Value = string.Join( ";", DuplicateTextureNames.ToArray() );
         Save();
      }

      private static List<Regex> ParseRegexCollection( string rawPatterns, string settingName )
      {
         var regexes = new List<Regex>();
         if( string.IsNullOrEmpty( rawPatterns ) ) return regexes;

         var patterns = rawPatterns.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries );
         foreach( var rawPattern in patterns )
         {
            var pattern = rawPattern.Trim();
            if( pattern.Length == 0 ) continue;

            try
            {
               regexes.Add( new Regex( pattern, RegexOptions.None ) );
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Warn( e, $"Failed to parse regex '{pattern}' from setting '{settingName}'. Ignoring that regex." );
            }
         }

         return regexes;
      }

      public static void SetEndpoint( string id )
      {
         id = id ?? string.Empty;

         ServiceEndpoint = id;
         PluginEnvironment.Current.Preferences[ "Service" ][ "Endpoint" ].Value = id;
         Save();
      }

      public static void SetFallback( string id )
      {
         id = id ?? string.Empty;

         FallbackServiceEndpoint = id;
         PluginEnvironment.Current.Preferences[ "Service" ][ "FallbackEndpoint" ].Value = id;
         Save();
      }

      public static void SetSlientMode( bool enabled )
      {
         EnableSilentMode = enabled;
         PluginEnvironment.Current.Preferences[ "Behaviour" ][ "EnableSilentMode" ].Value = enabled.ToString( CultureInfo.InvariantCulture );
         Save();
      }

      public static void SetUIPathInspector( bool enabled )
      {
         EnableUIPathInspector = enabled;
         PluginEnvironment.Current.Preferences[ "Behaviour" ][ "EnableUIPathInspector" ].Value = enabled.ToString( CultureInfo.InvariantCulture );
         Save();
      }

      public static void SetTranslationAggregatorBounds( float width, float height )
      {
         Width = width;
         Height = height;
         PluginEnvironment.Current.Preferences[ "TranslationAggregator" ][ "Width" ].Value = Width.ToString( CultureInfo.InvariantCulture );
         PluginEnvironment.Current.Preferences[ "TranslationAggregator" ][ "Height" ].Value = Height.ToString( CultureInfo.InvariantCulture );
         Save();
      }

      public static void AddTranslator( string id )
      {
         EnabledTranslators.Add( id );
         PluginEnvironment.Current.Preferences[ "TranslationAggregator" ][ "EnabledTranslators" ].Value = string.Join( ";", EnabledTranslators.ToArray() );
         Save();
      }

      public static void RemoveTranslator( string id )
      {
         EnabledTranslators.Remove( id );
         PluginEnvironment.Current.Preferences[ "TranslationAggregator" ][ "EnabledTranslators" ].Value = string.Join( ";", EnabledTranslators.ToArray() );
         Save();
      }

      internal static void Save()
      {
         try
         {
            PluginEnvironment.Current.SaveConfig();
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred during while saving configuration." );
         }
      }

      private static string ResolveConfiguredLanguageOrSystemLanguage( string configuredLanguage, string detectedSystemLanguageCode )
      {
         if( !string.IsNullOrEmpty( configuredLanguage ) )
         {
            return configuredLanguage;
         }

         if( !string.IsNullOrEmpty( detectedSystemLanguageCode ) )
         {
            return detectedSystemLanguageCode;
         }

         XuaLogger.AutoTranslator.Warn( $"General.Language not specified and system language could not be resolved. Falling back to default language '{DefaultLanguage}'." );
         return DefaultLanguage;
      }

      private static void DetectSystemLanguage( out string detectedSystemLanguageCode, out string detectedSystemLanguageContext )
      {
         detectedSystemLanguageCode = null;
         detectedSystemLanguageContext = string.Empty;

         try
         {
            var unitySystemLanguage = Application.systemLanguage;
            var mappedUnityLanguage = MapSystemLanguage( unitySystemLanguage );
            if( !string.IsNullOrEmpty( mappedUnityLanguage ) )
            {
               detectedSystemLanguageCode = mappedUnityLanguage;
            }

            detectedSystemLanguageContext = AppendSystemLanguageContext( detectedSystemLanguageContext, "Unity=" + unitySystemLanguage );
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Warn( e, "An error occurred while trying to determine the system language through Unity." );
         }

         try
         {
            var culture = CultureInfo.CurrentUICulture;
            var cultureLanguage = GetLanguageCodeFromCulture( culture );
            if( string.IsNullOrEmpty( detectedSystemLanguageCode ) && !string.IsNullOrEmpty( cultureLanguage ) )
            {
               detectedSystemLanguageCode = cultureLanguage;
            }

            if( culture != null )
            {
               detectedSystemLanguageContext = AppendSystemLanguageContext( detectedSystemLanguageContext, "CurrentUICulture=" + culture.Name );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Warn( e, "An error occurred while trying to determine the system language through the current UI culture." );
         }
      }

      private static string AppendSystemLanguageContext( string existingContext, string value )
      {
         if( string.IsNullOrEmpty( value ) ) return existingContext ?? string.Empty;
         if( string.IsNullOrEmpty( existingContext ) ) return value;
         return existingContext + ", " + value;
      }

      private static void LogLanguageConfiguration()
      {
         var configuredLanguageText = string.IsNullOrEmpty( ConfiguredLanguage ) ? "(auto)" : ConfiguredLanguage;
         var detectedSystemLanguageText = string.IsNullOrEmpty( DetectedSystemLanguageCode ) ? "(unresolved)" : DetectedSystemLanguageCode;
         if( !string.IsNullOrEmpty( DetectedSystemLanguageContext ) )
         {
            detectedSystemLanguageText += " [" + DetectedSystemLanguageContext + "]";
         }

         var targetLanguageSource = UsingSystemLanguageAsDefault ? "system language" : "General.Language";
         XuaLogger.AutoTranslator.Info( $"Language settings resolved. General.Language={configuredLanguageText}, SystemLanguage={detectedSystemLanguageText}, TargetLanguage={Language}, FromLanguage={FromLanguage}, TargetLanguageSource={targetLanguageSource}." );
      }

      private static string MapSystemLanguage( SystemLanguage systemLanguage )
      {
         switch( systemLanguage )
         {
            case SystemLanguage.Afrikaans: return "af";
            case SystemLanguage.Arabic: return "ar";
            case SystemLanguage.Basque: return "eu";
            case SystemLanguage.Belarusian: return "be";
            case SystemLanguage.Bulgarian: return "bg";
            case SystemLanguage.Catalan: return "ca";
            case SystemLanguage.Chinese:
            case SystemLanguage.ChineseSimplified:
               return "zh";
            case SystemLanguage.ChineseTraditional:
               return "zh-TW";
            case SystemLanguage.Czech: return "cs";
            case SystemLanguage.Danish: return "da";
            case SystemLanguage.Dutch: return "nl";
            case SystemLanguage.English: return "en";
            case SystemLanguage.Estonian: return "et";
            case SystemLanguage.Faroese: return "fo";
            case SystemLanguage.Finnish: return "fi";
            case SystemLanguage.French: return "fr";
            case SystemLanguage.German: return "de";
            case SystemLanguage.Greek: return "el";
            case SystemLanguage.Hebrew: return "he";
            case SystemLanguage.Hungarian: return "hu";
            case SystemLanguage.Icelandic: return "is";
            case SystemLanguage.Indonesian: return "id";
            case SystemLanguage.Italian: return "it";
            case SystemLanguage.Japanese: return "ja";
            case SystemLanguage.Korean: return "ko";
            case SystemLanguage.Latvian: return "lv";
            case SystemLanguage.Lithuanian: return "lt";
            case SystemLanguage.Norwegian: return "no";
            case SystemLanguage.Polish: return "pl";
            case SystemLanguage.Portuguese: return "pt";
            case SystemLanguage.Romanian: return "ro";
            case SystemLanguage.Russian: return "ru";
            case SystemLanguage.SerboCroatian: return "sh";
            case SystemLanguage.Slovak: return "sk";
            case SystemLanguage.Slovenian: return "sl";
            case SystemLanguage.Spanish: return "es";
            case SystemLanguage.Swedish: return "sv";
            case SystemLanguage.Thai: return "th";
            case SystemLanguage.Turkish: return "tr";
            case SystemLanguage.Ukrainian: return "uk";
            case SystemLanguage.Vietnamese: return "vi";
            case SystemLanguage.Unknown:
            default:
               return null;
         }
      }

      private static string GetLanguageCodeFromCulture( CultureInfo culture )
      {
         if( culture == null ) return null;

         var cultureName = culture.Name ?? string.Empty;
         if( cultureName.StartsWith( "zh", StringComparison.OrdinalIgnoreCase ) )
         {
            if( cultureName.Equals( "zh-TW", StringComparison.OrdinalIgnoreCase )
               || cultureName.Equals( "zh-HK", StringComparison.OrdinalIgnoreCase )
               || cultureName.Equals( "zh-MO", StringComparison.OrdinalIgnoreCase )
               || cultureName.StartsWith( "zh-Hant", StringComparison.OrdinalIgnoreCase ) )
            {
               return "zh-TW";
            }

            return "zh";
         }

         var twoLetterIsoLanguageName = culture.TwoLetterISOLanguageName;
         if( string.IsNullOrEmpty( twoLetterIsoLanguageName ) || string.Equals( twoLetterIsoLanguageName, "iv", StringComparison.OrdinalIgnoreCase ) )
         {
            return null;
         }

         return twoLetterIsoLanguageName;
      }

      private static string NormalizeConfiguredLanguageCode( string languageCode )
      {
         if( string.IsNullOrEmpty( languageCode ) ) return string.Empty;

         var normalized = languageCode.Trim().Replace( '_', '-' );
         if( normalized.Length == 0 ) return string.Empty;

         if( normalized.StartsWith( "zh", StringComparison.OrdinalIgnoreCase ) )
         {
            if( normalized.Equals( "zh-TW", StringComparison.OrdinalIgnoreCase )
               || normalized.Equals( "zh-HK", StringComparison.OrdinalIgnoreCase )
               || normalized.Equals( "zh-MO", StringComparison.OrdinalIgnoreCase )
               || normalized.StartsWith( "zh-Hant", StringComparison.OrdinalIgnoreCase ) )
            {
               return "zh-TW";
            }

            if( normalized.Equals( "zh-CN", StringComparison.OrdinalIgnoreCase )
               || normalized.StartsWith( "zh-Hans", StringComparison.OrdinalIgnoreCase ) )
            {
               return "zh";
            }
         }

         return normalized;
      }

      private static string ResolveLanguageSpecificSetting( string settingName, string rawValue )
      {
         if( string.IsNullOrEmpty( rawValue ) ) return string.Empty;

         var trimmedValue = rawValue.Trim();
         if( trimmedValue.Length == 0 ) return string.Empty;

         Dictionary<string, string> mappings;
         if( !TryParseLanguageSpecificMappings( trimmedValue, out mappings ) )
         {
            return trimmedValue.Parameterize();
         }

         var resolvedValue = ResolveLanguageSpecificMapping( mappings, Language );
         if( resolvedValue != null )
         {
            XuaLogger.AutoTranslator.Info( $"Resolved language-specific setting '{settingName}' for destination language '{Language}'." );
            return resolvedValue.Parameterize();
         }

         XuaLogger.AutoTranslator.Warn( $"Could not resolve language-specific setting '{settingName}' for destination language '{Language}'. Leaving it empty." );
         return string.Empty;
      }

      private static bool TryParseLanguageSpecificMappings( string rawValue, out Dictionary<string, string> mappings )
      {
         mappings = null;

         var entries = rawValue.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries );
         if( entries.Length == 0 ) return false;

         var parsedMappings = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
         for( int i = 0; i < entries.Length; i++ )
         {
            var entry = entries[ i ].Trim();
            if( entry.Length == 0 ) continue;

            var separatorIndex = entry.IndexOf( '=' );
            if( separatorIndex <= 0 ) return false;

            var key = entry.Substring( 0, separatorIndex ).Trim();
            if( key.Length == 0 ) return false;

            var value = entry.Substring( separatorIndex + 1 ).Trim();
            parsedMappings[ key ] = value;
         }

         if( parsedMappings.Count == 0 ) return false;

         mappings = parsedMappings;
         return true;
      }

      private static string ResolveLanguageSpecificMapping( Dictionary<string, string> mappings, string language )
      {
         foreach( var candidate in EnumerateLanguageCandidates( language ) )
         {
            string resolved;
            if( mappings.TryGetValue( candidate, out resolved ) ) return resolved;
         }

         string fallback;
         if( mappings.TryGetValue( "default", out fallback ) ) return fallback;
         if( mappings.TryGetValue( "*", out fallback ) ) return fallback;
         return null;
      }

      private static IEnumerable<string> EnumerateLanguageCandidates( string language )
      {
         if( string.IsNullOrEmpty( language ) ) yield break;

         var seen = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
         var normalizedLanguage = language.Trim().Replace( '_', '-' );
         if( normalizedLanguage.Length == 0 ) yield break;

         if( seen.Add( normalizedLanguage ) ) yield return normalizedLanguage;

         var underscoreLanguage = normalizedLanguage.Replace( '-', '_' );
         if( seen.Add( underscoreLanguage ) ) yield return underscoreLanguage;

         var separatorIndex = normalizedLanguage.IndexOf( '-' );
         if( separatorIndex > 0 )
         {
            var neutralLanguage = normalizedLanguage.Substring( 0, separatorIndex );
            if( seen.Add( neutralLanguage ) ) yield return neutralLanguage;
         }
      }

      private static void Migrate()
      {
      }
   }
}
