using System;
using System.Reflection;

namespace XUnity.Common.Constants
{

   /// <summary>
   /// Class that allows you to check which features are availble of the Unity version that is used.
   /// </summary>
   public static class UnityFeatures
   {
#if MANAGED
      private static readonly BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
#endif

      /// <summary>
      /// Indicates whether <c>Input.mouseScrollDelta</c> is available in the current Unity runtime.
      /// </summary>
      public static bool SupportsMouseScrollDelta { get; } = false;
      /// <summary>
      /// Indicates whether text clipboard support exists (TextEditor.text setter available).
      /// </summary>
      public static bool SupportsClipboard { get; } = false;
      /// <summary>
      /// Indicates whether <c>CustomYieldInstruction</c> is available in the current Unity runtime.
      /// </summary>
      public static bool SupportsCustomYieldInstruction { get; } = false;
      /// <summary>
      /// Indicates whether the SceneManager API is present and can be used.
      /// </summary>
      public static bool SupportsSceneManager { get; } = false;
      /// <summary>
      /// Indicates whether <c>WaitForSecondsRealtime</c> is available in the current Unity runtime.
      /// </summary>
      public static bool SupportsWaitForSecondsRealtime { get; set; } = false;

      static UnityFeatures()
      {
         try
         {
            SupportsClipboard = UnityTypes.TextEditor?.ClrType.GetProperty( "text" )?.GetSetMethod() != null;
         }
         catch( Exception )
         {

         }

         try
         {
            SupportsCustomYieldInstruction = UnityTypes.CustomYieldInstruction != null;
         }
         catch( Exception )
         {

         }

         try
         {
            SupportsSceneManager = UnityTypes.Scene != null
               && UnityTypes.SceneManager != null
#if MANAGED
               && UnityTypes.SceneManager.ClrType.GetMethod( "add_sceneLoaded", All ) != null;
#else
               && UnityTypes.SceneManager_Methods.add_sceneLoaded != null;
#endif
         }
         catch( Exception )
         {

         }

         try
         {
            SupportsMouseScrollDelta = UnityTypes.Input?.ClrType.GetProperty( "mouseScrollDelta" ) != null;
         }
         catch( Exception )
         {

         }

         try
         {
            SupportsWaitForSecondsRealtime = UnityTypes.WaitForSecondsRealtime != null;
         }
         catch( Exception )
         {

         }
      }
   }
}
