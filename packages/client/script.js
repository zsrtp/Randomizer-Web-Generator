const $ = window.$;

let userJwt;
let generateCallInProgress = false;

const presetsMgr = (function () {
  const SYSTEM_PRESETS = [
    {
      name: 'Default',
      origSettingsStr: '6s1N9m000201W216000W4-WJ__-',
      description:
        'Aimed towards players who may have played the vanilla game but are not as familiar with the world. No timesavers are enabled and only the absolute minimum amount of checks are randomized.',
    },
    {
      name: 'Easy',
      origSettingsStr: '6sHg9gn9_V01W218000W4-WJ_hylPz9WJpWiEnQeEqPN_W',
      description:
        'Aimed towards players who are familiar with randomizers and want a little more randomness. Many of the story timesavers are skipped and the world is much more random. A number of time-intensive checks are excluded.',
    },
    {
      name: 'Experienced',
      origSettingsStr: '6s1Q9lxS_R9nq216000W4-WGPFF__-',
      description:
        'These settings are aimed towards players who have a lot of seeds under their belt and are looking for a new challenge. A majority of timesavers are enabled, all check types are randomized, and no checks are excluded.',
    },
    {
      name: 'Full Randomness',
      origSettingsStr: '6s1N4lxR_V81i2X9W7Y04-WJ__-',
      description:
        'These settings are for those who want the full randomizer experience: all possible checks randomized, entrance rando, and a majority of timesavers enabled. No fluff, just rando.',
    },
    {
      name: 'Quick and Easy',
      origSettingsStr:
        '6sPu029C_V6yT2fAO00W502GP8HvZyVa0a4H82mTgvNhzqVzVYUL0ENQAR_m',
      description:
        'These settings are for those who want a quick seed that requires less searching and more playing.',
    },
    {
      name: 'Nightmare',
      origSettingsStr: '6sPP6lxS_PW0iB81WNaJm-WGPV__m',
      description:
        'These settings are designed to cause pain. Everything is randomized and settings such as One-Hit-KO, Bonks Do Damage, and Nightmare trap items are enabled. These seeds rely on glitchless logic to be beatable. Good luck.',
    },
    {
      name: 'Glitched',
      origSettingsStr: '6sPPKlxS_R11i298002Ku-WGPV__m',
      description:
        'This preset is just commonly used settings when running/racing a glitched seed.',
    },
    {
      name: 'No Logic',
      origSettingsStr: '6s1WbFxS_VM4O21AW020G-WGPFD83jNB___-',
      description:
        'This preset is just commonly used settings when running/racing a no logic seed.',
    },
    {
      name: 'Season 1',
      origSettingsStr:
        '6sY4N2kPC__6KD2P2001WG-WGP8HvayVFbyej6er6fXtFvVkbqkbqkbqk1mE1mE1mE1mE1mE1mE1mE3_aw7fNCQGO3AacP0aOW1zZoruE2Ar3ABAWYv1ix359-_JmFo_2LhyJp-lujJWfgEGuigx9udSYf7EAJ2a8aZG74kiOCbYDg3dmL1QkYJHfZ8amkWwX05dyKP0CMg3jEOenjCkeWokixRk9mfHWauqf5C4T984e9hZDQIQccGTWMYqjOC9sw1eXmRnoXG_nJ92pvbcwGnB0X_u',
      description:
        'Season 1 tournament settings. Good for a quick seed or to learn racing basics.',
    },
    {
      name: 'Season 1.5',
      origSettingsStr:
        '6sQ5x2kPC_v6L92PDQG1WG6GmP8HvayVFbyej6er6fXtFuThzqEbqkbqkbqk5mE1mE1mE1mE1mE1mE1mE1mVydGzAuL6D8C1bIJCWIu8n03x7bhmS4Lg6KGHSdwVb-5-9v_NyMfDHo75bNPOCgqKEji2DM4xpnEv5MY1f5WI8vnIOKX4aQ0ubrZ1aiHjGS-2eBLqIQDCP4c5q7K80i_YZ81YrGTfp56Dfbr46LstSJXIZ19nfIAO8wIG9GJN6QqarDCWx0j5fQmOT0qGuDuvNUTz_aXPyopT8ObWG_-20AImAReCPOASeCRmC70Aw0ABGC5eCfGCd0CeWAouAemAvWAYeAU8AoeAE8CEGAceAkGCWWCs0EYWCTOCLOAmOB_u',
      description:
        'Temporary test settings. Will be replaced with Season 2 once it is ready.',
    },
  ];

  let inited = false;
  let loadSettingsInProgress = false;
  let customByName = {};

  function init() {
    if (inited) {
      return;
    }
    inited = true;

    try {
      const str = localStorage.getItem('customSettingsPresets');
      if (str != null) {
        const byName = JSON.parse(str);
        Object.keys(byName).forEach((key) => {
          const obj = byName[key];
          // Only retrieve objects which have expected properties, and only
          // retrive expected properties.
          if (obj.name && obj.origSettingsStr) {
            customByName[key] = {
              name: obj.name,
              description: obj.description,
              origSettingsStr: obj.origSettingsStr,
              origCommit: obj.origCommit,
              latestSettingsStr: obj.latestSettingsStr,
            };
          }
        });
      }
    } catch (e) {
      console.error(
        'Failed to retrieve customSettingsPresets from localStorage.'
      );
      console.error(e);
    }
  }

  function getPresetsByType() {
    return {
      system: SYSTEM_PRESETS,
      custom: Object.values(customByName).sort((a, b) => {
        return a.name.localeCompare(b.name);
      }),
    };
  }

  function getPresetByName(name) {
    if (customByName[name]) {
      return Object.assign({}, customByName[name]);
    } else {
      for (let i = 0; i < SYSTEM_PRESETS.length; i++) {
        const systemPreset = SYSTEM_PRESETS[i];
        if (systemPreset.name === name) {
          return Object.assign({}, systemPreset);
        }
      }
    }
    return null;
  }

  function isNameTaken(name) {
    if (customByName[name]) {
      return 'custom';
    }
    for (let i = 0; i < SYSTEM_PRESETS.length; i++) {
      const systemPreset = SYSTEM_PRESETS[i];
      if (systemPreset.name === name) {
        return 'system';
      }
    }
    return '';
  }

  function savePreset(diff) {
    if (!diff.name) {
      throw new Error(`Expected diff.name, but was ${diff.name}.`);
    }
    const prevObj = customByName[diff.name] || {};

    const allowedProperties = [
      'name',
      'description',
      'origSettingsStr',
      'origCommit',
      'latestSettingsStr',
    ];
    const newDiff = {};
    allowedProperties.forEach((prop) => {
      if (diff[prop] !== undefined) {
        newDiff[prop] = diff[prop];
      }
    });

    customByName[diff.name] = Object.assign({}, prevObj, newDiff);

    // Try to write to localStorage
    try {
      localStorage.setItem(
        'customSettingsPresets',
        JSON.stringify(customByName)
      );
    } catch (e) {
      console.error('Could not save custom settings to localStorage.');
      console.error(e);
      return false;
    }

    return true;
  }

  function renamePreset(oldName, newName) {
    const presetObj = customByName[oldName];
    if (!presetObj) {
      return 'Did not find existing preset to edit.';
    }

    // Note: once we can edit description, it is possible that the newName will
    // match the old name, so it is fine if these match. Otherwise, the new name
    // should not match any preset names.
    if (oldName !== newName && isNameTaken(newName)) {
      if (presetTakenResult === 'custom') {
        return 'A custom preset with this name already exists.';
      } else {
        return 'A system preset with this name already exists.';
      }
    }

    // Filter out oldName when creating newCustomByName obj.
    let newCustomByName = {};
    const keys = Object.keys(customByName);
    for (let i = 0; i < keys.length; i++) {
      const key = keys[i];
      if (key !== oldName) {
        newCustomByName[key] = customByName[key];
      }
    }
    newCustomByName[newName] = { ...presetObj, name: newName };

    // Try to write to localStorage
    try {
      localStorage.setItem(
        'customSettingsPresets',
        JSON.stringify(newCustomByName)
      );
      customByName = newCustomByName;
    } catch (e) {
      const msg = 'Could not save custom settings to localStorage during edit.';
      console.error(msg);
      console.error(e);
      return msg;
    }

    return '';
  }

  function deletePreset(name) {
    if (!customByName[name]) {
      return 'Did not find preset to delete.';
    }

    // Filter out name when creating newCustomByName obj.
    let newCustomByName = {};
    const keys = Object.keys(customByName);
    for (let i = 0; i < keys.length; i++) {
      const key = keys[i];
      if (key !== name) {
        newCustomByName[key] = customByName[key];
      }
    }

    // Try to write to localStorage
    try {
      localStorage.setItem(
        'customSettingsPresets',
        JSON.stringify(newCustomByName)
      );
      customByName = newCustomByName;
    } catch (e) {
      const msg =
        'Could not save custom settings to localStorage during delete.';
      console.error(msg);
      console.error(e);
      return msg;
    }

    return '';
  }

  function loadSettings(name) {
    loadSettingsInProgress = true;
    let settingsStr = '';

    if (customByName[name]) {
      settingsStr = customByName[name].origSettingsStr;
    } else {
      for (let i = 0; i < SYSTEM_PRESETS.length; i++) {
        const systemPreset = SYSTEM_PRESETS[i];
        if (systemPreset.name === name) {
          settingsStr = systemPreset.origSettingsStr;
        }
      }
    }

    if (!settingsStr) {
      const msg = 'Settings string was empty.';
      console.error(msg);
      return msg;
    }

    const error = populateFromSettingsString(settingsStr);
    loadSettingsInProgress = false;
    return error;
  }

  function getDebugStr() {
    return new Promise((resolve) => {
      try {
        const str = localStorage.getItem('customSettingsPresets');
        navigator.clipboard.writeText(str).then(
          () => {
            resolve('');
          },
          (err) => {
            resolve('Failed to copy');
          }
        );
        return '';
      } catch (e) {
        const msg = 'Failed to create presets debug string.';
        console.error(msg);
        console.error(e);
        resolve(msg);
      }
    });
  }

  function getNameForSettingsString(settingsString) {
    const keys = Object.keys(customByName);
    for (let i = 0; i < keys.length; i++) {
      const key = keys[i];
      if (customByName[key].origSettingsStr === settingsString) {
        return key;
      }
    }

    for (let i = 0; i < SYSTEM_PRESETS.length; i++) {
      const systemPreset = SYSTEM_PRESETS[i];
      if (systemPreset.origSettingsStr === settingsString) {
        return systemPreset.name;
      }
    }
  }

  function checkClearSelect(settingsString) {
    if (loadSettingsInProgress) {
      return;
    }
    const name = getNameForSettingsString(settingsString);
    if (!name) {
      $('#presetsSelect').val(null).trigger('change');
    }
  }

  return {
    state: {
      prevValue: undefined,
      cleanupFn: null,
    },
    init,
    getPresetsByType,
    getPresetByName,
    isNameTaken,
    savePreset,
    renamePreset,
    deletePreset,
    loadSettings,
    getDebugStr,
    getNameForSettingsString,
    checkClearSelect,
  };
})();

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
    'plandoTab',
    // 'legacyTab',
  ].forEach((id) => {
    byId(id + 'Btn').addEventListener('click', genOnTabClick(id));
  });
}

