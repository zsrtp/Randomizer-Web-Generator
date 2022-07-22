const $ = window.$;

let userJwt;
let generateCallInProgress = false;

function normalizeStringToMax128Bytes(inputStr, doTrims) {
  // substring to save lodash some work potentially. 256 because some
  // characters like emojis have length 2, and we want to leave at least 128
  // glyphs. Normalize is to handle writing the same unicode chars in
  // different ways.
  // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/normalize
  let seedStr = inputStr.normalize();

  if (doTrims) {
    seedStr = seedStr.trim();
  } else {
    seedStr = seedStr.replace(/^\s+/, '');
  }

  seedStr = seedStr.replace(/\s+/g, ' ').substring(0, 256);

  // The whitespace replacement already handles removing \n, \r, \f, and \t,
  // so the only characters left which have to be escaped are double-quote,
  // backslash, and backspace (\b or \x08). Even though they are only 1 byte in
  // UTF-8, we say those ones are 2 bytes because they will take 2 bytes in
  // the json file due to the backslash escape character.

  // Allow 128 bytes max
  const textEncoder = new TextEncoder();
  let len = 0;
  let str = '';

  // We use the lodash.toarray method because it handles chars like 👩‍❤️‍💋‍👩.
  // Another approach that almost works is str.match(/./gu), but this returns
  // [ "👩", "‍", "❤", "️", "‍", "💋", "‍", "👩" ] for 👩‍❤️‍💋‍👩.
  const chars = window.lodashToArray.toArray(seedStr);
  for (let i = 0; i < chars.length; i++) {
    const char = chars[i];

    let byteLength;
    if (char === '"' || char === '\\' || char === '\b') {
      byteLength = 2; // Will use 2 chars in the json file
    } else {
      byteLength = textEncoder.encode(char).length;
    }

    if (len + byteLength <= 128) {
      str += char;
      len += byteLength;
    } else {
      break;
    }
  }

  if (doTrims) {
    return str.trim();
  }
  return str;
}

function byId(id) {
  return document.getElementById(id);
}

function initTabButtons() {
  function genOnTabClick(id) {
    return function (e) {
      $('.tabcontent').removeClass('tabcontentactive');

      // const tabcontentEls = document.querySelectorAll('.tabcontent');
      // for (let i = 0; i < tabcontentEls.length; i++) {
      //   tabcontentEls[i].style.display = 'none';
      // }

      const tablinksEls = document.querySelectorAll('.tablinks');
      for (let i = 0; i < tablinksEls.length; i++) {
        tablinksEls[i].className = tablinksEls[i].className.replace(
          ' active',
          ''
        );
      }

      // Show the current tab, and add an "active" class to the button that opened the tab
      // byId(id).style.display = 'block';
      $('#' + id).addClass('tabcontentactive');
      e.currentTarget.className += ' active';
    };
  }

  [
    'randomizationSettingsTab',
    'gameplaySettingsTab',
    'excludedChecksTab',
    'startingInventoryTab',
    'cosmeticsAndQuirksTab',
    'audioTab',
    // 'legacyTab',
  ].forEach((id) => {
    byId(id + 'Btn').addEventListener('click', genOnTabClick(id));
  });
}

let showGeneratingModal; // fn
let hideGeneratingModal; // fn
let showGeneratingModalError; // fn
let showGenModalOngoingRequestError; // fn

window.addEventListener('DOMContentLoaded', onDomContentLoaded);
window.addEventListener('pageshow', function (event) {
  if (event.persisted) {
    // history traversal forward or backward
    generateCallInProgress = false;
    hideGeneratingModal();
  }
});

function onDomContentLoaded() {
  initJwt();

  initDevFooter();

  initTabButtons();

  setSettingsString();

  initSettingsModal();
  initGeneratingModal();

  document.getElementById('seed').addEventListener('input', (e) => {
    const { value } = e.target;
    e.target.value = normalizeStringToMax128Bytes(value);
  });

  // tempTestQueueFunc();
}

