namespace TPRandomizer.Assets.CLR0
{
    using System;
    using System.Collections.Generic;

    class Clr0EntryComparer : IComparer<Clr0Entry>
    {
        int IComparer<Clr0Entry>.Compare(Clr0Entry a, Clr0Entry b)
        {
            // Cast to `int` so we can get negative results.
            return (int)a.recolorId - (int)b.recolorId;
        }
    }

    public class CLR0
    {
        public static List<byte> BuildClr0(FileCreationSettings fcSettings)
        {
            // CLR0 section is set up as the following:
            // CLR0Header
            // BMDEntryList
            // TextureEntryList
            // RawRGBList
            List<CMPRTextureEntry> cmprTextureList = new();
            List<uint> rawRGBList = new();
            List<byte> clr0Raw = new();
            List<byte> rawRGBRaw = new();
            List<byte> bmdListRaw = new();
            List<byte> textureListRaw = new();
            List<BmdTextureAssociation> textureAssociations = new();
            int clr0HeaderSize = 0xC;
            List<CMPRTextureFileSettings> cmprFileModifications = new();
            List<Clr0Entry> entries = new();

            // Create any CMPR texture associations right here.

            // Link
            // Hero's Tunic
            cmprFileModifications.Add(
                new(
                    fcSettings.hTunicSkirtColor,
                    "al.bmd",
                    "al_lowbody",
                    (byte)ArchiveIndex.Link,
                    BMDDirectory.BMWR
                )
            );
            cmprFileModifications.Add(
                new(
                    fcSettings.hTunicBodyColor,
                    "al.bmd",
                    "al_upbody",
                    (byte)ArchiveIndex.Link,
                    BMDDirectory.BMWR
                )
            );
            cmprFileModifications.Add(
                new(
                    fcSettings.hTunicHatColor,
                    "al_head.bmd",
                    "al_cap",
                    (byte)ArchiveIndex.Link,
                    BMDDirectory.BMWR
                )
            );

            cmprFileModifications.Add(
                new(
                    fcSettings.linkHairColor,
                    "al_head.bmd",
                    "al_hair",
                    (byte)ArchiveIndex.Link,
                    BMDDirectory.BMWR
                )
            );

            // Zora Armor
            cmprFileModifications.Add(
                new(
                    fcSettings.zTunicHatColor,
                    "zl_head.bmd",
                    "zl_cap",
                    (byte)ArchiveIndex.ZoraArmor,
                    BMDDirectory.BMWR
                )
            );

            cmprFileModifications.Add(
                new(
                    fcSettings.zTunicHelmetColor,
                    "zl_head.bmd",
                    "zl_helmet",
                    (byte)ArchiveIndex.ZoraArmor,
                    BMDDirectory.BMWR
                )
            );
            cmprFileModifications.Add(
                new(
                    fcSettings.zTunicBodyColor,
                    "zl.bmd",
                    "zl_armor",
                    (byte)ArchiveIndex.ZoraArmor,
                    BMDDirectory.BMWR
                )
            );
            cmprFileModifications.Add(
                new(
                    fcSettings.zTunicScalesColor,
                    "zl.bmd",
                    "zl_body",
                    (byte)ArchiveIndex.ZoraArmor,
                    BMDDirectory.BMWR
                )
            );
            cmprFileModifications.Add(
                new(
                    fcSettings.zTunicBootsColor,
                    "zl.bmd",
                    "zl_boots",
                    (byte)ArchiveIndex.ZoraArmor,
                    BMDDirectory.BMWR
                )
            );
            cmprFileModifications.Add(
                new(
                    fcSettings.zTunicBodyColor,
                    "zl.bmd",
                    "zl_armL",
                    (byte)ArchiveIndex.ZoraArmor,
                    BMDDirectory.BMWR
                )
            );

            cmprFileModifications.Add(
                new(
                    fcSettings.zTunicBodyColor,
                    "o_gd_al_zora.bmd",
                    "zl_armor",
                    (byte)ArchiveIndex.ZoraArmorField,
                    BMDDirectory.BMDR
                )
            );

            cmprFileModifications.Add(
                new(
                    fcSettings.zTunicScalesColor,
                    "o_gd_al_zora.bmd",
                    "zl_body",
                    (byte)ArchiveIndex.ZoraArmorField,
                    BMDDirectory.BMDR
                )
            );

            cmprFileModifications.Add(
                new(
                    fcSettings.zTunicHelmetColor,
                    "o_gd_al_zora.bmd",
                    "zl_helmet",
                    (byte)ArchiveIndex.ZoraArmorField,
                    BMDDirectory.BMDR
                )
            );

            cmprFileModifications.Add(
                new(
                    fcSettings.zTunicHatColor,
                    "o_gd_al_zora.bmd",
                    "zl_cap",
                    (byte)ArchiveIndex.ZoraArmorField,
                    BMDDirectory.BMDR
                )
            );

            cmprFileModifications.Add(
                new(
                    fcSettings.msBladeColor,
                    "al_swm.bmd",
                    "al_SWM",
                    (byte)ArchiveIndex.ALink,
                    BMDDirectory.BMWE
                )
            );

            cmprFileModifications.Add(
                new(
                    fcSettings.msHandleColor,
                    "al_swm.bmd",
                    "al_SWgripM",
                    (byte)ArchiveIndex.ALink,
                    BMDDirectory.BMWE
                )
            );

            cmprFileModifications.Add(
                new(
                    fcSettings.boomerangColor,
                    "al_boom.bmd",
                    "L_al_boom00",
                    (byte)ArchiveIndex.ALink,
                    BMDDirectory.BMDR
                )
            );

            // Iron Boots
            cmprFileModifications.Add(
                new(
                    fcSettings.ironsColor,
                    "al_bootsh.bmd",
                    "al_bootsH",
                    (byte)ArchiveIndex.Link,
                    BMDDirectory.BMWR
                )
            );

            cmprFileModifications.Add(
                new(
                    fcSettings.ironsColor,
                    "al_bootsh.bmd",
                    "al_bootsH",
                    (byte)ArchiveIndex.ZoraArmor,
                    BMDDirectory.BMWR
                )
            );

            cmprFileModifications.Add(
                new(
                    fcSettings.ironsColor,
                    "al_bootsh.bmd",
                    "al_bootsH",
                    (byte)ArchiveIndex.MagicArmor,
                    BMDDirectory.BMWR
                )
            );

            // Spinner
            cmprFileModifications.Add(
                new(
                    fcSettings.spinnerColor,
                    "al_sp.bmd",
                    "al_SP",
                    (byte)ArchiveIndex.ALink,
                    BMDDirectory.BMDR
                )
            );

            cmprFileModifications.Add(
                new(
                    fcSettings.woodSwordColor,
                    "al_swb.bmd",
                    "al_SWB",
                    (byte)ArchiveIndex.Link,
                    BMDDirectory.BMWR
                )
            );
            cmprFileModifications.Add(
                new(
                    fcSettings.woodSwordColor,
                    "al_swb.bmd",
                    "al_SWB",
                    (byte)ArchiveIndex.ZoraArmor,
                    BMDDirectory.BMWR
                )
            );
            cmprFileModifications.Add(
                new(
                    fcSettings.woodSwordColor,
                    "al_swb.bmd",
                    "al_SWB",
                    (byte)ArchiveIndex.MagicArmor,
                    BMDDirectory.BMWR
                )
            );

            cmprFileModifications.Add(
                new(
                    fcSettings.eponaColor,
                    "hs.bmd",
                    "hs_body",
                    (byte)ArchiveIndex.Horse,
                    BMDDirectory.BMDR
                )
            );

            cmprFileModifications.Add(
                new(
                    fcSettings.wolfColor,
                    "wl.bmd",
                    "wl_body",
                    (byte)ArchiveIndex.Wolf,
                    BMDDirectory.BMWR
                )
            );

            entries.Add(fcSettings.hTunicHatColor);
            entries.Add(fcSettings.hTunicBodyColor);
            entries.Add(fcSettings.hTunicSkirtColor);
            entries.Add(fcSettings.zTunicHatColor);
            entries.Add(fcSettings.zTunicHelmetColor);
            entries.Add(fcSettings.zTunicBodyColor);
            entries.Add(fcSettings.zTunicScalesColor);
            entries.Add(fcSettings.zTunicBootsColor);
            entries.Add(fcSettings.msBladeColor);
            entries.Add(fcSettings.msHandleColor);
            entries.Add(fcSettings.boomerangColor);
            entries.Add(fcSettings.ironsColor);
            entries.Add(fcSettings.spinnerColor);
            entries.Add(fcSettings.woodSwordColor);
            entries.Add(fcSettings.eponaColor);
            entries.Add(fcSettings.wolfColor);
            entries.Add(fcSettings.lanternGlowColor);
            entries.Add(fcSettings.heartColor);
            entries.Add(fcSettings.aBtnColor);
            entries.Add(fcSettings.bBtnColor);
            entries.Add(fcSettings.xBtnColor);
            entries.Add(fcSettings.yBtnColor);
            entries.Add(fcSettings.zBtnColor);
            entries.Add(fcSettings.midnaDomeRingColor);
            entries.Add(fcSettings.linkHairColor);

            foreach (Clr0Entry entry in entries)
            {
                if (entry != null)
                {
                    switch (entry.recolorId)
                    {
                        case RecolorId.None:
                        {
                            Clr0Result result = entry.getResult();
                            rawRGBList.Add(((result.basicDataEntry << 0x8) | 0xFF));
                            break;
                        }
                        case RecolorId.CMPR:
                        {
                            foreach (CMPRTextureFileSettings modification in cmprFileModifications)
                            {
                                if (modification.colorEntry != null)
                                {
                                    if (modification.colorEntry == entry)
                                    {
                                        if (textureAssociations.Count > 0)
                                        {
                                            bool isInList = false;
                                            foreach (
                                                BmdTextureAssociation association in textureAssociations
                                            )
                                            {
                                                // Some archives use the same internal filenames for textures so we want to discern based on the archive index as well.
                                                if (
                                                    (modification.bmdFile == association.bmdFile)
                                                    && (
                                                        modification.archiveIndex
                                                        == association.archiveIndex
                                                    )
                                                )
                                                {
                                                    association.textures.Add(modification.texture);
                                                    isInList = true;
                                                    break;
                                                }
                                            }
                                            if (!isInList)
                                            {
                                                BmdTextureAssociation newAssociation =
                                                    new(
                                                        (byte)RecolorId.CMPR,
                                                        modification.bmdFile,
                                                        new List<string>(),
                                                        modification.archiveIndex,
                                                        modification.directory
                                                    );
                                                newAssociation.textures.Add(modification.texture);
                                                textureAssociations.Add(newAssociation);
                                            }
                                        }
                                        else
                                        {
                                            BmdTextureAssociation newAssociation =
                                                new(
                                                    (byte)RecolorId.CMPR,
                                                    modification.bmdFile,
                                                    new List<string>(),
                                                    modification.archiveIndex,
                                                    modification.directory
                                                );
                                            newAssociation.textures.Add(modification.texture);
                                            textureAssociations.Add(newAssociation);
                                        }

                                        Clr0Result result = entry.getResult();
                                        cmprTextureList.Add(
                                            new(result.basicDataEntry, modification.texture)
                                        );
                                    }
                                }
                            }
                            break;
                        }
                        default:
                        {
                            Console.WriteLine("Invalid recolor id: " + entry.recolorId);
                            break;
                        }
                    }
                }
            }

            foreach (BmdTextureAssociation association in textureAssociations)
            {
                bmdListRaw.Add(Converter.GcByte(association.recolorType));
                bmdListRaw.Add(Converter.GcByte(association.archiveIndex));
                bmdListRaw.Add(Converter.GcByte((byte)association.directory));
                bmdListRaw.Add(Converter.GcByte(association.textures.Count));
                bmdListRaw.AddRange(
                    Converter.GcBytes(
                        (UInt16)(
                            (textureAssociations.Count * 0x18)
                            + clr0HeaderSize
                            + textureListRaw.Count
                        )
                    )
                );
                bmdListRaw.AddRange(Converter.StringBytes(association.bmdFile, 0x12));

                // We want to handle CMPR textures first since they go before any other formats.
                foreach (string texture in association.textures)
                {
                    foreach (CMPRTextureFileSettings modification in cmprFileModifications)
                    {
                        if (
                            (modification.texture == texture)
                            && (modification.archiveIndex == association.archiveIndex)
                        )
                        {
                            Clr0Result result = modification.colorEntry.getResult();
                            textureListRaw.AddRange(
                                Converter.GcBytes((UInt32)((result.basicDataEntry << 0x8) | 0xFF))
                            );
                            textureListRaw.AddRange(Converter.StringBytes(texture, 0xC));
                        }
                    }
                }
                //.AddRange(Converter.GcBytes((UInt16)association.recolorType));
            }

            // Generate a list of the raw RGB values that will be used. The data associations should be static, meaning that they should always be in the same order.
            foreach (uint rawRGB in rawRGBList)
            {
                rawRGBRaw.AddRange(Converter.GcBytes((UInt32)rawRGB));
            }

            // Now that all of our lists are populated with the appropriate data. It is time to build the header and list contents with the dynamic data
            clr0Raw.AddRange( // size of the CLR0 chunk
                Converter.GcBytes(
                    (UInt32)(
                        clr0HeaderSize + bmdListRaw.Count + textureListRaw.Count + rawRGBList.Count
                    )
                )
            );
            clr0Raw.AddRange(Converter.GcBytes((UInt32)textureAssociations.Count)); // number of Bmd entries
            clr0Raw.AddRange(Converter.GcBytes((UInt16)clr0HeaderSize)); // offset to the list of bmd entries. It always follows the CLR0 header.
            clr0Raw.AddRange(
                Converter.GcBytes(
                    (UInt16)(clr0HeaderSize + bmdListRaw.Count + textureListRaw.Count)
                )
            ); // offset to raw rgb table.
            clr0Raw.AddRange(bmdListRaw);
            clr0Raw.AddRange(textureListRaw);
            clr0Raw.AddRange(rawRGBRaw);

            return clr0Raw;
        }
    }
}
