using BepInEx;
using BepInEx.Logging;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Crafting;
using Nautilus.Extensions; // Added for CloneTemplate
using Nautilus.Handlers;
using Nautilus.Utility;
using Newtonsoft.Json;
using SubnauticaResourceFabricatorMod.src; // Added for FabricatorTabManager
using SubnauticaResourceFabricatorMod.types; // Add this line
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection; // Added for Assembly
using UnityEngine;
using UnityEngine.SceneManagement; // New using statement

[BepInPlugin("com.jgg.resourcefabricator", "Resource Fabricator Mod", "1.0.0")]
public class ResourceFabricatorMod : BaseUnityPlugin
{
    // Removed custom Logger definition to use inherited BaseUnityPlugin.Logger

    private void Awake()
    {
        Logger.LogInfo("Resource Fabricator Mod: Awake method called. (Only once)");
        // Create the main fabricator tab
        string menuName = FabricatorTabManager.CreateFabricatorTab();
        Logger.LogInfo($"Resource Fabricator Menu created: {menuName}");

        SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to sceneLoaded event
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Logger.LogInfo($"Resource Fabricator Mod: Scene loaded: {scene.name} (Mode: {mode})");

        // Only register recipes when the main game scene is loaded
        if (scene.name == "XMenu") // Register recipes when XMenu is loaded
        {
            Logger.LogInfo($"Resource Fabricator Mod: Attempting to register recipes for scene: {scene.name}");
            RegisterAllRecipesAndUnlock();
        }
    }

    private static HashSet<string> createdTabPaths = new HashSet<string>();
    private static bool recipesRegistered = false;