function initSelect2(id, select2Options) {
  const sel2Options = Object.assign(
    {
      // width: 'style',
      minimumResultsForSearch: -1,
      templateResult(data, container) {
        // Try to init tooltip on rendered option
        if (data.element) {
          const tooltipText = data.element.getAttribute('data-tooltip-text');
          if (tooltipText) {
            container.setAttribute('data-tooltip-text', tooltipText);
            window.initTooltipsInTree(container, {
              fadeInDelay: 0,
              extraPadding: 20,
            });
          }
        }

        return data.text;
      },
    },
    select2Options
  );

  const $el = $(`#${id}`);

  function handleClosing() {
    window.removeTooltips();
  }

  $el.select2(sel2Options).on('select2:closing', handleClosing);

  var select2Container = $el.data('select2').$container[0];
  if (select2Container) {
    const tooltipText = $el[0].getAttribute('data-tooltip-text');
    if (tooltipText) {
      select2Container.setAttribute('data-tooltip-text', tooltipText);
      window.initTooltipsInTree(select2Container, {
        // fadeInDelay: 0,
        extraPadding: 20,
      });
    }
  }

  return function cleanup() {
    $el.select2(sel2Options).off('select2:closing', handleClosing);
  };
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
  presetsMgr.init();

  setDungeonERSettings();
  setOverworldERSettings();
  // If returning back from the seed page, the browser will fill in the state.
  // This updates the string after the browser updates all of the fields to
  // their previous values.
  window.addEventListener(
    'load',
    function () {
      try {
        const prevSettingsString = localStorage.getItem(
          'lastGeneratedSettingsString'
        );
        if (prevSettingsString) {
          // Check if in presets and load that preset if was present.
          const name = presetsMgr.getNameForSettingsString(prevSettingsString);
          if (name) {
            updatePresetsSelect(name);
            showPresetToast('Loaded previous settings');
          } else {
            // Else load settings string.
            const error = populateFromSettingsString(prevSettingsString);
            if (error) {
              showPresetToast('Failed to load previous settings', true);
            } else {
              showPresetToast('Loaded previous settings');
            }
          }
        }
      } catch (e) {
        console.error(
          'Failed to retrieve "lastGeneratedSettingsString" from localStorage.',
          e
        );
      }
    },
    { once: true }
  );

  initSettingsModal();
  initManagePresetsModal();
  initSavePresetModal();
  initGeneratingModal();

  document.getElementById('seed').addEventListener('input', (e) => {
    const { value } = e.target;
    e.target.value = normalizeStringToMax128Bytes(value);
  });

  // Make sure select2 auto-focuses on open.
  $(document).on('select2:open', () => {
    document.querySelector('.select2-search__field').focus();
  });

  $('#plandoCheckSelect').select2();
  $('#plandoItemSelect').select2();

  updatePresetsSelect();
  window.initTooltipsInTree(document);
}

