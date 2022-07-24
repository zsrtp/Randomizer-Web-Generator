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

  let pageData;
  let creationCallInProgress;
  let picrossOpened = false;

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

    pageData = JSON.parse(inputJsonDataEl.value);

    const decodedSettings = window.tpr.shared.decodeSettingsString(
      pageData.input.settings
    );

    initTabButtons();
    fillInInfo();
    fillInSettingsTable();

    window.tpr.shared.populateUiFromPSettings(decodedSettings.p);

    initSettingsModal();
    initShareModal();

    $('#create').on('click', handleCreateClick);
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

  function initTabButtons() {
    function genOnTabClick(id) {
      return function (e) {
        const tabcontentEls = document.querySelectorAll('.tabcontent');
        for (let i = 0; i < tabcontentEls.length; i++) {
          tabcontentEls[i].style.display = 'none';
        }

        const tablinksEls = document.querySelectorAll('.tablinks');
        for (let i = 0; i < tablinksEls.length; i++) {
          tablinksEls[i].className = tablinksEls[i].className.replace(
            ' active',
            ''
          );
        }

        // Show the current tab, and add an "active" class to the button that opened the tab
        byId(id).style.display = 'block';
        e.currentTarget.className += ' active';
      };
    }

    ['mainTab', 'cosmeticsTab', 'audioTab'].forEach((id) => {
      byId(id + 'Btn').addEventListener('click', genOnTabClick(id));
    });
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

  function fillInSettingsTable() {
    if (true) {
      return;
    }
    // Need to redo this
    const tbody = byId('settingsTBody');

    Object.keys(pageData.settings).forEach((key) => {
      const tr = document.createElement('tr');
      tbody.appendChild(tr);

      const labelEl = document.createElement('td');
      labelEl.textContent = key;
      tr.appendChild(labelEl);

      const valueEl = document.createElement('td');
      valueEl.textContent = pageData.settings[key];
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

  function genFcSettingsString(valuesArr) {
    function getVal(id) {
      const $el = $('#' + id);
      if ($el.prop('nodeName') === 'INPUT' && $el.attr('type') === 'checkbox') {
        return $el.prop('checked');
      }

      return $el.val();
    }

    const values = [
      { id: 'gameRegion', bitLength: 3 },
      { id: 'seedNumber', bitLength: 4 },

      { id: 'bgmFieldset', bitLength: 2 },
      { id: 'randomizeFanfaresCheckbox' },
      { id: 'disableEnemyBGMCheckbox' },

      // { id: 'tunicColorFieldset', bitLength: 4 },
      { id: 'lanternColorFieldset', bitLength: 4 },
      // { id: 'midnaHairColorFieldset', bitLength: 1 },
      { id: 'heartColorFieldset', bitLength: 4 },
      { id: 'aButtonColorFieldset', bitLength: 4 },
      { id: 'bButtonColorFieldset', bitLength: 3 },
      { id: 'xButtonColorFieldset', bitLength: 4 },
      { id: 'yButtonColorFieldset', bitLength: 4 },
      { id: 'zButtonColorFieldset', bitLength: 4 },
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

    const getVal = (id) => {
      return $('#' + id).val();
    };

    const isChecked = (id) => {
      return document.getElementById(id).checked;
    };

    // const arr = ['gameRegion', 'seedNumber'];

    // let values = arr.map(getVal);
    // values.push(genRecolorBits());
    // values = values.concat(
    //   [
    //     'randomizeBGMCheckbox',
    //     'randomizeFanfaresCheckbox',
    //     'disableEnemyBGMCheckbox',
    //   ].map(isChecked)
    // );

    // console.log(values);
    // const fileCreationSettings = encodeSettingsForFileCreationCall(values);
    const fileCreationSettings = genFcSettingsString();
    console.log(fileCreationSettings);

    callCreateGci(fileCreationSettings, (error, data) => {
      if (error) {
        console.log('error in response');
        console.log(error);
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
