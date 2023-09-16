(function () {
  const $ = window.$;

  const RawSettingType = {
    nineBitWithEndOfListPadding: 'nineBitWithEndOfListPadding',
    bitString: 'bitString',
    xBitNum: 'xBitNum',
  };

  const RecolorId = {
    herosClothes: 0x00, // Cap and Body
    zoraArmorPrimary: 0x01,
    zoraArmorSecondary: 0x02,
    zoraArmorHelmet: 0x03,
  };

  const RecolorDefType = {
    twentyFourBitRgb: 0b0,
    randomAnyPalette: 0b1,
    randomByMath: 0b10,
    paletteEntry: 0b11,
    randomWithinPalette: 0b100,
  };

  const encodingChars =
    '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_';

  function encodeBits(bitString) {
    const missingChars = (6 - (bitString.length % 6)) % 6;
    bitString += '0'.repeat(missingChars);

    let charString = '';

    let index = 0;
    while (index < bitString.length) {
      const bits = bitString.substring(index, index + 6);
      const charIndex = parseInt(bits, 2);
      charString += encodingChars[charIndex];

      index += 6;
    }

    return charString;
  }

  function sixBitCharsToBitString(sixBitChars) {
    if (!sixBitChars) {
      return '';
    }

    let result = '';

    for (let i = 0; i < sixBitChars.length; i++) {
      result += numToPaddedBits(encodingChars.indexOf(sixBitChars[i]), 6);
    }

    return result;
  }

  function genEncodeAsVlqX(numBitDef) {
    if (numBitDef < 3 || numBitDef > 4) {
      throw new Error(`Only valid for 3 or 4; provided ${numBitDef}`);
    }

    const xBitNumber = Math.pow(2, numBitDef);
    const max = (1 << xBitNumber) - 1;

    return function encodeAsVlqX(num) {
      if (num < 0 || num > max) {
        throw new Error(`Given num ${num} is outside of range 0-${max}.`);
      }

      if (num < 2) {
        return '0'.repeat(numBitDef) + num;
      }

      let bitsNeeded = 1;
      for (let i = 2; i <= xBitNumber; i++) {
        const oneOverMax = 1 << i;
        if (num < oneOverMax) {
          bitsNeeded = i - 1;
          break;
        }
      }

      return (
        numToPaddedBits(bitsNeeded, numBitDef) + num.toString(2).substring(1)
      );
    };
  }

  function genDecodeVlqX(numBitDef) {
    if (numBitDef < 3 || numBitDef > 4) {
      throw new Error(`Only valid for 3 or 4; provided ${numBitDef}`);
    }

    return function (str) {
      const bitsToRead = parseInt(str.substring(0, numBitDef), 2);
      if (bitsToRead === 0) {
        return parseInt(str.substring(numBitDef, numBitDef + 1), 2);
      }

      return (
        (1 << bitsToRead) +
        parseInt(str.substring(numBitDef, numBitDef + bitsToRead), 2)
      );
    };
  }

  const encodeAsVlq8 = genEncodeAsVlqX(3);
  const decodeVlq8 = genDecodeVlqX(3);

  const encodeAsVlq16 = genEncodeAsVlqX(4);
  const decodeVlq16 = genDecodeVlqX(4);

  /**
   * Adds '0' chars to front of bitStr such that the returned string length is
   * equal to the provided size. If bitStr.length > size, simply returns bitStr.
   *
   * @param {string} str String of '0' and '1' chars
   * @param {number} size Desired length of resultant string.
   * @return {string}
   */
  function padBits2(str, size) {
    const numZeroesToAdd = size - str.length;
    if (numZeroesToAdd <= 0) {
      return str;
    }

    let ret = '';
    for (let i = 0; i < numZeroesToAdd; i++) {
      ret += '0';
    }
    return ret + str;
  }

  /**
   * Converts a number to a bit string of a specified length.
   *
   * @param {number} number Number to convert to bit string.
   * @param {number} strLength Length of string to return.
   * @return {string} Bit string of specified length.
   */
  function numToPaddedBits(number, strLength) {
    return padBits2(number.toString(2), strLength);
  }

  function encodeSettings(version, idChar, valuesArr) {
    let bitString = '';

    valuesArr.forEach((value) => {
      if (typeof value === 'boolean') {
        bitString += value ? '1' : '0';
      } else if (typeof value === 'string') {
        let asNum = parseInt(value, 10);
        if (Number.isNaN(asNum)) {
          asNum = 0;
        }
        bitString += numToPaddedBits(asNum, 4);
      } else if (typeof value === 'object') {
        if (value === null) {
          // triple-equals here is intentional for now
          bitString += '0';
        } else if (value.type === RawSettingType.bitString) {
          bitString += value.bitString;
        } else if (value.type === RawSettingType.xBitNum) {
          bitString += numToPaddedBits(value.value, value.bitLength);
        }
      }
    });

    const extraBits = bitString.length % 6;
    const bitsAsChars = encodeBits(bitString);
    const ver = version.toString(16);

    let numLengthChars = 0;
    for (let i = 1; i < 6; i++) {
      const maxNum = 1 << (i * 6);
      if (bitsAsChars.length <= maxNum) {
        numLengthChars = i;
        break;
      }
    }

    const lengthChar = encodeBits(
      numToPaddedBits((extraBits << 3) + numLengthChars, 6)
    );
    const lenChars = encodeBits(
      numToPaddedBits(bitsAsChars.length, numLengthChars * 6)
    );

    return ver + idChar + lengthChar + lenChars + bitsAsChars;
  }

  function genStartingItemsBits() {
    let bits = '';

    $('#baseImportantItemsListbox')
      .find('input[type="checkbox"]')
      .each(function () {
        if ($(this).prop('checked')) {
          const itemId = parseInt($(this).attr('data-itemId'), 10);
          bits += numToPaddedBits(itemId, 9);
        }
      });

    bits += '111111111';

    return {
      type: RawSettingType.bitString,
      bitString: bits,
    };
  }

  function genExcludedChecksBits() {
    let bits = '';

    $('#baseExcludedChecksListbox')
      .find('input[type="checkbox"]')
      .each(function () {
        if ($(this).prop('checked')) {
          const itemId = parseInt($(this).attr('data-checkId'), 10);
          bits += numToPaddedBits(itemId, 9);
        }
      });

    bits += '111111111';

    return {
      type: RawSettingType.bitString,
      bitString: bits,
    };
  }

  function getVal(id) {
    const $el = $('#' + id);
    if ($el.prop('nodeName') === 'INPUT' && $el.attr('type') === 'checkbox') {
      return $el.prop('checked');
    }

    return $el.val();
  }

  function isEnumWithinRange(maxBits, enumVal) {
    if (maxBits > 30) {
      return false;
    }
    const val = parseInt(enumVal, 10);
    if (Number.isNaN(val)) {
      return false;
    }

    const maxValPlusOne = 1 << maxBits;

    return enumVal < maxValPlusOne;
  }

  function genTunicRecolorDef(selectElId) {
    const select = document.getElementById(selectElId);
    const selectedOption = select.children[select.selectedIndex];

    if (selectedOption.hasAttribute('data-rgb')) {
      return {
        type: RecolorDefType.twentyFourBitRgb,
        recolorId: RecolorId.herosClothes,
        value: parseInt(selectedOption.getAttribute('data-rgb'), 16),
      };
    }

    return null;
  }

  function genRecolorBits(recolorDefs) {
    if (!recolorDefs) {
      return false;
    }

    const version = 0;

    function isValidRecolorDef(recolorDef) {
      // Temp implementation until add support for more types.
      if (!recolorDef) {
        return false;
      }

      if (recolorDef.type === RecolorDefType.twentyFourBitRgb) {
        return recolorDef.value >= 0 && recolorDef.value <= 0x00ffffff;
      }

      return false;
    }

    // Process all recolorDefs
    recolorDefs = recolorDefs.filter(isValidRecolorDef);

    recolorDefs.sort(function (defA, defB) {
      return defA.recolorId - defB.recolorId;
    });

    if (recolorDefs.length < 1) {
      return false;
    }

    const enabledRecolorIds = {};
    let valuesBits = '';

    recolorDefs.forEach(function (recolorDef) {
      // Temp implementation until add support for more types.
      if (recolorDef.type === RecolorDefType.twentyFourBitRgb) {
        enabledRecolorIds[recolorDef.recolorId] = 1;
        valuesBits +=
          numToPaddedBits(recolorDef.type, 3) +
          numToPaddedBits(recolorDef.value, 24);
      }
    });

    const smallestRecolorId = recolorDefs[0].recolorId;
    const largestRecolorId = recolorDefs[recolorDefs.length - 1].recolorId;

    let bitString = '1'; // for on/off
    bitString += encodeAsVlq16(version);
    bitString += encodeAsVlq16(smallestRecolorId);
    bitString += encodeAsVlq16(largestRecolorId);

    for (let i = smallestRecolorId; i <= largestRecolorId; i++) {
      bitString += enabledRecolorIds[i] ? '1' : '0';
    }

    bitString += valuesBits;

    return {
      type: RawSettingType.bitString,
      bitString: bitString,
    };
  }

  function genSSettingsFromUi() {
    // Increment the version when you make changes to the format. Need to make
    // sure you don't break backwards compatibility!!
    const sSettingsVersion = 4;

    const values = [
      { id: 'logicRulesFieldset', bitLength: 2 },
      { id: 'castleRequirementsFieldset', bitLength: 3 },
      { id: 'palaceRequirementsFieldset', bitLength: 2 },
      { id: 'faronLogicFieldset', bitLength: 1 },
      { id: 'goldenBugsCheckbox' },
      { id: 'skyCharacterCheckbox' },
      { id: 'giftsFromNPCsCheckbox' },
      { id: 'poeSettingsFieldset', bitLength: 2 },
      { id: 'shopItemsCheckbox' },
      { id: 'hiddenSkillsCheckbox' },
      { id: 'smallKeyFieldset', bitLength: 3 },
      { id: 'bigKeyFieldset', bitLength: 3 },
      { id: 'mapAndCompassFieldset', bitLength: 3 },
      { id: 'introCheckbox' },
      { id: 'faronTwilightCheckbox' },
      { id: 'eldinTwilightCheckbox' },
      { id: 'lanayruTwilightCheckbox' },
      { id: 'mdhCheckbox' },
      { id: 'skipMinorCutscenesCheckbox' },
      { id: 'fastIBCheckbox' },
      { id: 'quickTransformCheckbox' },
      { id: 'transformAnywhereCheckbox' },
      { id: 'increaseWalletCheckbox' },
      { id: 'modifyShopModelsCheckbox' },
      { id: 'trapItemFieldset', bitLength: 3 },
      { id: 'barrenCheckbox' },
      { id: 'goronMinesEntranceFieldset', bitLength: 2 },
      { id: 'lakebedEntranceCheckbox' },
      { id: 'arbitersEntranceCheckbox' },
      { id: 'snowpeakEntranceCheckbox' },
      { id: 'totEntranceFieldset', bitLength: 2 },
      { id: 'cityEntranceCheckbox' },
      { id: 'instantTextCheckbox' },
      { id: 'openMapCheckbox' },
      { id: 'spinnerSpeedCheckbox' },
      { id: 'openDotCheckbox' },
      { id: 'itemScarcityFieldset', bitLength: 2 },
    ].map(({ id, bitLength }) => {
      const val = getVal(id);
      if (bitLength) {
        // select
        return {
          type: RawSettingType.xBitNum,
          bitLength,
          value: parseInt(getVal(id), 10),
        };
      }
      // checkbox
      return val;
    });

    values.push(genStartingItemsBits());
    values.push(genExcludedChecksBits());

    return encodeSettings(sSettingsVersion, 's', values);
  }

  const MidnaColorOptions = {
    default: 0,
    preset: 1,
    randomPreset: 2,
    detailed: 3,
  };

  function genMidnaSettings(recolorDefs) {
    const uEnumBitLen = 4;

    function uEnumToBits(number) {
      return numToPaddedBits(number, uEnumBitLen);
    }

    // This implementation will significantly change when recoloring options is
    // fleshed out.
    const selectVal = parseInt(getVal('midnaHairColorFieldset'), 10);

    if (selectVal < 1) {
      return null;
    }

    const version = 0;

    const midnaColorOption = MidnaColorOptions.preset;

    let bits = '1'; // write 1 bit for on
    const versionStr = version.toString(2);
    // write 4 bits for version bitLength
    bits += numToPaddedBits(versionStr.length, 4);
    // write X bits for version
    bits += versionStr;

    bits += uEnumToBits(midnaColorOption);

    if (midnaColorOption === MidnaColorOptions.preset) {
      bits += uEnumToBits(selectVal);
    } else if (midnaColorOption === MidnaColorOptions.detailed) {
      // Will need to add 1 bit for on/off on Active matches Inactive checkbox.
      // Will need to call this once for each, with either rgb or sValue
      // recolorDefs.push(...);
    }

    return {
      type: RawSettingType.bitString,
      bitString: bits,
    };
  }

  function genPSettingsFromUi(returnEvenIfEmpty) {
    // Don't do anything until we sort out pSettings better.
    return '';

    // const values = [].map(({ id, bitLength }) => {
    //   const val = getVal(id);
    //   if (bitLength) {
    //     // select
    //     return {
    //       type: RawSettingType.xBitNum,
    //       bitLength,
    //       value: parseInt(getVal(id), 10),
    //     };
    //   }
    //   // checkbox
    //   return val;
    // });

    // if (
    //   !returnEvenIfEmpty &&
    //   values.every((x) => {
    //     if (x) {
    //       return x.type === RawSettingType.xBitNum && x.value === 0;
    //     }

    //     return true;
    //   })
    // ) {
    //   return '';
    // }

    // return encodeSettings(0, 'p', values);
  }

  function decodeSettingsHeader(settingsString) {
    const match = settingsString.match(/^([a-fA-F0-9])+([sp])(.)/);
    if (match == null) {
      throw new Error('Invalid settings string.');
    }

    const version = parseInt(match[1], 16);
    const type = match[2];
    const lengthBits = sixBitCharsToBitString(match[3]);

    // return match;

    // extra bits, then lengthLength
    const extraBits = parseInt(lengthBits.substring(0, 3), 2);
    const lengthLength = parseInt(lengthBits.substring(3), 2);
    if (lengthLength < 1) {
      throw new Error('Invalid settings string.');
    }

    const lengthCharsStart = match[0].length;
    const contentStart = lengthCharsStart + lengthLength;

    if (contentStart > settingsString.length) {
      throw new Error('Invalid settings string.');
    }
    const contentLengthEncoded = settingsString.substring(
      lengthCharsStart,
      contentStart
    );
    const contentNumChars = parseInt(
      sixBitCharsToBitString(contentLengthEncoded),
      2
    );

    if (contentStart + contentNumChars > settingsString.length) {
      throw new Error('Invalid settings string.');
    }
    const chars = settingsString.substring(
      contentStart,
      contentStart + contentNumChars
    );
    let bits = sixBitCharsToBitString(chars);

    if (extraBits > 0) {
      if (bits.length < 1) {
        throw new Error('Invalid settings string.');
      }
      bits = bits.substring(0, bits.length - (6 - extraBits));
    }

    const remaining = settingsString.substring(contentStart + contentNumChars);

    return {
      version,
      type,
      bits,
      remaining,
    };
  }

  function breakUpSettingsString(settingsString) {
    // TODO: handle null check
    let remainingSettingsString = settingsString;

    const settingsByType = {};

    while (remainingSettingsString) {
      const obj = decodeSettingsHeader(remainingSettingsString);
      if (settingsByType[obj.type]) {
        return { error: `Multiple settings in string of type '${obj.type}'.` };
      }

      remainingSettingsString = obj.remaining;
      obj.remaining = undefined;
      settingsByType[obj.type] = obj;
    }

    return settingsByType;
  }

  function BitsProcessor(bits) {
    let remaining = bits;

    function nextXBitsAsNum(numBits) {
      if (remaining.length < numBits) {
        throw new Error(
          `Asked for ${numBits} bits, but only had ${remaining.length} remaining.`
        );
      }

      const value = parseInt(remaining.substring(0, numBits), 2);
      remaining = remaining.substring(numBits);
      return value;
    }

    function nextBoolean() {
      return Boolean(nextXBitsAsNum(1));
    }

    function nextEolList(bitLength) {
      let eolValue = 0;
      for (let i = 0; i < bitLength; i++) {
        eolValue += 1 << i;
      }

      const list = [];

      while (true) {
        if (remaining.length < bitLength) {
          throw new Error('Not enough bits remaining.');
        }

        const val = nextXBitsAsNum(bitLength);

        if (val === eolValue) {
          break;
        } else {
          list.push(val);
        }
      }

      return list;
    }

    function getVlq16BitLength(num) {
      if (num < 2) {
        return 5;
      }

      let bitsNeeded = 1;
      for (let i = 2; i <= 16; i++) {
        const oneOverMax = 1 << i;
        if (num < oneOverMax) {
          bitsNeeded = i - 1;
          break;
        }
      }

      return 4 + bitsNeeded;
    }

    function nextVlq16() {
      if (remaining.Length < 4) {
        throw new Error('Not enough bits remaining');
      }

      const val = decodeVlq16(remaining);
      remaining = remaining.substring(getVlq16BitLength(val));

      return val;
    }

    function nextRecolorTableEntry() {
      const type = nextXBitsAsNum(3);
      let value;

      if (type === RecolorDefType.twentyFourBitRgb) {
        value = nextXBitsAsNum(24);
      } else {
        throw new Error(
          'Not currently supporting anything other than 24-bit colors.'
        );
      }

      return { type, value };
    }

    function nextRecolorDefs() {
      const result = {};

      if (!nextBoolean()) {
        return result;
      }

      const version = nextVlq16();
      const smallestRecolorId = nextVlq16();
      const largestRecolorId = nextVlq16();

      const onRecolorIds = [];

      for (let i = smallestRecolorId; i <= largestRecolorId; i++) {
        if (nextBoolean()) {
          onRecolorIds.push(i);
        }
      }

      for (let i = 0; i < onRecolorIds.length; i++) {
        const recolorId = onRecolorIds[i];

        // Read next recolor table entry
        const obj = nextRecolorTableEntry();
        result[recolorId] = obj;
      }

      return result;
    }

    return {
      nextXBitsAsNum,
      nextBoolean,
      nextEolList,
      nextRecolorDefs,
    };
  }

  function decodeSSettings({ version, bits }) {
    const processor = BitsProcessor(bits);
    const res = {};

    function processBasic({ id, bitLength }) {
      if (bitLength != null) {
        const num = processor.nextXBitsAsNum(bitLength);
        res[id] = num;
      } else {
        const num = processor.nextBoolean();
        res[id] = num;
      }
    }

    processBasic({ id: 'logicRules', bitLength: 2 });
    processBasic({ id: 'castleRequirements', bitLength: 3 });
    processBasic({ id: 'palaceRequirements', bitLength: 2 });
    processBasic({ id: 'faronWoodsLogic', bitLength: 1 });
    processBasic({ id: 'goldenBugs' });
    processBasic({ id: 'skyCharacters' });
    processBasic({ id: 'giftsFromNpcs' });
    if (version >= 4) {
      // `poes` changed from a checkbox to a select
      processBasic({ id: 'poes', bitLength: 2 });
    } else {
      const poeSettings = {
        vanilla: 0,
        overworld: 1,
        dungeons: 2,
        all: 3
      };
      const shufflePoes = processor.nextBoolean();
      res.poes = shufflePoes
        ? poeSettings.overworld
        : poeSettings.vanilla;
    }
    processBasic({ id: 'shopItems' });
    processBasic({ id: 'hiddenSkills' });
    processBasic({ id: 'smallKeys', bitLength: 3 });
    processBasic({ id: 'bigKeys', bitLength: 3 });
    processBasic({ id: 'mapsAndCompasses', bitLength: 3 });
    processBasic({ id: 'skipIntro' });
    processBasic({ id: 'faronTwilightCleared' });
    processBasic({ id: 'eldinTwilightCleared' });
    processBasic({ id: 'lanayruTwilightCleared' });
    processBasic({ id: 'skipMdh' });
    processBasic({ id: 'skipMinorCutscenes' });
    processBasic({ id: 'fastIronBoots' });
    processBasic({ id: 'quickTransform' });
    processBasic({ id: 'transformAnywhere' });
    processBasic({ id: 'increaseWalletCapacity' });
    processBasic({ id: 'shopModelsShowTheReplacedItem' });
    processBasic({ id: 'trapItemsFrequency', bitLength: 3 });
    processBasic({ id: 'barrenDungeons' });
    if (version >= 2) {
      // `goronMinesEntrance` changed from a checkbox to a select
      processBasic({ id: 'goronMinesEntrance', bitLength: 2 });
    } else {
      const goronMinesEntrance = {
        closed: 0,
        noWrestling: 1,
        open: 2,
      };
      const skipMinesEntrance = processor.nextBoolean();
      res.goronMinesEntrance = skipMinesEntrance
        ? goronMinesEntrance.noWrestling
        : goronMinesEntrance.closed;
    }
    processBasic({ id: 'skipLakebedEntrance' });
    processBasic({ id: 'skipArbitersEntrance' });
    processBasic({ id: 'skipSnowpeakEntrance' });
    if (version >= 1) {
      // `totEntrance` changed from a checkbox to a select
      processBasic({ id: 'totEntrance', bitLength: 2 });
    } else {
      const totEntrance = {
        closed: 0,
        openGrove: 1,
        open: 2,
      };
      const totOpen = processor.nextBoolean();
      res.totEntrance = totOpen ? totEntrance.open : totEntrance.closed;
    }
    processBasic({ id: 'skipCityEntrance' });
    if (version >= 1) {
      // `instantText' added as an option in version 1
      processBasic({ id: 'instantText' });
    }
    if (version >= 3) {
      // `openMap' and 'spinnerSpeed' and 'openDot' were added as options in version 3
      processBasic({ id: 'openMap' });
      processBasic({ id: 'increaseSpinnerSpeed' });
      processBasic({ id: 'openDot' });
    }
    if (version >= 4) {
      processBasic({ id: 'itemScarcity', bitLength: 2 });
    } else {
      res.itemScarcity = 0; // Vanilla
    }

    res.startingItems = processor.nextEolList(9);
    res.excludedChecks = processor.nextEolList(9);

    return res;
  }

  // decodePSettings is not currently used until we better sort out pSettings.

  // function decodePSettings({ version, bits }) {
  //   // const processor = BitsProcessor(bits);

  //   // const res = {};

  //   // res.recolorDefs = processor.nextRecolorDefs();

  //   // res.randomizeBgm = processor.nextBoolean();
  //   // res.randomizeFanfares = processor.nextBoolean();
  //   // res.disableEnemyBgm = processor.nextBoolean();

  //   // return res;

  //   const a = [
  //     { id: 'hTunicHatColor', type: 'number', bitLength: 4 },
  //     { id: 'hTunicBodyColor', type: 'number', bitLength: 4 },
  //     { id: 'hTunicSkirtColor', type: 'number', bitLength: 4 },
  //     { id: 'zTunicHatColor', type: 'number', bitLength: 4 },
  //     { id: 'zTunicHelmetColor', type: 'number', bitLength: 4 },
  //     { id: 'zTunicBodyColor', type: 'number', bitLength: 4 },
  //     { id: 'zTunicScalesColor', type: 'number', bitLength: 4 },
  //     { id: 'zTunicBootsColor', type: 'number', bitLength: 4 },
  //     { id: 'lanternColor', type: 'number', bitLength: 4 },
  //     // { id: 'midnaHairColor', type: 'number', bitLength: 1 },
  //     { id: 'heartColor', type: 'number', bitLength: 4 },
  //     { id: 'aBtnColor', type: 'number', bitLength: 4 },
  //     { id: 'bBtnColor', type: 'number', bitLength: 3 },
  //     { id: 'xBtnColor', type: 'number', bitLength: 4 },
  //     { id: 'yBtnColor', type: 'number', bitLength: 4 },
  //     { id: 'zBtnColor', type: 'number', bitLength: 4 },
  //     { id: 'midnaHairBaseColor', type: 'number', bitLength: 4 },
  //     { id: 'midnaHairTipColor', type: 'number', bitLength: 4 },
  //     { id: 'midnaDomeRingColor', type: 'number', bitLength: 4 },

  //     { id: 'randomizeBgm', type: 'number', bitLength: 2 },
  //     { id: 'randomizeFanfares', type: 'boolean' },
  //     { id: 'disableEnemyBgm', type: 'boolean' },
  //   ];

  //   const processor = BitsProcessor(bits);

  //   const res = {};

  //   a.forEach(({ id, type, bitLength }) => {
  //     if (type === 'number') {
  //       const num = processor.nextXBitsAsNum(bitLength);
  //       res[id] = num;
  //     } else if (type === 'boolean') {
  //       const num = processor.nextBoolean();
  //       res[id] = num;
  //     } else {
  //       throw new Error(`Unknown type ${type} while decoding PSettings.`);
  //     }
  //   });

  //   return res;
  // }

  function decodeSettingsString(settingsString) {
    if (settingsString) {
      settingsString = settingsString.trim();
    }

    const byType = breakUpSettingsString(settingsString);

    const result = {};

    if (byType.s) {
      result.s = decodeSSettings(byType.s);
    }

    // Don't do this until we sort out pSettings better.
    // if (byType.p) {
    //   result.p = decodePSettings(byType.p);
    // }

    return result;
  }

  function populateUiFromPSettings(p) {
    // Don't do anything for now until we sort out pSettings.
    return;

    // if (!p) {
    //   return;
    // }

    // uncheckCheckboxes(['cosmeticsTab', 'audioTab']);

    // $('#hTunicHatColorFieldset').val(p.hTunicHatColor);
    // $('#hTunicBodyColorFieldset').val(p.hTunicBodyColor);
    // $('#hTunicSkirtColorFieldset').val(p.hTunicSkirtColor);
    // $('#zTunicHatColorFieldset').val(p.zTunicHatColor);
    // $('#zTunicHelmetColorFieldset').val(p.zTunicHelmetColor);
    // $('#zTunicBodyColorFieldset').val(p.zTunicBodyColor);
    // $('#zTunicScalesColorFieldset').val(p.zTunicScalesColor);
    // $('#zTunicBootsColorFieldset').val(p.zTunicBootsColor);
    // $('#lanternColorFieldset').val(p.lanternColor);
    // // $('#midnaHairColorFieldset').val(p.midnaHairColor);
    // $('#heartColorFieldset').val(p.heartColor);
    // $('#aButtonColorFieldset').val(p.aBtnColor);
    // $('#bButtonColorFieldset').val(p.bBtnColor);
    // $('#xButtonColorFieldset').val(p.xBtnColor);
    // $('#yButtonColorFieldset').val(p.yBtnColor);
    // $('#zButtonColorFieldset').val(p.zBtnColor);
    // $('#midnaHairBaseColorFieldset').val(p.midnaHairBaseColor);
    // $('#midnaHairTipColorFieldset').val(p.midnaHairTipColor);
    // $('#midnaDomeRingColorFieldset').val(p.midnaDomeRingColor);

    // $('#bgmFieldset').val(p.randomizeBgm);
    // $('#randomizeFanfaresCheckbox').prop('checked', p.randomizeFanfares);
    // $('#disableEnemyBGMCheckbox').prop('checked', p.disableEnemyBgm);
  }

  // window.decodeSettingsString = decodeSettingsString;

  let userJwt;

  window.addEventListener('DOMContentLoaded', () => {
    try {
      const jwt = localStorage.getItem('jwt');
      // 'undefined' check is to ignore localStorage jwt if we accidentally
      // saved a bad value which was possible before these changes. Maybe not
      // strictly necessary, but doesn't hurt.
      if (jwt && jwt !== 'undefined') {
        userJwt = jwt;
        return;
      }
    } catch (e) {
      console.error('Could not read jwt from localStorage.');
      console.error(e);
    }

    const jwtEl = document.getElementById('userJwtInput');
    if (jwtEl) {
      userJwt = jwtEl.value;
    }

    if (userJwt) {
      try {
        localStorage.setItem('jwt', userJwt);
      } catch (e) {
        console.error('Could not set jwt to localStorage.');
        console.error(e);
      }
    }
  });

  function fetchWrapper(resource, init = {}) {
    const newInit = init || {};
    newInit.headers = init.headers || {};

    if (!newInit.headers.Accept) {
      newInit.headers.Accept = 'application/json';
    }

    if (!newInit.headers['Content-Type']) {
      newInit.headers['Content-Type'] = 'application/json';
    }

    if (!newInit.headers.Authorization) {
      newInit.headers.Authorization = `Bearer ${userJwt}`;
    }

    return fetch(resource, newInit).then((res) => {
      if (res.status === 403) {
        try {
          localStorage.removeItem('jwt');
        } catch (e) {
          console.error(`Failed to remove 'jwt' from localStorage.`);
          console.error(e);
          console.error('Please refresh the page.');
        }
      }

      return res;
    });
  }

  function uncheckCheckboxes(parentIds) {
    parentIds.forEach(function (parentId) {
      const parentEl = document.getElementById(parentId);
      if (parentEl) {
        const inputs = parentEl.getElementsByTagName('input');
        for (let i = 0; i < inputs.length; i++) {
          const input = inputs[i];
          if (input.type.toLowerCase() === 'checkbox') {
            input.checked = false;
          }
        }
      }
    });
  }

  window.tpr = window.tpr || {};
  window.tpr.shared = {
    genSSettingsFromUi,
    genPSettingsFromUi,
    decodeSettingsString,
    populateUiFromPSettings,
    fetch: fetchWrapper,
    uncheckCheckboxes,
  };
})();
