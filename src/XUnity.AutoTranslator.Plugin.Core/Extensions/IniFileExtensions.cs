using System;
using System.Globalization;
using System.Linq;
using ExIni;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Extensions
{
   internal static class IniFileExtensions
   {
      public static void Set<T>( this IniFile that, string section, string key, T value )
      {
         var typeOfT = typeof( T ).UnwrapNullable();
         var iniSection = that[ section ];
         var iniKey = iniSection[ key ];

         if( value == null )
         {
            iniKey.Value = string.Empty;
         }
         else
         {
            if( typeOfT.IsEnum )
            {
               iniKey.Value = EnumHelper.GetNames( typeOfT, value );
            }
            else
            {
               iniKey.Value = Convert.ToString( value, CultureInfo.InvariantCulture );
            }
         }
      }

      public static T GetOrDefault<T>( this IniFile that, string section, string key, T defaultValue )
      {
         var typeOfT = typeof( T ).UnwrapNullable();
         IniKey iniKey = null;

         try
         {
            if( that.HasSection( section ) )
            {
               var iniSection = that.GetSection( section );
               if( iniSection != null && iniSection.HasKey( key ) )
               {
                  iniKey = iniSection.GetKey( key );
               }
            }

            if( iniKey == null )
            {
               return defaultValue;
            }

            var value = iniKey.Value;

            // there exists a value in the config, so we do not want to set anything
            // we just want to return what we can find, default not included
            if( !string.IsNullOrEmpty( value ) )
            {
               if( typeOfT.IsEnum )
               {
                  return (T)EnumHelper.GetValues( typeOfT, iniKey.Value );
               }

               return (T)Convert.ChangeType( iniKey.Value, typeOfT, CultureInfo.InvariantCulture );
            }

            return default( T );
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, $"Error occurred while reading config '{key}' in section '{section}'. Updating the config to its default value '{defaultValue}'." );

            that.Set( section, key, defaultValue );
            return defaultValue;
         }
      }
   }
}
