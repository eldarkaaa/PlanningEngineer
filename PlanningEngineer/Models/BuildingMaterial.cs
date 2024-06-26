using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace PlanningEngineerApplication.Models
{
    public enum BuildingMaterial
    {
        [Display(Name = "Кирпичный")]
        Brick,
        [Display(Name = "Панельный")]
        Panel,
        [Display(Name = "Монолитный")]
        Monolith,
        [Display(Name = "Каркасно-монолитный")]
        FrameMonolith
    }

    public static class BuildingMaterialExtensions
    {
        public static string GetDisplayName(this BuildingMaterial material)
        {
            var displayAttribute = material.GetType()
                .GetField(material.ToString())
                .GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.GetName() ?? material.ToString();
        }

        public static (double WallDeviation, double FloorAndCeilingDeviation) GetDeviationLimits(this BuildingMaterial material)
        {
            return material switch
            {
                BuildingMaterial.Brick => (15.0, 10.0),
                BuildingMaterial.Panel => (10.0, 5.0),
                BuildingMaterial.Monolith => (5.0, 5.0),
                BuildingMaterial.FrameMonolith => (5.0, 5.0),
                _ => (10.0, 10.0),
            };
        }
    }
}
