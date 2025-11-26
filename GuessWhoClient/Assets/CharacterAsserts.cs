using System.Collections.Generic;

namespace GuessWhoClient.Assets
{
    public static class CharacterAssets
    {
        private const string CHARACTERS_BASE_PATH = "/Images/Characters/";

        private static readonly string[] CharacterPaths =
        {
            CHARACTERS_BASE_PATH + "Mazo1_Adonis_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Alberto_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Alejandro_Animado.jpg",
            CHARACTERS_BASE_PATH + "Mazo1_Alexander_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Austin_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Beatriz_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Claudia_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Edward_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Genaro_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Giovanni_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Hailee_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Jaime_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Jose_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Josh_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Kai_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Kenia_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Laura_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Lia_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Lucas_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Luis_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Luisa_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Margarita_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Mauricio_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Nadia_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Omar_Animado.jpg",
            CHARACTERS_BASE_PATH + "Mazo1_Pablo_Animado.jpg",
            CHARACTERS_BASE_PATH + "Mazo1_Rhodey_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Ricardo_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Rodrigo_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Samuel_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Shai_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Terry_Animado.png",
            CHARACTERS_BASE_PATH + "Mazo1_Will_Animado.png",
        };

        public static IReadOnlyList<string> GetAllCharacterPaths()
        {
            return CharacterPaths;
        }
    }
}
