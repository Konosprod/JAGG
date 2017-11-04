using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mgl;

namespace JAGG
{
    public class I18n : Mgl.I18n
    {
        new protected static readonly I18n instance = new I18n();

        new protected static string[] locales = new string[] {
            "en-US",
            "fr-FR"
        };

        new public static I18n Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