function initJwt() {
  try {
    const jwt = localStorage.getItem('jwt');
    if (jwt) {
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

  try {
    localStorage.setItem('jwt', userJwt);
  } catch (e) {
    console.error('Could not set jwt to localStorage.');
    console.error(e);
  }
}

function initDevFooter() {
  const imageVersion = $('#envImageVersion').val();
  const gitCommit = $('#envGitCommit').val();

  $('#devFooterText').text(imageVersion + ' ' + gitCommit);
}

const RawSettingType = {
  nineBitWithEndOfListPadding: 'nineBitWithEndOfListPadding',
  bitString: 'bitString',
};

// Dropdown menus
var headers = document.querySelectorAll('.dropdown-container header');
for (var i = 0; i < headers.length; i++) {
  headers[i].addEventListener('click', openCurrAccordion);
}
function openCurrAccordion(e) {
  for (var i = 0; i < headers.length; i++) {
    var parent = headers[i].parentElement;
    var article = headers[i].nextElementSibling;

    if (this === headers[i] && !parent.classList.contains('open')) {
      parent.classList.add('open');
      article.style.maxHeight = article.scrollHeight + 'px';
    } else {
      parent.classList.remove('open');
      article.style.maxHeight = '0px';
    }
  }
}

// Dolphin Version toggle
function DolVer(ver) {
  var low5 = document.getElementsByClassName('low5');
  var high5 = document.getElementsByClassName('high5');
  var spanLow5 = document.getElementsByClassName('spanLow5');
  var spanHigh5 = document.getElementsByClassName('spanHigh5');

  if (ver === 'high5') {
    for (var i = 0; i < low5.length; i++) {
      low5[i].style.display = 'none';
      high5[i].style.display = 'block';
    }
    for (var i = 0; i < spanLow5.length; i++) {
      spanLow5[i].style.display = 'none';
      spanHigh5[i].style.display = 'inline';
    }
  } else if (ver === 'low5') {
    for (var i = 0; i < high5.length; i++) {
      low5[i].style.display = 'block';
      high5[i].style.display = 'none';
    }
    for (var i = 0; i < spanHigh5.length; i++) {
      spanLow5[i].style.display = 'inline';
      spanHigh5[i].style.display = 'none';
    }
  }
}

// Game Version toggle
function GameVer(ver) {
  var usa = document.getElementsByClassName('usa');
  var eur = document.getElementsByClassName('eur');
  var jap = document.getElementsByClassName('jap');

  if (ver === 'usa') {
    for (var i = 0; i < usa.length; i++) {
      usa[i].style.display = 'inline';
      eur[i].style.display = 'none';
      jap[i].style.display = 'none';
    }
  } else if (ver === 'eur') {
    for (var i = 0; i < eur.length; i++) {
      usa[i].style.display = 'none';
      eur[i].style.display = 'inline';
      jap[i].style.display = 'none';
    }
  } else if (ver === 'jap') {
    for (var i = 0; i < jap.length; i++) {
      usa[i].style.display = 'none';
      eur[i].style.display = 'none';
      jap[i].style.display = 'inline';
    }
  }
}

for (
  var j = 0;
  j <
  document
    .getElementById('baseExcludedChecksListbox')
    .getElementsByTagName('input').length;
  j++
) {
  document
    .getElementById('baseExcludedChecksListbox')
    .getElementsByTagName('input')
    [j].addEventListener('click', setSettingsString);
}
for (
  var j = 0;
  j <
  document
    .getElementById('baseImportantItemsListbox')
    .getElementsByTagName('input').length;
  j++
) {
  document
    .getElementById('baseImportantItemsListbox')
    .getElementsByTagName('input')
    [j].addEventListener('click', setSettingsString);
}

var settingsLetters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ23456789';
document.getElementById('logicRulesFieldset').onchange = setSettingsString;
document.getElementById('gameRegionFieldset').onchange = setSettingsString;
document.getElementById('seedNumberFieldset').onchange = setSettingsString;
document.getElementById('castleRequirementsFieldset').onchange =
  setSettingsString;
document.getElementById('palaceRequirementsFieldset').onchange =
  setSettingsString;
document.getElementById('faronLogicFieldset').onchange = setSettingsString;
document
  .getElementById('mdhCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('introCheckbox')
  .addEventListener('click', setSettingsString);
document.getElementById('smallKeyFieldset').onchange = setSettingsString;
document.getElementById('bigKeyFieldset').onchange = setSettingsString;
document.getElementById('mapAndCompassFieldset').onchange = setSettingsString;
document
  .getElementById('goldenBugsCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('skyCharacterCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('giftsFromNPCsCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('poesCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('shopItemsCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('hiddenSkillsCheckbox')
  .addEventListener('click', setSettingsString);
document.getElementById('foolishItemFieldset').onchange = setSettingsString;
document
  .getElementById('faronTwilightCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('eldinTwilightCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('lanayruTwilightCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('skipMinorCutscenesCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('fastIBCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('quickTransformCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('transformAnywhereCheckbox')
  .addEventListener('click', setSettingsString);
document.getElementById('bgmFieldset').onchange = setSettingsString;
document
  .getElementById('randomizeFanfaresCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('disableEnemyBGMCheckbox')
  .addEventListener('click', setSettingsString);
document.getElementById('tunicColorFieldset').onchange = setSettingsString;
document.getElementById('lanternColorFieldset').onchange = setSettingsString;
document.getElementById('midnaHairColorFieldset').onchange = setSettingsString;
document.getElementById('heartColorFieldset').onchange = setSettingsString;
document.getElementById('aButtonColorFieldset').onchange = setSettingsString;
document.getElementById('bButtonColorFieldset').onchange = setSettingsString;
document.getElementById('xButtonColorFieldset').onchange = setSettingsString;
document.getElementById('yButtonColorFieldset').onchange = setSettingsString;
document.getElementById('zButtonColorFieldset').onchange = setSettingsString;
document
  .getElementById('barrenCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('minesEntranceCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('lakebedEntranceCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('arbitersEntranceCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('snowpeakEntranceCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('totEntranceCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('cityEntranceCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('importSettingsStringButton')
  .addEventListener('click', importSettingsString);
document
  .getElementById('increaseWalletCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('modifyShopModelsCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('importSettingsStringButton')
  .addEventListener('click', importSettingsString);

function importSettingsString() {
  parseSettingsString(document.getElementById('settingsStringTextbox').value);
}

function setSettingsString() {
  var settingsStringRaw = [];
  settingsStringRaw[0] =
    document.getElementById('logicRulesFieldset').selectedIndex;
  settingsStringRaw[1] = document.getElementById(
    'castleRequirementsFieldset'
  ).selectedIndex;
  settingsStringRaw[2] = document.getElementById(
    'palaceRequirementsFieldset'
  ).selectedIndex;
  settingsStringRaw[3] =
    document.getElementById('faronLogicFieldset').selectedIndex;
  settingsStringRaw[4] = document.getElementById('mdhCheckbox').checked;
  settingsStringRaw[5] = document.getElementById('introCheckbox').checked;
  settingsStringRaw[6] =
    document.getElementById('smallKeyFieldset').selectedIndex;
  settingsStringRaw[7] =
    document.getElementById('bigKeyFieldset').selectedIndex;
  settingsStringRaw[8] = document.getElementById(
    'mapAndCompassFieldset'
  ).selectedIndex;
  settingsStringRaw[9] = document.getElementById('goldenBugsCheckbox').checked;
  settingsStringRaw[10] = document.getElementById('poesCheckbox').checked;
  settingsStringRaw[11] = document.getElementById(
    'giftsFromNPCsCheckbox'
  ).checked;
  settingsStringRaw[12] = document.getElementById('shopItemsCheckbox').checked;
  settingsStringRaw[13] = document.getElementById(
    'faronTwilightCheckbox'
  ).checked;
  settingsStringRaw[14] = document.getElementById(
    'eldinTwilightCheckbox'
  ).checked;
  settingsStringRaw[15] = document.getElementById(
    'lanayruTwilightCheckbox'
  ).checked;
  settingsStringRaw[16] = document.getElementById(
    'skipMinorCutscenesCheckbox'
  ).checked;
  settingsStringRaw[17] = document.getElementById('fastIBCheckbox').checked;
  settingsStringRaw[18] = document.getElementById(
    'quickTransformCheckbox'
  ).checked;
  settingsStringRaw[19] = document.getElementById(
    'transformAnywhereCheckbox'
  ).checked;
  settingsStringRaw[20] = document.getElementById(
    'foolishItemFieldset'
  ).selectedIndex;
  var listItem = document
    .getElementById('baseImportantItemsListbox')
    .getElementsByTagName('input');
  var options = [];
  for (var i = 0; i < listItem.length; i++) {
    if (listItem[i].checked)
      options.push(listItem[i].getAttribute('data-itemId'));
  }
  settingsStringRaw[21] = options;
  listItem = document
    .getElementById('baseExcludedChecksListbox')
    .getElementsByTagName('input');
  options = [];
  for (var i = 0; i < listItem.length; i++) {
    if (listItem[i].checked)
      options.push(listItem[i].getAttribute('data-checkId'));
  }
  settingsStringRaw[22] = options;
  settingsStringRaw[23] =
    document.getElementById('tunicColorFieldset').selectedIndex;
  settingsStringRaw[24] = document.getElementById(
    'midnaHairColorFieldset'
  ).selectedIndex;
  settingsStringRaw[25] = document.getElementById(
    'lanternColorFieldset'
  ).selectedIndex;
  settingsStringRaw[26] =
    document.getElementById('heartColorFieldset').selectedIndex;
  settingsStringRaw[27] = document.getElementById(
    'aButtonColorFieldset'
  ).selectedIndex;
  settingsStringRaw[28] = document.getElementById(
    'bButtonColorFieldset'
  ).selectedIndex;
  settingsStringRaw[29] = document.getElementById(
    'xButtonColorFieldset'
  ).selectedIndex;
  settingsStringRaw[30] = document.getElementById(
    'yButtonColorFieldset'
  ).selectedIndex;
  settingsStringRaw[31] = document.getElementById(
    'zButtonColorFieldset'
  ).selectedIndex;
  settingsStringRaw[32] = document.getElementById('bgmFieldset').selectedIndex;
  settingsStringRaw[33] = document.getElementById(
    'randomizeFanfaresCheckbox'
  ).checked;
  settingsStringRaw[34] = document.getElementById(
    'disableEnemyBGMCheckbox'
  ).checked;
  settingsStringRaw[35] = document.getElementById(
    'hiddenSkillsCheckbox'
  ).checked;
  settingsStringRaw[36] = document.getElementById(
    'skyCharacterCheckbox'
  ).checked;
  settingsStringRaw[37] =
    document.getElementById('seedNumberFieldset').selectedIndex;
  settingsStringRaw[38] = document.getElementById(
    'increaseWalletCheckbox'
  ).checked;
  settingsStringRaw[39] = document.getElementById(
    'modifyShopModelsCheckbox'
  ).checked;
  settingsStringRaw[40] = document.getElementById('barrenCheckbox').checked;
  settingsStringRaw[41] = document.getElementById(
    'minesEntranceCheckbox'
  ).checked;
  settingsStringRaw[42] = document.getElementById(
    'lakebedEntranceCheckbox'
  ).checked;
  settingsStringRaw[43] = document.getElementById(
    'arbitersEntranceCheckbox'
  ).checked;
  settingsStringRaw[44] = document.getElementById(
    'snowpeakEntranceCheckbox'
  ).checked;
  settingsStringRaw[45] = document.getElementById(
    'totEntranceCheckbox'
  ).checked;
  settingsStringRaw[46] = document.getElementById(
    'cityEntranceCheckbox'
  ).checked;
  // document.getElementById('settingsStringTextbox').value =
  document.getElementById('settingsStringTextbox').textContent =
    getSettingsString(settingsStringRaw);

  const sSettingsString = genSettingsString();
  const pSettingsString = window.tpr.shared.genPSettingsFromUi();
  // const pSettingsString = '';

  document.getElementById('combinedSettingsString').textContent =
    // sSettingsString + pSettingsString;
    sSettingsString + window.tpr.shared.genUSettingsFromUi();

  // document.getElementById('newSettingsDisplay').value = genSettingsString();
  // document.getElementById('newSettingsDisplay').textContent = sSettingsString;

  // document.getElementById('seed').value = window.tpr.shared.genPSettingsFromUi();
  // document.getElementById('uSettingsDisplay').textContent =
  //   pSettingsString || '<empty>';

  // document.getElementById('uSettingsString').value =
  //   window.tpr.shared.genUSettingsFromUi();
}

function getSettingsString(settingsStringRaw) {
  var bits = '';
  //Get the properties of the class that contains the settings values so we can iterate through them.
  for (var i = 0; i < settingsStringRaw.length; i++) {
    var settingValue = settingsStringRaw[i];
    var i_bits = '';
    if (typeof settingValue == 'boolean') {
      //Settings that only have two options (Shuffle Golden Bugs, etc.)
      if (settingValue) {
        i_bits = '1';
      } else {
        i_bits = '0';
      }
    }
    if (typeof settingValue == 'number') {
      //Settings that have multiple options (Hyrule Castle Requirements, etc.)
      //Pad the integer value to 4 bits. No drop down menu uses more than 15 options so this is a safe bet.
      i_bits = padBits((settingValue >>> 0).toString(2), 4);
    }
    if (typeof settingValue == 'object') {
      //Starting Items list
      for (var j = 0; j < settingValue.length; j++) {
        //We pad the byte to 8 bits since item IDs don't go over 0xFF
        i_bits += padBits((settingValue[j] >>> 0).toString(2), 9);
      }
      //Place this at the end of the bit string. Will be useful when decoding to know when we've reached the end of the list.
      i_bits += '111111111';
    }
    bits += i_bits;
  }
  return btoa(bitStringToText(bits));
}

function padBits(num, size) {
  var s = '000000000' + num;
  return s.substr(s.length - size);
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
function toPaddedBits(number, strLength) {
  return padBits2(number.toString(2), strLength);
}

function bitStringToText(bits) {
  var result = '';
  //Pad the string to a value of 5
  while (bits.length % 5 != 0) {
    bits += '0';
  }

  for (var i = 0; i < bits.length; i += 5) {
    var value = '';
    for (var j = 0; j < 5; j++) {
      value = value + bits[i + j];
    }
    byteValue = parseInt(value, 2);
    result += index_to_letter(byteValue);
  }
  return result;
}

function textToBitString(text) {
  byteToBinary = '';
  for (var i = 0; i < text.length; i++) {
    var index = letter_to_index(text[i]);
    byteToBinary += padBits((index >>> 0).toString(2), 5);
  }
  while (byteToBinary.length % 5 != 0) {
    byteToBinary = byteToBinary.slice(0, byteToBinary.length - 1);
  }
  return byteToBinary;
}

function index_to_letter(index) {
  var c = settingsLetters[index];
  return c;
}
function letter_to_index(letter) {
  for (var i = 0; i < settingsLetters.length; i++) {
    if (letter == settingsLetters[i]) {
      return i;
    }
  }
  return 0;
}

var arrayOfSettingsItems = [
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
  'baseImportantItemsListbox',
  'baseExcludedChecksListbox',
  'tunicColorFieldset',
  'midnaHairColorFieldset',
  'lanternColorFieldset',
  'heartColorFieldset',
  'aButtonColorFieldset',
  'bButtonColorFieldset',
  'xButtonColorFieldset',
  'yButtonColorFieldset',
  'zButtonColorFieldset',
  'bgmFieldset',
  'randomizeFanfaresCheckbox',
  'disableEnemyBGMCheckbox',
  'gameRegionFieldset',
  'hiddenSkillsCheckbox',
  'skyCharacterCheckbox',
  'seedNumberFieldset',
  'increaseWalletCheckbox',
  'modifyShopModelsCheckbox',
  'barrenCheckbox',
  'minesEntranceCheckbox',
  'lakebedEntranceCheckbox',
  'arbitersEntranceCheckbox',
  'snowpeakEntranceCheckbox',
  'totEntranceCheckbox',
  'cityEntranceCheckbox',
];

function parseSettingsString(settingsString) {
  settingsString = atob(settingsString);
  //Convert the settings string into a binary string to be interpreted.
  var bitString = textToBitString(settingsString);
  for (var i = 0; i < arrayOfSettingsItems.length; i++) {
    var currentSettingsItem = arrayOfSettingsItems[i];
    var evaluatedByteString = '';
    var settingBitWidth = 0;
    var reachedEndofList = false;
    if (currentSettingsItem.includes('Checkbox')) {
      var value = parseInt(bitString[0], 2);
      if (value == 1) {
        document.getElementById(currentSettingsItem).checked = true;
      } else {
        document.getElementById(currentSettingsItem).checked = false;
      }
      bitString = bitString.substring(1);
    }
    if (currentSettingsItem.includes('Fieldset')) {
      settingBitWidth = 4;
      //We want to get the binary values in the string in 4 bit pieces since that is what is was encrypted with.
      for (var j = 0; j < settingBitWidth; j++) {
        evaluatedByteString += bitString[0];
        bitString = bitString.substring(1);
      }
      document.getElementById(currentSettingsItem).selectedIndex = parseInt(
        evaluatedByteString,
        2
      );
    }
    if (currentSettingsItem.includes('Listbox')) {
      var checkList = document
        .getElementById(currentSettingsItem)
        .getElementsByTagName('input');
      for (var j = 0; j < checkList.length; j++) {
        checkList[j].checked = false;
      }
      //We want to get the binary values in the string in 8 bit pieces since that is what is was encrypted with.
      settingBitWidth = 9;
      while (!reachedEndofList) {
        for (var j = 0; j < settingBitWidth; j++) {
          evaluatedByteString += bitString[0];
          bitString = bitString.substring(1);
        }
        itemIndex = parseInt(evaluatedByteString, 2);
        if (itemIndex != 511) {
          //Checks for the padding that was put in place upon encryption to know it has reached the end of the list.
          var checkList = document
            .getElementById(currentSettingsItem)
            .getElementsByTagName('input');
          for (var j = 0; j < checkList.length; j++) {
            if (itemIndex == checkList[j].id && !checkList[j].checked) {
              checkList[j].checked = true;
              break;
            }
          }
        } else {
          reachedEndofList = true;
        }
        evaluatedByteString = '';
      }
    }
  }
  return;
}

function encodeBits(bitString) {
  const missingChars = (6 - (bitString.length % 6)) % 6;
  bitString += '0'.repeat(missingChars);

  let charString = '';

  const chars =
    '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_';

  let index = 0;
  while (index < bitString.length) {
    const bits = bitString.substring(index, index + 6);
    const charIndex = parseInt(bits, 2);
    charString += chars[charIndex];

    index += 6;
  }

  return charString;
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
      bitString += toPaddedBits(asNum, 4);
    } else if (value && typeof value === 'object') {
      if (value.type === RawSettingType.bitString) {
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
    toPaddedBits((extraBits << 3) + numLengthChars, 6)
  );
  const lenChars = encodeBits(
    toPaddedBits(bitsAsChars.length, numLengthChars * 6)
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
        bits += toPaddedBits(itemId, 9);
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
        bits += toPaddedBits(itemId, 9);
      }
    });

  bits += '111111111';

  return {
    type: RawSettingType.bitString,
    bitString: bits,
  };
}

function genSettingsString() {
  return window.tpr.shared.genSSettingsFromUi();
}

function doGenerateCall(isRaceSeed) {
  if (generateCallInProgress) {
    return;
  }

  generateCallInProgress = true;

  showGeneratingModal();

  const settingsString =
    genSettingsString() + window.tpr.shared.genUSettingsFromUi();

  let requesterHash = undefined;

  try {
    requesterHash = localStorage.getItem('requesterHash');
  } catch (e) {
    // do nothing
  }

  window.tpr.shared
    .fetch('/api/seed/generate', {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        settingsString: settingsString,
        seed: $('#seed').val(),
        isRaceSeed: Boolean(isRaceSeed),
        requesterHash,
      }),
    })
    .then((response) => response.json())
    .then(({ data, error }) => {
      if (error) {
        generateCallInProgress = false;
        console.error('`/api/generateseed` error:');
        if (error.message) {
          showGeneratingModalError(`Error:\n${error}`);
        } else if (error.seedId) {
          showGenModalOngoingRequestError(error.seedId, error.canCancel);
        } else {
          console.error('`/api/generateseed` unrecognized error');
        }
      } else if (data && data.seedId && data.requesterHash) {
        try {
          localStorage.setItem('lastGeneratedSeedId', data.seedId);
          localStorage.setItem('requesterHash', data.requesterHash);
        } catch (e) {
          console.error(
            `Failed to set lastGeneratedSeedId in localStorage to ${data.data}`
          );
          console.error(e);
        }

        window.location.href = `/s/${data.seedId}`;
      } else {
        generateCallInProgress = false;
        console.error('Unrecognized response from `/api/generateseed`');
        console.error(data);
        showGeneratingModalError(
          'Unrecognized response from `/api/generateseed`.'
        );
      }
    })
    .catch((err) => {
      generateCallInProgress = false;
      showGeneratingModalError('/api/generateseed error');
      console.error('/api/generateseed error');
      console.error(err);
    });
}

$('#generateSeed').on('click', () => {
  doGenerateCall(false);
});

$('#generateRaceSeed').on('click', () => {
  doGenerateCall(true);
});

function initSettingsModal() {
  const modal = document.getElementById('myModal');
  const btn = document.getElementById('editSettingsBtn');
  const span = modal.querySelector('.modal-close');
  const $copySuccessText = $('#modalFieldCopiedText');
  const fieldErrorText = document.getElementById('modalFieldError');
  const input = document.getElementById('modalSettingsStringInput');
  const currentSettings = document.getElementById('modalCurrentSettings');

  input.addEventListener('input', () => {
    $copySuccessText.hide();
    $(fieldErrorText).hide();
  });

  // When the user clicks the button, open the modal
  btn.addEventListener('click', () => {
    // Prepare modal
    currentSettings.textContent =
      genSettingsString() + window.tpr.shared.genPSettingsFromUi();
    $copySuccessText.hide();
    $(fieldErrorText).hide();
    input.value = '';

    $(modal).show();

    input.focus();
  });

  span.addEventListener('click', () => {
    $(modal).hide();
  });

  document.getElementById('modalCancel').addEventListener('click', () => {
    $(modal).hide();
  });

  document.getElementById('modalImport').addEventListener('click', () => {
    if (!input.value) {
      $(modal).hide();
      return;
    }

    const error = populateFromSettingsString(input.value);

    if (error) {
      $(fieldErrorText)
        .text(
          'Unable to understand those settings. Do you have the correct string?'
        )
        .show();
    } else {
      $(modal).hide();
    }
  });

  document.getElementById('modalCopy').addEventListener('click', () => {
    $copySuccessText.hide();
    $(fieldErrorText).hide();

    const text = currentSettings.textContent;
    navigator.clipboard.writeText(text).then(
      () => {
        $copySuccessText.show();
      },
      (err) => {
        $(fieldErrorText).text('Failed to copy text.').show();
      }
    );
  });

  let canHide = true;

  $('#myModal')
    .on('mousedown', function (e) {
      canHide = e.target === this;
    })
    .on('mouseup', function (e) {
      if (canHide && e.target === this) {
        $(modal).hide();
      }
    });
}

function initGeneratingModal() {
  const bg = document.getElementById('modal2Bg');
  const modal = document.getElementById('generatingModal');
  const $generatingModalTitle = $('#generatingModalTitle');
  const $progressRow = $('#generatingProgressRow');
  const $deleteRequestSection = $('#deleteRequestSection');
  const $errorEl = $('#generatingError');
  const $doneBtnRow = $('#doneBtnRow');
  const $bodyMsg = $('#bodyMsg');
  let ongoingRequestId = null;

  function showModal() {
    $errorEl.text('').hide();
    $bodyMsg.text('').hide();
    $generatingModalTitle.text('Requesting generation...');
    $progressRow.show();
    $deleteRequestSection.hide();
    $doneBtnRow.hide();
    bg.style.display = '';
    modal.style.display = '';
    modal.classList.add('isOpen');
  }

  function hideModal() {
    bg.style.display = 'none';
    modal.style.display = 'none';
    modal.classList.remove('isOpen');
    $('#ongoingRequestBtnRow').hide();
  }

  function showError(msg) {
    $progressRow.hide();
    $errorEl.text(msg).show();
  }

  function showOngoingRequestError(id, canCancel) {
    ongoingRequestId = id;
    $progressRow.hide();
    $generatingModalTitle.text('Request in progress');
    $errorEl
      .text(
        'You already have a seed request in progress. Please wait for it to finish before creating a new request.'
      )
      .show();

    if (canCancel) {
      $deleteRequestSection.show();
    }
    $('#ongoingRequestBtnRow').show();
    $('#showOngoingRequestBtn').show();
  }

  function doCancelRequest() {
    console.log('doing cancel request here');

    if (!ongoingRequestId) {
      hideModal();
      return;
    }

    $generatingModalTitle.text('Deleting request...');
    $progressRow.show();
    $errorEl.text('').hide();
    $deleteRequestSection.hide();
    $('#ongoingRequestBtnRow').hide();
    $('#showOngoingRequestBtn').hide();

    let requesterHash = undefined;

    try {
      requesterHash = localStorage.getItem('requesterHash');
    } catch (e) {
      // do nothing
    }

    window.tpr.shared
      .fetch('/api/seed/cancel/', {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        // TODO: handle on server when forget to stringify on client so don't
        // expose stack trace
        body: JSON.stringify({
          seedId: ongoingRequestId,
          userId: userJwt,
          // requesterHash: requesterHash + 'a',
          requesterHash,
        }),
      })
      .then((response) => response.json())
      .then(({ error }) => {
        if (error) {
          handleCancelRequestError(error);
        } else {
          handleCancelRequestSuccess();
        }
      })
      .catch((err) => {
        console.error(err);
        handleCancelRequestError(err.message);
      });
  }

  function handleCancelRequestSuccess() {
    $progressRow.hide();
    $generatingModalTitle.text('Request deleted');
    $bodyMsg.text('You can now request a new seed generation.').show();
    $doneBtnRow.show();
  }

  function handleCancelRequestError(errorMsg) {
    $progressRow.hide();
    $generatingModalTitle.text('Request deletion failed');
    $errorEl.text(errorMsg).show();
    $doneBtnRow.show();
  }

  showGeneratingModal = showModal;
  hideGeneratingModal = hideModal;
  showGeneratingModalError = showError;
  showGenModalOngoingRequestError = showOngoingRequestError;

  // window.addEventListener('click', () => {
  //   if (!generateCallInProgress) {
  //     hideModal();
  //   }
  // });

  $('#viewOngoingRequestBtn').on('click', () => {
    window.open(`/s/${ongoingRequestId}`, '_blank');
    hideModal();
  });

  $('#hideOngoingRequestBtn').on('click', hideModal);
  $('#generatingModalDoneBtn').on('click', hideModal);
  $('#deleteRequest').on('click', doCancelRequest);
}

function populateFromSettingsString(settingsString) {
  let byType;

  try {
    byType = window.tpr.shared.decodeSettingsString(settingsString);
  } catch (e) {
    console.error(e);
    return e.message;
  }

  if (byType.s) {
    populateSSettings(byType.s);
  }

  if (byType.p) {
    window.tpr.shared.populateUiFromPSettings(byType.p);
  }

  setSettingsString();

  return null;
}

function populateSSettings(s) {
  if (!s) {
    return;
  }

  $('#logicRulesFieldset').val(s.logicRules);
  $('#castleRequirementsFieldset').val(s.castleRequirements);
  $('#palaceRequirementsFieldset').val(s.palaceRequirements);
  $('#faronLogicFieldset').val(s.faronWoodsLogic);
  $('#goldenBugsCheckbox').prop('checked', s.goldenBugs);
  $('#skyCharacterCheckbox').prop('checked', s.skyCharacters);
  $('#giftsFromNPCsCheckbox').prop('checked', s.giftsFromNpcs);
  $('#poesCheckbox').prop('checked', s.poes);
  $('#shopItemsCheckbox').prop('checked', s.shopItems);
  $('#hiddenSkillsCheckbox').prop('checked', s.hiddenSkills);
  $('#smallKeyFieldset').val(s.smallKeys);
  $('#bigKeyFieldset').val(s.bigKeys);
  $('#mapAndCompassFieldset').val(s.mapsAndCompasses);
  $('#introCheckbox').prop('checked', s.skipIntro);
  $('#faronTwilightCheckbox').prop('checked', s.faronTwilightCleared);
  $('#eldinTwilightCheckbox').prop('checked', s.eldinTwilightCleared);
  $('#lanayruTwilightCheckbox').prop('checked', s.lanayruTwilightCleared);
  $('#mdhCheckbox').prop('checked', s.skipMdh);
  $('#skipMinorCutscenesCheckbox').prop('checked', s.skipMinorCutscenes);
  $('#fastIBCheckbox').prop('checked', s.fastIronBoots);
  $('#quickTransformCheckbox').prop('checked', s.quickTransform);
  $('#transformAnywhereCheckbox').prop('checked', s.transformAnywhere);
  $('#increaseWalletCheckbox').prop('checked', s.increaseWalletCapacity);
  $('#modifyShopModelsCheckbox').prop(
    'checked',
    s.shopModelsShowTheReplacedItem
  );
  $('#foolishItemFieldset').val(s.trapItemsFrequency);
  $('#barrenCheckbox').prop('checked', s.barrenDungeons);
  $('#minesEntranceCheckbox').prop('checked', s.skipMinesEntrance);
  $('#lakebedEntranceCheckbox').prop('checked', s.skipLakebedEntrance);
  $('#arbitersEntranceCheckbox').prop('checked', s.skipArbitersEntrance);
  $('#snowpeakEntranceCheckbox').prop('checked', s.skipSnowpeakEntrance);
  $('#totEntranceCheckbox').prop('checked', s.skipToTEntrance);
  $('#cityEntranceCheckbox').prop('checked', s.skipCityEntrance);

  const $excludedChecksParent = $('#baseExcludedChecksListbox');
  s.excludedChecks.forEach((checkNumId) => {
    $excludedChecksParent
      .find(`input[data-checkid="${checkNumId}"`)
      .prop('checked', true);
  });

  if (s.startingItems.length > 0) {
    const byId = {};

    const $startingItemsParent = $('#baseImportantItemsListbox');

    s.startingItems.forEach((id) => {
      byId[id] = (byId[id] || 0) + 1;
    });

    Object.keys(byId).forEach((id) => {
      const count = byId[id];

      const checkboxes = $startingItemsParent.find(
        `input[data-itemid="${id}"]`
      );

      for (let i = 0; i < count && i < checkboxes.length; i++) {
        checkboxes[i].checked = true;
      }
    });
  }
}

function testProgressFunc(id) {
  window.tpr.shared
    .fetch(`/api/seed/progress/${id}`, {
      method: 'GET',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
    })
    .then((response) => response.json())
    .then((data) => {
      console.log('/api/seed/progress data');
      console.log(data);

      if (!data.data.obj.done) {
        setTimeout(() => {
          testProgressFunc(id);
        }, 2000);
      }
    })
    .catch((err) => {
      console.error('/api/seed/progress error');
      console.error(err);
    });
}

function tempTestQueueFunc() {
  const settingsString =
    genSettingsString() + window.tpr.shared.genUSettingsFromUi();

  // fetch('/api/seed/generate', {
  window.tpr.shared
    .fetch('/api/seed/generate', {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
        // Authorization: 'example auth content',
        // Authorization: `Bearer ${userJwt}`,
      },
      body: JSON.stringify({
        settingsString: settingsString,
        seed: $('#seed').val(),
      }),
    })
    .then((response) => response.json())
    .then((data) => {
      console.log('/api/seed/generate data');
      console.log(data);

      testProgressFunc(data.data.id);

      // if (data.error) {
      //   generateCallInProgress = false;
      //   console.error('`/api/generateseed` error:');
      //   console.error(data);
      //   showGeneratingModalError(`Error:\n${data.error}`);
      // } else if (data.data && data.data.id) {
      //   window.location.href = '/seed?id=' + data.data.id;
      // } else {
      //   generateCallInProgress = false;
      //   console.error('Unrecognized response from `/api/generateseed`');
      //   console.error(data);
      //   showGeneratingModalError(
      //     'Unrecognized response from `/api/generateseed`.'
      //   );
      // }
    })
    .catch((err) => {
      console.error('/api/seed/generate error');
      console.error(err);
    });
}
