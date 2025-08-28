using Nautilus.Handlers;
using Nautilus.Utility; // Added for ImageUtils
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SubnauticaResourceFabricatorMod.src
{
    internal class FabricatorTabManager
    {
        private static bool tabCreated = false;
        public const string MainTabId = "Ores";
        private const string MainTabDisplayName = "Ores from Fish";

        public static string CreateFabricatorTab()
        {
            if (!tabCreated)
            {
                // Load the custom tab icon from assets
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string iconPath = Path.Combine(assemblyFolder, "assets", "fabricator_tab_icon.png");

                // Cargar el icono
                Sprite tabIcon = ImageUtils.LoadSpriteFromFile(iconPath);

                if (tabIcon == null)
                {
                    // Fallback to a white texture if the icon fails to load
                    tabIcon = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
                    // Note: Cannot use BepInEx.Logger directly in a static class without passing it.
                }

                // Create the main tab for the mod
                CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, MainTabId, MainTabDisplayName, tabIcon);

                tabCreated = true;

                return MainTabId;
            }
            return "";
        }
    }
}