function buildPlandoListItemElStr(checkId, checkName, itemId, itemName) {
  return `<li class="plandoListItem" data-itemid="${itemId}" data-checkid="${checkId}">
  <div>
    <button type="button" class="plandoItemDeleteBtn">✖</button>
    <span>${checkName} => ${itemName}</span>
  </div>
</li>`;
}

$('#plandoBtnAdd').on('click', function () {
  const checkName = $('#plandoCheckSelect option:selected').text();
  const checkId = $('#plandoCheckSelect').val();

  const itemName = $('#plandoItemSelect option:selected').text();
  const itemId = $('#plandoItemSelect').val();

  // First remove row if already there.
  $(`.plandoListItem[data-checkid=${checkId}]`).remove();

  $('#basePlandoListbox').prepend(
    buildPlandoListItemElStr(checkId, checkName, itemId, itemName)
  );

  setSettingsString();
});

$(document).on('click', '.plandoItemDeleteBtn', function () {
  $(this).parent().parent().remove();
  setSettingsString();
});

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

// Starting item checkboxes
$('#baseImportantItemsListbox input[type="checkbox"]').each(function () {
  this.addEventListener('click', setSettingsString);
});

// Starting item sliders
$('#baseImportantItemsListbox .liSlider').each(function () {
  const inputEl = this.querySelector('input');
  const textEl = this.querySelector('.liSlider-inputRowText');

  // Handles page load and when returning from next page.
  window.addEventListener(
    'load',
    () => {
      textEl.textContent = inputEl.value;
    },
    { once: true }
  );

  // Every change
  inputEl.addEventListener('input', (e) => {
    textEl.textContent = e.target.value;
  });

  // User releases mouse (or is using keyboard)
  inputEl.addEventListener('change', (e) => {
    textEl.textContent = e.target.value;
    setSettingsString();
  });
});

