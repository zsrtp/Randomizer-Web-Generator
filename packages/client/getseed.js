(() => {
  const $ = window.$;

  const SeedGenProgress = {
    Queued: 'Queued',
    Started: 'Started',
    Done: 'Done',
  };

  const PageStates = {
    default: 'default',
    inQueue: 'inQueue',
    beingWorked: 'beingWorked',
    generationSuccess: 'generationSuccess',
    generationFailure: 'generationFailure',
    progressCallFailure: 'progressCallFailure',
  };

  const Region = {
    All: 0,
    USA: 1,
    EUR: 2,
    JAP: 3,
  };
  const regionBitLength = 2;

  const EurLanguageTag = {
    English: 0,
    Deutsch: 2,
    Español: 4,
    Français: 1,
    Italiano: 3,
  };
  const eurLangTagBitLength = 3;

  let pageData;
  let creationCallInProgress;
  let picrossOpened = false;
  let selectedRegion = null;
  let selectedLanguage = null;
  let hasSelectedRegionError = false;
  let defaultIncludeSpoilerLog = false;

  function createBasicEvent() {
    let listeners = [];

    return {
      subscribe: function (newListener) {
        listeners.push(newListener);
      },
      hasListener: function (fn) {
        return listeners.indexOf(fn) >= 0;
      },
      notify: function () {
        listeners.forEach((listener) => {
          if (typeof listener === 'function') {
            listener();
          }
        });
      },
    };
  }

  const regionSelectedEvent = createBasicEvent();
  const languageSelectedEvent = createBasicEvent();

  const RawSettingType = {
    nineBitWithEndOfListPadding: 'nineBitWithEndOfListPadding',
    bitString: 'bitString',
    xBitNum: 'xBitNum',
    rgb: 'rgb',
    midnaHairBase: 'midnaHairBase',
    midnaHairTips: 'midnaHairTips',
  };

  const RecolorId = {
    herosClothes: 0x00, // Cap and Body
    zoraArmor: 0x01,
    zoraArmorSecondary: 0x02,
    zoraArmorHelmet: 0x03,
  };

  function isRgbHex(str) {
    return /^[a-fA-F0-9]{6}$/.test(str);
  }

  function genTunicRecolorDef(id, recolorId) {
    const select = document.getElementById(id);
    const selectedOption = select.children[select.selectedIndex];
    return {
      recolorId: recolorId,
      rgb: selectedOption.getAttribute('data-rgb'),
    };
  }

  function genRecolorBits() {
    let recolorDefs = [];

    // Add recolorDefs to list.
    recolorDefs.push(
      genTunicRecolorDef('hTunicHatColorFieldset', RecolorId.herosClothes)
    );
    recolorDefs.push(
      genTunicRecolorDef('hTunicBodyColorFieldset', RecolorId.herosClothes)
    );
    recolorDefs.push(
      genTunicRecolorDef('hTunicSkirtColorFieldset', RecolorId.herosClothes)
    );
    recolorDefs.push(
      genTunicRecolorDef('zTunicHatColorFieldset', RecolorId.zoraArmor)
    );

    recolorDefs.push(
      genTunicRecolorDef('zTunicHelmetColorFieldset', RecolorId.zoraArmor)
    );
    recolorDefs.push(
      genTunicRecolorDef('zTunicBodyColorFieldset', RecolorId.zoraArmor)
    );
    recolorDefs.push(
      genTunicRecolorDef('zTunicScalesColorFieldset', RecolorId.zoraArmor)
    );
    recolorDefs.push(
      genTunicRecolorDef('zTunicBootsColorFieldset', RecolorId.zoraArmor)
    );
    recolorDefs.push(
      genTunicRecolorDef('linkHairColorFieldset', RecolorId.herosClothes)
    );

    // Process all recolorDefs
    recolorDefs = recolorDefs.filter(function (recolorDef) {
      return recolorDef && isRgbHex(recolorDef.rgb);
    });

    recolorDefs.sort(function (defA, defB) {
      return defA.recolorId - defB.recolorId;
    });

    if (recolorDefs.length < 1) {
      return {
        type: RawSettingType.bitString,
        bitString: '0000000000000000', // 16 zeroes
      };
    }

    const enabledRecolorIds = {};
    let rgbBits = '';

    recolorDefs.forEach(function (recolorDef) {
      enabledRecolorIds[recolorDef.recolorId] = true;
      rgbBits += toPaddedBits(parseInt(recolorDef.rgb, 16), 24);
    });

    const recolorIdEnabledBitsLength =
      recolorDefs[recolorDefs.length - 1].recolorId + 1;

    let bitString = toPaddedBits(recolorIdEnabledBitsLength, 16);

    for (let i = 0; i < recolorIdEnabledBitsLength; i++) {
      bitString += enabledRecolorIds[i] ? '1' : '0';
    }

    bitString += rgbBits;

    return {
      type: RawSettingType.bitString,
      bitString: bitString,
    };
  }

  function byId(id) {
    return document.getElementById(id);
  }

  /**
   * Adds '0' chars to front of bitStr such that the returned string length is
   * equal to the provided size. If bitStr.length > size, simply returns bitStr.
   *
   * @param {string} bitStr String of '0' and '1' chars
   * @param {number} size Desired length of resultant string.
   * @return {string}
   */
  function padBits(bitStr, size) {
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
    return padBits(number.toString(2), strLength);
  }

  window.addEventListener('DOMContentLoaded', onDomContentLoaded);

  function onDomContentLoaded() {
    const inputJsonDataEl = document.getElementById('inputJsonData');
    if (inputJsonDataEl) {
      handleGenerationCompletedPage(inputJsonDataEl);
    } else {
      let shouldCheckProgress = false;

      const requesterHashEl = document.getElementById('requesterHash');
      if (requesterHashEl) {
        try {
          const requesterHash = localStorage.getItem('requesterHash');
          if (requesterHash === requesterHashEl.value) {
            shouldCheckProgress = true;
          }
        } catch (e) {
          shouldCheckProgress = true;
        }
      }

      if (shouldCheckProgress) {
        handleCheckProgressPage();
      } else {
        handleInvalidSeedPage();
      }
    }
  }

  function handleGenerationCompletedPage(inputJsonDataEl) {
    $('#sectionProgress').hide();
    $('#sectionFileCreation').show();

    restoreDefaultFcSettings();

    pageData = JSON.parse(inputJsonDataEl.value);

    const decodedSettings = window.tpr.shared.decodeSettingsString(
      pageData.input.settings
    );

    initTabButtons([
      {
        buttonId: 'cosmeticsTabBtn',
        contentId: 'cosmeticsTab',
      },
      {
        buttonId: 'audioTabBtn',
        contentId: 'audioTab',
      },

      // ['mainTab', 'cosmeticsTab', 'audioTab'].forEach((id) => {
    ]);
    fillInInfo();

    window.tpr.shared.populateUiFromPSettings(decodedSettings.p);

    initSettingsModal();
    initShareModal();

    $('#create').on('click', handleCreateClick);

    handleSpoilerData();

    initCustomColorPickers();

    function handleToggleTranslationsWarning() {
      let showTranslationsWarning = false;
      if (selectedRegion !== 'USA' && selectedLanguage !== 'English') {
        showTranslationsWarning =
          selectedRegion !== 'EUR' || selectedLanguage !== 'Français';
      }

      $('#translationsWarning').toggle(showTranslationsWarning);
    }

    function handleRegionChange() {
      updateLangDisplay();

      handleToggleTranslationsWarning();
    }

    // Run once at startup
    handleRegionChange();

    regionSelectedEvent.subscribe(() => {
      if (hasSelectedRegionError) {
        hasSelectedRegionError = false;
        $('#downloadsParent').hide();
      }

      handleRegionChange();
    });

    languageSelectedEvent.subscribe(() => {
      handleToggleTranslationsWarning();
    });
  }

  function updateLangDisplay() {
    const shouldShowLangSection =
      selectedRegion == 'All' || selectedRegion === 'EUR';

    $('#downloadOptionsLanguageFilterGroup').toggle(shouldShowLangSection);
  }

  function restoreDefaultFcSettings() {
    try {
      const fcSettingsDefault = JSON.parse(
        window.localStorage.getItem('fcSettingsDefault')
      );

      if (Region.hasOwnProperty(fcSettingsDefault.region)) {
        selectedRegion = fcSettingsDefault.region;
      }

      if (EurLanguageTag.hasOwnProperty(fcSettingsDefault.eurLanguage)) {
        selectedLanguage = fcSettingsDefault.eurLanguage;
      }

      if (typeof fcSettingsDefault.includeSpoilerLog === 'boolean') {
        defaultIncludeSpoilerLog = fcSettingsDefault.includeSpoilerLog;
      }
    } catch (e) {
      // do nothing
    }

    if (!EurLanguageTag.hasOwnProperty(selectedLanguage)) {
      pickDefaultEurLanguage();
    }
  }

  function pickDefaultEurLanguage() {
    let locales = navigator.languages;
    if (locales == null) {
      const navLang = navigator.language;
      if (navLang != null) {
        locales = [navLang];
      }
    }

    const localeMapping = {
      en: 'English',
      de: 'Deutsch',
      es: 'Español',
      fr: 'Français',
      it: 'Italiano',
    };

    if (Array.isArray(locales)) {
      for (let i = 0; i < locales.length; i++) {
        let locale = locales[i];

        if (typeof locale === 'string') {
          const dashIndex = locale.indexOf('-');
          if (dashIndex >= 0) {
            locale = locale.substring(0, dashIndex);
          }

          const lang = localeMapping[locale];

          if (EurLanguageTag.hasOwnProperty(lang)) {
            selectedLanguage = lang;
            return;
          }
        }
      }
    }

    if (!EurLanguageTag.hasOwnProperty(selectedLanguage)) {
      selectedLanguage = 'English';
    }
  }

  function handleSpoilerData() {
    $('#settingsSpoilerSection').show();

    const rawSpoilerData = document.getElementById('spoilerData').value;
    const spoilerData = JSON.parse(rawSpoilerData);
    console.log(spoilerData);

    initTabButtons([
      {
        buttonId: 'tabBtnSharedSettings',
        contentId: 'tabSharedSettings',
      },
      {
        buttonId: 'tabBtnPlaythroughSpoilers',
        contentId: 'tabPlaythroughSpoilers',
      },
    ]);

    fillInSettingsTable(spoilerData);

    if (!spoilerData.isRaceSeed) {
      $('#spoilerSectionTitle').text('Settings & Spoilers');
      $('#tabBtnPlaythroughSpoilers').show();

      initPlaythroughSpoilers(spoilerData);
      createSpoilerLogDownload(spoilerData, rawSpoilerData);
    }

    initDownloadOptions(spoilerData.isRaceSeed);
  }

  function initDownloadOptions(isRaceSeed) {
    const downloadOptionsRegionFilterGroupEl = document.getElementById(
      'downloadOptionsRegionFilterGroup'
    );

    renderFilterGroup({
      rootEl: downloadOptionsRegionFilterGroupEl,
      title: 'Region',
      filters: ['USA', 'EUR', 'JAP', 'All'],
      defaultFilter: selectedRegion,
      onFilterSelected: (filter) => {
        selectedRegion = filter;
        regionSelectedEvent.notify();
        // selectedLocationFilter = filter;
        // onFilterChange();
      },
    });

    const downloadOptionsLanguageFilterGroupEl = document.getElementById(
      'downloadOptionsLanguageFilterGroup'
    );

    renderFilterGroup({
      rootEl: downloadOptionsLanguageFilterGroupEl,
      title: 'EUR Language',
      filters: ['English', 'Deutsch', 'Español', 'Français', 'Italiano'],
      defaultFilter: selectedLanguage,
      onFilterSelected: (filter) => {
        selectedLanguage = filter;
        languageSelectedEvent.notify();
      },
    });

    if (!isRaceSeed) {
      renderBasicCheckbox({
        parent: document.getElementById('downloadOptionsSpoilerCheckboxParent'),
        checkboxId: 'includeSpoilerCheckbox',
        text: 'Include spoiler log',
        defaultChecked: defaultIncludeSpoilerLog,
      });
    }
  }

  // Return value is an array of 16 colors like [ "69567a", "6d5980", ...]. If
  // don't provide a baseHue or the provided value is invalid, picks a random
  // baseHue.
  function get16ColorsPalette(baseHue) {
    if (typeof baseHue !== 'number' || baseHue < 0) {
      baseHue = Math.random() * 360;
    } else if (baseHue >= 360) {
      baseHue = baseHue % 360;
    }
    baseHue = Math.floor(baseHue);

    return (
      new window.ColorScheme()
        .from_hue(baseHue) // Start the scheme
        .scheme('triade') // Use the 'tetrade' scheme, that is, colors
        // selected from 3 points equidistant around
        // the color wheel.
        .variation('soft') // Use the 'soft' color variation
        .distance(0.5)
        .colors()
    );
  }

  // `colorHex` does not have the '#' at the front.
  function randomizeCosmeticSetting(elId, colorHex, preventCustomColor) {
    const element = document.getElementById(elId);

    if (!preventCustomColor && elId.includes('ColorPicker')) {
      // Is a custom color input.

      // Set color input's value and trigger 'input' event.
      const hexValue = '#' + colorHex;
      element.value = hexValue;
      element.setAttribute('value', hexValue);
      element.dispatchEvent(new Event('input', { bubbles: true }));

      // Change selected option to be the Custom one.
      const selectEl = document.getElementById(elId.replace('ColorPicker', ''));
      const $customOption = $(selectEl).find('option[data-custom-color]');
      if ($customOption.length > 0) {
        const customOption = $customOption[0];
        $(selectEl).val(customOption.value);
        selectEl.dispatchEvent(new Event('change', { bubbles: true }));
      }
    } else {
      const options = element.getElementsByTagName('option');

      if (preventCustomColor) {
        let possibleIndexes = [];
        for (let i = 0; i < options.length; i++) {
          if (options[i].getAttribute('data-custom-color') !== 'true') {
            possibleIndexes.push(i);
          }
        }

        const indexIndex = Math.floor(Math.random() * possibleIndexes.length);
        element.selectedIndex = possibleIndexes[indexIndex];

        // Trigger event so color picker input hides.
        const selectEl = document.getElementById(
          elId.replace('ColorPicker', '')
        );
        selectEl.dispatchEvent(new Event('change', { bubbles: true }));
      } else {
        element.selectedIndex = Math.floor(Math.random() * options.length);
      }
    }
  }

  function renderBasicCheckbox({ parent, defaultChecked, text, checkboxId }) {
    const label = document.createElement('label');
    label.className = 'basicCheckboxLabel';
    const checkbox = document.createElement('input');
    checkbox.setAttribute('type', 'checkbox');
    if (checkboxId) {
      checkbox.setAttribute('id', checkboxId);
    }
    if (defaultChecked) {
      checkbox.setAttribute('checked', 'checked');
    }
    label.appendChild(checkbox);

    const span = document.createElement('span');
    span.className = 'basicCheckboxSpan';
    span.textContent = text;
    label.appendChild(span);

    parent.appendChild(label);

    return label;
  }

  document
    .getElementById('randomizeCosmeticsButton')
    .addEventListener('click', randomizeCosmetics);

  function randomizeCosmetics() {
    const arrayOfCosmeticSettings = [
      [
        'linkHairColorFieldsetColorPicker',
        'hTunicHatColorFieldsetColorPicker',
        'hTunicBodyColorFieldsetColorPicker',
        'hTunicSkirtColorFieldsetColorPicker',
      ],
      [
        'zTunicHatColorFieldsetColorPicker',
        'zTunicHelmetColorFieldsetColorPicker',
        'zTunicBodyColorFieldsetColorPicker',
        'zTunicScalesColorFieldsetColorPicker',
        'zTunicBootsColorFieldsetColorPicker',
      ],
      'lanternColorFieldsetColorPicker',
      'heartColorFieldset',
      'aButtonColorFieldset',
      'bButtonColorFieldset',
      'xButtonColorFieldset',
      'yButtonColorFieldset',
      'zButtonColorFieldset',
      { id: 'midnaHairBaseColorFieldset', preventCustomColor: true },
      { id: 'midnaHairTipColorFieldset', preventCustomColor: true },
      'midnaDomeRingColorFieldset',
    ];

    for (let i = 0; i < arrayOfCosmeticSettings.length; i++) {
      const entry = arrayOfCosmeticSettings[i];
      if (Array.isArray(entry)) {
        const colors = get16ColorsPalette();

        for (let j = 0; j < entry.length; j++) {
          const elId = entry[j];
          const randomIndex = Math.floor(Math.random() * colors.length);
          randomizeCosmeticSetting(elId, colors[randomIndex]);
        }
      } else if (typeof entry === 'object') {
        if (entry) {
          randomizeCosmeticSetting(entry.id, null, entry.preventCustomColor);
        }
      } else {
        const elId = arrayOfCosmeticSettings[i];
        randomizeCosmeticSetting(elId, get16ColorsPalette()[0]);
      }
    }
  }

  function initPlaythroughSpoilers(spoilerData) {
    // const ids = ['tabAgitha', 'tabArbiters'].map((contentId) => {
    //   return {
    //     buttonId: 'tabBtn' + contentId.substring(3),
    //     contentId,
    //   };
    // });

    // initTabButtons(ids);

    // const agithaMap = {};
    // const arbitersMap = {};

    // Object.keys(spoilerData.itemPlacements).forEach((checkName) => {
    //   const value = spoilerData.itemPlacements[checkName];

    //   if (checkName.startsWith('Agitha')) {
    //     agithaMap[checkName] = value;
    //   } else if (checkName.startsWith('Arbiter')) {
    //     arbitersMap[checkName] = value;
    //   }
    // });

    // initPlaythroughSpoilersTable('tabAgitha', agithaMap);
    // initPlaythroughSpoilersTable('tabArbiters', arbitersMap);

    const skippingGroups = {
      ARC: true,
      REL: true,
      DZX: true,
      ObjectARC: true,
      Overworld: true,
      Dungeon: true,
      'Dungeon Items': true,
      'Dungeon Map': true,
      Chest: true,
      'Big Key': true,
      'Small Key': true,
      Compass: true,
      'Ordon Pumpkin': true,
      Boss: true,
      Cutscene: true,

      // Ones we actually want to be selectable in the UI
      Npc: true,
      'Bug Reward': true,
      'Dungeon Reward': true,
      'Golden Bug': true,
      'Heart Container': true,
      'Hidden Skill': true,
      Shop: true,
      'Sky Book': true,
      Poe: true,
    };
    const groups = {};
    const multiTagThings = {};

    const checkData = window.tpr.checkData;
    Object.keys(checkData).forEach((key) => {
      const arr = checkData[key].category;

      const newArr = arr.filter((value) => {
        return !skippingGroups[value];
      });

      newArr.forEach((group) => {
        groups[group] = true;
        if (newArr.length > 1) {
          const multiKey = newArr.join('____');
          if (multiTagThings[multiKey]) {
            multiTagThings[multiKey].push(key);
          } else {
            multiTagThings[multiKey] = [key];
          }
        }
      });
    });

    console.log('groups');
    console.log(groups);
    console.log('multiTagThings');
    console.log(multiTagThings);

    initPlaythroughSpoilersTable2(spoilerData, checkData);
  }

  function initPlaythroughSpoilersTable2(spoilerData, checkData) {
    const skippingGroups = {
      ARC: true,
      REL: true,
      DZX: true,
      ObjectARC: true,
      Overworld: true,
      Dungeon: true,
      'Dungeon Items': true,
      'Dungeon Map': true,
      Chest: true,
      'Big Key': true,
      'Small Key': true,
      Compass: true,
      'Ordon Pumpkin': true,
      Boss: true,
      Cutscene: true,
    };

    const lookupTable = {};

    Object.keys(checkData).forEach((checkName) => {
      const checkObj = checkData[checkName];

      const tagsForCheck = checkObj.category.reduce((acc, group) => {
        if (!skippingGroups[group]) {
          acc[group] = true;
        }
        return acc;
      }, {});

      let value = spoilerData.itemPlacements[checkName];

      if (value) {
        // if (value === checkObj.itemId) {
        //   value += ' (V)';
        // }
      } else {
        // value = checkObj.itemId + ' (V)';
        value = checkObj.itemId;
      }

      lookupTable[checkName] = {
        tags: tagsForCheck,
        // value: value || checkObj.itemId + '@@@@@@@@',
        // value: value || checkObj.itemId,
        value,
        vanillaItem: checkObj.itemId,
      };
    });

    console.log('lookupTable');
    console.log(lookupTable);

    initSpoilersFilters(lookupTable);
  }

  function initSpoilersFilters(lookupTable) {
    const locationFiltersSet = {
      'Arbiters Grounds': true,
      'Bulblin Camp': true,
      'Castle Town': true,
      'Cave of Ordeals': true,
      'City in The Sky': true,
      'Death Mountain': true,
      'Eldin Lantern Cave': true,
      'Eldin Stockcave': true,
      'Faron Woods': true,
      'Fishing Hole': true,
      'Forest Temple': true,
      'Gerudo Desert': true,
      'Goron Mines': true,
      'Hidden Village': true,
      'Hyrule Castle': true,
      'Hyrule Field - Faron Province': true,
      'Hyrule Field - Eldin Province': true,
      'Hyrule Field - Lanayru Province': true,
      'Kakariko Village': true,
      'Kakariko Graveyard': true,
      'Lake Hylia': true,
      'Lake Lantern Cave': true,
      'Lakebed Temple': true,
      'Ordona Province': true,
      'Palace of Twilight': true,
      'Sacred Grove': true,
      'Snowpeak Province': true,
      'Snowpeak Ruins': true,
      'Temple of Time': true,
      'Upper Zoras River': true,
      'Zoras Domain': true,
    };

    const typeFiltersSet = {
      'Bug Reward': true,
      'Dungeon Reward': true,
      'Golden Bug': true,
      'Heart Container': true,
      'Hidden Skill': true,
      Npc: true,
      Shop: true,
      'Sky Book': true,
      Poe: true,
    };

    const importantItems = {
      Progressive_Sword: true,
      Progressive_Wallet: true,
      Boomerang: true,
      Lantern: true,
      Slingshot: true,
      Progressive_Fishing_Rod: true,
      Iron_Boots: true,
      Progressive_Bow: true,
      Filled_Bomb_Bag: true,
      Zora_Armor: true,
      Progressive_Clawshot: true,
      Shadow_Crystal: true,
      Aurus_Memo: true,
      Asheis_Sketch: true,
      Spinner: true,
      Ball_and_Chain: true,
      Progressive_Dominion_Rod: true,
      Progressive_Sky_Book: true,
      Renados_Letter: true,
      Invoice: true,
      Wooden_Statue: true,
      Ilias_Charm: true,
      Horse_Call: true,
      Gate_Keys: true,
      Empty_Bottle: true,
      Progressive_Hidden_Skill: true,
      Magic_Armor: true,
      Ordon_Shield: true,
      Progressive_Fused_Shadow: true,
      Progressive_Mirror_Shard: true,
    };

    function isItemImportant(itemId) {
      // return importantItems[itemId] || itemId.indexOf('Key') >= 0;
      return Boolean(importantItems[itemId]);
    }

    const filterGroupsParent = document.getElementById(
      'spoilerFilterGroupsParent'
    );
    const spoilerResultsCountEl = document.getElementById(
      'spoilerResultsCount'
    );
    const inputFilterEl = document.getElementById('spoilerFilterInput');
    const $noSpoilerResults = $('#noSpoilerResults');
    const importantItemsOnlyCheck = document.getElementById(
      'checkboxImportantItemsOnly'
    );

    let selectedLocationFilter = 'All';
    let selectedTypeFilter = 'All';
    let inputFilter = inputFilterEl.value;
    let importantOnlyChecked = importantItemsOnlyCheck.checked;
    let checkList = [];

    function onFilterChange() {
      let matchingCount = 0;

      checkList.forEach(({ checkName, el, item }) => {
        const tags = Object.keys(lookupTable[checkName].tags);
        let isLocationMatch = selectedLocationFilter === 'All';
        let isTypeMatch = selectedTypeFilter === 'All';

        const canShow = !importantOnlyChecked || isItemImportant(item);

        const inputFilterText = inputFilter.toLowerCase();

        const isInputMatch =
          inputFilter.length < 1 ||
          checkName.toLowerCase().indexOf(inputFilterText) >= 0 ||
          item.toLowerCase().indexOf(inputFilterText) >= 0;

        if (isInputMatch && canShow && (!isLocationMatch || !isTypeMatch)) {
          for (let i = 0; i < tags.length; i++) {
            const tag = tags[i];

            if (locationFiltersSet[tag]) {
              if (tag === selectedLocationFilter) {
                isLocationMatch = true;
              }
            } else if (typeFiltersSet[tag]) {
              if (tag === selectedTypeFilter) {
                isTypeMatch = true;
              }
            }
          }
        }

        const isMatch =
          canShow && isInputMatch && isLocationMatch && isTypeMatch;
        $(el).toggle(isMatch);
        if (isMatch) {
          matchingCount += 1;
        }
      });

      console.log(`matchingCount is ${matchingCount}`);
      spoilerResultsCountEl.textContent = `${matchingCount} match${
        matchingCount !== 1 ? 'es' : ''
      }`;

      if (matchingCount > 0) {
        $noSpoilerResults.hide();
      } else {
        const selectedFiltersText = [];
        if (selectedLocationFilter !== 'All') {
          selectedFiltersText.push(`'${selectedLocationFilter}'`);
        }
        if (selectedTypeFilter !== 'All') {
          selectedFiltersText.push(`'${selectedTypeFilter}'`);
        }
        if (importantOnlyChecked) {
          selectedFiltersText.push("'Important items only'");
        }
        if (inputFilter.length > 0) {
          selectedFiltersText.push(`"${inputFilter}"`);
        }

        $noSpoilerResults
          .text(`No results for ${selectedFiltersText.join(' + ')}`)
          .show();
      }

      $('.spoilerTable tr:visible:even').css('background-color', '');
      $('.spoilerTable tr:visible:odd').css(
        'background-color',
        'rgba(255,255,255,0.3)'
      );
    }

    importantItemsOnlyCheck.addEventListener('change', () => {
      importantOnlyChecked = importantItemsOnlyCheck.checked;
      onFilterChange();
    });

    renderFilterGroup({
      rootEl: filterGroupsParent,
      title: 'Location',
      filters: ['All'].concat(Object.keys(locationFiltersSet)),
      defaultFilter: 'All',
      onFilterSelected: (filter) => {
        selectedLocationFilter = filter;
        onFilterChange();
      },
    });

    renderFilterGroup({
      rootEl: filterGroupsParent,
      title: 'Type',
      // filters: Object.keys(typeFiltersSet),
      filters: ['All'].concat(Object.keys(typeFiltersSet)),
      defaultFilter: 'All',
      onFilterSelected: (filter) => {
        selectedTypeFilter = filter;
        onFilterChange();
      },
    });

    inputFilterEl.addEventListener('input', () => {
      inputFilter = inputFilterEl.value;
      onFilterChange();
    });

    checkList = renderSpoilerList({
      parent: document.getElementById('spoilerTableBody'),
      lookupTable,
    });

    onFilterChange();

    initSpoilerCheckRowColors();
  }

  function initSpoilerCheckRowColors() {
    let els = document.querySelectorAll('.spoilerTable tr');
    const filteredEls = [];
    for (let i = 0; i < els.length; i++) {
      const el = els[i];
      if (el.style.display !== 'none') {
        filteredEls.push(el);
      }
    }

    for (let i = 0; i < filteredEls.length; i++) {
      filteredEls[i].style.backgroundColor =
        i % 2 === 0 ? '' : 'rgba(255,255,255,0.3)';
    }
  }

  function renderSpoilerList({ parent, lookupTable }) {
    const arr = [];

    Object.keys(lookupTable).forEach((checkName) => {
      const item = lookupTable[checkName].value;

      const el = renderSpoilerCheckRow({ parent, checkName, item });
      arr.push({ checkName, el, item });
    });

    return arr;
  }

  function renderSpoilerCheckRow({ parent, checkName, item }) {
    const el = document.createElement('tr');
    const checkEl = document.createElement('td');
    el.appendChild(checkEl);
    const itemEl = document.createElement('td');
    el.appendChild(itemEl);

    el.style.margin = '4px';
    checkEl.textContent = checkName + ':';
    itemEl.textContent = item;
    parent.appendChild(el);
    return el;
  }

  function renderFilterGroup({
    rootEl,
    title,
    filters,
    onFilterSelected,
    defaultFilter,
  }) {
    const filterGroupRoot = document.createElement('div');
    filterGroupRoot.className = 'filterGroupParent';
    rootEl.appendChild(filterGroupRoot);

    if (title) {
      const titleEl = document.createElement('div');
      titleEl.className = 'filterGroupTitle';
      titleEl.textContent = title;
      filterGroupRoot.appendChild(titleEl);
    }

    // filters = ['All'].concat(filters);
    const filterEls = [];

    let selectedFilter = defaultFilter || '';

    function updateFilterAppearances() {
      filterEls.forEach(({ name, el }) => {
        $(el).toggleClass('filterSelected', name === selectedFilter);
      });
    }

    filters.forEach((filter) => {
      const filterEl = renderFilter(filterGroupRoot, filter, (filterName) => {
        selectedFilter = filterName;
        updateFilterAppearances();
        onFilterSelected(filterName);
      });

      filterEls.push({
        name: filter,
        el: filterEl,
      });
    });

    updateFilterAppearances();
  }

  function renderFilter(parent, filterName, onFilterSelected) {
    const el = document.createElement('div');
    el.textContent = filterName;
    el.className = 'filterGroupFilter';
    parent.appendChild(el);
    el.addEventListener('click', function () {
      onFilterSelected(filterName);
    });
    return el;
  }

  function initPlaythroughSpoilersTable(tabId, checkToItemMap) {
    const table = document.createElement('table');
    document.getElementById(tabId).appendChild(table);

    const tbody = document.createElement('tbody');
    table.appendChild(tbody);

    Object.keys(checkToItemMap).forEach((key) => {
      const tr = document.createElement('tr');
      tbody.appendChild(tr);

      const labelEl = document.createElement('td');
      labelEl.textContent = key;
      tr.appendChild(labelEl);

      const valueEl = document.createElement('td');
      valueEl.textContent = checkToItemMap[key];
      tr.appendChild(valueEl);
    });
  }

  function createSpoilerLogDownload(spoilerData, rawSpoilerData) {
    const enc = new TextEncoder(); // always utf-8
    const fileBytes = enc.encode(rawSpoilerData);

    // const link = document.createElement('a');
    const link = document.getElementById('downloadSpoilerLogBtn');
    // link.className = 'downloadAnchor';
    link.href = URL.createObjectURL(new Blob([fileBytes]));
    // link.download = `TprSpoilerLog--${spoilerData.playthroughName}.txt`;
    link.download = `Tpr--${spoilerData.playthroughName}--SpoilerLog-${spoilerData.meta.seedId}.json`;
    // link.textContent = 'Download Spoiler Log';
    // const downloadLinkParent = document.getElementById('downloadLinkParent');
    $(link).show();
    // downloadLinkParent.appendChild(link);
  }

  function handleCheckProgressPage() {
    $('#progressTitle').text('Checking progress...');
    $('#sectionPlayPicross').show();

    $('#playPicrossBtn').on('click', () => {
      picrossOpened = true;
    });

    startCheckProgressRoutine();
  }

  function handleInvalidSeedPage() {
    $('#progressTitle').text('Invalid seed ID.');
  }

  function initTabButtons(tabIds) {
    const tabBtnEls = tabIds.map(({ buttonId }) => {
      return document.getElementById(buttonId);
    });

    const tabContentEls = tabIds.map(({ contentId }) => {
      return document.getElementById(contentId);
    });

    function genOnTabClick(contentEl) {
      return function (e) {
        for (let i = 0; i < tabContentEls.length; i++) {
          tabContentEls[i].style.display = 'none';
        }

        for (let i = 0; i < tabBtnEls.length; i++) {
          tabBtnEls[i].className = tabBtnEls[i].className.replace(
            ' active',
            ''
          );
        }

        // Show the current tab, and add an "active" class to the button that opened the tab
        // byId(id).style.display = 'block';
        contentEl.style.display = 'inline-block';
        e.currentTarget.className += ' active';
      };
    }

    for (let i = 0; i < tabIds.length; i++) {
      tabBtnEls[i].addEventListener('click', genOnTabClick(tabContentEls[i]));
    }

    // ['mainTab', 'cosmeticsTab', 'audioTab'].forEach((id) => {
    //   byId(id + 'Btn').addEventListener('click', genOnTabClick(id));
    // });
  }

  function escapeHtml(unsafe) {
    return unsafe
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }

  function fillInInfo() {
    const date = new Date(pageData.meta.ts);

    let locales = navigator.languages;
    if (locales == null) {
      locales = navigator.language;
    }

    $('#timestamp').text(
      date.toLocaleDateString(locales, {
        // date.toLocaleDateString('en-US', {
        weekday: 'short',
        year: 'numeric',
        month: 'numeric',
        day: 'numeric',
        hour: 'numeric',
        minute: 'numeric',
        second: 'numeric',
        timeZoneName: 'short',
      })
    );
    $('#seed').text(pageData.input.seed);
    $('#settingsString').text(pageData.input.settings);

    const arr = [
      { label: 'Created', value: pageData.meta.ts },
      { label: 'Seed', value: pageData.input.seed },
      {
        label: 'Settings String',
        value: pageData.input.settings,
      },
    ];

    byId('info').innerHTML = arr
      .map((obj) => {
        return '<strong>' + obj.label + '</strong> ' + escapeHtml(obj.value);
      })
      .join(' -- ');

    byId('filename').textContent = pageData.output.name;
    const wiiFilenameEl = byId('wiiFilename');
    if (pageData.output.wiiName) {
      wiiFilenameEl.textContent = `Wii: ${pageData.output.wiiName}`;
    } else {
      wiiFilenameEl.style.display = 'none';
    }
  }

  // Parse SSetting to object.
  // Parse PSettings to object.

  function fillInSettingsTable(spoilerData) {
    const table = document.createElement('table');
    table.className = 'settingsTable';
    document.getElementById('tabSharedSettings').appendChild(table);

    const tbody = document.createElement('tbody');
    table.appendChild(tbody);

    Object.keys(spoilerData.settings).forEach((key) => {
      let value = spoilerData.settings[key];

      if (Array.isArray(value)) {
        value = value.join(', ');
      }

      const tr = document.createElement('tr');
      tbody.appendChild(tr);

      const labelEl = document.createElement('td');
      labelEl.textContent = key + ':';
      tr.appendChild(labelEl);

      const valueEl = document.createElement('td');
      valueEl.textContent = value;
      tr.appendChild(valueEl);
    });
  }

  function encodeBitStringTo6BitsString(bitString) {
    const remainder = bitString.length % 6;
    if (remainder > 0) {
      const missingChars = 6 - remainder;
      bitString += '0'.repeat(missingChars);
    }

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

  function genFcSettingsString() {
    function getVal(id) {
      const $el = $('#' + id);
      if ($el.length < 1) {
        return false;
      }
      if ($el.prop('nodeName') === 'INPUT' && $el.attr('type') === 'checkbox') {
        return $el.prop('checked');
      }

      return $el.val();
    }

    let values = [];

    if (Region.hasOwnProperty(selectedRegion)) {
      values.push({
        type: RawSettingType.xBitNum,
        bitLength: regionBitLength,
        value: parseInt(Region[selectedRegion], 10),
      });
    } else {
      values.push({
        type: RawSettingType.xBitNum,
        bitLength: regionBitLength,
        value: 0,
      });
    }

    if (EurLanguageTag.hasOwnProperty(selectedLanguage)) {
      values.push({
        type: RawSettingType.xBitNum,
        bitLength: eurLangTagBitLength,
        value: parseInt(EurLanguageTag[selectedLanguage], 10),
      });
    } else {
      values.push({
        type: RawSettingType.xBitNum,
        bitLength: eurLangTagBitLength,
        value: 0,
      });
    }

    values = values.concat(
      [
        // { id: 'gameRegion', bitLength: 3 },
        { id: 'includeSpoilerCheckbox' },

        { id: 'bgmFieldset', bitLength: 2 },
        { id: 'randomizeFanfaresCheckbox' },
        { id: 'disableEnemyBGMCheckbox' },

        { id: 'hTunicHatColorFieldset', rgb: true },
        { id: 'hTunicBodyColorFieldset', rgb: true },
        { id: 'hTunicSkirtColorFieldset', rgb: true },
        { id: 'zTunicHatColorFieldset', rgb: true },
        { id: 'zTunicHelmetColorFieldset', rgb: true },
        { id: 'zTunicBodyColorFieldset', rgb: true },
        { id: 'zTunicScalesColorFieldset', rgb: true },
        { id: 'zTunicBootsColorFieldset', rgb: true },
        { id: 'lanternColorFieldset', rgb: true },
        // { id: 'midnaHairColorFieldset', bitLength: 1 },
        { id: 'heartColorFieldset', rgb: true },
        { id: 'aButtonColorFieldset', rgb: true },
        { id: 'bButtonColorFieldset', rgb: true },
        { id: 'xButtonColorFieldset', rgb: true },
        { id: 'yButtonColorFieldset', rgb: true },
        { id: 'zButtonColorFieldset', rgb: true },
        { id: 'midnaHairBaseColorFieldset', midnaHairBase: true },
        { id: 'midnaHairTipColorFieldset', midnaHairTips: true },
        { id: 'midnaDomeRingColorFieldset', rgb: true },
        { id: 'linkHairColorFieldset', rgb: true },
      ].map(({ id, bitLength, rgb, midnaHairBase, midnaHairTips }) => {
        if (bitLength) {
          // select
          return {
            type: RawSettingType.xBitNum,
            bitLength,
            value: parseInt(getVal(id), 10),
          };
        } else if (rgb) {
          const selVal = getVal(id);
          const $option = $(`#${id}`).find(`option[value="${selVal}"]`);
          const value = $option[0].getAttribute('data-rgb');

          return {
            type: RawSettingType.rgb,
            value,
          };
        } else if (midnaHairBase || midnaHairTips) {
          const selVal = getVal(id);
          const $option = $(`#${id}`).find(`option[value="${selVal}"]`);
          const rgbVal = $option[0].getAttribute('data-rgb');
          const isCustomColor =
            $option[0].getAttribute('data-custom-color') === 'true';

          return {
            type: midnaHairTips
              ? RawSettingType.midnaHairTips
              : RawSettingType.midnaHairBase,
            valueNum: parseInt(selVal, 10),
            rgbVal,
            isCustomColor,
          };
        }
        // checkbox
        return getVal(id);
      })
    );

    let bitString = '';

    // valuesArr.forEach((value) => {
    //   if (typeof value === 'boolean') {
    //     bitString += value ? '1' : '0';
    //   } else if (typeof value === 'string') {
    //     let asNum = parseInt(value, 10);
    //     if (Number.isNaN(asNum)) {
    //       asNum = 0;
    //     }
    //     bitString += toPaddedBits(asNum, 4);
    //   } else if (value && typeof value === 'object') {
    //     if (value.type === RawSettingType.bitString) {
    //       bitString += value.bitString;
    //     }
    //   }
    // });

    values.forEach((value) => {
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
        } else {
          switch (value.type) {
            case RawSettingType.bitString:
              bitString += value.bitString;
              break;
            case RawSettingType.xBitNum:
              bitString += numToPaddedBits(value.value, value.bitLength);
              break;
            case RawSettingType.rgb: {
              if (value.value == null) {
                bitString += '0';
              } else {
                bitString += '1';
                bitString += hexStrToBits(value.value);
              }
              break;
            }
            case RawSettingType.midnaHairBase:
              bitString += encodeMidnaHairBase(value);
              break;
            case RawSettingType.midnaHairTips:
              bitString += encodeMidnaHairTips(value);
              break;
          }
        }
      }
    });

    return encodeBitStringTo6BitsString(bitString);
  }

  function encodeMidnaHairBase({ valueNum, rgbVal, isCustomColor }) {
    if (!isCustomColor) {
      return '0' + numToPaddedBits(valueNum, 4);
    }

    let ret = '1';

    let sixCharHex = rgbVal;
    if (sixCharHex.length > 6) {
      sixCharHex = sixCharHex.substring(sixCharHex.length - 6);
    }

    const colors = window.MidnaHairColors.calcBaseAndGlow(sixCharHex);

    ret += hexStrToBits(colors.midnaHairBaseLightWorldInactive);
    ret += hexStrToBits(colors.midnaHairBaseDarkWorldInactive);
    ret += hexStrToBits(colors.midnaHairBaseAnyWorldActive);
    ret += hexStrToBits(colors.midnaHairGlowAnyWorldInactive);
    ret += hexStrToBits(sixCharHex); // midnaHairGlowLightWorldActive
    ret += hexStrToBits(colors.midnaHairGlowDarkWorldActive);

    return ret;
  }

  function encodeMidnaHairTips({ valueNum, rgbVal, isCustomColor }) {
    if (!isCustomColor) {
      return '0' + numToPaddedBits(valueNum, 4);
    }

    let ret = '1';

    let sixCharHex = rgbVal;
    if (sixCharHex.length > 6) {
      sixCharHex = sixCharHex.substring(sixCharHex.length - 6);
    }

    const colors = window.MidnaHairColors.calcTips(sixCharHex);

    ret += hexStrToBits(sixCharHex); // midnaHairTipsLightWorldInactive
    ret += hexStrToBits(colors.midnaHairTipsDarkWorldAnyActive);
    ret += hexStrToBits(colors.midnaHairTipsLightWorldActive);

    return ret;
  }

  function _base64ToUint8Array(base64Str) {
    const binary_string = window.atob(base64Str);
    const len = binary_string.length;
    const bytes = new Uint8Array(len);
    for (var i = 0; i < len; i++) {
      bytes[i] = binary_string.charCodeAt(i);
    }
    return bytes;
  }

  function handleCreateClick() {
    if (creationCallInProgress) {
      return;
    }
    creationCallInProgress = true;

    // Update UI
    $('#downloadsParent').show();
    $('#downloadLinkParent').hide().empty();
    $('#downloadLinkError').hide();

    // Validate input
    if (!Region.hasOwnProperty(selectedRegion)) {
      hasSelectedRegionError = true;
      $('#downloadLinkError').text('Select a region.').show();
      creationCallInProgress = false;

      return;
    }

    const fileCreationSettings = genFcSettingsString();
    console.log(fileCreationSettings);

    // Save preferences to localStorage
    try {
      const fcSettingsDefault = {
        region: selectedRegion,
        eurLanguage: selectedLanguage,
        includeSpoilerLog: $('#includeSpoilerCheckbox').prop('checked'),
      };

      window.localStorage.setItem(
        'fcSettingsDefault',
        JSON.stringify(fcSettingsDefault)
      );
    } catch (e) {
      // do nothing
    }

    // Show progress in UI
    $('#preparingDownloadLinkRow').show();

    callCreateGci(fileCreationSettings, (error, data) => {
      $('#preparingDownloadLinkRow').hide();

      // if (error || true) {
      if (error) {
        console.log('error in response');
        console.log(error);
        $('#downloadLinkError').text('Failed to get download link.').show();
      } else if (data) {
        data.forEach(({ name, bytes }) => {
          const fileBytes = _base64ToUint8Array(bytes);

          const link = document.createElement('a');
          link.className = 'downloadAnchor';
          link.href = URL.createObjectURL(new Blob([fileBytes]));
          link.download = name;
          link.textContent = `Download ${name}`;
          const downloadLinkParent =
            document.getElementById('downloadLinkParent');
          $(downloadLinkParent).show();
          downloadLinkParent.appendChild(link);
        });
      }

      creationCallInProgress = false;
    });
  }

  function callCreateGci(fileCreationSettings, cb) {
    window.tpr.shared
      .fetch('/api/final', {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          fileCreationSettings,
        }),
      })
      .then((response) => response.json())
      .then(({ error, data }) => {
        cb(error, data);
      })
      .catch((err) => {
        cb(err);
      });
  }

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

  /**
   * Converts a hex string like "fc8a" to a bit string like "10110...".
   *
   * @param {string} hexStr hex string to convert to a bit string
   * @return {string} Bit string
   */
  function hexStrToBits(hexStr) {
    if (!hexStr) {
      return '';
    }

    let result = '';

    for (let i = 0; i < hexStr.length; i++) {
      const character = hexStr.substring(i, i + 1);
      const num = parseInt(character, 16);
      result += numToPaddedBits(num, 4);
    }

    return result;
  }

  function initSettingsModal() {
    // Do nothing until pSettings are sorted out better.
    return;

    // const modal = document.getElementById('myModal');
    // // const btn = document.getElementById('editSettingsBtn');
    // const btn = document.getElementById('importSettingsStringButton');
    // const span = modal.querySelector('.modal-close');
    // const fieldErrorText = document.getElementById('modalFieldError');
    // const input = document.getElementById('modalSettingsStringInput');
    // const currentSettings = document.getElementById('modalCurrentSettings');

    // input.addEventListener('input', () => {
    //   $(fieldErrorText).hide();
    // });

    // // When the user clicks the button, open the modal
    // btn.addEventListener('click', () => {
    //   // Prepare modal
    //   currentSettings.textContent = window.tpr.shared.genPSettingsFromUi();
    //   $(fieldErrorText).hide();
    //   input.value = '';

    //   $(modal).show();

    //   input.focus();
    // });

    // span.addEventListener('click', () => {
    //   $(modal).hide();
    // });

    // document.getElementById('modalCancel').addEventListener('click', () => {
    //   $(modal).hide();
    // });

    // document.getElementById('modalImport').addEventListener('click', () => {
    //   if (!input.value) {
    //     $(modal).hide();
    //     return;
    //   }

    //   const error = populateFromSettingsString(input.value);

    //   if (error) {
    //     $(fieldErrorText)
    //       .text(
    //         'Unable to understand those settings. Do you have the correct string?'
    //       )
    //       .show();
    //   } else {
    //     $(modal).hide();
    //   }
    // });

    // document.getElementById('modalCopy').addEventListener('click', () => {
    //   $(fieldErrorText).hide();

    //   const text = currentSettings.textContent;
    //   navigator.clipboard.writeText(text).then(
    //     () => {
    //       // success
    //     },
    //     (err) => {
    //       $(fieldErrorText).text('Failed to copy text.').show();
    //     }
    //   );
    // });

    // let canHide = true;

    // $('#myModal')
    //   .on('mousedown', function (e) {
    //     canHide = e.target === this;
    //   })
    //   .on('mouseup', function (e) {
    //     if (canHide && e.target === this) {
    //       $(modal).hide();
    //     }
    //   });
  }

  function populateFromSettingsString(settingsString) {
    let byType;

    try {
      byType = window.tpr.shared.decodeSettingsString(settingsString);
    } catch (e) {
      console.error(e);
      return e.message;
    }

    if (byType.p) {
      window.tpr.shared.populateUiFromPSettings(byType.p);
    }

    // setSettingsString();

    return null;
  }

  function initShareModal() {
    const $bg = $('#modal2Bg');
    const $modal = $('#generatingModal');
    const $successEl = $('#linkCopiedMsg');
    const $errorEl = $('#linkCopiedError');

    function showModal() {
      $successEl.hide();
      $errorEl.hide();
      $bg.show();
      $modal.addClass('isOpen').show();
    }

    function hideModal() {
      $bg.hide();
      $modal.hide().removeClass('isOpen');
    }

    document
      .getElementById('shareDoneBtn')
      .addEventListener('click', hideModal);

    document.getElementById('copyLinkBtn').addEventListener('click', () => {
      $successEl.hide();
      $errorEl.hide();

      navigator.clipboard.writeText(window.location.href).then(
        () => {
          $successEl.show();
        },
        (err) => {
          $errorEl.show();
        }
      );
    });

    $('#shareUrl').text(window.location.href);

    document.getElementById('shareBtn').addEventListener('click', showModal);

    let canHide = true;

    $('.boqDrivesharedialogDialogsShareContainer')
      .on('mousedown', function (e) {
        canHide = e.target === this;
      })
      .on('mouseup', function (e) {
        if (canHide && e.target === this) {
          hideModal();
        }
      });
  }

  function startCheckProgressRoutine() {
    const match = window.location.pathname.match(/[^\/]+$/);
    if (!match) {
      console.error('Failed to parse `location.pathname` to check progress.');
      return;
    }

    const id = match[0];
    runProgressCheck(id);
  }

  let timesProgressCallFailed = 0;

  let secondsUntilNextProgressCheck = 0;
  let progressCallCheckInterval = null;

  function showRetryingInXSecondsMsg() {
    let secondsText = 'seconds';
    if (secondsUntilNextProgressCheck === 1) {
      secondsText = 'second';
    }

    $('#sectionCheckFailed')
      .show()
      .text(`Retrying in ${secondsUntilNextProgressCheck} ${secondsText}.`);
  }

  function runProgressCheck(id) {
    window.tpr.shared
      .fetch(`/api/seed/progress/${id}`)
      .then((response) => response.json())
      .then(({ error, data = {} }) => {
        console.log('/api/seed/progress data');
        console.log(data);

        timesProgressCallFailed = 0;

        if (error) {
          handleGenerationFailed(error);
        } else if (data) {
          const { progress, queuePos } = data;

          if (progress === SeedGenProgress.Done) {
            handleProgressCheckDone();
            return;
          }

          updateQueuePosState(queuePos);

          setTimeout(() => {
            runProgressCheck(id);
          }, 2000);
        }
      })
      .catch((err) => {
        timesProgressCallFailed += 1;
        secondsUntilNextProgressCheck = Math.pow(2, timesProgressCallFailed);

        console.error('/api/seed/progress error');
        console.error(err);

        handleProgressCheckFailed();

        showRetryingInXSecondsMsg();

        progressCallCheckInterval = setInterval(() => {
          secondsUntilNextProgressCheck -= 1;
          if (secondsUntilNextProgressCheck > 0) {
            showRetryingInXSecondsMsg();
          } else {
            clearInterval(progressCallCheckInterval);
            progressCallCheckInterval = -1;

            $('#sectionCheckFailed').text(`Retrying...`);
            runProgressCheck(id);
          }
        }, 1000);
      });
  }

  function swapQueueImages(queuePosition) {
    const queueParent = document.getElementById('sectionQueueImages');
    const images = [];
    let imgTags = queueParent.querySelectorAll('img');
    for (let i = 0; i < imgTags.length; i++) {
      images.push(imgTags[i]);
    }

    let obachanPos = queuePosition;
    if (queuePosition < 0 || queuePosition > 4) {
      obachanPos = -1;
    }

    images.forEach((img, i) => {
      if (i === obachanPos) {
        img.className = 'queueImg';
        img.setAttribute('src', '/img/queue/im_obachan_48.bti.png');
      } else {
        img.className = 'queueImg2';
        img.setAttribute('src', '/img/queue/im_musuko_48.bti.png');
      }
    });
  }

  function updateQueuePosState(queuePos) {
    const inTheQueue = queuePos >= 0;

    if (inTheQueue) {
      updateSectionVisibilities(PageStates.inQueue);
      $('#queuePosNum').text(queuePos + 1);
    } else {
      updateSectionVisibilities(PageStates.beingWorked);
    }

    $('#progressTitle').text('Please Wait');

    swapQueueImages(queuePos);
  }

  function handleProgressCheckDone() {
    updateSectionVisibilities(PageStates.generationSuccess);
    $('#progressTitle').text('Seed Generated');

    function reloadPage() {
      document.body.scrollTop = document.documentElement.scrollTop = 0;
      window.location.reload();
    }

    if (picrossOpened) {
      $('#picrossGoToGeneratedSeedFirst, #picrossGoToGeneratedSeed')
        .show()
        .on('click', reloadPage);
    } else {
      reloadPage();
    }
  }

  function handleProgressCheckFailed() {
    updateSectionVisibilities(PageStates.progressCallFailure);
    $('#progressTitle').text('Progress check failed');
  }

  function handleGenerationFailed(error) {
    updateSectionVisibilities(PageStates.generationFailure);
    $('#progressTitle').text('Failure');

    if (typeof error === 'string') {
      $('#sectionErrorReturned').text(error);
    } else if (Array.isArray(error.errors) && error.errors.length > 0) {
      $('#sectionErrorReturned').text(error.errors[0].message);
    }
  }

  function updateSectionVisibilities(pageState) {
    $('#sectionQueueImages').toggle(
      pageState === PageStates.inQueue || pageState === PageStates.beingWorked
    );
    $('#sectionQueuePos').toggle(pageState === PageStates.inQueue);
    $('#sectionGenWarning').toggle(pageState === PageStates.inQueue);
    $('#sectionGenText').toggle(pageState === PageStates.beingWorked);
    $('#sectionPlayPicross').toggle(
      !picrossOpened &&
        pageState !== PageStates.generationFailure &&
        pageState !== PageStates.progressCallFailure
    );
    $('#sectionCheckFailed').toggle(
      pageState === PageStates.progressCallFailure
    );
    $('#fullGameWrapper').toggle(
      picrossOpened &&
        pageState !== PageStates.progressCallFailure &&
        pageState !== PageStates.generationFailure
    );
    $('#sectionErrorReturned').toggle(
      pageState === PageStates.generationFailure
    );
  }

  function handleCustomColorValueChange(colorInput, selectEl, hexValue) {
    colorInput.value = hexValue;
    colorInput.setAttribute('value', hexValue);
    const $customOption = $(selectEl).find('option[data-custom-color]');
    if ($customOption.length > 0) {
      const customOption = $customOption[0];
      customOption.setAttribute('data-rgb', '00' + hexValue.slice(1));
    }
  }

  function handleSelectWithCustomColorOptionChange(e, colorInputEl) {
    const selectEl = e.target;

    const selectedOption = selectEl.children[selectEl.selectedIndex];
    const customOption = $(selectEl).find('option[data-custom-color]')[0];

    $(colorInputEl).toggle(
      Boolean(selectedOption && selectedOption === customOption)
    );
  }

  function initCustomColorPickerPair(selectId, colorInputId) {
    const selectEl = document.getElementById(selectId);
    const colorInputEl = document.getElementById(colorInputId);

    selectEl.addEventListener('change', (e) => {
      handleSelectWithCustomColorOptionChange(e, colorInputEl);
    });
    colorInputEl.addEventListener('input', (e) => {
      handleCustomColorValueChange(e.target, selectEl, e.target.value);
    });
    // Sync data-rgb attribute of custom option with default value of color
    // input
    handleCustomColorValueChange(colorInputEl, selectEl, colorInputEl.value);
  }

  function initCustomColorPickers() {
    [
      'linkHairColorFieldset',
      'hTunicHatColorFieldset',
      'hTunicBodyColorFieldset',
      'hTunicSkirtColorFieldset',
      'zTunicHatColorFieldset',
      'zTunicHelmetColorFieldset',
      'zTunicBodyColorFieldset',
      'zTunicScalesColorFieldset',
      'zTunicBootsColorFieldset',
      'lanternColorFieldset',
      'midnaHairBaseColorFieldset',
      'midnaHairTipColorFieldset',
    ].forEach((selectId) => {
      const colorInputId = selectId + 'ColorPicker';
      initCustomColorPickerPair(selectId, colorInputId);
    });
  }
})();
