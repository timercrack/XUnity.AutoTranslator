using System;
using System.IO;
using UnityEngine;

namespace XUnity.Common.Utilities
{
   /// <summary>
   /// WARNING: Pubternal API (internal). Do not use. May change during any update.
   /// </summary>
   public static class Paths
   {
      private static string _gameRoot;

      /// <summary>
      /// WARNING: Pubternal API (internal). Do not use. May change during any update.
      /// </summary>
      public static string GameRoot
      {
         get => _gameRoot ?? GetAndSetGameRoot();
         set => _gameRoot = value;
      }

      /// <summary>
      /// Initializes the cached game root path if it has not been set already.
      /// </summary>
      public static void Initialize()
      {
         GetAndSetGameRoot();
      }

      /// <summary>
      /// Resolves and stores the application directory root.
      /// </summary>
      private static string GetAndSetGameRoot()
      {
         return _gameRoot = new DirectoryInfo( Application.dataPath ).Parent.FullName;
      }
   }
}
