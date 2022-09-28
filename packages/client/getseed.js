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

  let pageData;
  let creationCallInProgress;
  let picrossOpened = false;
  let selectedRegion = null;
  let hasSelectedRegionError = false;
  let defaultIncludeSpoilerLog = false;

  function createBasicEvent() {
    let listener = null;

    return {
      subscribe: function (newListener) {
        listener = newListener;
      },
      hasListener: function () {
        return typeof listener === 'function';
      },
      notify: function () {
        if (typeof listener === 'function') {
          listener();
        }
      },
    };
  }

  const regionSelectedEvent = createBasicEvent();

  const RawSettingType = {
    nineBitWithEndOfListPadding: 'nineBitWithEndOfListPadding',
    bitString: 'bitString',
    xBitNum: 'xBitNum',
    rgb: 'rgb',
  };

  const RecolorId = {
    herosClothes: 0x00, // Cap and Body
    zoraArmorPrimary: 0x01,
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
      genTunicRecolorDef('tunicColorFieldset', RecolorId.herosClothes)
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
  }

  function restoreDefaultFcSettings() {
    try {
      const fcSettingsDefault = JSON.parse(
        window.localStorage.getItem('fcSettingsDefault')
      );

      if (Region.hasOwnProperty(fcSettingsDefault.region)) {
        selectedRegion = fcSettingsDefault.region;
      }

      if (typeof fcSettingsDefault.includeSpoilerLog === 'boolean') {
        defaultIncludeSpoilerLog = fcSettingsDefault.includeSpoilerLog;
      }
    } catch (e) {
      // do nothing
    }
  }

  function handleSpoilerData() {
    $('#settingsSpoilerSection').show();

    const spoilerData = JSON.parse(
      document.getElementById('spoilerData').value
    );
    console.log(spoilerData);

    initTabButtons([
      {
        buttonId: 'tabBtnSharedSettings',
        contentId: 'tabSharedSettings',
      },
      {
        buttonId: 'tabBtnDefaultCosmetics',
        contentId: 'tabDefaultCosmetics',
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
      createSpoilerLogDownload(spoilerData);
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

    if (!isRaceSeed) {
      renderBasicCheckbox({
        parent: document.getElementById('downloadOptionsSpoilerCheckboxParent'),
        checkboxId: 'includeSpoilerCheckbox',
        text: 'Include spoiler log',
        defaultChecked: defaultIncludeSpoilerLog,
      });
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
      'Eldin Province': true,
      'Faron Woods': true,
      'Forest Temple': true,
      'Gerudo Desert': true,
      'Goron Mines': true,
      'Hidden Village': true,
      'Hyrule Castle': true,
      'Hyrule Field': true,
      'Lakebed Temple': true,
      'Lanayru Province': true,
      'Ordona Province': true,
      'Palace of Twilight': true,
      'Sacred Grove': true,
      'Snowpeak Province': true,
      'Snowpeak Ruins': true,
      'Temple of Time': true,
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

  function createSpoilerLogDownload(spoilerData) {
    const spoilerDataString = JSON.stringify(spoilerData, null, 2);

    const enc = new TextEncoder(); // always utf-8
    const fileBytes = enc.encode(spoilerDataString);

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
        contentEl.style.display = 'block';
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
      values.push(0);
    }

    values = values.concat(
      [
        // { id: 'gameRegion', bitLength: 3 },
        { id: 'includeSpoilerCheckbox' },

        { id: 'bgmFieldset', bitLength: 2 },
        { id: 'randomizeFanfaresCheckbox' },
        { id: 'disableEnemyBGMCheckbox' },

        { id: 'tunicColorFieldset', rgb: true },
        { id: 'lanternColorFieldset', bitLength: 4 },
        // { id: 'midnaHairColorFieldset', bitLength: 1 },
        { id: 'heartColorFieldset', bitLength: 4 },
        { id: 'aButtonColorFieldset', rgb: true },
        { id: 'bButtonColorFieldset', rgb: true },
        { id: 'xButtonColorFieldset', bitLength: 4 },
        { id: 'yButtonColorFieldset', bitLength: 4 },
        { id: 'zButtonColorFieldset', bitLength: 4 },
      ].map(({ id, bitLength, rgb }) => {
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
          const value = $option.data('rgb');

          return {
            type: RawSettingType.rgb,
            value,
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
        } else if (value.type === RawSettingType.bitString) {
          bitString += value.bitString;
        } else if (value.type === RawSettingType.xBitNum) {
          bitString += numToPaddedBits(value.value, value.bitLength);
        } else if (value.type === RawSettingType.rgb) {
          if (value.value == null) {
            bitString += '0';
          } else {
            bitString += '1';
            const numBits = value.value.length * 4;
            const colorAsNumber = parseInt(value.value, 16);
            bitString += numToPaddedBits(colorAsNumber, numBits);
          }
        }
      }
    });

    return encodeBitStringTo6BitsString(bitString);
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

      if (!regionSelectedEvent.hasListener()) {
        regionSelectedEvent.subscribe(() => {
          if (hasSelectedRegionError) {
            hasSelectedRegionError = false;
            $('#downloadsParent').hide();
          }
        });
      }

      return;
    }

    const fileCreationSettings = genFcSettingsString();
    console.log(fileCreationSettings);

    // Save preferences to localStorage
    try {
      const fcSettingsDefault = {
        region: selectedRegion,
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

  // function populateRecolorSelect(pSettings, elId, recolorId) {
  //   const $sel = $(`#${elId}`);

  //   const recolorDef = pSettings.recolorDefs[recolorId];

  //   if (recolorDef) {
  //     const rgbHex = padBits2(recolorDef.value.toString(16), 6);

  //     const option = $sel.find(`option[data-rgb="${rgbHex}"]`)[0];
  //     if (option) {
  //       option.selected = true;
  //       // $sel.val(rgbHex);
  //     } else {
  //       $sel.val('0');
  //     }
  //   } else {
  //     $sel.val('0');
  //   }
  // }

  // function populateFromPSettings(pSettings) {
  //   console.log(pSettings);

  //   // $('#tunicColor')
  //   populateRecolorSelect(pSettings, 'tunicColor', RecolorId.herosClothes);

  //   $('#randomizeBgm').prop('checked', pSettings.randomizeBgm);
  //   $('#randomizeFanfares').prop('checked', pSettings.randomizeFanfares);
  //   $('#disableEnemyBgm').prop('checked', pSettings.disableEnemyBgm);
  // }

  function initSettingsModal() {
    const modal = document.getElementById('myModal');
    // const btn = document.getElementById('editSettingsBtn');
    const btn = document.getElementById('importSettingsStringButton');
    const span = modal.querySelector('.modal-close');
    const fieldErrorText = document.getElementById('modalFieldError');
    const input = document.getElementById('modalSettingsStringInput');
    const currentSettings = document.getElementById('modalCurrentSettings');

    input.addEventListener('input', () => {
      $(fieldErrorText).hide();
    });

    // When the user clicks the button, open the modal
    btn.addEventListener('click', () => {
      // Prepare modal
      currentSettings.textContent = window.tpr.shared.genPSettingsFromUi();
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
      $(fieldErrorText).hide();

      const text = currentSettings.textContent;
      navigator.clipboard.writeText(text).then(
        () => {
          // success
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
})();
