using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using SubnauticaResourceFabricatorMod.types; // Added for RecipeConfig and Ingredient

public class RecipeLoader
{
    private const string RecipesDirectory = "assets/recipes/";

    public static List<RecipeConfig> LoadRecipes()
    {
        List<RecipeConfig> recipes = new List<RecipeConfig>();
        string[] recipeFiles = Directory.GetFiles(RecipesDirectory, "*.json");

        foreach (string file in recipeFiles)
        {
            string json = File.ReadAllText(file);
            RecipeConfig recipe = JsonConvert.DeserializeObject<RecipeConfig>(json);
            recipes.Add(recipe);
        }

        return recipes;
    }
}