    private void RegisterAllRecipesAndUnlock() // Changed from IEnumerator to void
    {
        Logger.LogInfo("Resource Fabricator Mod: RegisterAllRecipesAndUnlock method called."); // Updated log

        if (recipesRegistered)
        {
            Logger.LogInfo("Recipes already registered. Skipping.");
            return;
        }

        // ********************
        /*
        // THIS WORKS !!!!
        TechType customOre = TechType.Titanium; // Esto es conceptual

        var recipeTest = new RecipeData()
        {
            craftAmount = 1,
            Ingredients =
                    {
                        new Ingredient(TechType.Titanium, 2),
                        new Ingredient(TechType.Copper, 1)
                    }
        };

        // Registrar la receta
        CraftDataHandler.SetRecipeData(customOre, recipeTest);

        var titaniumIcon = SpriteManager.Get(TechType.Titanium);

        // Asignar al menú Ores
        CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, "Ores", "Ores from Fish", titaniumIcon);

        // Asignar tu TechType al nuevo menú
        CraftTreeHandler.AddCraftingNode(
            CraftTree.Type.Fabricator,
            customOre,
            new[] { "Ores" } // Ruta dentro del árbol
        );
        */



        /*
        // !!!!!!!!!!! Esta receta sólo aparece en creativo, no en supervivencia !!!!!!!!!!!
        // First Recipe of the new Menu Entry (Ores) = Titanium = 2 Pepper (fish)
        RecipeConfig recipeObj = new RecipeConfig
        {
            ItemID = "PeeperTitaniumOrigin",
            DisplayName = "Peeper Titanium",
            Description = "Extracted Titanium from a Peeper.",
            FunctionalID = "Titanium",
            AmountCrafted = 1
        };
        string alternativeTechTypeId = recipeObj.ItemID; // e.g., "PeeperTitanium"
        string alternativeDisplayName = recipeObj.DisplayName + " (from Fish)"; // e.g., "Titanium (from Fish)"

        // Create a CustomPrefab for the alternative recipe
        // The icon should be the original item's icon
        var recipeIcon = SpriteManager.Get(TechType.Titanium);
        CustomPrefab alternativePrefab = new CustomPrefab(alternativeTechTypeId, alternativeDisplayName, recipeObj.Tooltip, recipeIcon);

        if (TechTypeExtensions.FromString(recipeObj.FunctionalID, out TechType recipeTechType, true))
        {
            // Set the GameObject for the alternative prefab (can clone the original item's GameObject)
            // Generalize fallback for cloning if the target TechType does not have a suitable GameObject
            TechType cloneFromTechType = recipeTechType;

            try
            {
                alternativePrefab.SetGameObject(new CloneTemplate(alternativePrefab.Info, cloneFromTechType));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error cloning GameObject for {recipeObj.FunctionalID} from {cloneFromTechType}: {ex.Message}. Falling back to Titanium.");
                cloneFromTechType = TechType.Titanium; // Fallback to Titanium
                alternativePrefab.SetGameObject(new CloneTemplate(alternativePrefab.Info, cloneFromTechType)); // Try again with Titanium
            }

            var recipeToAdd = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>() // Initialize the list
            };

            recipeToAdd.Ingredients.Add(new Ingredient(TechType.Peeper, 2)); // 2 Pepper fish

            // Set the recipe for the alternative prefab
            alternativePrefab.SetRecipe(recipeToAdd)
                                .WithFabricatorType(CraftTree.Type.Fabricator)
                                .WithStepsToFabricatorTab(new[] { FabricatorTabManager.MainTabId });

            // Register the alternative prefab
            alternativePrefab.Register();

            Logger.LogInfo($"Registered alternative recipe {alternativeTechTypeId} for {recipeObj.FunctionalID}.");

            // Asignar tu TechType al nuevo menú
            CraftTreeHandler.AddCraftingNode(
                CraftTree.Type.Fabricator,
                alternativePrefab.Info.TechType,
                new[] { "Ores" } // Ruta dentro del árbol
            );

        }
        */



        // Now add all the recipes in json files (folder ./src/recipes)
        string recipesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets", "recipes");
        Logger.LogInfo($"Looking for recipes in: {recipesPath}"); // New log
        if (!Directory.Exists(recipesPath))
        {
            Logger.LogWarning($"Recipes folder not found: {recipesPath}");
            return; // Changed from yield break to return
        }

        string[] recipeFiles = Directory.GetFiles(recipesPath, "*.json");
        Logger.LogInfo($"Found {recipeFiles.Length} recipe files.");

        // Removed DayNightCycle.main check

        foreach (var file in recipeFiles)
        {
            Logger.LogInfo($"Processing recipe file: {Path.GetFileName(file)}");
            string json = File.ReadAllText(file);
            RecipeConfig recipe = null; // Declare outside try-catch
            try
            {
                Logger.LogInfo($"Attempting to deserialize {Path.GetFileName(file)}.");
                recipe = JsonConvert.DeserializeObject<RecipeConfig>(json);
                Logger.LogInfo($"Deserialization successful for {Path.GetFileName(file)}.");

                Logger.LogInfo($"Attempting to register recipe from {Path.GetFileName(file)}.");
                RegisterRecipe(recipe, file); // Pass the file path here
                Logger.LogInfo($"Recipe registration successful for {Path.GetFileName(file)}.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error processing recipe file {file}: {ex.Message}\n{ex.StackTrace}");
                continue; // Continue to next file if deserialization or registration fails
            }

            if (recipe != null && recipe.ForceUnlockAtStart)
            {
                // KnownTech.Add(TechTypeExtensions.FromString(recipe.FunctionalID, out TechType techType, true) ? techType : TechType.None, true);
            }
        }
        recipesRegistered = true; // Set recipesRegistered here
    }



