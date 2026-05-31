using System.IO;
using System.Reflection;
using UnityEngine;

namespace PhotoModePlus {
    public class Assets {

        public static AssetBundle assetBundle;

        public static void Init() {
            GetAssetBundle();
        }

        private static void GetAssetBundle() {
            using (Stream assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"PhotoModePlus.mwmwphotomodeplusbundle")) {
                if (assetStream != null) {
                    assetBundle = AssetBundle.LoadFromStream(assetStream);
                } else {
                    Log.Error("Failed to load assetbundle!");
                }
            }
        }
    }
}
