using System.Collections.Generic;

namespace GuessWhoClient.Assets
{
    public static class CharacterAssets
    {
        private const string CHARACTERS_BASE_PATH = "/Images/Characters/";

        private static readonly Dictionary<string, string> charactersById =
            new Dictionary<string, string>
            {
                ["A0001"] = CHARACTERS_BASE_PATH + "Mazo1_Adonis_Animado.png",
                ["A0002"] = CHARACTERS_BASE_PATH + "Mazo1_Adriana_Animado.png",
                ["A0003"] = CHARACTERS_BASE_PATH + "Mazo1_Alberto_Animado.png",
                ["A0004"] = CHARACTERS_BASE_PATH + "Mazo1_Alejandro_Animado.jpg",
                ["A0005"] = CHARACTERS_BASE_PATH + "Mazo1_Alexander_Animado.png",
                ["A0006"] = CHARACTERS_BASE_PATH + "Mazo1_Alicia_Animado.png",
                ["A0007"] = CHARACTERS_BASE_PATH + "Mazo1_Ana_Animado.png",
                ["A0008"] = CHARACTERS_BASE_PATH + "Mazo1_Austin_Animado.png",
                ["A0009"] = CHARACTERS_BASE_PATH + "Mazo1_Beatriz_Animado.png",
                ["A0010"] = CHARACTERS_BASE_PATH + "Mazo1_Caleb_Animado.png",
                ["A0011"] = CHARACTERS_BASE_PATH + "Mazo1_Camila_Animado.png",
                ["A0012"] = CHARACTERS_BASE_PATH + "Mazo1_Carolina_Animado.png",
                ["A0013"] = CHARACTERS_BASE_PATH + "Mazo1_Charles_Animado.png",
                ["A0014"] = CHARACTERS_BASE_PATH + "Mazo1_Charlotte_Animado.png",
                ["A0015"] = CHARACTERS_BASE_PATH + "Mazo1_Claudia_Animado.png",
                ["A0016"] = CHARACTERS_BASE_PATH + "Mazo1_Daniel_Animado.png",
                ["A0017"] = CHARACTERS_BASE_PATH + "Mazo1_Edward_Animado.png",
                ["A0018"] = CHARACTERS_BASE_PATH + "Mazo1_Elena_Animado.png",
                ["A0019"] = CHARACTERS_BASE_PATH + "Mazo1_Emma_Animado.png",
                ["A0020"] = CHARACTERS_BASE_PATH + "Mazo1_Genaro_Animado.png",
                ["A0021"] = CHARACTERS_BASE_PATH + "Mazo1_Giovanni_Animado.png",
                ["A0022"] = CHARACTERS_BASE_PATH + "Mazo1_Hailee_Animado.png",
                ["A0023"] = CHARACTERS_BASE_PATH + "Mazo1_Isis_Animado.png",
                ["A0024"] = CHARACTERS_BASE_PATH + "Mazo1_Jaime_Animado.png",
                ["A0025"] = CHARACTERS_BASE_PATH + "Mazo1_Jose_Animado.png",
                ["A0026"] = CHARACTERS_BASE_PATH + "Mazo1_Josh_Animado.png",
                ["A0027"] = CHARACTERS_BASE_PATH + "Mazo1_Kai_Animado.png",
                ["A0028"] = CHARACTERS_BASE_PATH + "Mazo1_Karen_Animado.png",
                ["A0029"] = CHARACTERS_BASE_PATH + "Mazo1_Kenia_Animado.png",
                ["A0030"] = CHARACTERS_BASE_PATH + "Mazo1_Laura_Animado.png",
                ["A0031"] = CHARACTERS_BASE_PATH + "Mazo1_Lia_Animado.png",
                ["A0032"] = CHARACTERS_BASE_PATH + "Mazo1_Lois_Animado.png",
                ["A0033"] = CHARACTERS_BASE_PATH + "Mazo1_Lucas_Animado.png",
                ["A0034"] = CHARACTERS_BASE_PATH + "Mazo1_Lucia_Animado.png",
                ["A0035"] = CHARACTERS_BASE_PATH + "Mazo1_Luis_Animado.png",
                ["A0036"] = CHARACTERS_BASE_PATH + "Mazo1_Luisa_Animado.png",
                ["A0037"] = CHARACTERS_BASE_PATH + "Mazo1_Margarita_Animado.png",
                ["A0038"] = CHARACTERS_BASE_PATH + "Mazo1_Mauricio_Animado.png",
                ["A0039"] = CHARACTERS_BASE_PATH + "Mazo1_Max_Animado.png",
                ["A0040"] = CHARACTERS_BASE_PATH + "Mazo1_Maya_Animado.png",
                ["A0041"] = CHARACTERS_BASE_PATH + "Mazo1_Nadia_Animado.png",
                ["A0042"] = CHARACTERS_BASE_PATH + "Mazo1_Nathaly_Animado.png",
                ["A0043"] = CHARACTERS_BASE_PATH + "Mazo1_Omar_Animado.jpg",
                ["A0044"] = CHARACTERS_BASE_PATH + "Mazo1_Pablo_Animado.jpg",
                ["A0045"] = CHARACTERS_BASE_PATH + "Mazo1_Ricardo_Animado.png",
                ["A0046"] = CHARACTERS_BASE_PATH + "Mazo1_Rhodey_Animado.png",
                ["A0047"] = CHARACTERS_BASE_PATH + "Mazo1_Rick_Animado.png",
                ["A0048"] = CHARACTERS_BASE_PATH + "Mazo1_Rodrigo_Animado.png",
                ["A0049"] = CHARACTERS_BASE_PATH + "Mazo1_Samuel_Animado.png",
                ["A0050"] = CHARACTERS_BASE_PATH + "Mazo1_Sara_Animado.png",
                ["A0051"] = CHARACTERS_BASE_PATH + "Mazo1_Sergio_Animado.png",
                ["A0052"] = CHARACTERS_BASE_PATH + "Mazo1_Shai_Animado.png",
                ["A0053"] = CHARACTERS_BASE_PATH + "Mazo1_Shawn_Animado.png",
                ["A0054"] = CHARACTERS_BASE_PATH + "Mazo1_Sofia_Animado.png",
                ["A0055"] = CHARACTERS_BASE_PATH + "Mazo1_Sonia_Animado.png",
                ["A0056"] = CHARACTERS_BASE_PATH + "Mazo1_Tom_Animado.png",
                ["A0057"] = CHARACTERS_BASE_PATH + "Mazo1_Trae_Animado.png",
                ["A0058"] = CHARACTERS_BASE_PATH + "Mazo1_Tyrese_Animado.png",
                ["A0059"] = CHARACTERS_BASE_PATH + "Mazo1_Wilma_Animado.png",
                ["A0060"] = CHARACTERS_BASE_PATH + "Mazo1_Luke_Animado.png"
            };

        public static string GetCharacterPathById(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            if (charactersById.TryGetValue(characterId, out var characterPath))
            {
                return characterPath;
            }

            return null;
        }
    }
}
