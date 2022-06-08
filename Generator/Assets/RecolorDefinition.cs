namespace TPRandomizer
{
    using System.Collections.Generic;
    using System.Linq;

    public enum RecolorId : ushort
    {
        HerosClothes = 0x00, // Cap and Body
        ZoraArmorPrimary = 0x01,
        ZoraArmorSecondary = 0x02,
        ZoraArmorHelmet = 0x03,
    };

    public class RecolorDefinition
    {
        public RecolorId recolorId { get; }
        public List<byte> rgb { get; set; }

        public RecolorDefinition(RecolorId recolorId)
        {
            this.recolorId = recolorId;
        }
    }
}
