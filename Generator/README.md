# Twilight Princess Randomizer Seed Generator Proof-of-Concept (GUI)

# Logic Writing Guide:
The logic for the randomizer is supported by a custom designed backend for easy understanding, implementation, and collaboration. Below are a list of "tokens" that are used when defining logic:
* `!` is a negation token, which refers to the inverse of the token that is to follow it.
* `(` and `)` are open parenthesis tokens. You must always have a closing parenthesis for every open in parenthesis.
* `, #` is a comma token and is always followed by a number. This token is commonly used when grouping items together and used for checking quantity.
* `True` is an affirmation token. If a check has no requirements then this can be used as a default. When comparing to settings, the "T" in true must be capitalized
* `False` is a negation token. Similar to `!`, this token refers to the inverse of the token that preceeded it. When comparing to settings, the "F" in false must be capialized.
* `and` is an inclusion token. Use this if you want to ensure that multiple values meet their respective requirements before returning a result.
* `or` is an inclusion token. Use this  if you have multiple values but only care about one of them meeting their respective requirements.
* `equals` is an assignment token. This can be used when you want to ensure that an object has a certain value before returning a result.

## Special Tokens
There are a number of special tokens that are used when defining logic that can be used in the randomizer.
* Item tokens are just the enum name that item IDs use for the randomizer. (see /Assets/Items.cs for a list of enum definitions)
* Setting tokens are used to identify certain settings and perform checks on their values. These tokens must always contain `Setting.`
* Room tokens are used to identify if the player has reached a certain room. These tokens must always contain `Room.`
* Any other text that is not defined as a token listed above is listed as a `Function` token. This means that the randomizer will identify it as a macro for a function and try to run it as such. Unfortunately the logic does not support line breaks at the moment.

# Logic Writing Demonstration and Translation
Using the above notes, we can now write logic for a room or check in a token form. This section breaks down the tokens into a raw code format so that you understand how it looks to the frontend.

Let's say that a check we want to write logic for requires the Lantern. The logic for the check would look like this: 
```
Lantern
```
This is valid logic because `Lantern` is an Item token. In terms of raw code, it can be translated to the following: `canUse(Item.Lantern)`

To extend on this, now let's say we did some testing and found out that the check also requires two sword upgrades. The logic would now change to this:
```
(Progressive_Sword, 2) and Lantern
```
The raw code would translate to `verifyItemQuantity(Item.Progressive_Sword, 2) && canUse(Item.Lantern`.

To take it to a much farther step. We now want the check to have the following requirements to be available:
* 3 Swords
* Lantern
* the ability to access and defeat Diababa
* Clawshot or Boomerang
* The Faron Woods setting to be set to "closed"

After using the above guide and looking at function, item, room, and setting definitions: we would come up with the following:
```
(Progressive_Sword, 3) and Lantern and (Room.Diababa and CanDefeatDiababa) and ((Progressive_Clawshot, 1) or Boomerang) and Setting.faronWoodsLogic equals Closed
```
If you have any other questions about writing logic, please ask in the discord or message me personally 
- Lunar Soap
