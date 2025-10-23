using System;
using System.IO;
using System.Text;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Configuration
{
    /// <summary>
    /// Provides helpers for creating default configuration assets when no user file exists.
    /// </summary>
    public static class ConfigurationDefaults
    {
        private const string LogTag = "[ConfigurationDefaults]";
        private const string ResourceName = "XUnity.AutoTranslator.Plugin.Core.Configuration.AutoTranslatorConfig.default.ini";

        /// <summary>
        /// Ensures that the configuration file exists by materializing the embedded default if necessary.
        /// </summary>
        /// <param name="configPath">Absolute path to the configuration file that should be available.</param>
        public static void EnsureExists( string configPath )
        {
            if( string.IsNullOrEmpty( configPath ) || File.Exists( configPath ) ) return;

            try
            {
                var directory = Path.GetDirectoryName( configPath );
                if( !string.IsNullOrEmpty( directory ) && !Directory.Exists( directory ) ) Directory.CreateDirectory( directory );

                using( var stream = typeof( ConfigurationDefaults ).Assembly.GetManifestResourceStream( ResourceName ) )
                {
                    if( stream == null )
                    {
                        XuaLogger.AutoTranslator.Warn( $"{LogTag} 无法找到内置默认配置资源 '{ResourceName}'." );
                        return;
                    }

                    using( var reader = new StreamReader( stream, Encoding.UTF8, false ) )
                    {
                        var content = reader.ReadToEnd();
                        File.WriteAllText( configPath, content, new UTF8Encoding( false ) );
                    }
                }
            }
            catch( Exception e )
            {
                XuaLogger.AutoTranslator.Warn( e, $"{LogTag} 创建默认配置文件时发生异常." );
            }
        }
    }
}
