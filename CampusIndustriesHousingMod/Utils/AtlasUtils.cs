using UnityEngine;

namespace CampusIndustriesHousingMod.Utils
{
    public static class AtlasUtils
    {
        public static string[] LockButtonSpriteNames =
        [
            "UnLock",
            "Lock"
        ];

        public static void CreateAtlas()
        {
            if (TextureUtils.GetAtlas("LockButtonAtlas") == null)
            {
                TextureUtils.InitialiseAtlas("LockButtonAtlas");
                TextureUtils.AddSpriteToAtlas(new Rect(1, 1, 36, 32), LockButtonSpriteNames[0], "LockButtonAtlas");
                TextureUtils.AddSpriteToAtlas(new Rect(39, 1, 28, 32), LockButtonSpriteNames[1], "LockButtonAtlas");
            }
        }
    }
}
