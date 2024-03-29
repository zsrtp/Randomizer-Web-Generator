namespace TPRandomizer.Hints.HintCreator
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks.Dataflow;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Newtonsoft.Json.Linq;
    using TPRandomizer.Hints;
    using TPRandomizer.Hints.Settings;
    using TPRandomizer.Util;

    public class ItemHintCreator : HintCreator
    {
        private List<Item> validItems = null;
        private bool badItemsHintable = false;
        private bool itemsOrdered = false;

        // Creates item hints with the following properties:

        // - checkName (check this hint directs toward)
        // - item (for easy lookup; not encoded)
        // - areaId (description of where to find item; cannot just be the type
        //     of area since a checkName can map to multiple categories)
        // - itemVagueness (named = 0, vague (somethingGood, still named if not
        //   somethingGood), unhelpful (an {item}))

        // Creator options:
        // - areaType(req): "zone" or "province", etc. (ex: Item X is in {Kakariko Village})
        // - validAreas(req): ["kakarikoVillage"] // required
        // - itemVagueness(optional): 'named'(default), 'vague', 'unhelpful'
        // - areaVagueness(optional): 'named', 'unhelpful'
        // - validItems(optional): ["Progressive_Clawshot", etc.] // defaults to "alias:majorItems"
        //     can have aliases: "alias:junk", "alias:majorItems"
        // - invalidItems(optional): ["Slingshot", etc.]
        // - validChecks(optional): ["Wrestling with Bo", "zone:Snowpeak", etc.]
        // - invalidChecks(optional): ["Snowpeak Freezard Grotto Chest", "zone:Ordon", "var:mostSwordsProvince.areaId"]

        // SEEMS LIKE MIGHT BE ABLE TO MERGE THIS WITH NumItemInArea HintCreator.
        // Literally the only difference is that the numberInArea one indicates the number.
        // Also the numbered one can hint like "3 major items in area".
        //   This is the same as "something good in area" for multiple.

        // They say that {Clawshot/somethingGood/redRupee/an item} can be found at {KakVil|LanayruProv|OwlStatues}.
        // They say that {Clawshot/somethingGood/redRupee/an item} can be found at {KakVil|LanayruProv|OwlStatues}.

        // Junk example: They say that a {Red Rupee} can be found at {Lakebed Temple}.
        // Multi example: They say that {2 major items} can be found at {Lanayru Spring}.
        // Note that major items should be based off of items and not "2 claws are hard required, so the 3rd
        // is not considered a major check, because if someone sees that there is 1 in an area and they find that 3rd claw,
        // they would expect that they found the 1 item and that there are no other major items there, but they would
        // be mistaken. This could lead to unhappy people.

        // Actually probably better to keep separate since:
        // - numItemInArea is an assessment that doesn't point to anything
        // - item hints especially can have a bunch of unique stuff going on:
        // - option to list an ordered list of items which get hinted
        // - For example, ["claw", "claw", "claw", "slingshot"]
        // -- This would first hint claw, then claw, then 3rd would fail, so move on to slingshot.
        // -- Then it would keep track of its own cacheState for that node ("0.1.3"), and once
        // -- it made it through the list then it would never product any more hints.

        private ItemHintCreator()
        {
            this.type = HintCreatorType.Item;
        }

        new public static ItemHintCreator fromJObject(JObject obj)
        {
            ItemHintCreator inst = new ItemHintCreator();

            if (obj.ContainsKey("options"))
            {
                JObject options = (JObject)obj["options"];

                List<string> validItemsStrList = HintSettingUtils.getOptionalStringList(
                    options,
                    "validItems",
                    null
                );
                if (!ListUtils.isEmpty(validItemsStrList))
                {
                    inst.validItems = new();

                    foreach (string itemStr in validItemsStrList)
                    {
                        if (itemStr.StartsWith("alias:"))
                        {
                            string alias = itemStr.Substring(6);
                            HashSet<Item> resolved = resolveItemsAlias(alias);
                            inst.validItems.AddRange(resolved);
                        }
                        else
                        {
                            Item item = HintSettingUtils.parseItem(itemStr);
                            inst.validItems.Add(item);
                        }
                    }

                    // If user specifies validItems, by default treat any item
                    // they specify as hintable (even if they say "Purple_Rupee"
                    // for example).
                    if (!ListUtils.isEmpty(inst.validItems))
                        inst.badItemsHintable = true;
                }

                inst.badItemsHintable = HintSettingUtils.getOptionalBool(
                    options,
                    "badItemsHintable",
                    inst.badItemsHintable
                );

                inst.itemsOrdered = HintSettingUtils.getOptionalBool(
                    options,
                    "itemsOrdered",
                    inst.itemsOrdered
                );
            }

            // TODO: load options and use them.
            return inst;
        }

        public override List<Hint> tryCreateHint(
            HintGenData genData,
            HintSettings hintSettings,
            int numHints,
            HintGenCache cache
        )
        {
            if (numHints < 1)
                return null;

            HashSet<Item> validItemsSet;
            if (!ListUtils.isEmpty(validItems))
            {
                validItemsSet = new();
                foreach (Item item in validItems)
                {
                    validItemsSet.Add(item);
                }
            }
            else
            {
                validItemsSet = new()
                {
                    Item.Progressive_Clawshot,
                    Item.Progressive_Dominion_Rod,
                    Item.Ball_and_Chain,
                    Item.Spinner,
                    Item.Progressive_Bow,
                    Item.Iron_Boots,
                    Item.Boomerang,
                    Item.Lantern,
                    Item.Progressive_Fishing_Rod,
                    Item.Filled_Bomb_Bag,
                    Item.Aurus_Memo,
                };
            }

            Dictionary<Item, HashSet<string>> itemToHintableChecks = new();

            foreach (KeyValuePair<string, Check> pair in Randomizer.Checks.CheckDict)
            {
                string checkName = pair.Value.checkName;
                Item item = HintUtils.getCheckContents(checkName);
                if (
                    validItemsSet.Contains(item)
                    && (badItemsHintable || genData.CheckIsGood(checkName))
                    && CheckIsItemHintable(genData, checkName)
                )
                {
                    if (!itemToHintableChecks.ContainsKey(item))
                        itemToHintableChecks[item] = new();
                    itemToHintableChecks[item].Add(checkName);
                }
            }

            List<Hint> results = new();
            bool pickfromOrderedList = itemsOrdered && !ListUtils.isEmpty(validItems);

            for (int i = 0; i < numHints; i++)
            {
                if (itemToHintableChecks.Count < 1)
                    break;

                Item selectedItem;
                if (pickfromOrderedList)
                {
                    // Break once reach the end of the list.
                    if (i >= validItems.Count)
                        break;
                    selectedItem = validItems[i];
                }
                else
                {
                    // Randomly pick item first so items with multiple copies are
                    // not way more likely than unique items (3 Bows vs 1 Lantern).
                    KeyValuePair<Item, HashSet<string>> pair = HintUtils.PickRandomDictionaryPair(
                        genData.rnd,
                        itemToHintableChecks
                    );
                    selectedItem = pair.Key;
                }

                // If there are no checks for the selectedItem, then skip. This
                // could happen if the user specifies an item which is rewarded
                // by 0 checks or picks an item which has a single copy multiple
                // times.
                if (!itemToHintableChecks.ContainsKey(selectedItem))
                    continue;

                HashSet<string> possibleChecks = itemToHintableChecks[selectedItem];
                if (ListUtils.isEmpty(possibleChecks))
                    continue;

                List<KeyValuePair<double, string>> weightedList = new();
                foreach (string checkName in possibleChecks)
                {
                    // Slightly prefer hinting non-sphere0 checks since these
                    // are more interesting.
                    double weight = genData.isCheckSphere0(checkName) ? 1 : 1.5;
                    weightedList.Add(new(weight, checkName));
                }

                VoseInstance<string> voseInst = VoseAlgorithm.createInstance(weightedList);
                string selectedCheckName = voseInst.NextAndKeep(genData.rnd);

                possibleChecks.Remove(selectedCheckName);
                if (possibleChecks.Count < 1)
                    itemToHintableChecks.Remove(selectedItem);

                AreaId areaId = genData.GetRecommendedAreaId(selectedCheckName);

                ItemHint hint = new ItemHint(areaId, selectedCheckName, selectedItem, true);
                results.Add(hint);

                // Update hinted
                genData.hinted.alreadyCheckDirectedToward.Add(selectedCheckName);
            }

            return results;
        }

        private bool CheckIsItemHintable(HintGenData genData, string checkName)
        {
            HintedThings3 hinted = genData.hinted;

            return !genData.CheckShouldBeIgnored(checkName)
                && !hinted.alreadyCheckContentsHinted.Contains(checkName)
                && !hinted.alreadyCheckDirectedToward.Contains(checkName)
                && !hinted.alreadyCheckKnownBarren.Contains(checkName);
        }

        private static HashSet<Item> resolveItemsAlias(string alias)
        {
            switch (alias)
            {
                case "rupees":
                    return new()
                    {
                        Item.Green_Rupee,
                        Item.Blue_Rupee,
                        Item.Yellow_Rupee,
                        Item.Red_Rupee,
                        Item.Purple_Rupee,
                        Item.Orange_Rupee,
                        Item.Silver_Rupee,
                        Item.Purple_Rupee_Links_House,
                    };
                case "junk":
                    return new(Randomizer.Items.vanillaJunkItems);
                default:
                    throw new Exception($"Failed to resolve alias '{alias}'.");
            }
        }
    }
}
