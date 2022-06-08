'use strict';

(function () {
  const sEnumBitLen = 4;
  const uEnumBitLen = 4;
  const pEnumBitLen = 4;

  const encodingChars =
    '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_';

  const RawSettingType = {
    nineBitWithEndOfListPadding: 'nineBitWithEndOfListPadding',
    bitString: 'bitString',
  };

  const RecolorId = {
    herosClothes: 0x00, // Cap and Body
    zoraArmorPrimary: 0x01,
    zoraArmorSecondary: 0x02,
    zoraArmorHelmet: 0x03,
  };

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

  /**
   * Adds '0' chars to front of bitStr such that the returned string length is
   * equal to the provided size. If bitStr.length > size, simply returns bitStr.
   *
   * @param {string} bitStr String of '0' and '1' chars
   * @param {number} size Desired length of resultant string.
   * @return {string}
   */
  function padBits2(bitStr, size) {
    const numZeroesToAdd = size - bitStr.length;
    if (numZeroesToAdd <= 0) {
      return bitStr;
    }

    let ret = '';
    for (let i = 0; i < numZeroesToAdd; i++) {
      ret += '0';
    }
    return ret + bitStr;
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

  function sEnumToBits(number) {
    return numToPaddedBits(number, sEnumBitLen);
  }

  function uEnumToBits(number) {
    return numToPaddedBits(number, uEnumBitLen);
  }

  function pEnumToBits(number) {
    return numToPaddedBits(number, pEnumBitLen);
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

  function isRgbHex(str) {
    return /^[a-fA-F0-9]{6}$/.test(str);
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

  function genTunicRecolorDef(id, recolorId) {
    const select = document.getElementById(id);
    const selectedOption = select.children[select.selectedIndex];

    if (selectedOption.hasAttribute('data-rgb')) {
      return {
        recolorId: recolorId,
        rgb: selectedOption.getAttribute('data-rgb'),
      };
    } else if (selectedOption.value !== '0') {
      return {
        recolorId: recolorId,
        sValue: selectedOption.value,
      };
    }
  }

  function genRecolorBits(recolorDefs, enumBitLen) {
    if (!recolorDefs) {
      return false;
    }

    // Process all recolorDefs
    recolorDefs = recolorDefs.filter(function (recolorDef) {
      return (
        recolorDef &&
        (isRgbHex(recolorDef.rgb) ||
          isEnumWithinRange(enumBitLen, recolorDef.sValue))
      );
    });

    recolorDefs.sort(function (defA, defB) {
      return defA.recolorId - defB.recolorId;
    });

    if (recolorDefs.length < 1) {
      return false;
      // return {
      //   type: RawSettingType.bitString,
      //   bitString: '0000000000000000', // 16 zeroes
      // };
    }

    let metaBitsPerEnum = 1;
    const enabledRecolorIds = {};
    let rgbAndSBits = '';

    recolorDefs.forEach(function (recolorDef) {
      if (recolorDef.sValue) {
        metaBitsPerEnum = 2;
        enabledRecolorIds[recolorDef.recolorId] = 2;
        rgbAndSBits += numToPaddedBits(
          parseInt(recolorDef.sValue, 10),
          enumBitLen
        );
      } else if (recolorDef.rgb) {
        enabledRecolorIds[recolorDef.recolorId] = 1;
        rgbAndSBits += numToPaddedBits(parseInt(recolorDef.rgb, 16), 24);
      }
    });

    const recolorIdEnabledBitsLength =
      recolorDefs[recolorDefs.length - 1].recolorId + 1;

    let bitString = '1'; // for On/Off
    bitString += numToPaddedBits(recolorIdEnabledBitsLength, 16);
    bitString += numToPaddedBits(metaBitsPerEnum, enumBitLen);

    for (let i = 0; i < recolorIdEnabledBitsLength; i++) {
      // bitString += enabledRecolorIds[i] ? '1' : '0';
      bitString += numToPaddedBits(enabledRecolorIds[i] || 0, metaBitsPerEnum);
    }

    bitString += rgbAndSBits;

    return {
      type: RawSettingType.bitString,
      bitString: bitString,
    };
  }

  function genSSettingsFromUi() {
    const values = [
      'logicRulesFieldset',
      'castleRequirementsFieldset',
      'palaceRequirementsFieldset',
      'faronLogicFieldset',
      'mdhCheckbox',
      'introCheckbox',
      'smallKeyFieldset',
      'bigKeyFieldset',
      'mapAndCompassFieldset',
      'goldenBugsCheckbox',
      'poesCheckbox',
      'giftsFromNPCsCheckbox',
      'shopItemsCheckbox',
      'faronTwilightCheckbox',
      'eldinTwilightCheckbox',
      'lanayruTwilightCheckbox',
      'skipMinorCutscenesCheckbox',
      'fastIBCheckbox',
      'quickTransformCheckbox',
      'transformAnywhereCheckbox',
      'foolishItemFieldset',
    ].map(getVal);

    values.push(genStartingItemsBits());
    values.push(genExcludedChecksBits());

    return encodeSettings(0, 's', values);
  }

  function genPSettingsFromUi() {
    const values = [
      'randomizeBGMCheckbox',
      'randomizeFanfaresCheckbox',
      'disableEnemyBGMCheckbox',
    ].map(getVal);

    const recolorDefs = [];
    // Add recolorDefs to list.
    recolorDefs.push(
      genTunicRecolorDef('tunicColorFieldset', RecolorId.herosClothes)
    );

    values.push(genRecolorBits(recolorDefs, pEnumBitLen));

    return encodeSettings(0, 'p', values);
  }

  const MidnaColorOptions = {
    default: 0,
    preset: 1,
    randomPreset: 2,
    detailed: 3,
  };

  function genMidnaSettings(recolorDefs) {
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

    if (midnaColorOption == MidnaColorOptions.preset) {
      bits += uEnumToBits(selectVal);
    } else if (midnaColorOption == MidnaColorOptions.detailed) {
      // Will need to add 1 bit for on/off on Active matches Inactive checkbox.
      // Will need to call this once for each, with either rgb or sValue
      // recolorDefs.push(...);
    }

    return {
      type: RawSettingType.bitString,
      bitString: bits,
    };
  }

  function genUSettingsFromUi(returnEvenIfEmpty) {
    let values = [];

    const recolorDefs = [];

    recolorDefs.push(genTunicRecolorDef('tunicColor', RecolorId.herosClothes));

    values.push(genMidnaSettings(recolorDefs));

    values = [genRecolorBits(recolorDefs, pEnumBitLen)].concat(values);

    if (!returnEvenIfEmpty && values.every((x) => !x)) {
      return '';
    }

    return encodeSettings(0, 'u', values);
  }

  window.tpr = window.trp || {};
  window.tpr.shared = {
    genSSettingsFromUi: genSSettingsFromUi,
    genUSettingsFromUi: genUSettingsFromUi,
    genPSettingsFromUi: genPSettingsFromUi,
  };
})();
