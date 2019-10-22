using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public static class ServerNameGenerator
{
	private static readonly string [] adjectives = {
		"Adamant", "Adroit", "Amatory", "Animistic", "Antic",
		"Arcadian", "Baleful", "Bellicose","Bilious","Boorish",
		"Calamitous", "Caustic", "Cerulean", "Comely", "Concomitant",
		"Contumacious", "Corpulent", "Crapulous","Defamatory","Didactic",
		"Dilatory", "Dowdy", "Efficacious", "Effulgent", "Egregious",
		"Endemic", "Equanimous", "Execrable","Fastidious","Feckless",
		"Fecund", "Friable", "Fulsome", "Garrulous", "Guileless",
		"Gustatory", "Heuristic", "Histrionic","Hubristic","Incendiary",
		"Insidious", "Insolent", "Intransigent", "Inveterate", "Invidious",
		"Irksome", "Jejune", "Jocular","Judicious","Lachrymose",
		"Limpid", "Loquacious", "Luminous", "Mannered", "Mendacious",
		"Meretricious", "Minatory", "Mordant","Munificent","Nefarious",
		"Noxious", "Obtuse", "Parsimonious", "Pendulous", "Pernicious",
		"Pervasive", "Petulant", "Platitudinous","Precipitate","Propitious",
		"Puckish", "Querulous", "Quiescent", "Rebarbative", "Recalcitrant",
		"Redolent", "Rhadamanthine", "Risible","Ruminative","Sagacious",
		"Salubrious", "Sartorial", "Sclerotic", "Serpentine", "Spasmodic",
		"Strident", "Taciturn", "Tenacious","Tremulous","Trenchant",
		"Turbulent", "Turgid", "Ubiquitous", "Uxorious", "Verdant",
		"Voluble", "Voracious", "Wheedling","Withering","Zealous"
	};

	private static readonly string [] substantives = {
	    "Temple", "Shrine", "Church", "Cathedral", "Tabernacle",
	    "Ark", "Sanctum", "Parish", "Chapel", "Synagogue",
	    "Mosque", "Pyramid", "Ziggurat", "Prison", "Jail",
	    "Dungeon", "Oubliette", "Hospital", "Hospice", "Stocks",
	    "Gallows", "Asylum", "Madhouse", "Bedlam", "Vault",
	    "Treasury", "Warehouse", "Cellar", "Relicry", "Repository",
	    "Barracks", "Armoury", "Sewer", "Gutter", "Catacombs",
	    "Dump", "Middens", "Pipes", "Baths", "Heap",
	    "Mill", "Windmill", "Sawmill", "Smithy", "Forge",
	    "Workshop", "Brickyard", "Shipyard", "Forgeworks", "Foundry",
	    "Bakery", "Brewery", "Almshouse", "counting House", "Courthouse",
	    "Apothecary", "Haberdashery", "Cobbler", "Garden", "Menagerie",
	    "Zoo", "Aquarium", "Terrarium", "Conservatory", "Lawn",
	    "Greenhouse", "Farm", "Orchard", "Vineyard", "Ranch",
	    "Apiary", "Farmstead", "Homestead", "Pasture", "Commons",
	    "Granary", "Silo", "Crop", "Barn", "Stable",
	    "Pen", "Kennel", "Mews", "Hutch", "Pound",
	    "Coop", "Stockade", "Yard", "lumber Yard", "Tavern",
	    "Inn", "Pub", "Brothel", "Whorehouse", "Cathouse",
	    "Discotheque", "Lighthouse", "Beacon", "Amphitheatre", "Colosseum",
	    "Stadium", "Arena", "Circus", "Academy", "University",
	    "Campus", "College", "Library", "Scriptorium", "Laboratory",
	    "Observatory", "Museum"  
	};

	public static string GetRandom()
	{
		Debug.Log("Name genarator not moderated, may contain offensive words :)");

		int adjectiveIndex = Random.Range(0, adjectives.Length);
		int substantiveIndex = Random.Range(0, substantives.Length);

		return $"{adjectives[adjectiveIndex]} {substantives[substantiveIndex]}";
	}
}



