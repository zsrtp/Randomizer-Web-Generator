import { escapeHtml } from 'src/util/escapeHtml';

export const PRESETS = [
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
    aliases: ['s1', 'season1'],
    origSettingsStr:
      '6sY4N2kPC__6KD2P2001WG-WGP8HvayVFbyej6er6fXtFvVkbqkbqkbqk1mE1mE1mE1mE1mE1mE1mE3_aw7fNCQGO3AacP0aOW1zZoruE2Ar3ABAWYv1ix359-_JmFo_2LhyJp-lujJWfgEGuigx9udSYf7EAJ2a8aZG74kiOCbYDg3dmL1QkYJHfZ8amkWwX05dyKP0CMg3jEOenjCkeWokixRk9mfHWauqf5C4T984e9hZDQIQccGTWMYqjOC9sw1eXmRnoXG_nJ92pvbcwGnB0X_u',
    description:
      'Season 1 tournament settings. Good for a quick seed or to learn racing basics.',
  },
  {
    name: 'Season 2',
    aliases: ['s2', 'season2', 'tournament'],
    origSettingsStr:
      '6s28H2kPC_v6592PDAa1W536qP8HvayVFbylHwej6er6fXtFuTgvVkXqkbqkbqkbmk1mE1mE1mE1mE1mE1mE1mE3_aw7fN2enf1WCgIPa2N1680VOyjU3WYjGoY2Ba_JylmlnFBKcev3Yohii6LQA7Ms16h2TvudSYhH0qYm95OHpYamf298q1nBh639OZQWvy5GMheaqQOo9CBeEeG1P_56G35gWxJcACRJBg8Chjkud2b62JZIaK8x136T984e9fqECrf9gQP1s1QBIrWmdRe6Y71l7A53_5CaBFcMRf34i27_qg0WG1IM1JT1JB1Jb1JU1Gu1NG1HQ1Gj1LA1Ku1L41MN1L61NC1KL1Jn1ML1Hn1Ho1Kr1Lo1K41OX1OW1OV1OU1OT1OS1OR1OQ1OP1OM1OJ1PL1PK1PJ1PI1PH1PE1PD1PC1PB1PA1P91P81P71PG1PF1P61OI1OH1OF1OE1OD1OC1OB1OA1O91O81O61O51O41O31On1Om1Ok1Oj1Oi1Mm1qK1Zh1Yh1M31V_',
    description:
      'Season 2 tournament settings. These are the latest tournament settings.',
  },
];

export const PRESETS_SAFE_STR = escapeHtml(JSON.stringify(PRESETS));