    public static TechType GetTechTypeFromIDName(string spriteName)
    {
        // Mapeo manual de nombres comunes
        switch (spriteName.ToLower())
        {
            case "titanium":
                return TechType.Titanium;
            case "copper":
                return TechType.Copper;
            case "gold":
                return TechType.Gold;
            case "diamond":
                return TechType.Diamond;
            case "lithium":
                return TechType.Lithium;
            case "quartz":
                return TechType.Quartz;
            case "uranium":
            case "uraninite":
            case "uraninitecrystal":
                return TechType.UraniniteCrystal;
            case "pepper":
                return TechType.Peeper;
            case "bladderfish":
                return TechType.Bladderfish;
            case "reginald":
                return TechType.Reginald;
            case "hoverfish":
                return TechType.Hoverfish;
            case "hoopfish":
                return TechType.Hoopfish;
            case "oculus":
                return TechType.Oculus;
            case "eyeye":
                return TechType.Eyeye;
            case "garryfish":
                return TechType.GarryFish;
            case "boomerang":
                return TechType.Boomerang;
            case "spinefish":
                return TechType.Spinefish;
            case "spadefish":
                return TechType.Spadefish;
            case "ingot":
            case "titaniumingot":
                return TechType.TitaniumIngot;
            case "metal":
            case "scrapmetal":
                return TechType.ScrapMetal;
            case "jeweleddiskpiece":
            case "tablecoral":
            case "coraltable":
            case "coral":
                return TechType.JeweledDiskPiece;
            case "stalkertooth":
            case "stalkerteeth":
            case "tooth":
            case "teeth":
                return TechType.StalkerTooth;
            case "nickel":
                return TechType.Nickel;
            case "silver":
                return TechType.Silver;
            case "kyanite":
                return TechType.Kyanite;
            case "aluminumoxide":
            case "ruby":
            case "rubi":
                return TechType.AluminumOxide;
            case "salt":
                return TechType.Salt;
            case "ion":
            case "ionpowercell":
            case "precursorionpowercell":
                return TechType.PrecursorIonPowerCell;
            case "magnetite":
                return TechType.Magnetite;
            case "magnesium":
                return TechType.Magnesium;
            case "sulphur":
                return TechType.Sulphur;
            case "lead":
                return TechType.Lead;
            case "bleach":
                return TechType.Bleach;
            case "aramidfibers":
            case "syntheticfibers":
                return TechType.AramidFibers;
            case "lubricant":
            case "oil":
                return TechType.Lubricant;
            case "polyaniline":
                return TechType.Polyaniline;
            case "benzene":
                return TechType.Benzene;
            case "aerogel":
                return TechType.Aerogel;
            case "hydrochloricacid":
            case "hydrochloric":
                return TechType.HydrochloricAcid;
            case "bloodoil":
                return TechType.BloodOil;
            case "acidmushroom":
            case "acid":
                return TechType.AcidMushroom;
            case "whitemushroom":
            case "deepmushroom":
            case "deep":
                return TechType.WhiteMushroom;
            case "bigfilteredwater":
            case "filteredwater":
            case "water":
                return TechType.BigFilteredWater;
            case "hangingfruit":
                return TechType.HangingFruit;
            case "purplevegetable":
                return TechType.PurpleVegetable;
            case "melon":
                return TechType.MelonSeed;
            case "nutrientblock":
            case "nutrient":
                return TechType.Lead;
            // Añade más según necesites
            default:
                return TechType.None;
        }
    }

    public bool TechTypeExists(string techTypeId)
    {
        return EnumHandler.TryGetValue<TechType>(techTypeId, out _);
    }

    private void RegisterRecipe(RecipeConfig recipe, string jsonFilePath)
    {
        Logger.LogInfo($"Registering recipe for: {recipe.FunctionalID}");

        TechType outputTechType = GetTechTypeFromIDName(recipe.FunctionalID);

        if (outputTechType == TechType.None)
        {
            Logger.LogError($"Could not find TechType for output item: {recipe.FunctionalID}. Skipping recipe registration for {recipe.DisplayName}.");
            return;
        }

        var recipeData = new RecipeData()
        {
            craftAmount = recipe.AmountCrafted,
            Ingredients = new List<Ingredient>()
        };

        // Add all the ingredients from the JSON
        for (int i = 0; i < recipe.Ingredients.Count; i++)
        {
            TechType ingredientTechType = GetTechTypeFromIDName(recipe.Ingredients[i].ItemID);
            if (ingredientTechType != TechType.None)
            {
                recipeData.Ingredients.Add(new Ingredient(ingredientTechType, recipe.Ingredients[i].Required));
                Logger.LogInfo($"   Ingredient {i + 1}: {recipe.Ingredients[i].ItemID} x {recipe.Ingredients[i].Required}");
            }
            else
            {
                Logger.LogWarning($"Could not find TechType for ingredient: {recipe.Ingredients[i].ItemID}. Skipping ingredient for recipe: {recipe.DisplayName}.");
            }
        }

        // Register the recipe using CraftDataHandler.SetRecipeData
        CraftDataHandler.SetRecipeData(outputTechType, recipeData);

        Logger.LogInfo($"Registered recipe for existing item {outputTechType} ({recipe.FunctionalID}).");

        // Add to craft tree
        CraftTreeHandler.AddCraftingNode(
            CraftTree.Type.Fabricator,
            outputTechType,
            FabricatorTabManager.MainTabId // Use the defined MainTabId
        );

        // Forzar desbloqueo inmediato
        if (recipe.ForceUnlockAtStart)
        {
            KnownTechHandler.UnlockOnStart(outputTechType);
        }
    }
}