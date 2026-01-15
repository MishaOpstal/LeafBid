using Bogus;
using LeafBidAPI.Models;

namespace LeafBidAPI.Data.factories;

public class ProductFactory : Factory<Product>
{
    private static readonly (string Species, string[] Variants, string Description, string ImageFile)[] PlantCatalog =
    {
        ("Monstera deliciosa", new[] { "XL", "Compacta", "Variegata" },
            "Populaire kamerplant met grote, geperforeerde bladeren. Houdt van halfschaduw en licht vochtige grond.",
            "monstera-deliciosa.jpg"),

        ("Ficus lyrata", new[] { "Bush", "Branched", "Column" },
            "Bekend als de vioolbladplant. Sterke sierplant met grote, glanzende bladeren.",
            "ficus-lyrata.jpg"),

        ("Areca dypsis", new[] { "Golden Cane", "XL Palm" },
            "Luchtzuiverende palm met sierlijke, geveerde bladeren. Ideaal voor kantoor en woonkamer.",
            "areca-dypsis.jpg"),

        ("Dracaena marginata", new[] { "Bicolor", "Tricolor", "Magenta" },
            "Sterke plant die weinig water nodig heeft. Geschikt voor moderne interieurs.",
            "dracaena-marginata.jpg"),

        ("Calathea orbifolia", new[] { "Standard", "Large" },
            "Schitterende bladplant met zilvergroene patronen. Houdt van hoge luchtvochtigheid.",
            "calathea-orbifolia.jpg"),

        ("Spathiphyllum", new[] { "Sensation", "Sweet Chico", "Cupido" },
            "Bekend als de lepelplant. Bloeit gemakkelijk en is sterk luchtzuiverend.",
            "spathiphyllum.jpg"),

        ("Sansevieria zeylanica", new[] { "Classic", "Compacta" },
            "Bijna onverwoestbare plant. Ideaal voor beginners en kantoren.",
            "sansevieria-zeylanica.jpg"),

        ("Schefflera arboricola", new[] { "Compacta", "Gold Capella" },
            "Decoratieve plant met handvormige bladeren. Groeit snel en is makkelijk te verzorgen.",
            "schefflera-arboricola.jpg"),

        ("Chlorophytum comosum", new[] { "Variegatum", "Bonnie" },
            "Bekend als de graslelie. Zeer sterke plant die snel uitlopers vormt.",
            "chlorophytum-comosum.jpg"),

        ("Zamioculcas zamiifolia", new[] { "Classic", "Raven" },
            "Moderne, sterke plant met glanzende bladeren. Kan goed tegen droogte.",
            "zamioculcas-zamiifolia.jpg")
    };

    protected override Faker<Product> BuildRules()
    {
        return new Faker<Product>("nl")
            .CustomInstantiator(f =>
            {
                var plant = f.PickRandom(PlantCatalog);
                string variant = f.PickRandom(plant.Variants);

                return new Product
                {
                    Species = plant.Species,
                    Name = $"{plant.Species} {variant}",
                    Description = plant.Description,
                    Picture = $"/images/plants/{plant.ImageFile}"
                };
            });
    }
}