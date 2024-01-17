'use strict';

(function () {
  const Hct = window.MatColorUtil.Hct;

  function cloneHct(hct) {
    return Hct.fromInt(hct.toInt());
  }

  function rgbArrToHct(arr) {
    const rgbNum = (arr[0] << 16) + (arr[1] << 8) + arr[2];
    return Hct.fromInt(0xff000000 | rgbNum);
  }

  function hctToRgbArr(hct) {
    const num = hct.toInt();
    return [(num & 0xff0000) >> 16, (num & 0xff00) >> 8, num & 0xff];
  }

  function normalizeColor(num) {
    const val = Math.round(num);
    if (val < 0) {
      return 0;
    } else if (val > 0xff) {
      return 0xff;
    }
    return val;
  }

  function colorHexToU24(colorHex) {
    const match = colorHex.match(/([0-9a-f]){6}/gi);
    if (!match) {
      throw new Error(`Failed to parse color from colorHex "${colorHex}".`);
    }
    return parseInt(match[0], 16);
  }

  function tipsActiveFromTips(tipsHct) {
    const tones = [
      10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95,
      97, 98, 99,
    ];

    let closestIdx = 0;
    let lowestDiff = 200;

    for (let i = 0; i < tones.length; i++) {
      const absDiff = Math.abs(tipsHct.tone - tones[i]);
      if (absDiff <= lowestDiff) {
        lowestDiff = absDiff;
        closestIdx = i;
      }
    }

    const kToneDiff = 2;

    let desiredIdx = closestIdx + kToneDiff;
    if (desiredIdx >= tones.length) {
      desiredIdx = tones.length - 1;
    }

    const toneDiff = tones[desiredIdx] - tones[desiredIdx - kToneDiff];

    let newTone = tipsHct.tone + toneDiff;
    if (newTone > 100) {
      newTone = 100;
    }

    const hctOut = cloneHct(tipsHct);
    if (hctOut.tone < 99) {
      hctOut.tone = newTone;
    }

    return hctOut;
  }

  function primaryFromGlowAndActiveGlow(glowChan, activeGlowChan) {
    const result = Math.round((11 * activeGlowChan - 5 * glowChan) / 6);

    if (result < 0) {
      return 0;
    } else if (result > 0xff) {
      return 0xff;
    }
    return result;
  }

  function rgbArrToHexStr(arr) {
    const num = (arr[0] << 16) + (arr[1] << 8) + arr[2];
    let str = num.toString(16);
    while (str.length < 6) {
      str = '0' + str;
    }
    return str;
  }

  function rgbArrToInt(arr) {
    return (arr[0] << 16) + (arr[1] << 8) + arr[2];
  }

  function getGlowFromActiveGlow(activeGlowHex) {
    const activeGlowNum = colorHexToU24(activeGlowHex);

    const activeGlowR = (activeGlowNum & 0xff0000) >> 16;
    const activeGlowG = (activeGlowNum & 0xff00) >> 8;
    const activeGlowB = activeGlowNum & 0xff;

    const hctActiveGlow = Hct.fromInt(0xff000000 | activeGlowNum);

    const hctGlow = Hct.fromInt(0xff000000 | activeGlowNum);
    // hctGlow.hue = (hctActiveGlow.hue + 360 - 21.776860078480574) % 360;
    // hctGlow.hue = hctActiveGlow.hue;
    hctGlow.tone = 13.858842702172808;
    // hctGlow.tone = 20;

    const huesGivingChromaApexes = [
      27.274268318411323, 141.9452969622818, 283.0112923058967,
      387.274268318411323,
    ];

    const diffs = huesGivingChromaApexes.map((val) => {
      return Math.abs(hctActiveGlow.hue - val);
    });

    let selectedDiffIdx = 0;
    diffs.forEach((diff, i) => {
      if (diff < diffs[selectedDiffIdx]) {
        selectedDiffIdx = i;
      }
    });

    console.log(selectedDiffIdx);

    // const kMaxHueDiff = 30;
    const kMaxHueDiff = 10;

    if (diffs[selectedDiffIdx] <= kMaxHueDiff) {
      hctGlow.hue = huesGivingChromaApexes[selectedDiffIdx];
      // can just set hue to match apex.
    } else {
      const currentHue = hctGlow.hue;
      const idealHue = huesGivingChromaApexes[selectedDiffIdx];
      if (idealHue > currentHue) {
        hctGlow.hue += kMaxHueDiff;
      } else {
        hctGlow.hue -= kMaxHueDiff;
      }
      console.log(`actualHue:${currentHue};newHue:${hctGlow.hue}`);
    }

    let glow = hctToRgbArr(hctGlow);

    let highestIdx = 0;
    glow.forEach((color, i) => {
      if (color > glow[highestIdx]) {
        highestIdx = i;
      }
    });
    let scaledUpGlow;

    switch (highestIdx) {
      case 0: {
        const coeff = 0xff / glow[0];
        scaledUpGlow = glow.map((color) => {
          return normalizeColor(coeff * color);
        });
        break;
      }
      case 1: {
        const coeff = 0xff / glow[1];
        scaledUpGlow = glow.map((color) => {
          return normalizeColor(coeff * color);
        });
        break;
      }
      case 2: {
        const coeff = 0xff / glow[2];
        scaledUpGlow = glow.map((color) => {
          return normalizeColor(coeff * color);
        });
        break;
      }
    }

    console.log(scaledUpGlow);

    const primaryR = primaryFromGlowAndActiveGlow(scaledUpGlow[0], activeGlowR);
    const primaryG = primaryFromGlowAndActiveGlow(scaledUpGlow[1], activeGlowG);
    const primaryB = primaryFromGlowAndActiveGlow(scaledUpGlow[2], activeGlowB);

    const primaryHct = rgbArrToHct([primaryR, primaryG, primaryB]);
    if (primaryHct.tone >= 95) {
      // Don't shift hue if primary is close to white.
      primaryHct.hue = hctActiveGlow.hue;
      primaryHct.tone = 95;
      // glow = hctToRgbArr(hctGlow);
    }
    // else if (primaryHct.tone < 80) {
    //   primaryHct.tone = 80;
    // }

    if (Math.abs(primaryHct.hue - hctActiveGlow.hue) >= kMaxHueDiff) {
      primaryHct.hue = hctActiveGlow.hue;
    }

    // For green, use #012b00
    // For blue, use #07097d

    const hctPrimaryActive = cloneHct(hctActiveGlow);
    hctPrimaryActive.tone = 3;

    const primaryArr = hctToRgbArr(primaryHct);

    const dwCoeffs = [0xb4 / 0xff, 0x87 / 0xdc, 0.5213903743315508];

    const primaryDwArr = primaryArr.map((val, i) => {
      return Math.round(val * dwCoeffs[i]);
    });

    // const secondaryHct = Hct.fromInt(0xff000000 | colorHexToU24(secondaryHex));
    // const secondaryArr = hctToRgbArr(secondaryHct);

    // const secDwCoeffs = [1, 1, 0xc3 / 0xeb];

    // const secondaryDwArr = secondaryArr.map((val, i) => {
    //   return Math.round(val * secDwCoeffs[i]);
    // });

    // const secActiveHct = tipsActiveFromTips(secondaryHct);

    // glowActiveDw

    const glowActiveDwR = activeGlowR;
    const glowActiveDwG = Math.round((activeGlowG * 0x64) / 0x78);
    const glowActiveDwB = Math.round((activeGlowB * 0x87 + 0xff * 0x78) / 0xff);
    // const glowActiveDwB = activeGlowB;

    return {
      primary: rgbArrToHexStr(hctToRgbArr(primaryHct)),
      primary2: rgbArrToInt(hctToRgbArr(primaryHct)),
      glow: rgbArrToHexStr(glow),
      primaryActive: rgbArrToHexStr(hctToRgbArr(hctPrimaryActive)),
      primaryDw: rgbArrToHexStr(primaryDwArr),
      // secondaryDw: rgbArrToHexStr(secondaryDwArr),
      // secondaryActive: rgbArrToHexStr(hctToRgbArr(secActiveHct)),
      glowActiveDw: rgbArrToHexStr([
        glowActiveDwR,
        glowActiveDwG,
        glowActiveDwB,
      ]),
    };
  }

  function midnaHairColorsBaseAndGlow(primaryHex) {
    return getGlowFromActiveGlow(primaryHex);
  }

  function midnaHairColorsTips(tipsHex) {
    const tipsHct = Hct.fromInt(0xff000000 | colorHexToU24(tipsHex));
    const tipsArr = hctToRgbArr(tipsHct);

    const tipsDarkWorldCoeffs = [1, 1, 0xc3 / 0xeb];

    const tipsDarkWorldArr = tipsArr.map((val, i) => {
      return Math.round(val * tipsDarkWorldCoeffs[i]);
    });

    const tipsActiveHct = tipsActiveFromTips(tipsHct);

    return {
      secondaryDw: rgbArrToHexStr(tipsDarkWorldArr),
      secondaryActive: rgbArrToHexStr(hctToRgbArr(tipsActiveHct)),
    };
  }

  window.MidnaHairColors = {
    calcBaseAndGlow: midnaHairColorsBaseAndGlow,
    calcTips: midnaHairColorsTips,
  };
})();
