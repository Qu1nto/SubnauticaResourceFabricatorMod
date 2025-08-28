using System.Collections.Generic;

namespace SubnauticaResourceFabricatorMod.types
{
    public class RecipeConfig
    {
        public string ItemID { get; set; }
        public string DisplayName { get; set; }
        public string Tooltip { get; set; }
        public string FunctionalID { get; set; }
        public string SpriteItemID { get; set; }
        public string UnlockedBy { get; set; }
        public int AmountCrafted { get; set; }
        public bool ForceUnlockAtStart { get; set; }
        public List<Ingredient> Ingredients { get; set; }
        public string Path { get; set; }
        public string Description { get; set; } // Added for CustomPrefab
    }

    public class Ingredient
    {
        public string ItemID { get; set; }
        public int Required { get; set; }
    }

}