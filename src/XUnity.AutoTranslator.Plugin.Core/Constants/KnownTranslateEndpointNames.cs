using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Constants
{
   /// <summary>
   /// Class representing known endpoint names.
   /// </summary>
   public static class KnownTranslateEndpointNames
   {
      /// <summary>
      /// Gets the id used by LLMTranslate.
      /// </summary>
      public const string LLMTranslate = "LLMTranslate";

      /// <summary>
      /// Gets the id used by GoogleTranslate.
      /// </summary>
      public const string GoogleTranslateV2 = "GoogleTranslateV2";

      /// <summary>
      /// Gets the id used by GoogleTranslate legitimate.
      /// </summary>
      public const string GoogleTranslateLegitimate = "GoogleTranslateLegitimate";
   }
}
