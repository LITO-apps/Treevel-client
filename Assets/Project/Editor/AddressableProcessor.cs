using System.IO;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;

namespace Treevel.Editor
{
    public class AddressableProcessor
    {
        public static void BuildContents_iOS()
        {
            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;

            if (aaSettings != null && aaSettings.BuildRemoteCatalog)
            {
                var id = aaSettings.profileSettings.GetProfileId("Default");
                aaSettings.activeProfileId = id;
                var path = ContentUpdateScript.GetContentStateDataPath(false);
                if (File.Exists(path))
                {
                    ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, path);
                }
                else
                {
                    AddressableAssetSettings.BuildPlayerContent();
                }
            }
            else
            {
                AddressableAssetSettings.BuildPlayerContent();
            }
        }
    }
}
