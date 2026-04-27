using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Fonts
{
   internal static class FontHelper
   {
      public static UnityEngine.Object GetTextMeshProFont( string assetBundle )
      {
         if( assetBundle == null || assetBundle.Trim().Length == 0 )
         {
            XuaLogger.AutoTranslator.Warn( "TextMesh Pro override font bundle name is empty." );
            return null;
         }

         var trimmedAssetBundle = assetBundle.Trim();
         UnityEngine.Object font = null;
         var resolvedPath = FindAssetBundlePath( trimmedAssetBundle );

         if( resolvedPath != null )
         {
            XuaLogger.AutoTranslator.Info( $"Attempting to load TextMesh Pro font from asset bundle '{Path.GetFileName( resolvedPath )}'." );

            AssetBundle bundle = null;
            try
            {
               bundle = LoadAssetBundle( resolvedPath );
               if( bundle == null )
               {
   #if IL2CPP
                     font = GetTextMeshProFontByCustomProxies( trimmedAssetBundle );
   #else
                  XuaLogger.AutoTranslator.Error( $"Failed to load font asset bundle: {resolvedPath}" );
   #endif
               }
               else
               {
                  font = LoadFontFromBundle( bundle, resolvedPath );
               }
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, $"An error occurred while loading font asset bundle: {resolvedPath}" );
            }
            finally
            {
               bundle?.Unload( false );
            }
         }
         else
         {
            XuaLogger.AutoTranslator.Warn( $"Font asset bundle '{trimmedAssetBundle}' was not found near the game directory. Falling back to Resources." );
         }

         if( font == null )
         {
            XuaLogger.AutoTranslator.Info( $"Attempting to load TextMesh Pro font '{trimmedAssetBundle}' from internal Resources API." );
            font = Resources.Load( trimmedAssetBundle );
         }

         if( font != null )
         {
            var versionProperty = UnityTypes.TMP_FontAsset_Properties.Version;
            var version = (string)versionProperty?.Get( font ) ?? "Unknown";
            XuaLogger.AutoTranslator.Info( $"Loaded TextMesh Pro font uses version: {version}" );

            if( versionProperty != null && Settings.TextMeshProVersion != null && version != Settings.TextMeshProVersion )
            {
               XuaLogger.AutoTranslator.Warn( $"TextMesh Pro version mismatch. Font asset version: {version}, TextMesh Pro version: {Settings.TextMeshProVersion}" );
            }

            GameObject.DontDestroyOnLoad( font );
         }
         else
         {
            XuaLogger.AutoTranslator.Error( "Could not find the TextMeshPro font asset: " + trimmedAssetBundle );
         }

         return font;
      }

#if IL2CPP
      public static UnityEngine.Object GetTextMeshProFontByCustomProxies( string assetBundle )
      {
         UnityEngine.Object font = null;

         var overrideFontPath = FindAssetBundlePath( assetBundle == null ? null : assetBundle.Trim() );
         if( overrideFontPath != null )
         {
            XuaLogger.AutoTranslator.Info( $"Attempting to load TextMesh Pro font from asset bundle '{Path.GetFileName( overrideFontPath )}'." );
            
            var bundle = AssetBundleProxy.LoadFromFile( overrideFontPath );

            if( bundle == null )
            {
               XuaLogger.AutoTranslator.Warn( "Could not load asset bundle while loading font: " + overrideFontPath );
               return null;
            }

            if( UnityTypes.TMP_FontAsset != null )
            {
               var assets = bundle.LoadAllAssets( UnityTypes.TMP_FontAsset.UnityType );
               font = assets?.FirstOrDefault();
            }
         }
         else
         {
            XuaLogger.AutoTranslator.Warn( "Font asset bundle '" + assetBundle + "' was not located for IL2CPP loader." );
         }

         if( font != null )
         {
            var versionProperty = UnityTypes.TMP_FontAsset_Properties.Version;
            var version = (string)versionProperty?.Get( font ) ?? "Unknown";
            XuaLogger.AutoTranslator.Info( $"Loaded TextMesh Pro font uses version: {version}" );

            if( versionProperty != null && Settings.TextMeshProVersion != null && version != Settings.TextMeshProVersion )
            {
               XuaLogger.AutoTranslator.Warn( $"TextMesh Pro version mismatch. Font asset version: {version}, TextMesh Pro version: {Settings.TextMeshProVersion}" );
            }

            GameObject.DontDestroyOnLoad( font );
         }
         else
         {
            XuaLogger.AutoTranslator.Error( "Could not find the TextMeshPro font asset: " + assetBundle );
         }

         return font;
      }
#endif

      private static AssetBundle LoadAssetBundle( string path )
      {
         if( UnityTypes.AssetBundle_Methods.LoadFromFile != null )
         {
            return (AssetBundle)UnityTypes.AssetBundle_Methods.LoadFromFile.Invoke( null, new object[] { path } );
         }

         if( UnityTypes.AssetBundle_Methods.CreateFromFile != null )
         {
            return (AssetBundle)UnityTypes.AssetBundle_Methods.CreateFromFile.Invoke( null, new object[] { path } );
         }

#if IL2CPP
         XuaLogger.AutoTranslator.Warn( $"No suitable AssetBundle load method was found for '{path}'. Falling back to IL2CPP custom proxy loader." );
#else
         XuaLogger.AutoTranslator.Error( "No suitable AssetBundle load method was found." );
#endif
         return null;
      }

      private static UnityEngine.Object LoadFontFromBundle( AssetBundle bundle, string bundlePath )
      {
         if( UnityTypes.TMP_FontAsset == null )
         {
            XuaLogger.AutoTranslator.Warn( "TMP_FontAsset type is unavailable while loading font bundle." );
            return null;
         }

         UnityEngine.Object font = null;

         if( UnityTypes.AssetBundle_Methods.LoadAllAssets != null )
         {
#if MANAGED
            var assets = UnityTypes.AssetBundle_Methods.LoadAllAssets.Invoke( bundle, new object[] { UnityTypes.TMP_FontAsset.UnityType } ) as UnityEngine.Object[];
            font = assets?.FirstOrDefault();
#else
            var assets = (Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object>)UnityTypes.AssetBundle_Methods.LoadAllAssets.Invoke( bundle, new object[] { UnityTypes.TMP_FontAsset.UnityType } );
            font = assets?.FirstOrDefault();
#endif
         }
         else if( UnityTypes.AssetBundle_Methods.LoadAll != null )
         {
#if MANAGED
            var assets = UnityTypes.AssetBundle_Methods.LoadAll.Invoke( bundle, new object[] { UnityTypes.TMP_FontAsset.UnityType } ) as UnityEngine.Object[];
            font = assets?.FirstOrDefault();
#else
            var assets = (Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object>)UnityTypes.AssetBundle_Methods.LoadAll.Invoke( bundle, new object[] { UnityTypes.TMP_FontAsset.UnityType } );
            font = assets?.FirstOrDefault();
#endif
         }
         else
         {
            XuaLogger.AutoTranslator.Warn( "No AssetBundle.LoadAll overloads were available while loading font bundle." );
         }

         string[] assetNames = new string[ 0 ];
         try
         {
            assetNames = bundle.GetAllAssetNames() ?? new string[ 0 ];
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Warn( e, "Failed to enumerate assets in font bundle." );
         }

#if MANAGED
         if( font == null && assetNames.Length > 0 )
         {
            foreach( var assetName in assetNames )
            {
               UnityEngine.Object candidate = null;
               try
               {
                  candidate = bundle.LoadAsset( assetName );
               }
               catch( Exception e )
               {
                  XuaLogger.AutoTranslator.Warn( e, $"Failed to load asset '{assetName}' from font bundle." );
               }

               var tmpFontType = UnityTypes.TMP_FontAsset.UnityType;
               var candidateType = candidate != null ? candidate.GetType() : null;
               if( candidate != null && tmpFontType != null && tmpFontType.IsAssignableFrom( candidateType ) )
               {
                  XuaLogger.AutoTranslator.Info( $"Loaded TMP_FontAsset '{Path.GetFileName( assetName )}' from bundle." );
                  font = candidate;
                  break;
               }
               else if( candidate != null )
               {
                  var typeName = candidateType != null ? candidateType.FullName : "(unknown)";
                  XuaLogger.AutoTranslator.Debug( $"Font bundle candidate '{assetName}' has runtime type '{typeName}'." );

                  bool nameMatches = candidateType != null && typeName != null && typeName.EndsWith( "TMP_FontAsset", StringComparison.Ordinal );
                  if( nameMatches )
                  {
                     XuaLogger.AutoTranslator.Warn( $"Accepting asset '{assetName}' with type '{typeName}' as TMP_FontAsset based on name match." );
                     font = candidate;
                     break;
                  }
                  else
                  {
                     XuaLogger.AutoTranslator.Debug( $"Asset '{assetName}' loaded but type '{typeName}' does not match TMP_FontAsset." );
                  }
               }
            }
         }
#endif

         if( font == null )
         {
            try
            {
#if MANAGED
               var allAssets = bundle.LoadAllAssets();
#else
               var allAssets = bundle.LoadAllAssets();
#endif
               if( allAssets != null )
               {
                  foreach( var asset in allAssets )
                  {
                     if( asset == null ) continue;
                     var assetType = asset.GetType();
                     var assetTypeName = assetType?.FullName ?? "(unknown)";
                     var tmpType = UnityTypes.TMP_FontAsset.UnityType;

                     if( tmpType != null && tmpType.IsAssignableFrom( assetType ) )
                     {
                        XuaLogger.AutoTranslator.Info( $"Loaded TMP_FontAsset '{asset.name}' via unfiltered LoadAllAssets." );
                        font = asset;
                        break;
                     }

                     if( assetTypeName != null && assetTypeName.EndsWith( "TMP_FontAsset", StringComparison.Ordinal ) )
                     {
                        XuaLogger.AutoTranslator.Warn( $"Accepting asset '{asset.name}' with type '{assetTypeName}' as TMP_FontAsset (fallback)." );
                        font = asset;
                        break;
                     }
                     else
                     {
                        XuaLogger.AutoTranslator.Debug( $"LoadAllAssets candidate '{asset.name}' has runtime type '{assetTypeName}'." );
                     }
                  }
               }
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Warn( e, "Failed to load all assets while searching for TMP_FontAsset." );
            }
         }

         if( font == null )
         {
            var sampleNames = assetNames.Select( Path.GetFileName ).Take( 5 ).Where( x => !string.IsNullOrEmpty( x ) ).ToArray();
            var sample = sampleNames.Length > 0 ? string.Join( ", ", sampleNames ) : "(no assets)";
            XuaLogger.AutoTranslator.Error( $"No TMP_FontAsset found inside asset bundle '{bundlePath}'. Assets: {sample}" );
         }

         return font;
      }

      private static string FindAssetBundlePath( string bundleName )
      {
         if( bundleName == null || bundleName.Length == 0 ) return null;

         var trimmed = bundleName.Trim();
         if( trimmed.Length == 0 ) return null;
         var searchDirectories = EnumerateBundleSearchDirectories();

         foreach( var directory in searchDirectories )
         {
            if( string.IsNullOrEmpty( directory ) || !Directory.Exists( directory ) )
            {
               continue;
            }

            string fallback = null;
            var files = Directory.GetFiles( directory );
            foreach( var file in files )
            {
               var fileName = Path.GetFileName( file );
               if( string.Equals( fileName, trimmed, StringComparison.OrdinalIgnoreCase ) )
               {
                  return file;
               }

               if( fallback == null && string.Equals( Path.GetFileNameWithoutExtension( file ), trimmed, StringComparison.OrdinalIgnoreCase ) )
               {
                  fallback = file;
               }
            }

            if( fallback != null )
            {
               XuaLogger.AutoTranslator.Info( $"Resolved font bundle '{bundleName}' to '{Path.GetFileName( fallback )}' inside '{directory}'." );
               return fallback;
            }
            else if( files.Length == 0 )
            {
               XuaLogger.AutoTranslator.Debug( $"No files found while searching for font bundle in '{directory}'." );
            }
            else
            {
               XuaLogger.AutoTranslator.Debug( $"Font bundle '{bundleName}' not matched in '{directory}'." );
            }
         }

         return null;
      }

      private static IEnumerable<string> EnumerateBundleSearchDirectories()
      {
         yield return Paths.GameRoot;

         var bepinex = Path.Combine( Paths.GameRoot, "BepInEx" );
         if( Directory.Exists( bepinex ) )
         {
            yield return bepinex;

            var plugins = Path.Combine( bepinex, "plugins" );
            if( Directory.Exists( plugins ) )
            {
               yield return plugins;
            }
         }
      }

      public static Font GetTextFont( int size )
      {
         var font = Font.CreateDynamicFontFromOSFont( Settings.OverrideFont, size );
         GameObject.DontDestroyOnLoad( font );

         return font;
      }

      public static string[] GetOSInstalledFontNames()
      {
         return Font.GetOSInstalledFontNames();
      }
   }
}