var settingsLetters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ23456789';
document.getElementById('logicRulesFieldset').onchange = setSettingsString;
document.getElementById('gameRegionFieldset').onchange = setSettingsString;
document.getElementById('seedNumberFieldset').onchange = setSettingsString;
document.getElementById('castleRequirementsFieldset').onchange =
  setCastleRequirementsSettings;
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
document.getElementById('poeSettingsFieldset').onchange = setSettingsString;
document
  .getElementById('shopItemsCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('hiddenSkillsCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('hiddenRupeeCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('gmShortcutCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('hcShortcutCheckbox')
  .addEventListener('click', setSettingsString);
document.getElementById('itemScarcityFieldset').onchange = setSettingsString;
document.getElementById('damageMagFieldset').onchange = setSettingsString;
document.getElementById('todFieldset').onchange = setSettingsString;
document.getElementById('hintDistributionFieldset').onchange =
  setSettingsString;
document.getElementById('trapItemFieldset').onchange = setSettingsString;
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
  .getElementById('skipMajorCutscenesCheckbox')
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
document
  .getElementById('barrenCheckbox')
  .addEventListener('click', setSettingsString);
document;
document.getElementById('goronMinesEntranceFieldset').onchange =
  setSettingsString;
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
  .getElementById('groveEntranceCheckbox')
  .addEventListener('click', setSettingsString);
document.getElementById('totEntranceFieldset').onchange = setSettingsString;
document
  .getElementById('cityEntranceCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('instantTextCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('openMapCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('spinnerSpeedCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('openDotCheckbox')
  .addEventListener('click', setSettingsString);
document.getElementById('walletSizeFieldset').onchange = setSettingsString;
document
  .getElementById('modifyShopModelsCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('bonksDoDamageCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('noSmallKeysOnBossesCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('shuffleRewardsCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('randomizeStartingPointCheckbox')
  .addEventListener('click', setOverworldERSettings);
document.getElementById('iliaQuestFieldset').onchange = setSettingsString;
document.getElementById('mirrorChamberFieldset').onchange = setSettingsString;
document.getElementById('dungeonERFieldset').onchange = setDungeonERSettings;
document
  .getElementById('unpairedEntrancesCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('decoupleEntrancesCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('freestandingRupeeCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('importSettingsStringButton')
  .addEventListener('click', importSettingsString);

document.getElementById('castleRequirementsSlider').oninput =
  setCastleRequirementsValue;
document.getElementById('castleBKRequirementsFieldset').onchange =
  setCastleBKRequirementsSettings;
document.getElementById('castleBKRequirementsSlider').oninput =
  setCastleBKRequirementsValue;

document
  .getElementById('autoFillWalletCheckbox')
  .addEventListener('click', setSettingsString);

document
  .getElementById('skipBridgeDonationCheckbox')
  .addEventListener('click', setSettingsString);

document.getElementById('maloShopDonationSlider').oninput =
  setMaloShopDonationValue;

document.getElementById('hintImportanceFieldset').onchange = setSettingsString;
document
  .getElementById('noPlandoHintsCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('adjustHintsForCompletionistsCheckbox')
  .addEventListener('click', setSettingsString);
document
  .getElementById('hintDungeonEntrancesCheckbox')
  .addEventListener('click', setSettingsString);

function importSettingsString() {
  parseSettingsString(document.getElementById('settingsStringTextbox').value);
}

function setCastleRequirementsSettings() {
  var reqs = document.getElementById('castleRequirementsFieldset').value;
  let sliderName = 'castleRequirementsSlider';

  document.getElementById(sliderName).min = 1;
  document.getElementById(sliderName).value = 1;

  // Hide the slider info if we are not using an option that uses it
  if (reqs == '0' || reqs == '4') {
    document.getElementById(sliderName).hidden = true;
    document.getElementById(sliderName + 'Label').hidden = true;
    document.getElementById(sliderName + 'Output').hidden = true;
  } else {
    document.getElementById(sliderName).hidden = false;
    document.getElementById(sliderName + 'Label').hidden = false;
    document.getElementById(sliderName + 'Output').hidden = false;
  }

  switch (reqs) {
    case '1': {
      // Fused Shadows
      document.getElementById(sliderName).max = 3;
      document.getElementById(sliderName + 'Label').innerHTML =
        'Fused Shadows Required:';

      break;
    }
    case '2': {
      // Mirror Shards
      document.getElementById(sliderName).max = 4;
      document.getElementById(sliderName + 'Label').innerHTML =
        'Mirror Shards Required:';

      break;
    }
    case '3': {
      // Dungeons
      document.getElementById(sliderName).max = 8;
      document.getElementById(sliderName + 'Label').innerHTML =
        'Number of Dungeons Required:';

      break;
    }
    case '5': {
      // Poe Souls
      document.getElementById(sliderName).max = 60;
      document.getElementById(sliderName + 'Label').innerHTML =
        'Poe Souls Required:';

      break;
    }
    case '6': {
      // Hearts
      document.getElementById(sliderName).min = 4;
      document.getElementById(sliderName).value = 4;
      document.getElementById(sliderName).max = 20;
      document.getElementById(sliderName + 'Label').innerHTML =
        'Hearts Required:';

      break;
    }
    default: {
      break;
    }
  }

  document.getElementById(sliderName + 'Output').innerHTML =
    document.getElementById(sliderName).value;
  setSettingsString();
}

function setCastleRequirementsValue() {
  document.getElementById('castleRequirementsSliderOutput').innerHTML =
    document.getElementById('castleRequirementsSlider').value;
  setSettingsString();
}

function setMaloShopDonationValue() {
  document.getElementById('maloShopDonationSliderOutput').innerHTML =
    document.getElementById('maloShopDonationSlider').value;
  setSettingsString();
}

function setCastleBKRequirementsSettings() {
  var reqs = document.getElementById('castleBKRequirementsFieldset').value;
  let sliderName = 'castleBKRequirementsSlider';
  document.getElementById(sliderName).min = 1;
  document.getElementById(sliderName).value = 1;

  // Hide the slider info if we are not using an option that uses it
  if (reqs == '0') {
    document.getElementById(sliderName).hidden = true;
    document.getElementById(sliderName + 'Label').hidden = true;
    document.getElementById(sliderName + 'Output').hidden = true;
  } else {
    document.getElementById(sliderName).hidden = false;
    document.getElementById(sliderName + 'Label').hidden = false;
    document.getElementById(sliderName + 'Output').hidden = false;
  }

  switch (reqs) {
    case '1': {
      // Fused Shadows
      document.getElementById(sliderName).max = 3;
      document.getElementById(sliderName + 'Label').innerHTML =
        'Fused Shadows Required:';

      break;
    }
    case '2': {
      // Mirror Shards
      document.getElementById(sliderName).max = 4;
      document.getElementById(sliderName + 'Label').innerHTML =
        'Mirror Shards Required:';

      break;
    }
    case '3': {
      // Dungeons
      document.getElementById(sliderName).max = 8;
      document.getElementById(sliderName + 'Label').innerHTML =
        'Number of Dungeons Required:';

      break;
    }
    case '4': {
      // Poe Souls
      document.getElementById(sliderName).max = 60;
      document.getElementById(sliderName + 'Label').innerHTML =
        'Poe Souls Required:';

      break;
    }
    case '5': {
      // Hearts
      document.getElementById(sliderName).min = 4;
      document.getElementById(sliderName).value = 4;
      document.getElementById(sliderName).max = 20;
      document.getElementById(sliderName + 'Label').innerHTML =
        'Hearts Required:';

      break;
    }
    default: {
      break;
    }
  }

  document.getElementById(sliderName + 'Output').innerHTML =
    document.getElementById(sliderName).value;
  setSettingsString();
}

function setCastleBKRequirementsValue() {
  document.getElementById('castleBKRequirementsSliderOutput').innerHTML =
    document.getElementById('castleBKRequirementsSlider').value;
  setSettingsString();
}

function setOverworldERSettings() {
  var overworldEREnabled = document.getElementById(
    'randomizeStartingPointCheckbox'
  ).checked;
  document.getElementById('introCheckbox').checked = overworldEREnabled;
  document.getElementById('introCheckbox').disabled = overworldEREnabled;
  setSettingsString();
}

function setDungeonERSettings() {
  if (document.getElementById('dungeonERFieldset').value != 0) {
    document.getElementById('mdhCheckbox').checked = true;
    document.getElementById('mdhCheckbox').disabled = true;
    document.getElementById('unpairedEntrancesCheckbox').disabled = false;
    document.getElementById('decoupleEntrancesCheckbox').disabled = false;
  } else {
    document.getElementById('mdhCheckbox').disabled = false;
    document.getElementById('unpairedEntrancesCheckbox').checked = false;
    document.getElementById('decoupleEntrancesCheckbox').checked = false;
    document.getElementById('unpairedEntrancesCheckbox').disabled = true;
    document.getElementById('decoupleEntrancesCheckbox').disabled = true;
  }
  setSettingsString();
}

function setSettingsString() {
  const combinedSettingsString = window.tpr.shared.genSSettingsFromUi();
  document.getElementById('combinedSettingsString').textContent =
    combinedSettingsString;

  presetsMgr.checkClearSelect(combinedSettingsString);

  return combinedSettingsString;
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
  'poeSettingsFieldset',
  'giftsFromNPCsCheckbox',
  'shopItemsCheckbox',
  'faronTwilightCheckbox',
  'eldinTwilightCheckbox',
  'lanayruTwilightCheckbox',
  'skipMinorCutscenesCheckbox',
  'fastIBCheckbox',
  'quickTransformCheckbox',
  'transformAnywhereCheckbox',
  'trapItemFieldset',
  'baseImportantItemsListbox',
  'baseExcludedChecksListbox',
  'gameRegionFieldset',
  'hiddenSkillsCheckbox',
  'skyCharacterCheckbox',
  'seedNumberFieldset',
  'increaseWalletCheckbox',
  'modifyShopModelsCheckbox',
  'barrenCheckbox',
  'goronMinesEntranceFieldset',
  'lakebedEntranceCheckbox',
  'arbitersEntranceCheckbox',
  'snowpeakEntranceCheckbox',
  'totEntranceFieldset',
  'cityEntranceCheckbox',
  'instantTextCheckbox',
  'openMapCheckbox',
  'spinnerSpeedCheckbox',
  'openDotCheckbox',
];

function saveSettingsString() {
  const settingsString = $('#combinedSettingsString').text().trim();
  const version = $('#envImageVersion').val();

  if (!settingsString) {
    console.warn('No settings string to save.');
    return;
  }

  const payload = JSON.stringify({
    settingsString,
    version: version ?? null, // The explicit null check here is only for dev purposes. This should never happen in a production environment, and isn't a very critical feature anyways.
  });

  localStorage.setItem('settingsString', payload);

  $('#loadString').show();
}

function loadSettingsString() {
  const fieldErrorText = $('#loadFieldError');
  const raw = localStorage.getItem('settingsString');

  if (!raw) {
    console.warn('No saved settings string.');
    return;
  }

  let parsed;
  try {
    parsed = JSON.parse(raw);
  } catch (e) {
    console.warn('Corrupted settings string.');
    localStorage.removeItem('settingsString');
    return;
  }

  const { settingsString, version } = parsed;

  const currentVersion = $('#envImageVersion').val();
  if (version != currentVersion) {
    fieldErrorText.text(
      'Your setting string was saved on a previous version of the generator, rendering it incompatible with the current version. Your saved setting string will now be deleted.'
    );
    localStorage.removeItem('settingsString');
    return;
  }

  const error = populateFromSettingsString(settingsString);
  if (error) {
    fieldErrorText
      .text(
        'Unable to understand the settings string saved in local storage. Please select new settings or enter a new settings string manually.'
      )
      .show();
    return;
  }

  $('#combinedSettingsString').text(`${settingsString}`);
}

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

function doGenerateCall(isRaceSeed) {
  if (generateCallInProgress) {
    return;
  }

  generateCallInProgress = true;

  showGeneratingModal();

  const settingsString =
    window.tpr.shared.genSSettingsFromUi() +
    window.tpr.shared.genPSettingsFromUi();

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
        console.error('`/api/seed/generate` error:');
        if (error.message) {
          showGeneratingModalError(`Error:\n${error}`);
        } else if (error.seedId) {
          showGenModalOngoingRequestError(error.seedId, error.canCancel);
        } else {
          console.error('`/api/seed/generate` unrecognized error');
        }
      } else if (data && data.seedId && data.requesterHash) {
        try {
          localStorage.setItem('lastGeneratedSeedId', data.seedId);
          localStorage.setItem('lastGeneratedSettingsString', settingsString);
          localStorage.setItem('requesterHash', data.requesterHash);
        } catch (e) {
          console.error(
            `Failed to set localStorage values on /api/seed/generate call success.`
          );
          console.error(e);
        }

        window.location.href = `/s/${data.seedId}`;
      } else {
        generateCallInProgress = false;
        console.error('Unrecognized response from `/api/seed/generate`');
        console.error(data);
        showGeneratingModalError(
          'Unrecognized response from `/api/seed/generate`.'
        );
      }
    })
    .catch((err) => {
      generateCallInProgress = false;
      showGeneratingModalError('/api/seed/generate error');
      console.error('/api/seed/generate error');
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
  $('#copySettingsBtn').on('click', function () {
    const text = $('#combinedSettingsString').text().trim();
    navigator.clipboard.writeText(text).then(
      () => {
        showPresetToast('Copied settings');
      },
      (err) => {
        showPresetToast('Failed to copy', true);
      }
    );
  });

  // Init modal
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
      window.tpr.shared.genSSettingsFromUi() +
      window.tpr.shared.genPSettingsFromUi();
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

function validatePresetName(name) {
  if (name.length < 1) {
    return 'Name is required.';
  } else if (name.length > 50) {
    return 'Name must be at most 50 characters.';
  } else {
    const presetTakenResult = presetsMgr.isNameTaken(name);
    if (presetTakenResult) {
      if (presetTakenResult === 'custom') {
        return 'A custom preset with this name already exists.';
      } else {
        return 'A system preset with this name already exists.';
      }
    }
  }
  return '';
}

function initManagePresetsModal() {
  const modal = document.getElementById('managePresetsModal');
  const $modal = $(modal);
  const btn = document.getElementById('managePresets');
  const span = modal.querySelector('.modal-close');
  const $copySuccessText = $('#modalFieldCopiedText');
  const fieldErrorText = document.getElementById('modalFieldError');
  const input = document.getElementById('modalSettingsStringInput');
  const $select = $('#managePresetsModal-selectPreset');
  const $editBtn = $('#managePresetsModal-edit');
  const $deleteBtn = $('#managePresetsModal-delete');
  const $selectError = $('#managePresetsModal-selectError');
  const $pageMain = $('#managePresetsModal-pageMain');
  const $pageEdit = $('#managePresetsModal-pageEdit');
  const $pageDelete = $('#managePresetsModal-pageDelete');
  const nameInput = document.getElementById('managePresetsModal-nameInput');
  const $nameInputError = $('#managePresetsModal-nameInputError');
  const $editError = $('#managePresetsModal-editError');
  const $deleteError = $('#managePresetsModal-deleteError');
  const $mainDebugAlert = $('#managePresetsModal-mainDebugAlert');

  let selectedPresetName = null;

  function setPage(pageName) {
    $mainDebugAlert.hide();
    $pageMain.toggle(pageName === 'main');
    $pageEdit.toggle(pageName === 'edit');
    $pageDelete.toggle(pageName === 'delete');
  }

  function showPresetSelectError(msg) {
    $selectError.text(msg).show();
  }

  $('#managePresetsModal-debug').on('click', () => {
    $mainDebugAlert.hide();
    presetsMgr.getDebugStr().then((errorMsg) => {
      if (errorMsg) {
        $mainDebugAlert.text(errorMsg);
      } else {
        $mainDebugAlert.text('Copied debug string.');
      }

      $mainDebugAlert
        .toggleClass('alert-success-light', !errorMsg)
        .toggleClass('alert-error-light', Boolean(errorMsg))
        .show();
    });
  });

  $editBtn.on('click', function () {
    if (!selectedPresetName) {
      showPresetSelectError('Select a preset');
      return;
    }

    setPage('edit');
    $('#managePresetsModal-editInfo').text(`Renaming "${selectedPresetName}"`);
    nameInput.value = selectedPresetName;
    nameInput.focus();
    $nameInputError.hide();
    $editError.hide();
  });

  nameInput.addEventListener('input', () => {
    $nameInputError.hide();
    $editError.hide();
  });

  $deleteBtn.on('click', function () {
    if (!selectedPresetName) {
      showPresetSelectError('Select a preset');
      return;
    }

    setPage('delete');
    $deleteError.hide();
    $('#managePresetsModal-deleteInfo').text(
      `Are you sure you want to delete "${selectedPresetName}"?`
    );
  });

  $('#managePresetsModal-deleteConfirm').on('click', () => {
    $deleteError.hide();

    const errorMsg = presetsMgr.deletePreset(selectedPresetName);
    if (errorMsg) {
      $deleteError.text(errorMsg).show();
    } else {
      $modal.hide();
      showPresetToast(`Deleted preset "${selectedPresetName}"`);
      updatePresetsSelect();
    }
  });

  $('#managePresetsModal-editBack, #managePresetsModal-deleteBack').on(
    'click',
    () => {
      setPage('main');
    }
  );

  $('#managePresetsModal-editSave').on('click', () => {
    $nameInputError.hide();
    $editError.hide();

    const name = nameInput.value.trim();

    let nameError = '';
    if (name !== selectedPresetName) {
      nameError = validatePresetName(name);
    }
    if (nameError) {
      $nameInputError.text(nameError).show();
      return;
    }

    const errorMsg = presetsMgr.renamePreset(selectedPresetName, name);
    if (errorMsg) {
      $editError.text(errorMsg).show();
    } else {
      $modal.hide();
      showPresetToast('Successfully updated preset');

      const currVal = $('#presetsSelect').val();
      if (currVal && currVal === selectedPresetName) {
        updatePresetsSelect(name);
      } else {
        updatePresetsSelect();
      }
    }
  });

  function handlePresetChange(optionValue) {
    $selectError.hide();
    selectedPresetName = optionValue;
  }

  function presetChangeListener(e) {
    handlePresetChange(e.target.value);
  }

  function initPresetSelect() {
    if ($select.data('select2')) {
      $select.select2('destroy').off('change', presetChangeListener);
    }

    // Set values
    $select.empty();

    const customPresets = presetsMgr.getPresetsByType().custom;
    if (customPresets.length > 0) {
      for (let i = 0; i < customPresets.length; i++) {
        const preset = customPresets[i];
        const option = document.createElement('option');
        option.setAttribute('value', preset.name);
        option.textContent = preset.name;
        $select.append(option);
      }
    }

    $select
      .select2({
        minimumResultsForSearch: 10,
        allowClear: true,
        placeholder: 'Select preset',
      })
      .on('change', presetChangeListener)
      .val('')
      .trigger('change');
  }

  input.addEventListener('input', () => {
    $copySuccessText.hide();
    $(fieldErrorText).hide();
  });

  // When the user clicks the button, open the modal
  btn.addEventListener('click', () => {
    setPage('main');

    $modal.show();

    initPresetSelect();
  });

  span.addEventListener('click', () => {
    $modal.hide();
  });

  let canHide = true;
  $modal
    .on('mousedown', function (e) {
      canHide = e.target === this;
    })
    .on('mouseup', function (e) {
      if (canHide && e.target === this) {
        $modal.hide();
      }
    });
}

function initSavePresetModal() {
  const modal = document.getElementById('savePresetModal');
  const $modal = $(modal);
  const btn = document.getElementById('savePreset');
  const span = modal.querySelector('.modal-close');
  const $fieldErrorText = $('#savePresetModal-nameError');
  const $error = $('#savePresetModal-error');
  const input = document.getElementById('savePresetModal-nameInput');
  const descInput = document.getElementById('savePresetModal-descInput');
  const $presetSelect = $('#savePresetModal-selectPreset');
  const $nameInputBlock = $('#savePresetModal-nameInputBlock');
  const $warning = $('#savePresetModal-warning');

  let newPresetValue = null;

  function handlePresetChange(optionValue) {
    const newPresetSelected = optionValue === newPresetValue;
    $nameInputBlock.toggle(newPresetSelected);
    $warning.toggle(!newPresetSelected);
    hideErrors();

    if (!newPresetSelected) {
      const $option = $presetSelect.find(`option[value="${optionValue}"]`);
      if ($option.length > 0) {
        const option = $option[0];
        const optionText = option.textContent.trim();
        $warning.text(
          `This will overwrite your custom preset "${optionText}"!`
        );
        const preset = presetsMgr.getPresetByName(optionText);
        if (preset) {
          descInput.value = preset.description || '';
        } else {
          descInput.value = '';
        }
      }
    } else {
      descInput.value = '';
    }
  }

  function presetChangeListener(e) {
    handlePresetChange(e.target.value);
  }

  function showError(msg) {
    $error.text(msg).show();
  }

  function showNameError(msg) {
    $fieldErrorText.text(msg).show();
  }

  function hideErrors() {
    $error.hide();
    $fieldErrorText.hide();
  }

  function initPresetSelect() {
    if ($presetSelect.data('select2')) {
      $presetSelect.select2('destroy').off('change', presetChangeListener);
    }

    // Set values
    $presetSelect.empty();

    const takenValues = {};

    const customPresets = presetsMgr.getPresetsByType().custom;
    if (customPresets.length > 0) {
      for (let i = 0; i < customPresets.length; i++) {
        const preset = customPresets[i];
        const option = document.createElement('option');
        option.setAttribute('value', preset.name);
        option.textContent = preset.name;
        $presetSelect.append(option);
        takenValues[preset.name] = true;
      }
    }

    // Ensure unique value to know we are creating a new preset as opposed to
    // updating an existing preset.
    while (newPresetValue == null || takenValues[newPresetValue]) {
      newPresetValue = String(Math.random());
    }
    $presetSelect.prepend(
      $(`<option value="${newPresetValue}">(New preset)</option>`)
    );

    $presetSelect
      .select2({
        minimumResultsForSearch: 10,
      })
      .on('change', presetChangeListener)
      .val(newPresetValue)
      .trigger('change');
  }

  input.addEventListener('input', () => {
    hideErrors();
  });
  descInput.addEventListener('input', () => {
    hideErrors();
  });

  // When the user clicks the button, open the modal
  btn.addEventListener('click', () => {
    input.value = '';
    descInput.value = '';
    handlePresetChange($presetSelect.val());

    $modal.show();

    initPresetSelect();
    input.focus();
  });

  span.addEventListener('click', () => {
    $modal.hide();
  });

  document
    .getElementById('savePresetModal-cancel')
    .addEventListener('click', () => {
      $modal.hide();
    });

  document
    .getElementById('savePresetModal-save')
    .addEventListener('click', () => {
      hideErrors();

      let name = $presetSelect.val();
      if (name === newPresetValue) {
        // Saving new preset. Otherwise we're updating an existing one.
        name = input.value.trim();

        const nameError = validatePresetName(name);
        if (nameError) {
          showNameError(nameError);
          return;
        }
      }

      const presetDiff = {
        name,
        description: descInput.value.trim(),
        origCommit: $('#envGitCommit').val(),
        origSettingsStr: $('#combinedSettingsString').text().trim(),
      };

      const success = presetsMgr.savePreset(presetDiff);
      if (success) {
        updatePresetsSelect(name);
        $modal.hide();
        showPresetToast(`Saved preset "${name}"`);
      } else {
        showError('Failed to save preset');
      }
    });

  let canHide = true;
  $modal
    .on('mousedown', function (e) {
      canHide = e.target === this;
    })
    .on('mouseup', function (e) {
      if (canHide && e.target === this) {
        $modal.hide();
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
    setHiddenUIValues(byType.s);
    populateSSettings(byType.s);
  }

  if (byType.p) {
    window.tpr.shared.populateUiFromPSettings(byType.p);
  }

  setSettingsString();

  return null;
}

function setHiddenUIValues(s) {
  let val = s.castleRequirementCount;
  document.getElementById('castleRequirementsSliderOutput').innerHTML =
    s.castleRequirementCount;

  document.getElementById('castleRequirementsSlider').min = 1;
  // Hide the slider info if we are not using an option that uses it
  if (s.castleRequirements == 0 || s.castleRequirements == 4) {
    document.getElementById('castleRequirementsSlider').hidden = true;
    document.getElementById('castleRequirementsSliderLabel').hidden = true;
    document.getElementById('castleRequirementsSliderOutput').hidden = true;
  } else {
    document.getElementById('castleRequirementsSlider').hidden = false;
    document.getElementById('castleRequirementsSliderLabel').hidden = false;
    document.getElementById('castleRequirementsSliderOutput').hidden = false;
  }

  switch (s.castleRequirements) {
    case 1: {
      // Fused Shadows
      document.getElementById('castleRequirementsSlider').max = 3;
      document.getElementById('castleRequirementsSliderLabel').innerHTML =
        'Fused Shadows Required:';

      break;
    }
    case 2: {
      // Mirror Shards
      document.getElementById('castleRequirementsSlider').max = 4;
      document.getElementById('castleRequirementsSliderLabel').innerHTML =
        'Mirror Shards Required:';

      break;
    }
    case 3: {
      // Dungeons
      document.getElementById('castleRequirementsSlider').max = 8;
      document.getElementById('castleRequirementsSliderLabel').innerHTML =
        'Number of Dungeons Required:';

      break;
    }
    case 5: {
      // Poe Souls
      document.getElementById('castleRequirementsSlider').max = 60;
      document.getElementById('castleRequirementsSliderLabel').innerHTML =
        'Poe Souls Required:';

      break;
    }
    case 6: {
      // Hearts
      document.getElementById('castleRequirementsSlider').min = 4; // Maybe 4, because 3 would match "Open"
      document.getElementById('castleRequirementsSlider').max = 20;
      document.getElementById('castleRequirementsSliderLabel').innerHTML =
        'Hearts Required:';

      break;
    }
    default: {
      break;
    }
  }

  val = s.castleBKRequirementCount;
  document.getElementById('castleBKRequirementsSliderOutput').innerHTML =
    s.castleBKRequirementCount;

  document.getElementById('castleBKRequirementsSliderOutput').min = 1;
  // Hide the slider info if we are not using an option that uses it
  if (s.castleBKRequirements == 0) {
    document.getElementById('castleBKRequirementsSlider').hidden = true;
    document.getElementById('castleBKRequirementsSliderLabel').hidden = true;
    document.getElementById('castleBKRequirementsSliderOutput').hidden = true;
  } else {
    document.getElementById('castleBKRequirementsSlider').hidden = false;
    document.getElementById('castleBKRequirementsSliderLabel').hidden = false;
    document.getElementById('castleBKRequirementsSliderOutput').hidden = false;
  }

  switch (s.castleBKRequirements) {
    case 1: {
      // Fused Shadows
      document.getElementById('castleBKRequirementsSlider').max = 3;
      document.getElementById('castleBKRequirementsSliderLabel').innerHTML =
        'Fused Shadows Required:';

      break;
    }
    case 2: {
      // Mirror Shards
      document.getElementById('castleBKRequirementsSlider').max = 4;
      document.getElementById('castleBKRequirementsSliderLabel').innerHTML =
        'Mirror Shards Required:';

      break;
    }
    case 3: {
      // Dungeons
      document.getElementById('castleBKRequirementsSlider').max = 8;
      document.getElementById('castleBKRequirementsSliderLabel').innerHTML =
        'Number of Dungeons Required:';

      break;
    }
    case 4: {
      // Poe Souls
      document.getElementById('castleBKRequirementsSlider').max = 60;
      document.getElementById('castleBKRequirementsSliderLabel').innerHTML =
        'Poe Souls Required:';

      break;
    }
    case 5: {
      // Hearts
      document.getElementById('castleBKRequirementsSlider').min = 4;
      document.getElementById('castleBKRequirementsSlider').max = 20;
      document.getElementById('castleBKRequirementsSliderLabel').innerHTML =
        'Hearts Required:';

      break;
    }
    default: {
      break;
    }
  }
  document.getElementById('maloShopDonationSliderOutput').innerHTML =
    s.maloShopDonation;
}

function populateSSettings(s) {
  if (!s) {
    return;
  }

  window.tpr.shared.uncheckCheckboxes([
    'randomizationSettingsTab',
    'gameplaySettingsTab',
    'excludedChecksTab',
    'startingInventoryTab',
  ]);
  window.tpr.shared.setSlidersToMin(['startingInventoryTab']);

  $('#logicRulesFieldset').val(s.logicRules);
  $('#castleRequirementsFieldset').val(s.castleRequirements);
  $('#palaceRequirementsFieldset').val(s.palaceRequirements);
  $('#faronLogicFieldset').val(s.faronWoodsLogic);
  $('#goldenBugsCheckbox').prop('checked', s.goldenBugs);
  $('#skyCharacterCheckbox').prop('checked', s.skyCharacters);
  $('#giftsFromNPCsCheckbox').prop('checked', s.giftsFromNpcs);
  $('#poeSettingsFieldset').val(s.poes);
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
  $('#skipMajorCutscenesCheckbox').prop('checked', s.skipMajorCutscenes);
  $('#fastIBCheckbox').prop('checked', s.fastIronBoots);
  $('#quickTransformCheckbox').prop('checked', s.quickTransform);
  $('#transformAnywhereCheckbox').prop('checked', s.transformAnywhere);
  $('#walletSizeFieldset').val(s.walletSize);
  $('#modifyShopModelsCheckbox').prop(
    'checked',
    s.shopModelsShowTheReplacedItem
  );
  $('#trapItemFieldset').val(s.trapItemsFrequency);
  $('#barrenCheckbox').prop('checked', s.barrenDungeons);
  $('#goronMinesEntranceFieldset').val(s.goronMinesEntrance);
  $('#lakebedEntranceCheckbox').prop('checked', s.skipLakebedEntrance);
  $('#arbitersEntranceCheckbox').prop('checked', s.skipArbitersEntrance);
  $('#snowpeakEntranceCheckbox').prop('checked', s.skipSnowpeakEntrance);
  $('#groveEntranceCheckbox').prop('checked', s.skipGroveEntrance);
  $('#totEntranceFieldset').val(s.totEntrance);
  $('#cityEntranceCheckbox').prop('checked', s.skipCityEntrance);
  $('#instantTextCheckbox').prop('checked', s.instantText);
  $('#itemScarcityFieldset').val(s.itemScarcity);
  $('#damageMagFieldset').val(s.damageMagnification);
  $('#bonksDoDamageCheckbox').prop('checked', s.bonksDoDamage);
  $('#shuffleRewardsCheckbox').prop('checked', s.shuffleRewards);
  $('#openMapCheckbox').prop('checked', s.openMap);
  $('#spinnerSpeedCheckbox').prop('checked', s.increaseSpinnerSpeed);
  $('#openDotCheckbox').prop('checked', s.openDot);
  $('#noSmallKeysOnBossesCheckbox').prop('checked', s.noSmallKeysOnBosses);
  $('#todFieldset').val(s.startingToD);
  $('#hintDistributionFieldset').val(s.hintDistribution);
  $('#randomizeStartingPointCheckbox').prop(
    'checked',
    s.randomizeStartingPoint
  );
  $('#hiddenRupeeCheckbox').prop('checked', s.hiddenRupees);
  $('#gmShortcutCheckbox').prop('checked', s.gmShortcut);
  $('#hcShortcutCheckbox').prop('checked', s.hcShortcut);
  $('#iliaQuestFieldset').val(s.iliaQuest);
  $('#mirrorChamberFieldset').val(s.mirrorChamber);
  $('#dungeonERFieldset').val(s.dungeonER).trigger('change');
  $('#unpairedEntrancesCheckbox').prop('checked', s.unpairEntrances);
  $('#decoupleEntrancesCheckbox').prop('checked', s.decoupleEntrances);
  $('#freestandingRupeeCheckbox').prop('checked', s.freestandingRupees);
  $('#castleRequirementsSlider').val(s.castleRequirementCount);
  $('#castleBKRequirementsFieldset').val(s.castleBKRequirements);
  $('#castleBKRequirementsSlider').val(s.castleBKRequirementCount);
  $('#autoFillWalletCheckbox').prop('checked', s.autoFillWallet);
  $('#skipBridgeDonationCheckbox').prop('checked', s.skipBridgeDonation);
  $('#maloShopDonationSlider').val(s.maloShopDonation);
  $('#hintImportanceFieldset').val(s.hintImportance);
  $('#noPlandoHintsCheckbox').prop('checked', s.noPlandoHints);
  $('#adjustHintsForCompletionistsCheckbox').prop(
    'checked',
    s.adjustHintsForCompletionists
  );
  $('#hintDungeonEntrancesCheckbox').prop('checked', s.hintDungeonEntrances);

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
        `input[type="checkbox"][data-itemid="${id}"]`
      );

      for (let i = 0; i < count && i < checkboxes.length; i++) {
        checkboxes[i].checked = true;
      }

      const inputRanges = $startingItemsParent.find(
        `input[type="range"][data-itemid="${id}"]`
      );

      for (let i = 0; i < inputRanges.length; i++) {
        const inputRange = inputRanges[i];
        inputRange.value = count;
        inputRange.dispatchEvent(new Event('input', { bubbles: true }));
      }
    });
  }

  $('#basePlandoListbox').empty();
  s.plando.forEach((p) => {
    const checkId = p[0];
    const itemId = p[1];

    const checkName = $(`#plandoCheckSelect option[value=${checkId}]`).text();
    const itemName = $(`#plandoItemSelect option[value=${itemId}]`).text();

    $('#basePlandoListbox').append(
      buildPlandoListItemElStr(checkId, checkName, itemId, itemName)
    );
  });
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

function showPresetToast(msg, isError) {
  const $toast = $('#presetUpdateStatus');
  $toast
    .toggleClass('preset-toast-error', Boolean(isError))
    .text(msg)
    .css('display', 'block');

  // Allow time for display to apply before adding transition class
  requestAnimationFrame(() => {
    $toast.addClass('show');
  });

  setTimeout(() => {
    $toast.removeClass('show');
    setTimeout(() => {
      $toast.css('display', 'none');
    }, 200);
  }, 3000);
}

function updatePresetsSelect(defaultToValue) {
  const $select = $('#presetsSelect');

  if ($select.data('select2')) {
    $select
      .select2('destroy')
      .off('change', handleChange)
      .off('select2:unselecting', handleUnselecting)
      .off('select2:opening', handleOpening);

    if (presetsMgr.state.cleanupFn) {
      presetsMgr.state.cleanupFn();
      presetsMgr.state.cleanupFn = null;
    }
  }

  const presetsByType = presetsMgr.getPresetsByType();

  $select.empty();
  $select.append($(`<option value=""></option>`));

  const presetTypes = Object.keys(presetsByType);
  for (let typeIdx = 0; typeIdx < presetTypes.length; typeIdx++) {
    const presetType = presetTypes[typeIdx];
    const label = presetType === 'system' ? 'System' : 'Custom';
    const presets = presetsByType[presetType];

    const optGroup = document.createElement('optgroup');
    if (typeIdx === 0) {
      optGroup.setAttribute('label', 'System');
    } else {
      optGroup.setAttribute('label', 'Custom');
    }

    for (let i = 0; i < presets.length; i++) {
      const preset = presets[i];
      const option = document.createElement('option');
      option.setAttribute('value', preset.name);
      if (preset.description) {
        option.setAttribute('data-tooltip-text', preset.description);
      }
      option.textContent = preset.name;
      optGroup.append(option);
    }

    if (presets.length > 0) {
      $select.append(optGroup);
    }
  }

  function handleChange(e) {
    const val = e.target.value;
    const matchedPrevValue =
      presetsMgr.state.prevValue !== undefined &&
      presetsMgr.state.prevValue === val;
    presetsMgr.state.prevValue = val;
    if (matchedPrevValue) {
      return;
    }

    if (val) {
      const error = presetsMgr.loadSettings(val);
      if (error) {
        showPresetToast('Failed to load preset', true);
      }
    }
  }

  function handleUnselecting() {
    $(this).data('unselecting', true);
  }

  function handleOpening(e) {
    const $this = $(this);
    if ($this.data('unselecting')) {
      $this.removeData('unselecting');
      e.preventDefault();
    }
  }

  presetsMgr.state.cleanupFn = initSelect2('presetsSelect', {
    allowClear: true,
    default: null,
    placeholder: 'Select preset',
  });

  $select
    .on('change', handleChange)
    .on('select2:unselecting', handleUnselecting)
    .on('select2:opening', handleOpening);

  // For showing newly created custom option as the current selection.
  if (defaultToValue) {
    $select.val(defaultToValue).trigger('change');
  }
}
