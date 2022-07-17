# Seed Generation Design

This document goes over how seed generation works and some of the design decisions.

## Overview

### Step 1

- The user navigates to the seed generation page on the website.
- The user selects settings.
- The user selects Generate.
- The server adds that user's generation request to a queue.
- The user views a waiting page while they wait on the server to create the input to Step 2.
- Once the server generates the input to Step 2, the user's waiting page changes to the Seed page.

### Step 2

- The user is on the Seed page.
  - This page is the one that people will share when they want to share a seed with other players.
  - This page contains the `filename` (such as 'AngryMidna_5w9") which uniquely identifies that playthrough experience.
- The user is able to overwrite some of the settings from Step 1 (such as tunic color), but they are unable to change some settings (such as PoT access requirements).
  - The reasoning behind this is described later in this document.
- The user is also able to pick some new settings such as "I want a GCI for the PAL version".
- After picking settings, the user clicks Create.
- The server generates the GCI bytes and sends that data back to the browser.
- The browser creates a local temp file of the GCI which the user can download to their machine.
- The user clicks the new link in the UI to download the GCI file.

## Regarding settings

You can split up the settings into a few different categories:

- sSettings
  - "shared settings".
    These are things such as "PoT requirements" which are locked in during Step 1.
- pSettings
  - "personal settings".
    These are set in Step 1, but invidividual players can overwrite them in Step 2 (such as "tunic color").
- seedString
  - This is unicode string which is used to determine the starting position of the Random instances during generation.
- fileCreationSettings
  - These are passed up as part of Step 2.
    They are settings required for creating the output file which are not pSettings.
    Examples are "game region" and "seed index".

### The "settingsString"

A settings string that the user can import and export from the UI is a combination of sSettings and/or pSettings.

Here is a valid settingsStrings at the time of writing (it won't work in the future, but that isn't important for this example):

- `0sXK9MqckDZyW4ee__e0T1Vy0pX6C0OcRG`

This can be split into 2 sections:

- sSettings: `0sXK9MqckDZyW4ee__e0T1Vy`
- pSettings: `0pX6C0OcRG`

_Note: The other order is equivalent as well `0pX6C0OcRG0sXK9MqckDZyW4ee__e0T1Vy`._

_Note: The UI can import one of these at a time as well, so importing `0pX6C0OcRG` would just change the pSettings._

The internals of the strings will be documented elsewhere, but they both use the same header strategy for versioning, etc.

Let's look at the sSettings one `0sXK9MqckDZyW4ee__e0T1Vy`.

- The first `s` indicates that this is sSettings.
- The characters before the `s` are hex which indicates the sSettings format version.
  This is how we can adjust the string over time and provide backwards compatibility.
  - For example, staring with `11s` would indicate sSettings of version 0x11 or 17 decimal.

The rest of the string is composed of characters which map to 6 bits.

- For example, `kDZy` is a compressed representation of 24 bits.

We read the next character after the first `s`.

It is `X` which maps to 0b100001.

This is split into 2 sections of 3 bits, so `0b100` and `0b001`.

The lower 3 bits (0b001) indicates how many characters are used to say how long the string is.

- In this case, the value is `1`, so the next 1 character in the string says how many more characters are in the sSettings string.

The next character in the string is `K` (the one after the `X`).

- This character maps to 0b010100 which is 20 decimal.
- This means that the next 20 characters are the rest of the sSettings.
  - This checks out because `9MqckDZyW4ee__e0T1Vy` is 20 characters long.

Going back to the bits from the first character `X` that we looked at,
the upper 3 bits (0b100 or 4 decimal) indicates how many of the bits of the last character are actually part of the sSettings.

The last character in the sSettings is `y` which represents 6 bits.
However, it is very possible that the settings string's exact bit length is not a multiple of 6.

In this case, the value of 0b100 (4 decimal) indicates how many of the bits from the last character are actually used.
So in this case, the last 2 bits from the character `y` are not actually part of the sSettings, and they will be ignored.

This means that the bit length of the sSettings is (20 \* 6) - (6 - 4) or 118 bits.

Note that this approach allows for the following:

- No limit to the version number.
- The bitLength limit is 281,474,976,710,655 so basically unlimited.
