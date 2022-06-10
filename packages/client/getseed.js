(() => {
  const $ = window.$;

  let pageData;
  let creationCallInProgress;

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
    recolorDefs.push(genTunicRecolorDef('tunicColor', RecolorId.herosClothes));

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

  function isIE() {
    const ua = navigator.userAgent;
    return ua.indexOf('MSIE ') > -1 || ua.indexOf('Trident/') > -1;
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
    if (isIE()) {
      document.getElementById('IsIE').style.display = 'block';
      document.getElementById('IsNotIE').style.display = 'none';
      return;
    } else {
      document.getElementById('IsNotIE').style.display = 'block';
      document.getElementById('IsIE').style.display = 'none';
    }

    document.getElementById('logo').addEventListener('click', () => {
      window.location.href = '/';
    });

    initTabButtons();

    const inputJsonDataEl = document.getElementById('inputJsonData');
    if (inputJsonDataEl) {
      pageData = JSON.parse(inputJsonDataEl.value);

      fillInInfo();
      fillInSettingsTable();
    }

    fetch('/api/creategci')
      .then((response) => response.json())
      .then((data) => console.log(data));

    $('#create').on('click', handleCreateClick);
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
    const arr = [];

    arr.push({ label: 'Created', value: pageData.timestamp });
    arr.push({ label: 'Seed', value: pageData.seed });
    arr.push({ label: 'Settings String', value: pageData.settingsString });

    byId('info').innerHTML = arr
      .map((obj) => {
        return '<strong>' + obj.label + '</strong> ' + escapeHtml(obj.value);
      })
      .join(' -- ');

    byId('filename').textContent = pageData.filename;
  }

  function fillInSettingsTable() {
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

  function encodeBits(bitString) {
    const missingChars = 6 - (bitString.length % 6);
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

  function encodePSettings(valuesArr) {
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

    return '0p' + encodeBits(bitString);
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

    const arr = ['gameRegion', 'seedNumber'];

    let values = arr.map(getVal);
    values.push(genRecolorBits());
    values = values.concat(
      ['randomizeBgm', 'randomizeFanfares', 'disableEnemyBgm'].map(isChecked)
    );

    console.log(values);
    const pSettingsString = encodePSettings(values);
    console.log(pSettingsString);

    callCreateGci(pSettingsString, (error, data) => {
      if (error) {
        console.log('error in response');
        console.log(error);
      } else if (data) {
        data.forEach(({ name, bytes }) => {
          const fileBytes = _base64ToUint8Array(bytes);

          const link = document.createElement('a');
          link.href = URL.createObjectURL(new Blob([fileBytes]));
          link.download = name;
          link.textContent = `Download ${name}`;
          document.getElementById('downloadLinkParent').appendChild(link);
        });
      }

      creationCallInProgress = false;
    });
  }

  function callCreateGci(pSettingsString, cb) {
    fetch('/api/final', {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        pSettingsString,
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
})();
